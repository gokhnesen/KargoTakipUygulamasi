using KargoTakip.Application.Exceptions;
using KargoTakip.Application.Interfaces;
using KargoTakip.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

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
            _logger.LogInformation("Yeni kargo oluşturma işlemi başlatıldı. Gönderen: {SenderName}", request.SenderFullName);

            var sender = new CargoSender
            {
                FullName = request.SenderFullName,
                FullAddress = request.SenderAddress,
                PhoneNumber = request.SenderPhoneNumber
            };

            var createdSender = await _senderRepository.AddAsync(sender);
            if (createdSender == null)
            {
                _logger.LogError("Gönderen bilgileri kaydedilemedi.");
                throw new BusinessException("Gönderen bilgileri kaydedilemedi.");
            }

            _logger.LogDebug("Gönderen kaydedildi. Id: {SenderId}", createdSender.Id);

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
                _logger.LogError("Alıcı bilgileri kaydedilemedi.");
                throw new BusinessException("Alıcı bilgileri kaydedilemedi.");
            }

            _logger.LogDebug("Alıcı kaydedildi. Id: {ReceiverId}", createdReceiver.Id);

            var trackingNumber = GenerateTrackingNumber();

            var existingCargo = await _cargoRepository.GetValueByExpressionAsync(c => c.TrackingNumber == trackingNumber);
            if (existingCargo != null)
            {
                _logger.LogWarning("Takip numarası çakışması: {TrackingNumber}", trackingNumber);
                throw new BusinessException("Takip numarası zaten mevcut. Lütfen tekrar deneyin.");
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
                _logger.LogError("Kargo kaydı oluşturulamadı.");
                throw new BusinessException("Kargo kaydı oluşturulamadı.");
            }

            _logger.LogInformation("Kargo başarıyla oluşturuldu. Takip No: {TrackingNumber}", trackingNumber);

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

        private string GenerateTrackingNumber()
        {
            const string karakterler = "ABCDEFGH IJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();

            var sonuc = new string(Enumerable.Repeat(karakterler, 10)
                .Select(s => s[random.Next(s.Length)])
                .ToArray());

            return sonuc;
        }
    }
}
