using KargoTakip.Application.Dtos;
using KargoTakip.Application.Interfaces;
using KargoTakip.Application.Interfaces.Cargo;
using KargoTakip.Application.Services;
using KargoTakip.Domain.Entities;
using KargoTakip.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KargoTakip.Application.Features.Cargo.Queries.GetAll
{
    public class GetAllCargosQueryHandler : IRequestHandler<GetAllCargosQuery, List<GetAllCargosQueryResponse>>
    {
        private readonly ICargoRepository _cargoRepository;
        private readonly ILogger<GetAllCargosQueryHandler> _logger;
        private readonly CargoStatusHelper _statusHelper;

        public GetAllCargosQueryHandler(
            ICargoRepository cargoRepository,
            ILogger<GetAllCargosQueryHandler> logger,
            CargoStatusHelper statusHelper)
        {
            _cargoRepository = cargoRepository;
            _logger = logger;
            _statusHelper = statusHelper;
        }

        public async Task<List<GetAllCargosQueryResponse>> Handle(GetAllCargosQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Tum kargolar listeleniyor.");

            var cargos = await _cargoRepository.GetAllCargosAsync();

            _logger.LogInformation("Veritabanindan {Count} adet kargo getirildi.", cargos?.Count ?? 0);

            if (cargos == null || !cargos.Any())
            {
                _logger.LogWarning("Hic kargo bulunamadi.");
                return new List<GetAllCargosQueryResponse>();
            }

            var response = cargos.Select(c => new GetAllCargosQueryResponse
            {
                Id = c.Id,
                TrackingNumber = c.TrackingNumber,
                SenderName = c.Sender?.FullName ?? "Bilinmiyor",
                SenderAddress = c.Sender?.FullAddress ?? "Bilinmiyor",
                SenderPhoneNumber = c.Sender?.PhoneNumber ?? "Bilinmiyor",
                ReceiverName = c.Receiver?.Name ?? "Bilinmiyor",
                ReceiverSurname = c.Receiver?.Surname ?? string.Empty,
                ReceiverAddress = c.Receiver?.FullAddress ?? "Bilinmiyor",
                ReceiverPhoneNumber = c.Receiver?.PhoneNumber ?? "Bilinmiyor",
                ReceiverEmail = c.Receiver?.Email ?? "Bilinmiyor",
                Status = c.Status,
                CreatedDate = c.CreatedDate,
                UpdatedDate = c.UpdatedDate,
                Movements = c.Movements?.OrderByDescending(m => m.CreatedDate).Select(m => new CargoMovementDto
                {
                    Id = m.Id,
                    NewStatus = m.NewStatus,
                    Location = m.Location,
                    Description = m.Description,
                    CreatedDate = m.CreatedDate
                }).ToList() ?? new List<CargoMovementDto>()
            }).ToList();

            _logger.LogInformation("{Count} adet kargo basariyla listelendi.", response.Count);

            return response;
        }
    }
}