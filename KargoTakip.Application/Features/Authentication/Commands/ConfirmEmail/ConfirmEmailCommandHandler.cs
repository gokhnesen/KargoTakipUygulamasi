using KargoTakip.Application.Exceptions;
using KargoTakip.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KargoTakip.Application.Features.Authentication.Commands.ConfirmEmail
{
    public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, ConfirmEmailCommandResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ConfirmEmailCommandHandler> _logger;

        public ConfirmEmailCommandHandler(
            UserManager<ApplicationUser> userManager,
            ILogger<ConfirmEmailCommandHandler> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<ConfirmEmailCommandResponse> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.Token))
            {
                throw new AuthenticationException("Gecersiz onay linki.");
            }

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                _logger.LogWarning("E-posta onay: kullanici bulunamadi. UserId: {UserId}", request.UserId);
                throw new AuthenticationException("Gecersiz onay linki.");
            }

            if (user.EmailConfirmed)
            {
                return new ConfirmEmailCommandResponse
                {
                    Success = true,
                    Message = "E-posta adresiniz zaten onayli. Giris yapabilirsiniz."
                };
            }

            var result = await _userManager.ConfirmEmailAsync(user, request.Token);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("E-posta onay basarisiz: {Email}, Hatalar: {Errors}", user.Email, errors);
                throw new AuthenticationException("Onay linki gecersiz veya suresi dolmus. Lutfen yeni onay e-postasi talep edin.");
            }

            _logger.LogInformation("E-posta onaylandi: {Email}", user.Email);

            return new ConfirmEmailCommandResponse
            {
                Success = true,
                Message = "E-posta adresiniz basariyla onaylandi. Giris yapabilirsiniz."
            };
        }
    }
}
