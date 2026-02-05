using KargoTakip.Application.Interfaces.Notification;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace KargoTakip.Infrastructure.Services
{
    public class BrevoEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<BrevoEmailService> _logger;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _senderEmail;
        private readonly string _senderName;

        public BrevoEmailService(
            IConfiguration configuration,
            ILogger<BrevoEmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            _smtpServer = _configuration["Brevo:SmtpServer"] ?? "smtp-relay.brevo.com";
            _smtpPort = _configuration.GetValue<int>("Brevo:SmtpPort", 587);
            _smtpUsername = _configuration["Brevo:SmtpUsername"];
            _smtpPassword = _configuration["Brevo:SmtpPassword"];
            _senderEmail = _configuration["Brevo:SenderEmail"] ?? "noreply@kargotakip.com";
            _senderName = _configuration["Brevo:SenderName"] ?? "KargoTakip";

            if (string.IsNullOrEmpty(_smtpUsername) || string.IsNullOrEmpty(_smtpPassword))
                _logger.LogWarning("Brevo SMTP bilgileri yapilandirilmamis! Brevo:SmtpUsername ve Brevo:SmtpPassword (API key) gerekli.");
        }

        public async Task SendAsync(string email, string subject, string body, string htmlBody)
        {
            if (string.IsNullOrEmpty(_smtpUsername) || string.IsNullOrEmpty(_smtpPassword))
            {
                _logger.LogError("Brevo SMTP bilgileri eksik. OTP/email gonderilmiyor. appsettings.json veya User Secrets icinde Brevo:SmtpUsername ve Brevo:SmtpPassword (Brevo API key) tanimlayin.");
                throw new InvalidOperationException("Brevo SMTP yapilandirmasi eksik. Brevo:SmtpUsername ve Brevo:SmtpPassword gerekli.");
            }

            try
            {
                using var smtpClient = new SmtpClient(_smtpServer, _smtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(_smtpUsername, _smtpPassword)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_senderEmail, _senderName),
                    Subject = subject,
                    Body = string.IsNullOrEmpty(htmlBody) ? body : htmlBody,
                    IsBodyHtml = !string.IsNullOrEmpty(htmlBody)
                };

                mailMessage.To.Add(email);

                await smtpClient.SendMailAsync(mailMessage);

                _logger.LogInformation("Email basariyla gonderildi. Alici: {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Brevo email gonderme hatasi. Alici: {Email}. Brevo panelde gonderici adresin dogrulandigindan emin ol.", email);
                throw;
            }
        }
    }
}
