using KargoTakip.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System.Threading;
using System.Threading.Tasks;

namespace KargoTakip.Infrastructure.Services
{
    public class MemoryLoginOtpStore : ILoginOtpStore
    {
        private const string KeyPrefix = "login_otp:";
        private readonly IMemoryCache _cache;

        public MemoryLoginOtpStore(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Task SetAsync(string loginToken, string userId, string email, string code, System.TimeSpan expiry, CancellationToken cancellationToken = default)
        {
            var key = KeyPrefix + loginToken;
            var data = new LoginOtpData { UserId = userId, Email = email, Code = code };
            _cache.Set(key, data, new MemoryCacheEntryOptions().SetAbsoluteExpiration(expiry));
            return Task.CompletedTask;
        }

        public Task<LoginOtpData?> GetAndRemoveAsync(string loginToken, CancellationToken cancellationToken = default)
        {
            var key = KeyPrefix + loginToken;
            if (_cache.TryGetValue(key, out LoginOtpData? data) && data != null)
            {
                _cache.Remove(key);
                return Task.FromResult<LoginOtpData?>(data);
            }
            return Task.FromResult<LoginOtpData?>(null);
        }
    }
}
