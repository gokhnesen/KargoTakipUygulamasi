using KargoTakip.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace KargoTakip.API.Services
{
    public class TrustedDeviceCookieService : ITrustedDeviceService
    {
        private const string CookieName = "TrustedDevice";
        private const int TrustedHours = 2;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public TrustedDeviceCookieService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        public void SetTrustedDevice(string userId)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return;

            var expiry = DateTime.UtcNow.AddHours(TrustedHours);
            var payload = $"{userId}|{expiry.Ticks}";
            var value = SignAndEncode(payload);
            if (value == null) return;

            context.Response.Cookies.Append(CookieName, value, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                MaxAge = TimeSpan.FromHours(TrustedHours),
                Path = "/"
            });
        }

        public string? GetTrustedUserIdIfValid()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            if (!context.Request.Cookies.TryGetValue(CookieName, out var value) || string.IsNullOrEmpty(value))
                return null;

            var payload = DecodeAndVerify(value);
            if (payload == null) return null;

            var parts = payload.Split('|');
            if (parts.Length != 2) return null;
            if (!long.TryParse(parts[1], out var expiryTicks)) return null;
            if (DateTime.UtcNow.Ticks > expiryTicks) return null;

            return parts[0];
        }

        private string? SignAndEncode(string payload)
        {
            var key = _configuration["JwtSettings:SecretKey"];
            if (string.IsNullOrEmpty(key)) return null;

            var payloadBytes = Encoding.UTF8.GetBytes(payload);
            var signature = HMACSHA256.HashData(Encoding.UTF8.GetBytes(key), payloadBytes);
            var payloadB64 = Convert.ToBase64String(payloadBytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
            var sigB64 = Convert.ToBase64String(signature).TrimEnd('=').Replace('+', '-').Replace('/', '_');
            return $"{payloadB64}.{sigB64}";
        }

        private string? DecodeAndVerify(string value)
        {
            var key = _configuration["JwtSettings:SecretKey"];
            if (string.IsNullOrEmpty(key)) return null;

            var dot = value.IndexOf('.');
            if (dot <= 0) return null;

            var payloadB64 = value.Substring(0, dot).Replace('-', '+').Replace('_', '/');
            var pad = 4 - payloadB64.Length % 4;
            if (pad < 4) payloadB64 += new string('=', pad);

            byte[] payloadBytes;
            try
            {
                payloadBytes = Convert.FromBase64String(payloadB64);
            }
            catch
            {
                return null;
            }

            var sigB64 = value.Substring(dot + 1).Replace('-', '+').Replace('_', '/');
            pad = 4 - sigB64.Length % 4;
            if (pad < 4) sigB64 += new string('=', pad);
            byte[] signature;
            try
            {
                signature = Convert.FromBase64String(sigB64);
            }
            catch
            {
                return null;
            }

            var expectedSig = HMACSHA256.HashData(Encoding.UTF8.GetBytes(key), payloadBytes);
            if (signature.Length != expectedSig.Length || !CryptographicOperations.FixedTimeEquals(signature, expectedSig))
                return null;

            return Encoding.UTF8.GetString(payloadBytes);
        }
    }
}
