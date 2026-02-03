using KargoTakip.Application.Exceptions;
using KargoTakip.Application.Interfaces;
using KargoTakip.Application.Interfaces.Notification;
using KargoTakip.Application.Messages;
using KargoTakip.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KargoTakip.Application.Features.Authentication.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginCommandResponse>
    {
        private const int OtpExpiryMinutes = 10;
        private const int OtpLength = 6;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LoginCommandHandler> _logger;
        private readonly ILoginOtpStore _loginOtpStore;
        private readonly IEmailQueue _emailQueue;
        private readonly ITrustedDeviceService _trustedDeviceService;

        public LoginCommandHandler(
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService,
            IConfiguration configuration,
            ILogger<LoginCommandHandler> logger,
            ILoginOtpStore loginOtpStore,
            IEmailQueue emailQueue,
            ITrustedDeviceService trustedDeviceService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _configuration = configuration;
            _logger = logger;
            _loginOtpStore = loginOtpStore;
            _emailQueue = emailQueue;
            _trustedDeviceService = trustedDeviceService;
        }

        public async Task<LoginCommandResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Kullanici girisi deneniyor: {Email}", request.Email);

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("Kullanici bulunamadi: {Email}", request.Email);
                throw new AuthenticationException("Email veya sifre hatali.");
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                _logger.LogWarning("Kilitli hesap giris denemesi: {Email}", request.Email);
                throw new AuthenticationException("Hesabiniz cok fazla basarisiz giris denemesi nedeniyle gecici olarak kilitlenmistir.");
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Pasif kullanici giris denemesi: {Email}", request.Email);
                throw new AuthenticationException("Hesabiniz pasif durumda.");
            }

            if (!user.EmailConfirmed)
            {
                _logger.LogWarning("E-posta onaylanmamis kullanici giris denemesi: {Email}", request.Email);
                throw new AuthenticationException("E-posta adresiniz henuz onaylanmadi. Lutfen e-posta kutunuzdaki onay linkine tiklayin.");
            }

            if (!await _userManager.CheckPasswordAsync(user, request.Password))
            {
                await _userManager.AccessFailedAsync(user);

                _logger.LogWarning("Basarisiz giris denemesi: {Email}", request.Email);
                throw new AuthenticationException("Email veya sifre hatali.");
            }

            await _userManager.ResetAccessFailedCountAsync(user);

            var trustedUserId = _trustedDeviceService.GetTrustedUserIdIfValid();
            if (trustedUserId == user.Id.ToString())
            {
                var roles = await _userManager.GetRolesAsync(user);
                var token = _tokenService.GenerateAccessToken(user, roles);
                var refreshToken = _tokenService.GenerateRefreshToken();
                _logger.LogInformation("Kullanici guvenilir cihaz ile giris yapti (OTP atlandi): {Email}", request.Email);
                return new LoginCommandResponse
                {
                    Success = true,
                    Token = token,
                    RefreshToken = refreshToken,
                    Expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpirationMinutes"])),
                    Message = "Giris basarili."
                };
            }

            var otp = GenerateOtp();
            var loginToken = Guid.NewGuid().ToString("N");
            await _loginOtpStore.SetAsync(loginToken, user.Id.ToString(), user.Email ?? request.Email, otp, TimeSpan.FromMinutes(OtpExpiryMinutes), cancellationToken);

            var subject = "Giris dogrulama kodu - KargoTakip";
            var body = $"Giris dogrulama kodunuz: {otp}. Bu kod {OtpExpiryMinutes} dakika gecerlidir. Bu islemi siz yapmadiysaniz dikkate almayin.";
            var htmlBody = $@"
                <h2>Giris dogrulama kodu</h2>
                <p>Giris dogrulama kodunuz: <strong>{otp}</strong></p>
                <p>Bu kod <strong>{OtpExpiryMinutes} dakika</strong> gecerlidir.</p>
                <p>Bu islemi siz yapmadiysaniz lutfen dikkate almayin.</p>";
            await _emailQueue.PublishAsync(new SendEmailMessage
            {
                Email = user.Email ?? request.Email,
                Subject = subject,
                PlainBody = body,
                HtmlBody = htmlBody,
                MessageType = "LoginOtp"
            }, cancellationToken);

            _logger.LogInformation("Giris OTP gonderildi: {Email}", request.Email);

            return new LoginCommandResponse
            {
                Success = true,
                RequiresOtp = true,
                LoginToken = loginToken,
                Message = "E-posta adresinize gonderilen 6 haneli kodu girin."
            };
        }

        private static string GenerateOtp()
        {
            var rnd = new Random();
            var digits = new char[OtpLength];
            for (int i = 0; i < OtpLength; i++)
                digits[i] = (char)('0' + rnd.Next(0, 10));
            return new string(digits);
        }
    }
}