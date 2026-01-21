using KargoTakip.Application.Exceptions;
using KargoTakip.Application.Interfaces;
using KargoTakip.Application.Interfaces.Cargo;
using KargoTakip.Application.Interfaces.Notification;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KargoTakip.Application.Features.CargoMovement.Commands.Create
{
    public class CreateCargoMovementCommandHandler : IRequestHandler<CreateCargoMovementCommand, CreateCargoMovementCommandResponse>
    {
        private readonly IGenericRepository<Domain.Entities.CargoMovement> _movementRepository;
        private readonly IGenericRepository<Domain.Entities.Cargo> _cargoRepository;
        private readonly IGenericRepository<Domain.Entities.CargoReceiver> _receiverRepository;
        private readonly INotificationService _notificationService;
        private readonly ILogger<CreateCargoMovementCommandHandler> _logger;

        public CreateCargoMovementCommandHandler(
            IGenericRepository<Domain.Entities.CargoMovement> movementRepository,
            IGenericRepository<Domain.Entities.Cargo> cargoRepository,
            IGenericRepository<Domain.Entities.CargoReceiver> receiverRepository,
            INotificationService notificationService,
            ILogger<CreateCargoMovementCommandHandler> logger)
        {
            _movementRepository = movementRepository;
            _cargoRepository = cargoRepository;
            _receiverRepository = receiverRepository;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<CreateCargoMovementCommandResponse> Handle(CreateCargoMovementCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Yeni kargo hareketi oluşturuluyor. CargoId: {CargoId}", request.CargoId);

            var cargo = await _cargoRepository.GetValueByExpressionAsync(c => c.Id == request.CargoId);
            if (cargo == null)
            {
                _logger.LogError("Kargo bulunamadı. CargoId: {CargoId}", request.CargoId);
                throw new BusinessException("Kargo bulunamadı.");
            }

            var movement = new Domain.Entities.CargoMovement
            {
                CargoId = request.CargoId,
                NewStatus = request.NewStatus,
                Location = request.Location,
                Description = request.Description
            };

            var createdMovement = await _movementRepository.AddAsync(movement);
            if (createdMovement == null)
            {
                _logger.LogError("Kargo hareketi kaydedilemedi.");
                throw new BusinessException("Kargo hareketi kaydedilemedi.");
            }

            cargo.Status = request.NewStatus;
            await _cargoRepository.UpdateAsync(cargo);

            _logger.LogInformation("Kargo hareketi kaydedildi. MovementId: {MovementId}", createdMovement.Id);

            var notificationSent = false;
            if (request.SendNotification)
            {
                notificationSent = await SendNotificationToReceiverAsync(cargo, request);
            }

            return new CreateCargoMovementCommandResponse
            {
                MovementId = createdMovement.Id,
                CargoId = cargo.Id,
                TrackingNumber = cargo.TrackingNumber,
                NewStatus = request.NewStatus,
                Location = request.Location,
                MovementDate = createdMovement.CreatedDate,
                NotificationSent = notificationSent
            };
        }

        private async Task<bool> SendNotificationToReceiverAsync(Domain.Entities.Cargo cargo, CreateCargoMovementCommand request)
        {
            try
            {
                var receiver = await _receiverRepository.GetValueByExpressionAsync(r => r.Id == cargo.ReceiverId);
                if (receiver == null)
                {
                    _logger.LogWarning("Alıcı bilgisi bulunamadı. ReceiverId: {ReceiverId}", cargo.ReceiverId);
                    return false;
                }

                var statusMessage = GetStatusMessage(request.NewStatus);
                var smsMessage = $"Sayın {receiver.Name} {receiver.Surname}, {cargo.TrackingNumber} takip numaralı kargonuz {statusMessage}. Konum: {request.Location}";

                await _notificationService.SendSmsAsync(receiver.PhoneNumber, smsMessage);
                _logger.LogInformation("SMS gönderildi. Telefon: {Phone}", receiver.PhoneNumber);

                var emailSubject = $"Kargo Durum Güncellesi - {cargo.TrackingNumber}";
                var emailBody = BuildEmailBody(receiver, cargo, request, statusMessage);

                await _notificationService.SendEmailAsync(receiver.Email, emailSubject, emailBody);
                _logger.LogInformation("Email gönderildi. Email: {Email}", receiver.Email);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bildirim gönderilirken hata oluştu. CargoId: {CargoId}", cargo.Id);
                return false;
            }
        }

        private string GetStatusMessage(Domain.Enums.CargoStatus status)
        {
            return status switch
            {
                Domain.Enums.CargoStatus.Received => "teslim alındı",
                Domain.Enums.CargoStatus.InTransit => "yolda",
                Domain.Enums.CargoStatus.OutForDelivery => "dağıtımda",
                Domain.Enums.CargoStatus.Delivered => "teslim edildi",
                Domain.Enums.CargoStatus.Undelivered => "teslim edilemedi",
                _ => "güncellendi"
            };
        }

        private string BuildEmailBody(Domain.Entities.CargoReceiver receiver, Domain.Entities.Cargo cargo, CreateCargoMovementCommand request, string statusMessage)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"<html><body>");
            sb.AppendLine($"<h2>Merhaba {receiver.Name} {receiver.Surname},</h2>");
            sb.AppendLine($"<p>Takip numarası <strong>{cargo.TrackingNumber}</strong> olan kargonuzun durumu güncellendi.</p>");
            sb.AppendLine($"<hr/>");
            sb.AppendLine($"<p><strong>Yeni Durum:</strong> {statusMessage}</p>");
            sb.AppendLine($"<p><strong>Konum:</strong> {request.Location}</p>");
            sb.AppendLine($"<p><strong>Açıklama:</strong> {request.Description}</p>");
            sb.AppendLine($"<p><strong>Güncelleme Tarihi:</strong> {DateTime.Now:dd.MM.yyyy HH:mm}</p>");
            sb.AppendLine($"<hr/>");
            sb.AppendLine($"<p>Kargonuzu takip etmek için takip numaranızı kullanabilirsiniz.</p>");
            sb.AppendLine($"<p>İyi günler dileriz.</p>");
            sb.AppendLine($"</body></html>");
            return sb.ToString();
        }
    }
}