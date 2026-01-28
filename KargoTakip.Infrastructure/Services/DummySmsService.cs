using KargoTakip.Application.Interfaces.Notification;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace KargoTakip.Infrastructure.Services
{
    public class DummySmsService : ISmsService
    {
        private readonly ILogger<DummySmsService> _logger;

        public DummySmsService(ILogger<DummySmsService> logger)
        {
            _logger = logger;
        }

        public Task SendAsync(string phoneNumber, string message)
        {
            _logger.LogInformation("SMS servisi henuz implementasyona alinmadi. Telefon: {PhoneNumber}, Mesaj: {Message}", 
                phoneNumber, message);
            
            return Task.CompletedTask;
        }
    }
}
