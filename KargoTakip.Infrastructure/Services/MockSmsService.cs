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
            _logger.LogInformation("[MOCK SMS] Telefon: {Phone}, Mesaj: {Message}", phoneNumber, message);
            return Task.CompletedTask;
        }
    }
}
