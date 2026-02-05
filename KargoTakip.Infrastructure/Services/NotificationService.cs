using KargoTakip.Application.Interfaces.Notification;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace KargoTakip.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IEmailService emailService,
            ISmsService smsService,
            ILogger<NotificationService> logger)
        {
            _emailService = emailService;
            _smsService = smsService;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string body)
        {
            await _emailService.SendAsync(email, subject, body, body);
            _logger.LogInformation("Email basariyla gonderildi. Email: {Email}", email);
        }

        public async Task SendSmsAsync(string phoneNumber, string message)
        {
            await _smsService.SendAsync(phoneNumber, message);
            _logger.LogInformation("SMS basariyla gonderildi. Telefon: {Phone}", phoneNumber);
        }
    }
}