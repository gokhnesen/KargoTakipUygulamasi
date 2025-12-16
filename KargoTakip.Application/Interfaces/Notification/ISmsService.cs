using System.Threading.Tasks;

namespace KargoTakip.Application.Interfaces.Notification
{
    public interface ISmsService
    {
        Task SendAsync(string phoneNumber, string message);
    }
}