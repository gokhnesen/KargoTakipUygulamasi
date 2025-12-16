using KargoTakip.Application.Interfaces.Notification;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace KargoTakip.Infrastructure.Services
{
    public class MockSmsService : ISmsService
    {
        private readonly ILogger<MockSmsService> _logger;

        public MockSmsService(ILogger<MockSmsService> logger)
        {
            _logger = logger;
        }

        public Task SendAsync(string phoneNumber, string message)
        {
            _logger.LogInformation("Mock SMS Gönderiliyor:");
            _logger.LogInformation("To: {Phone}", phoneNumber);
            _logger.LogInformation("Message: {Message}", message);
            
            return Task.CompletedTask;
        }
    }
}