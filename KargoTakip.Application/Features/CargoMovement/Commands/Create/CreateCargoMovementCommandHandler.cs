using KargoTakip.Application.Exceptions;
using KargoTakip.Application.Helpers.Email;
using KargoTakip.Application.Interfaces;
using KargoTakip.Application.Interfaces.Notification;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace KargoTakip.Application.Features.CargoMovement.Commands.Create
{
    public class CreateCargoMovementCommandHandler : IRequestHandler<CreateCargoMovementCommand, CreateCargoMovementCommandResponse>
    {
        private readonly IGenericRepository<Domain.Entities.CargoMovement> _movementRepository;
        private readonly IGenericRepository<Domain.Entities.Cargo> _cargoRepository;
        private readonly IGenericRepository<Domain.Entities.CargoReceiver> _receiverRepository;
        private readonly IEmailQueue _emailQueue;
        private readonly ILogger<CreateCargoMovementCommandHandler> _logger;

        public CreateCargoMovementCommandHandler(
            IGenericRepository<Domain.Entities.CargoMovement> movementRepository,
            IGenericRepository<Domain.Entities.Cargo> cargoRepository,
            IGenericRepository<Domain.Entities.CargoReceiver> receiverRepository,
            IEmailQueue emailQueue,
            ILogger<CreateCargoMovementCommandHandler> logger)
        {
            _movementRepository = movementRepository;
            _cargoRepository = cargoRepository;
            _receiverRepository = receiverRepository;
            _emailQueue = emailQueue;
            _logger = logger;
        }

        public async Task<CreateCargoMovementCommandResponse> Handle(CreateCargoMovementCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Yeni kargo hareketi olusturuluyor. CargoId: {CargoId}", request.CargoId);

            var cargo = await _cargoRepository.GetValueByExpressionAsync(c => c.Id == request.CargoId, cancellationToken);
            if (cargo == null)
            {
                _logger.LogError("Kargo bulunamadi. CargoId: {CargoId}", request.CargoId);
                throw new BusinessException("Kargo bulunamadi.");
            }

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
                _logger.LogError("Kargo hareketi kaydedilemedi.");
                throw new BusinessException("Kargo hareketi kaydedilemedi.");
            }

            cargo.Status = request.NewStatus;
            cargo.LastLocation = request.Location;
            await _cargoRepository.UpdateAsync(cargo, cancellationToken);

            _logger.LogInformation("Kargo hareketi kaydedildi. MovementId: {MovementId}", createdMovement.Id);

            var notificationSent = false;
            if (request.SendNotification)
            {
                notificationSent = await CargoStatusEmailHelper.SendCargoStatusUpdateToQueueAsync(
                    _emailQueue, _receiverRepository, cargo, request.NewStatus, request.Location, request.Description,
                    _logger, cancellationToken);
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
    }
}