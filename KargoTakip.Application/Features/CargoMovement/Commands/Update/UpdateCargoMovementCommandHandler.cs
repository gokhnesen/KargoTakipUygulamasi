using KargoTakip.Application.Exceptions;
using KargoTakip.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace KargoTakip.Application.Features.CargoMovement.Commands.UpdateCargoMovement
{
    public class UpdateCargoMovementCommandHandler : IRequestHandler<UpdateCargoMovementCommand, UpdateCargoMovementCommandResponse>
    {
        private readonly IGenericRepository<Domain.Entities.CargoMovement> _movementRepository;
        private readonly IGenericRepository<Domain.Entities.Cargo> _cargoRepository;
        private readonly ILogger<UpdateCargoMovementCommandHandler> _logger;

        public UpdateCargoMovementCommandHandler(
            IGenericRepository<Domain.Entities.CargoMovement> movementRepository,
            IGenericRepository<Domain.Entities.Cargo> cargoRepository,
            ILogger<UpdateCargoMovementCommandHandler> logger)
        {
            _movementRepository = movementRepository;
            _cargoRepository = cargoRepository;
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
            cargo.UpdatedDate = DateTime.UtcNow;

            var updatedCargo = await _cargoRepository.UpdateAsync(cargo);
            if (updatedCargo == null)
            {
                _logger.LogError("Kargo durumu güncellenemedi. CargoId: {CargoId}", request.CargoId);
                throw new BusinessException("Kargo durumu güncellenemedi.");
            }

            _logger.LogInformation("Kargo hareketi ba?ar?yla güncellendi. CargoId: {CargoId}, Yeni Durum: {NewStatus}, Lokasyon: {Location}", 
                request.CargoId, request.NewStatus, request.Location);

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