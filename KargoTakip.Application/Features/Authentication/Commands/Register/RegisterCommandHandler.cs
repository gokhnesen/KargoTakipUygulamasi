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

namespace KargoTakip.Application.Features.Authentication.Commands.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterCommandResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RegisterCommandHandler> _logger;

        public RegisterCommandHandler(
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<RegisterCommandHandler> logger)
        {
            _userManager = userManager;
            _emailService = emailService;
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

            var subject = "E-posta Adresinizi Onaylayin - KargoTakip";
            var htmlBody = $@"
                <h2>Hos geldiniz</h2>
                <p>KargoTakip hesabinizi olusturdunuz. Asagidaki linke tiklayarak e-posta adresinizi onaylayin:</p>
                <p><a href=""{confirmUrl}"" style=""display:inline-block; padding:10px 20px; background:#667eea; color:#fff; text-decoration:none; border-radius:8px;"">E-postami Onayla</a></p>
                <p>Linke tiklayamazsaniz asagidaki adresi tarayiciniza kopyalayin:</p>
                <p style=""word-break:break-all; color:#666;"">{confirmUrl}</p>
                <p>Bu e-postayi siz talep etmediyseniz lutfen dikkate almayin.</p>";
            var plainBody = $"E-posta adresinizi onaylamak icin asagidaki linke tiklayin: {confirmUrl}";

            await _emailService.SendAsync(user.Email, subject, plainBody, htmlBody);
        }
    }
}