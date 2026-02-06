using KargoTakip.Application.Helpers.Cargo;
using KargoTakip.Application.Interfaces;
using KargoTakip.Application.Interfaces.Notification;
using KargoTakip.Application.Messages;
using KargoTakip.Domain.Entities;
using KargoTakip.Domain.Enums;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace KargoTakip.Application.Helpers.Email
{
    public static class CargoStatusEmailHelper
    {
        /// <summary>
        /// Kargo alıcısını bulur, durum güncellemesi e-postasını oluşturup kuyruğa ekler. Yardımcı metod; DI kullanmaz.
        /// </summary>
        public static async Task<bool> SendCargoStatusUpdateToQueueAsync(
            IEmailQueue emailQueue,
            IGenericRepository<CargoReceiver> receiverRepository,
            KargoTakip.Domain.Entities.Cargo cargo,
            CargoStatus newStatus,
            string location,
            string description,
            ILogger? logger,
            CancellationToken cancellationToken = default)
        {
            var receiver = await receiverRepository.GetValueByExpressionAsync(r => r.Id == cargo.ReceiverId, cancellationToken);
            if (receiver == null)
            {
                logger?.LogWarning("Alici bilgisi bulunamadi. ReceiverId: {ReceiverId}", cargo.ReceiverId);
                return false;
            }

            var statusMessage = CargoStatusHelper.GetStatusMessage(newStatus);
            var subject = $"Kargo Durum Guncellemesi - {cargo.TrackingNumber}";
            var htmlBody = EmailTemplateBuilder.BuildCargoStatusUpdateEmail(
                receiver, cargo, statusMessage, location, description);

            var message = new SendEmailMessage
            {
                Email = receiver.Email,
                Subject = subject,
                PlainBody = htmlBody,
                HtmlBody = htmlBody,
                MessageType = "KargoDurumGuncelleme"
            };

            await emailQueue.PublishAsync(message, cancellationToken);
            logger?.LogInformation("Email kuyruga eklendi. Email: {Email}", receiver.Email);

            return true;
        }
    }
}
