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
            try
            {
                await _emailService.SendAsync(email, subject, body, body);
                _logger.LogInformation("Email başarıyla gönderildi. Email: {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email gönderme hatası. Email: {Email}", email);
                throw;
            }
        }

        public async Task SendSmsAsync(string phoneNumber, string message)
        {
            try
            {
                await _smsService.SendAsync(phoneNumber, message);
                _logger.LogInformation("SMS başarıyla gönderildi. Telefon: {Phone}", phoneNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMS gönderme hatası. Telefon: {Phone}", phoneNumber);
                throw;
            }
        }
    }
}