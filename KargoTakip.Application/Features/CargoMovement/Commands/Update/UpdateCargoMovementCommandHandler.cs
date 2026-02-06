using KargoTakip.Application.Exceptions;
using KargoTakip.Application.Helpers.Email;
using KargoTakip.Application.Interfaces;
using KargoTakip.Application.Interfaces.Notification;
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
        private readonly IEmailQueue _emailQueue;
        private readonly ILogger<UpdateCargoMovementCommandHandler> _logger;

        public UpdateCargoMovementCommandHandler(
            IGenericRepository<Domain.Entities.CargoMovement> movementRepository,
            IGenericRepository<Domain.Entities.Cargo> cargoRepository,
            IGenericRepository<Domain.Entities.CargoReceiver> receiverRepository,
            IEmailQueue emailQueue,
            ILogger<UpdateCargoMovementCommandHandler> logger)
        {
            _movementRepository = movementRepository;
            _cargoRepository = cargoRepository;
            _receiverRepository = receiverRepository;
            _emailQueue = emailQueue;
            _logger = logger;
        }

        public async Task<UpdateCargoMovementCommandResponse> Handle(UpdateCargoMovementCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Kargo hareketi guncelleme islemi baslatildi. CargoId: {CargoId}, Yeni Durum: {NewStatus}", 
                request.CargoId, request.NewStatus);

            var cargo = await _cargoRepository.GetByIdAsync(request.CargoId, cancellationToken);
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

            var createdMovement = await _movementRepository.AddAsync(movement, cancellationToken);
            if (createdMovement == null)
            {
                _logger.LogError("Kargo hareketi kaydi olusturulamadi. CargoId: {CargoId}", request.CargoId);
                throw new BusinessException("Kargo hareketi kaydi olusturulamadi.");
            }

            _logger.LogDebug("Kargo hareketi kaydedildi. MovementId: {MovementId}", createdMovement.Id);

            cargo.Status = request.NewStatus;
            cargo.LastLocation = request.Location;
            cargo.UpdatedDate = DateTime.UtcNow;

            var updatedCargo = await _cargoRepository.UpdateAsync(cargo, cancellationToken);
            if (updatedCargo == null)
            {
                _logger.LogError("Kargo durumu guncellenemedi. CargoId: {CargoId}", request.CargoId);
                throw new BusinessException("Kargo durumu guncellenemedi.");
            }

            _logger.LogInformation("Kargo hareketi basariyla guncellendi. CargoId: {CargoId}, Yeni Durum: {NewStatus}, Lokasyon: {Location}", 
                request.CargoId, request.NewStatus, request.Location);

            var notificationSent = await CargoStatusEmailHelper.SendCargoStatusUpdateToQueueAsync(
                _emailQueue, _receiverRepository, cargo, request.NewStatus, request.Location, request.Description,
                _logger, cancellationToken);

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
    }
}
