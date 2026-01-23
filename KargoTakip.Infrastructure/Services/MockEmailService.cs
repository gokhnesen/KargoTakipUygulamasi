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

        public Task SendAsync(string email, string subject, string body, string htmlBody)
        {
            _logger.LogInformation("[MOCK EMAIL] Email: {Email}, Konu: {Subject}, Body: {Body}", 
                email, subject, body);
            return Task.CompletedTask;
        }
    }
}
