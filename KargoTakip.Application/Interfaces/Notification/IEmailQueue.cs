using KargoTakip.Application.Messages;
using System.Threading;
using System.Threading.Tasks;

namespace KargoTakip.Application.Interfaces.Notification
{
    /// <summary>
    /// Publishes email send requests to a message queue (e.g. RabbitMQ) for async processing.
    /// </summary>
    public interface IEmailQueue
    {
        Task PublishAsync(SendEmailMessage message, CancellationToken cancellationToken = default);
    }
}
