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

namespace KargoTakip.Application.Features.Authentication.Commands.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterCommandResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailQueue _emailQueue;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RegisterCommandHandler> _logger;

        public RegisterCommandHandler(
            UserManager<ApplicationUser> userManager,
            IEmailQueue emailQueue,
            IConfiguration configuration,
            ILogger<RegisterCommandHandler> logger)
        {
            _userManager = userManager;
            _emailQueue = emailQueue;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<RegisterCommandResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Yeni kullanici kaydi baslatiliyor: {Email}", request.Email);

            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Kullanici zaten mevcut: {Email}", request.Email);
                throw new AuthenticationException("Bu email adresi zaten kullaniliyor.");
            }

            var user = new ApplicationUser
            {
                Email = request.Email,
                UserName = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Kullanici kaydi basarisiz: {Errors}", errors);
                throw new AuthenticationException($"Kullanici kaydi basarisiz: {errors}");
            }

            await SendConfirmationEmailAsync(user, cancellationToken);

            _logger.LogInformation("Kullanici basariyla kaydedildi, onay e-postasi gonderildi: {Email}", request.Email);

            return new RegisterCommandResponse
            {
                Success = true,
                Message = "Kayit basarili. Lutfen e-posta adresinize gelen onay linkine tiklayin.",
                UserId = user.Id.ToString()
            };
        }

        private async Task SendConfirmationEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var clientBaseUrl = _configuration["App:ClientBaseUrl"]?.TrimEnd('/') ?? "https://localhost:4200";
            var confirmUrl = $"{clientBaseUrl}/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

            var subject = $"E-posta adresinizi onaylayın - {EmailTemplateBuilder.CompanyName}";
            var innerHtml = $@"
                <h2 style=""margin:0 0 16px; font-size:18px; color:#111827;"">Hoş geldiniz</h2>
                <p style=""margin:0 0 12px;"">{EmailTemplateBuilder.CompanyName} hesabınızı oluşturdunuz. Aşağıdaki linke tıklayarak e-posta adresinizi onaylayın:</p>
                <p style=""margin:0 0 16px;""><a href=""{confirmUrl}"" style=""display:inline-block; padding:12px 24px; background:#E85D04; color:#fff; text-decoration:none; border-radius:8px; font-weight:600;"">E-postamı onayla</a></p>
                <p style=""margin:0 0 8px;"">Linke tıklayamazsanız aşağıdaki adresi tarayıcınıza kopyalayın:</p>
                <p style=""word-break:break-all; color:#6B7280; font-size:13px; margin:0 0 12px;"">{confirmUrl}</p>
                <p style=""margin:0;"">Bu e-postayı siz talep etmediyseniz lütfen dikkate almayın.</p>";
            var htmlBody = EmailTemplateBuilder.WrapWithBranding(innerHtml);
            var plainBody = $"E-posta adresinizi onaylamak icin asagidaki linke tiklayin: {confirmUrl}";

            await _emailQueue.PublishAsync(new SendEmailMessage
            {
                Email = user.Email ?? string.Empty,
                Subject = subject,
                PlainBody = plainBody,
                HtmlBody = htmlBody,
                MessageType = "KayitOnay"
            }, cancellationToken);
        }
    }
}