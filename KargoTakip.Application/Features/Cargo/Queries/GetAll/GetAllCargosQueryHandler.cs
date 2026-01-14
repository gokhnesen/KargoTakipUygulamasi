using KargoTakip.Application.Interfaces;
using KargoTakip.Application.Interfaces.Cargo;
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

        public GetAllCargosQueryHandler(
            ICargoRepository cargoRepository,
            ILogger<GetAllCargosQueryHandler> logger)
        {
            _cargoRepository = cargoRepository;
            _logger = logger;
        }

        public async Task<List<GetAllCargosQueryResponse>> Handle(GetAllCargosQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Tüm kargolar listeleniyor.");

            var cargos = await _cargoRepository.GetAllCargosAsync();

            _logger.LogInformation("Veritabanından {Count} adet kargo getirildi.", cargos?.Count ?? 0);

            if (cargos == null || !cargos.Any())
            {
                _logger.LogWarning("Hiç kargo bulunamadı.");
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
                UpdatedDate = c.UpdatedDate
            }).ToList();

            _logger.LogInformation("{Count} adet kargo başarıyla listelendi.", response.Count);

            return response;
        }

        private string GetStatusText(CargoStatus status)
        {
            return status switch
            {
                CargoStatus.Received => "Teslim Alındı",
                CargoStatus.InTransit => "Yolda",
                CargoStatus.OutForDelivery => "Dağıtımda",
                CargoStatus.Delivered => "Teslim Edildi",
                CargoStatus.Undelivered => "Teslim Edilemedi",
                _ => "Bilinmiyor"
            };
        }
    }
}