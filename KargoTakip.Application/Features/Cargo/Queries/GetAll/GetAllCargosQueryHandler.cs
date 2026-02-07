using KargoTakip.Application.Interfaces;
using KargoTakip.Application.Mappings;
using KargoTakip.Application.Specifications.CargoSpecifications;
using KargoTakip.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KargoTakip.Application.Features.Cargo.Queries.GetAll
{
    public class GetAllCargosQueryHandler : IRequestHandler<GetAllCargosQuery, GetAllCargosQueryResponse>
    {
        private readonly IGenericRepository<Domain.Entities.Cargo> _cargoRepository;
        private readonly ILogger<GetAllCargosQueryHandler> _logger;

        public GetAllCargosQueryHandler(
            IGenericRepository<Domain.Entities.Cargo> cargoRepository,
            ILogger<GetAllCargosQueryHandler> logger)
        {
            _cargoRepository = cargoRepository;
            _logger = logger;
        }

        public async Task<GetAllCargosQueryResponse> Handle(GetAllCargosQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Tum kargolar listeleniyor. Filtreler: Status={Status}, TrackingNumber={TrackingNumber}, Email={Email}, Page={Page}",
                request.Status, request.TrackingNumber, request.ReceiverEmail, request.EffectivePageIndex);

            var (filterParams, pageIndex, pageSize, runCountQuery, fetchOneMore) = CargoFilterSpecification.FromGetAllCargosQuery(request);
            var spec = new CargoFilterSpecification(filterParams);

            var totalCount = -1;
            if (runCountQuery)
            {
                var countSpec = new CargoFilterSpecification(filterParams.ToCountParams());
                totalCount = await _cargoRepository.CountAsync(countSpec, cancellationToken);
            }

            var cargos = await _cargoRepository.GetAllWithSpecAsync(spec, cancellationToken);

            if (cargos == null || !cargos.Any())
            {
                _logger.LogWarning("Hic kargo bulunamadi.");
                return new GetAllCargosQueryResponse
                {
                    Items = new List<GetAllCargosQueryItem>(),
                    TotalCount = runCountQuery ? 0 : -1,
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    HasMore = false
                };
            }

            bool hasMore = false;
            if (fetchOneMore && cargos.Count > pageSize)
            {
                hasMore = true;
                cargos = cargos.Take(pageSize).ToList();
            }
            else if (totalCount >= 0)
            {
                hasMore = (pageIndex * pageSize) < totalCount;
            }

            var response = cargos.Select(c => CargoMapping.ToGetAllCargosQueryItem(c)).ToList();

            return new GetAllCargosQueryResponse
            {
                Items = response,
                TotalCount = runCountQuery ? totalCount : -1,
                PageIndex = pageIndex,
                PageSize = pageSize,
                HasMore = hasMore
            };
        }
    }
}
