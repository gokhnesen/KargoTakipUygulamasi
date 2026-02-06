using KargoTakip.Application.Messages;
using System.Threading;
using System.Threading.Tasks;

namespace KargoTakip.Application.Interfaces.Notification
{
    public interface IEmailQueue
    {
        Task PublishAsync(SendEmailMessage message, CancellationToken cancellationToken = default);
    }
}
