using KargoTakip.Application.Exceptions;
using KargoTakip.Application.Helpers.Email;
using KargoTakip.Application.Interfaces.Notification;
using KargoTakip.Application.Messages;
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
        private readonly IEmailQueue _emailQueue;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ResendConfirmationEmailCommandHandler> _logger;

        public ResendConfirmationEmailCommandHandler(
            UserManager<ApplicationUser> userManager,
            IEmailQueue emailQueue,
            IConfiguration configuration,
            ILogger<ResendConfirmationEmailCommandHandler> logger)
        {
            _userManager = userManager;
            _emailQueue = emailQueue;
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

            var subject = $"E-posta onay linki - {EmailTemplateBuilder.CompanyName}";
            var innerHtml = $@"
                <h2 style=""margin:0 0 16px; font-size:18px; color:#111827;"">E-posta onay linki</h2>
                <p style=""margin:0 0 12px;"">Aşağıdaki linke tıklayarak e-posta adresinizi onaylayın:</p>
                <p style=""margin:0 0 16px;""><a href=""{confirmUrl}"" style=""display:inline-block; padding:12px 24px; background:#E85D04; color:#fff; text-decoration:none; border-radius:8px; font-weight:600;"">E-postamı onayla</a></p>
                <p style=""word-break:break-all; color:#6B7280; font-size:13px; margin:0 0 12px;"">{confirmUrl}</p>
                <p style=""margin:0;"">Bu e-postayı siz talep etmediyseniz lütfen dikkate almayın.</p>";
            var htmlBody = EmailTemplateBuilder.WrapWithBranding(innerHtml);
            var plainBody = $"E-posta adresinizi onaylamak icin: {confirmUrl}";

            await _emailQueue.PublishAsync(new SendEmailMessage
            {
                Email = user.Email ?? string.Empty,
                Subject = subject,
                PlainBody = plainBody,
                HtmlBody = htmlBody,
                MessageType = "ResendOnay"
            }, cancellationToken);

            _logger.LogInformation("Onay e-postasi tekrar gonderildi: {Email}", request.Email);

            return new ResendConfirmationEmailCommandResponse
            {
                Success = true,
                Message = "Onay linki e-posta adresinize gonderildi. Lutfen e-posta kutunuzu kontrol edin."
            };
        }
    }
}
