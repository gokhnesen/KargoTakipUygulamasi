using KargoTakip.Application.Exceptions;
using KargoTakip.Application.Helpers.Cargo;
using KargoTakip.Application.Interfaces;
using KargoTakip.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KargoTakip.Application.Features.Cargo.Commands.Create
{
    public class CreateCargoCommandHandler : IRequestHandler<CreateCargoCommand, CreateCargoCommandResponse>
    {
        private readonly IGenericRepository<Domain.Entities.Cargo> _cargoRepository;
        private readonly IGenericRepository<CargoSender> _senderRepository;
        private readonly IGenericRepository<CargoReceiver> _receiverRepository;
        private readonly ILogger<CreateCargoCommandHandler> _logger;

        public CreateCargoCommandHandler(
            IGenericRepository<Domain.Entities.Cargo> cargoRepository,
            IGenericRepository<CargoSender> senderRepository,
            IGenericRepository<CargoReceiver> receiverRepository,
            ILogger<CreateCargoCommandHandler> logger)
        {
            _cargoRepository = cargoRepository;
            _senderRepository = senderRepository;
            _receiverRepository = receiverRepository;
            _logger = logger;
        }

        public async Task<CreateCargoCommandResponse> Handle(CreateCargoCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Yeni kargo olusturma islemi baslatildi. Gonderen: {SenderName}", request.SenderFullName);

            var sender = new CargoSender
            {
                FullName = request.SenderFullName,
                FullAddress = request.SenderAddress,
                PhoneNumber = request.SenderPhoneNumber
            };

            var createdSender = await _senderRepository.AddAsync(sender, cancellationToken);
            if (createdSender == null)
            {
                _logger.LogError("Gonderen bilgileri kaydedilemedi.");
                throw new BusinessException("Gonderen bilgileri kaydedilemedi.");
            }

            _logger.LogDebug("Gonderen kaydedildi. Id: {SenderId}", createdSender.Id);

            var receiver = new CargoReceiver
            {
                Name = request.ReceiverName,
                Surname = request.ReceiverSurname,
                FullAddress = request.ReceiverAddress,
                PhoneNumber = request.ReceiverPhoneNumber,
                Email = request.ReceiverEmail
            };

            var createdReceiver = await _receiverRepository.AddAsync(receiver, cancellationToken);
            if (createdReceiver == null)
            {
                _logger.LogError("Alici bilgileri kaydedilemedi.");
                throw new BusinessException("Alici bilgileri kaydedilemedi.");
            }

            _logger.LogDebug("Alici kaydedildi. Id: {ReceiverId}", createdReceiver.Id);

            var trackingNumber = TrackingNumberGenerator.Generate();

            var existingCargo = await _cargoRepository.GetValueByExpressionAsync(c => c.TrackingNumber == trackingNumber, cancellationToken);
            if (existingCargo != null)
            {
                _logger.LogWarning("Takip numarasi cakismasi: {TrackingNumber}", trackingNumber);
                throw new BusinessException("Takip numarasi zaten mevcut. Lutfen tekrar deneyin.");
            }

            var cargo = new Domain.Entities.Cargo
            {
                TrackingNumber = trackingNumber,
                SenderId = createdSender.Id,
                ReceiverId = createdReceiver.Id,
                Status = Domain.Enums.CargoStatus.Received
            };

            var createdCargo = await _cargoRepository.AddAsync(cargo, cancellationToken);
            if (createdCargo == null)
            {
                _logger.LogError("Kargo kaydi olusturulamadi.");
                throw new BusinessException("Kargo kaydi olusturulamadi.");
            }

            _logger.LogInformation("Kargo basariyla olusturuldu. Takip No: {TrackingNumber}", trackingNumber);

            return new CreateCargoCommandResponse
            {
                Id = createdCargo.Id,
                TrackingNumber = createdCargo.TrackingNumber,
                SenderName = createdSender.FullName,
                SenderAddress = createdSender.FullAddress,
                ReceiverName = $"{createdReceiver.Name} {createdReceiver.Surname}",
                ReceiverAddress = createdReceiver.FullAddress,
                CargoStatus = createdCargo.Status
            };
        }
    }
}
