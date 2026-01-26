using KargoTakip.Application.Exceptions;
using KargoTakip.Application.Interfaces;
using KargoTakip.Application.Interfaces.Notification;
using KargoTakip.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
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
        private readonly CargoStatusHelper _statusHelper;
        private readonly EmailTemplateBuilder _emailTemplateBuilder;

        public UpdateCargoMovementCommandHandler(
            IGenericRepository<Domain.Entities.CargoMovement> movementRepository,
            IGenericRepository<Domain.Entities.Cargo> cargoRepository,
            IGenericRepository<Domain.Entities.CargoReceiver> receiverRepository,
            INotificationService notificationService,
            ILogger<UpdateCargoMovementCommandHandler> logger,
            CargoStatusHelper statusHelper,
            EmailTemplateBuilder emailTemplateBuilder)
        {
            _movementRepository = movementRepository;
            _cargoRepository = cargoRepository;
            _receiverRepository = receiverRepository;
            _notificationService = notificationService;
            _logger = logger;
            _statusHelper = statusHelper;
            _emailTemplateBuilder = emailTemplateBuilder;
        }

        public async Task<UpdateCargoMovementCommandResponse> Handle(UpdateCargoMovementCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Kargo hareketi guncelleme islemi baslatildi. CargoId: {CargoId}, Yeni Durum: {NewStatus}", 
                request.CargoId, request.NewStatus);

            var cargo = await _cargoRepository.GetByIdAsync(request.CargoId);
            if (cargo == null)
            {
                _logger.LogError("Kargo bulunamadi. CargoId: {CargoId}", request.CargoId);
                throw new BusinessException("Kargo bulunamadi.");
            }

            if (cargo.IsDeleted)
            {
                _logger.LogError("Silinmis kargo icin hareket guncellenemez. CargoId: {CargoId}", request.CargoId);
                throw new BusinessException("Silinmis kargo icin hareket guncellenemez.");
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
                _logger.LogError("Kargo hareketi kaydi olusturulamadi. CargoId: {CargoId}", request.CargoId);
                throw new BusinessException("Kargo hareketi kaydi olusturulamadi.");
            }

            _logger.LogDebug("Kargo hareketi kaydedildi. MovementId: {MovementId}", createdMovement.Id);

            cargo.Status = request.NewStatus;
            cargo.UpdatedDate = DateTime.Now;

            var updatedCargo = await _cargoRepository.UpdateAsync(cargo);
            if (updatedCargo == null)
            {
                _logger.LogError("Kargo durumu guncellenemedi. CargoId: {CargoId}", request.CargoId);
                throw new BusinessException("Kargo durumu guncellenemedi.");
            }

            _logger.LogInformation("Kargo hareketi basariyla guncellendi. CargoId: {CargoId}, Yeni Durum: {NewStatus}, Lokasyon: {Location}", 
                request.CargoId, request.NewStatus, request.Location);

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
                    _logger.LogWarning("Alici bilgisi bulunamadi. ReceiverId: {ReceiverId}", cargo.ReceiverId);
                    return false;
                }

                var statusMessage = _statusHelper.GetStatusMessage(request.NewStatus);
                var emailSubject = $"Kargo Durum Guncellemesi - {cargo.TrackingNumber}";
                var emailBody = _emailTemplateBuilder.BuildCargoStatusUpdateEmail(
                    receiver, cargo, statusMessage, request.Location, request.Description);

                await _notificationService.SendEmailAsync(receiver.Email, emailSubject, emailBody);
                _logger.LogInformation("Email gonderildi. Email: {Email}", receiver.Email);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bildirim gonderilirken hata olustu. CargoId: {CargoId}", cargo.Id);
                return false;
            }
        }
    }
}
