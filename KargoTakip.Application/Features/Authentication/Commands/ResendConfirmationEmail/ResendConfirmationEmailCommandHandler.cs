using KargoTakip.Application.Exceptions;
using KargoTakip.Application.Interfaces.Notification;
using KargoTakip.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KargoTakip.Application.Features.Authentication.Commands.ResendConfirmationEmail
{
    public class ResendConfirmationEmailCommandHandler : IRequestHandler<ResendConfirmationEmailCommand, ResendConfirmationEmailCommandResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ResendConfirmationEmailCommandHandler> _logger;

        public ResendConfirmationEmailCommandHandler(
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<ResendConfirmationEmailCommandHandler> logger)
        {
            _userManager = userManager;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ResendConfirmationEmailCommandResponse> Handle(ResendConfirmationEmailCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                throw new AuthenticationException("E-posta adresi gerekli.");
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("Onay e-postasi tekrar: kullanici bulunamadi: {Email}", request.Email);
                throw new AuthenticationException("Bu e-posta adresiyle kayitli kullanici bulunamadi.");
            }

            if (user.EmailConfirmed)
            {
                throw new AuthenticationException("E-posta adresiniz zaten onayli. Giris yapabilirsiniz.");
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var clientBaseUrl = _configuration["App:ClientBaseUrl"]?.TrimEnd('/') ?? "https://localhost:4200";
            var confirmUrl = $"{clientBaseUrl}/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

            var subject = "E-posta Onay Linki - KargoTakip";
            var htmlBody = $@"
                <h2>E-posta Onay Linki</h2>
                <p>Asagidaki linke tiklayarak e-posta adresinizi onaylayin:</p>
                <p><a href=""{confirmUrl}"" style=""display:inline-block; padding:10px 20px; background:#667eea; color:#fff; text-decoration:none; border-radius:8px;"">E-postami Onayla</a></p>
                <p style=""word-break:break-all; color:#666;"">{confirmUrl}</p>
                <p>Bu e-postayi siz talep etmediyseniz lutfen dikkate almayin.</p>";
            var plainBody = $"E-posta adresinizi onaylamak icin: {confirmUrl}";

            await _emailService.SendAsync(user.Email, subject, plainBody, htmlBody);

            _logger.LogInformation("Onay e-postasi tekrar gonderildi: {Email}", request.Email);

            return new ResendConfirmationEmailCommandResponse
            {
                Success = true,
                Message = "Onay linki e-posta adresinize gonderildi. Lutfen e-posta kutunuzu kontrol edin."
            };
        }
    }
}
