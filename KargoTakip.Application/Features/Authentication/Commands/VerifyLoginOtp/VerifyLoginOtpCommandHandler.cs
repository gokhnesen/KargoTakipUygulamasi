using KargoTakip.Application.Exceptions;
using KargoTakip.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using KargoTakip.Domain.Entities;

namespace KargoTakip.Application.Features.Authentication.Commands.VerifyLoginOtp
{
    public class VerifyLoginOtpCommandHandler : IRequestHandler<VerifyLoginOtpCommand, VerifyLoginOtpCommandResponse>
    {
        private readonly ILoginOtpStore _loginOtpStore;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<VerifyLoginOtpCommandHandler> _logger;
        private readonly ITrustedDeviceService _trustedDeviceService;

        public VerifyLoginOtpCommandHandler(
            ILoginOtpStore loginOtpStore,
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService,
            IConfiguration configuration,
            ILogger<VerifyLoginOtpCommandHandler> logger,
            ITrustedDeviceService trustedDeviceService)
        {
            _loginOtpStore = loginOtpStore;
            _userManager = userManager;
            _tokenService = tokenService;
            _configuration = configuration;
            _logger = logger;
            _trustedDeviceService = trustedDeviceService;
        }

        public async Task<VerifyLoginOtpCommandResponse> Handle(VerifyLoginOtpCommand request, CancellationToken cancellationToken)
        {
            var data = await _loginOtpStore.GetAndRemoveAsync(request.LoginToken, cancellationToken);
            if (data == null)
            {
                _logger.LogWarning("OTP dogrulama: gecersiz veya suresi dolmus login token");
                throw new AuthenticationException("Gecersiz veya suresi dolmus kod. Lutfen tekrar giris yapin.");
            }

            var code = request.Code?.Trim().Replace(" ", "") ?? "";
            if (string.IsNullOrEmpty(code) || code != data.Code)
            {
                _logger.LogWarning("OTP dogrulama: hatali kod girildi: {Email}", data.Email);
                throw new AuthenticationException("Hatali dogrulama kodu. Lutfen e-postaniza gelen kodu kontrol edin.");
            }

            var user = await _userManager.FindByIdAsync(data.UserId);
            if (user == null || !user.IsActive)
            {
                _logger.LogWarning("OTP dogrulama: kullanici bulunamadi veya pasif: {UserId}", data.UserId);
                throw new AuthenticationException("Hesap bulunamadi veya pasif.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenService.GenerateAccessToken(user, roles);
            var refreshToken = _tokenService.GenerateRefreshToken();

            _trustedDeviceService.SetTrustedDevice(user.Id.ToString());

            _logger.LogInformation("Kullanici OTP ile giris yapti: {Email}", data.Email);

            return new VerifyLoginOtpCommandResponse
            {
                Success = true,
                Token = token,
                RefreshToken = refreshToken,
                Expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpirationMinutes"])),
                Message = "Giris basarili."
            };
        }
    }
}
