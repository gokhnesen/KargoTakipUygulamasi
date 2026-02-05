using KargoTakip.Application.Exceptions;
using KargoTakip.Application.Interfaces;
using KargoTakip.Application.Interfaces.Cargo;
using KargoTakip.Application.Services;
using KargoTakip.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace KargoTakip.Application.Features.Cargo.Commands.Create
{
    public class CreateCargoCommandHandler : IRequestHandler<CreateCargoCommand, CreateCargoCommandResponse>
    {
        private readonly IGenericRepository<Domain.Entities.Cargo> _cargoRepository;
        private readonly IGenericRepository<CargoSender> _senderRepository;
        private readonly IGenericRepository<CargoReceiver> _receiverRepository;
        private readonly ICargoSearchService _cargoSearchService;
        private readonly ILogger<CreateCargoCommandHandler> _logger;
        private readonly TrackingNumberGenerator _trackingNumberGenerator;

        public CreateCargoCommandHandler(
            IGenericRepository<Domain.Entities.Cargo> cargoRepository,
            IGenericRepository<CargoSender> senderRepository,
            IGenericRepository<CargoReceiver> receiverRepository,
            ICargoSearchService cargoSearchService,
            ILogger<CreateCargoCommandHandler> logger,
            TrackingNumberGenerator trackingNumberGenerator)
        {
            _cargoRepository = cargoRepository;
            _senderRepository = senderRepository;
            _receiverRepository = receiverRepository;
            _cargoSearchService = cargoSearchService;
            _logger = logger;
            _trackingNumberGenerator = trackingNumberGenerator;
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

            var createdSender = await _senderRepository.AddAsync(sender);
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

            var createdReceiver = await _receiverRepository.AddAsync(receiver);
            if (createdReceiver == null)
            {
                _logger.LogError("Alici bilgileri kaydedilemedi.");
                throw new BusinessException("Alici bilgileri kaydedilemedi.");
            }

            _logger.LogDebug("Alici kaydedildi. Id: {ReceiverId}", createdReceiver.Id);

            var trackingNumber = _trackingNumberGenerator.Generate();

            var existingCargo = await _cargoRepository.GetValueByExpressionAsync(c => c.TrackingNumber == trackingNumber);
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

            var createdCargo = await _cargoRepository.AddAsync(cargo);
            if (createdCargo == null)
            {
                _logger.LogError("Kargo kaydi olusturulamadi.");
                throw new BusinessException("Kargo kaydi olusturulamadi.");
            }

            _logger.LogInformation("Kargo basariyla olusturuldu. Takip No: {TrackingNumber}", trackingNumber);

            var searchDoc = new CargoSearchDocument
            {
                Id = createdCargo.Id,
                TrackingNumber = createdCargo.TrackingNumber,
                SenderFullName = createdSender.FullName ?? string.Empty,
                SenderAddress = createdSender.FullAddress ?? string.Empty,
                SenderPhone = createdSender.PhoneNumber ?? string.Empty,
                ReceiverFullName = $"{createdReceiver.Name} {createdReceiver.Surname}".Trim(),
                ReceiverEmail = createdReceiver.Email ?? string.Empty,
                ReceiverAddress = createdReceiver.FullAddress ?? string.Empty,
                ReceiverPhone = createdReceiver.PhoneNumber ?? string.Empty,
                Status = (int)createdCargo.Status,
                CreatedDate = createdCargo.CreatedDate
            };
            await _cargoSearchService.IndexAsync(searchDoc, cancellationToken);

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
