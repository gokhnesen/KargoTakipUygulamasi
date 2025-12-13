using KargoTakip.Application.Exceptions;
using KargoTakip.Application.Interfaces;
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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LoginCommandHandler> _logger;

        public LoginCommandHandler(
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService,
            IConfiguration configuration,
            ILogger<LoginCommandHandler> logger)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<LoginCommandResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Kullanıcı girişi deneniyor: {Email}", request.Email);

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("Kullanıcı bulunamadı: {Email}", request.Email);
                throw new AuthenticationException("Email veya şifre hatalı.");
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                _logger.LogWarning("Kilitli hesap giriş denemesi: {Email}", request.Email);
                throw new AuthenticationException("Hesabınız çok fazla başarısız giriş denemesi nedeniyle geçici olarak kilitlenmiştir.");
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Pasif kullanıcı giriş denemesi: {Email}", request.Email);
                throw new AuthenticationException("Hesabınız pasif durumda.");
            }

            if (!await _userManager.CheckPasswordAsync(user, request.Password))
            {
                await _userManager.AccessFailedAsync(user);

                _logger.LogWarning("Başarısız giriş denemesi: {Email}", request.Email);
                throw new AuthenticationException("Email veya şifre hatalı.");
            }

            await _userManager.ResetAccessFailedCountAsync(user);

            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenService.GenerateAccessToken(user, roles);
            var refreshToken = _tokenService.GenerateRefreshToken();

            _logger.LogInformation("Kullanıcı başarıyla giriş yaptı: {Email}", request.Email);

            return new LoginCommandResponse
            {
                Success = true,
                Token = token,
                RefreshToken = refreshToken,
                Expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpirationMinutes"])),
                Message = "Giriş başarılı."
            };
        }
    }
}