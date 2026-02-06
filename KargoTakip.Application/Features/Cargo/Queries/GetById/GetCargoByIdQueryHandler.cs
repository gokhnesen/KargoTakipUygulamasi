using KargoTakip.Application.Dtos;
using KargoTakip.Application.Exceptions;
using KargoTakip.Application.Interfaces;
using KargoTakip.Application.Interfaces.Cargo;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KargoTakip.Application.Features.Cargo.Queries.GetById
{
    public class GetCargoByIdQueryHandler : IRequestHandler<GetCargoByIdQuery, GetCargoByIdQueryResponse>
    {
        private readonly ICargoRepository _cargoRepository;
        private readonly ILogger<GetCargoByIdQueryHandler> _logger;

        public GetCargoByIdQueryHandler(
            ICargoRepository cargoRepository,
            ILogger<GetCargoByIdQueryHandler> logger)
        {
            _cargoRepository = cargoRepository;
            _logger = logger;
        }

        public async Task<GetCargoByIdQueryResponse> Handle(GetCargoByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Kargo getiriliyor. Id: {CargoId}", request.Id);

            var cargo = await _cargoRepository.GetCargoByIdWithDetailsAsync(request.Id, cancellationToken);

            if (cargo == null)
            {
                _logger.LogWarning("Kargo bulunamadi. Id: {CargoId}", request.Id);
                throw new NotFoundException("Cargo", request.Id);
            }

            _logger.LogInformation("Kargo bulundu. Takip No: {TrackingNumber}", cargo.TrackingNumber);

            return new GetCargoByIdQueryResponse
            {
                Id = cargo.Id,
                TrackingNumber = cargo.TrackingNumber,
                SenderName = cargo.Sender?.FullName ?? "Bilinmiyor",
                SenderAddress = cargo.Sender?.FullAddress ?? "Bilinmiyor",
                SenderPhoneNumber = cargo.Sender?.PhoneNumber ?? "Bilinmiyor",
                ReceiverName = cargo.Receiver?.Name ?? "Bilinmiyor",
                ReceiverSurname = cargo.Receiver?.Surname ?? string.Empty,
                ReceiverAddress = cargo.Receiver?.FullAddress ?? "Bilinmiyor",
                ReceiverPhoneNumber = cargo.Receiver?.PhoneNumber ?? "Bilinmiyor",
                ReceiverEmail = cargo.Receiver?.Email ?? "Bilinmiyor",
                Status = cargo.Status,
                CreatedDate = cargo.CreatedDate,
                UpdatedDate = cargo.UpdatedDate,
                Movements = cargo.Movements?.OrderByDescending(m => m.CreatedDate).Select(m => new CargoMovementDto
                {
                    Id = m.Id,
                    NewStatus = m.NewStatus,
                    Location = m.Location,
                    Description = m.Description,
                    CreatedDate = m.CreatedDate
                }).ToList() ?? new List<CargoMovementDto>()
            };
        }
    }
}