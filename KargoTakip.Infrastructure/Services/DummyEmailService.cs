using KargoTakip.Application.Interfaces.Notification;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace KargoTakip.Infrastructure.Services
{
    /// <summary>
    /// Brevo kapalıyken kullanılır; e-posta göndermez, sadece loglar.
    /// </summary>
    public class DummyEmailService : IEmailService
    {
        private readonly ILogger<DummyEmailService> _logger;

        public DummyEmailService(ILogger<DummyEmailService> logger)
        {
            _logger = logger;
        }

        public Task SendAsync(string toEmail, string subject, string body, string htmlBody = null)
        {
            _logger.LogWarning("Brevo devre disi - e-posta gonderilmedi. To: {Email}, Subject: {Subject}. Brevo:Enabled=true ve Brevo:Smtp* bilgilerini appsettings veya User Secrets ile verin.", toEmail, subject);
            return Task.CompletedTask;
        }
    }
}
