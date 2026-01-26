using KargoTakip.Application.Exceptions;
using KargoTakip.Application.Interfaces;
using KargoTakip.Application.Interfaces.Cargo;
using KargoTakip.Application.Interfaces.Notification;
using KargoTakip.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
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
        private readonly CargoStatusHelper _statusHelper;
        private readonly EmailTemplateBuilder _emailTemplateBuilder;

        public CreateCargoMovementCommandHandler(
            IGenericRepository<Domain.Entities.CargoMovement> movementRepository,
            IGenericRepository<Domain.Entities.Cargo> cargoRepository,
            IGenericRepository<Domain.Entities.CargoReceiver> receiverRepository,
            INotificationService notificationService,
            ILogger<CreateCargoMovementCommandHandler> logger,
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

        public async Task<CreateCargoMovementCommandResponse> Handle(CreateCargoMovementCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Yeni kargo hareketi olusturuluyor. CargoId: {CargoId}", request.CargoId);

            var cargo = await _cargoRepository.GetValueByExpressionAsync(c => c.Id == request.CargoId);
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