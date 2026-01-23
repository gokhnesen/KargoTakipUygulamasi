using KargoTakip.Application.Exceptions;
using KargoTakip.Application.Interfaces;
using KargoTakip.Application.Interfaces.Notification;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KargoTakip.Application.Features.CargoMovement.Commands.UpdateCargoMovement
{
    public class UpdateCargoMovementCommandHandler : IRequestHandler<UpdateCargoMovementCommand, UpdateCargoMovementCommandResponse>
    {
        private readonly IGenericRepository<Domain.Entities.CargoMovement> _movementRepository;
        private readonly IGenericRepository<Domain.Entities.Cargo> _cargoRepository;
        private readonly IGenericRepository<Domain.Entities.CargoReceiver> _receiverRepository;
        private readonly INotificationService _notificationService;
        private readonly ILogger<UpdateCargoMovementCommandHandler> _logger;

        public UpdateCargoMovementCommandHandler(
            IGenericRepository<Domain.Entities.CargoMovement> movementRepository,
            IGenericRepository<Domain.Entities.Cargo> cargoRepository,
            IGenericRepository<Domain.Entities.CargoReceiver> receiverRepository,
            INotificationService notificationService,
            ILogger<UpdateCargoMovementCommandHandler> logger)
        {
            _movementRepository = movementRepository;
            _cargoRepository = cargoRepository;
            _receiverRepository = receiverRepository;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<UpdateCargoMovementCommandResponse> Handle(UpdateCargoMovementCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Kargo hareketi güncelleme i?lemi ba?lat?ld?. CargoId: {CargoId}, Yeni Durum: {NewStatus}", 
                request.CargoId, request.NewStatus);

            var cargo = await _cargoRepository.GetByIdAsync(request.CargoId);
            if (cargo == null)
            {
                _logger.LogError("Kargo bulunamad?. CargoId: {CargoId}", request.CargoId);
                throw new BusinessException("Kargo bulunamad?.");
            }

            if (cargo.IsDeleted)
            {
                _logger.LogError("Silinmi? kargo için hareket güncellenemez. CargoId: {CargoId}", request.CargoId);
                throw new BusinessException("Silinmi? kargo için hareket güncellenemez.");
            }

            _logger.LogDebug("Kargo bulundu. Takip No: {TrackingNumber}, Mevcut Durum: {CurrentStatus}", 
                cargo.TrackingNumber, cargo.Status);

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
                _logger.LogError("Kargo hareketi kayd? olu?turulamad?. CargoId: {CargoId}", request.CargoId);
                throw new BusinessException("Kargo hareketi kayd? olu?turulamad?.");
            }

            _logger.LogDebug("Kargo hareketi kaydedildi. MovementId: {MovementId}", createdMovement.Id);

            cargo.Status = request.NewStatus;
            cargo.UpdatedDate = DateTime.Now;

            var updatedCargo = await _cargoRepository.UpdateAsync(cargo);
            if (updatedCargo == null)
            {
                _logger.LogError("Kargo durumu güncellenemedi. CargoId: {CargoId}", request.CargoId);
                throw new BusinessException("Kargo durumu güncellenemedi.");
            }

            _logger.LogInformation("Kargo hareketi ba?ar?yla güncellendi. CargoId: {CargoId}, Yeni Durum: {NewStatus}, Lokasyon: {Location}", 
                request.CargoId, request.NewStatus, request.Location);

            // Email bildirimi gönder
            var notificationSent = await SendNotificationToReceiverAsync(cargo, request);

            return new UpdateCargoMovementCommandResponse
            {
                Id = createdMovement.Id,
                CargoId = createdMovement.CargoId,
                TrackingNumber = cargo.TrackingNumber,
                NewStatus = createdMovement.NewStatus,
                Location = createdMovement.Location,
                Description = createdMovement.Description,
                CreatedDate = createdMovement.CreatedDate
            };
        }

        private async Task<bool> SendNotificationToReceiverAsync(Domain.Entities.Cargo cargo, UpdateCargoMovementCommand request)
        {
            try
            {
                var receiver = await _receiverRepository.GetValueByExpressionAsync(r => r.Id == cargo.ReceiverId);
                if (receiver == null)
                {
                    _logger.LogWarning("Al?c? bilgisi bulunamad?. ReceiverId: {ReceiverId}", cargo.ReceiverId);
                    return false;
                }

                var statusMessage = GetStatusMessage(request.NewStatus);
                var emailSubject = $"Kargo Durum Güncellesi - {cargo.TrackingNumber}";
                var emailBody = BuildEmailBody(receiver, cargo, request, statusMessage);

                await _notificationService.SendEmailAsync(receiver.Email, emailSubject, emailBody);
                _logger.LogInformation("Email gönderildi. Email: {Email}", receiver.Email);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bildirim gönderilirken hata olu?tu. CargoId: {CargoId}", cargo.Id);
                return false;
            }
        }

        private string GetStatusMessage(Domain.Enums.CargoStatus status)
        {
            return status switch
            {
                Domain.Enums.CargoStatus.Received => "teslim al?nd?",
                Domain.Enums.CargoStatus.InTransit => "yolda",
                Domain.Enums.CargoStatus.OutForDelivery => "da??t?mda",
                Domain.Enums.CargoStatus.Delivered => "teslim edildi",
                Domain.Enums.CargoStatus.Undelivered => "teslim edilemedi",
                _ => "güncellendi"
            };
        }

        private string BuildEmailBody(Domain.Entities.CargoReceiver receiver, Domain.Entities.Cargo cargo, UpdateCargoMovementCommand request, string statusMessage)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"<html><body>");
            sb.AppendLine($"<h2>Merhaba {receiver.Name} {receiver.Surname},</h2>");
            sb.AppendLine($"<p>Takip numaras? <strong>{cargo.TrackingNumber}</strong> olan kargonuzun durumu güncellendi.</p>");
            sb.AppendLine($"<hr/>");
            sb.AppendLine($"<p><strong>Yeni Durum:</strong> {statusMessage}</p>");
            sb.AppendLine($"<p><strong>Konum:</strong> {request.Location}</p>");
            sb.AppendLine($"<p><strong>Aç?klama:</strong> {request.Description}</p>");
            sb.AppendLine($"<p><strong>Güncelleme Tarihi:</strong> {DateTime.Now:dd.MM.yyyy HH:mm}</p>");
            sb.AppendLine($"<hr/>");
            sb.AppendLine($"<p>Kargonuzu takip etmek için takip numaran?z? kullanabilirsiniz.</p>");
            sb.AppendLine($"<p>?yi günler dileriz.</p>");
            sb.AppendLine($"</body></html>");
            return sb.ToString();
        }
    }
}
