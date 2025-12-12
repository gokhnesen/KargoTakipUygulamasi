using KargoTakip.Application.Interfaces;
using KargoTakip.Application.Interfaces.Cargo;
using KargoTakip.Domain.Entities;
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

            if (cargos == null || !cargos.Any())
            {
                _logger.LogWarning("Hiç kargo bulunamad?.");
                return new List<GetAllCargosQueryResponse>();
            }

            var response = cargos.Select(c => new GetAllCargosQueryResponse
            {
                Id = c.Id,
                TrackingNumber = c.TrackingNumber,
                SenderName = c.Sender?.FullName ?? "Bilinmiyor",
                SenderAddress = c.Sender?.FullAddress ?? "Bilinmiyor",
                ReceiverName = c.Receiver != null ? $"{c.Receiver.Name} {c.Receiver.Surname}" : "Bilinmiyor",
                ReceiverAddress = c.Receiver?.FullAddress ?? "Bilinmiyor",
                Status = c.Status,
                CreatedDate = c.CreatedDate
            }).ToList();

            _logger.LogInformation("{Count} adet kargo listelendi.", response.Count);

            return response;
        }
    }
}