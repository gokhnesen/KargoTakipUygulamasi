using System.Threading.Tasks;

namespace KargoTakip.Application.Interfaces.Notification
{
    public interface IEmailService
    {
        Task SendAsync(string toEmail, string subject, string body, string htmlBody = null);
    }
}