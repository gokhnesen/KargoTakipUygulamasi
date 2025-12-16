using KargoTakip.Application.Interfaces.Notification;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace KargoTakip.Infrastructure.Services
{
    public class MockEmailService : IEmailService
    {
        private readonly ILogger<MockEmailService> _logger;

        public MockEmailService(ILogger<MockEmailService> logger)
        {
            _logger = logger;
        }

        public Task SendAsync(string toEmail, string subject, string body, string htmlBody = null)
        {
            _logger.LogInformation("Mock Email Gönderiliyor:");
            _logger.LogInformation("To: {Email}", toEmail);
            _logger.LogInformation("Subject: {Subject}", subject);
            _logger.LogInformation("Body: {Body}", body);
            
            return Task.CompletedTask;
        }
    }
}