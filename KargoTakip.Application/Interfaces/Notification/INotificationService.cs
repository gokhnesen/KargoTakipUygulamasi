using System.Threading.Tasks;

namespace KargoTakip.Application.Interfaces.Notification
{
    public interface INotificationService
    {
        Task SendSmsAsync(string phoneNumber, string message);
        Task SendEmailAsync(string email, string subject, string body);
    }
}