using KargoTakip.Application.Interfaces;
using KargoTakip.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace KargoTakip.Application.Features.Cargo.Commands
{
    public class CreateCargoCommandHandler : IRequestHandler<CreateCargoCommand, CreateCargoCommandResponse>
    {
        private readonly IGenericRepository<Domain.Entities.Cargo> _cargoRepository;
        public CreateCargoCommandHandler(IGenericRepository<Domain.Entities.Cargo> cargoRepository)
        {
            _cargoRepository = cargoRepository;
        }
        public async Task<CreateCargoCommandResponse> Handle(CreateCargoCommand request, CancellationToken cancellationToken)
        {
            var cargo = new Domain.Entities.Cargo
            {
                TrackingNumber = GenerateTrackingNumber(),
                SenderName = request.SenderName,
                SenderAddress = request.SenderAddress,
                ReceiverName = request.ReceiverName,
                ReceiverAddress = request.ReceiverAddress,
                Status = Domain.Enums.CargoStatus.Received
            };
            await _cargoRepository.AddAsync(cargo);

            return new CreateCargoCommandResponse
            {
                TrackingNumber = cargo.TrackingNumber,
                SenderName = cargo.SenderName,
                SenderAddress = cargo.SenderAddress,
                ReceiverName = cargo.ReceiverName,
                ReceiverAddress = cargo.ReceiverAddress,
                CargoStatus = cargo.Status
            };
        }


        private string GenerateTrackingNumber()
        {
            const string karakterler = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();

            var sonuc = new string(Enumerable.Repeat(karakterler, 10)
                .Select(s => s[random.Next(s.Length)])
                .ToArray());

            return sonuc;
        }
    }
}
