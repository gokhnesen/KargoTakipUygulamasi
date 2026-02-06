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
    public class GetAllCargosQueryHandler : IRequestHandler<GetAllCargosQuery, GetAllCargosQueryResult>
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

        public async Task<GetAllCargosQueryResult> Handle(GetAllCargosQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Tum kargolar listeleniyor. Filtreler: Status={Status}, TrackingNumber={TrackingNumber}, Email={Email}, Page={Page}",
                request.Status, request.TrackingNumber, request.ReceiverEmail, request.EffectivePageIndex);

            var (filterParams, useKeyset, pageIndex, pageSize, runCountQuery, fetchOneMore) = CargoFilterSpecification.FromGetAllCargosQuery(request);
            var spec = new CargoFilterSpecification(filterParams);

            var totalCount = -1;
            if (runCountQuery)
            {
                var countSpec = new CargoFilterSpecification(filterParams.ToCountParams());
                totalCount = await _cargoRepository.CountAsync(countSpec);
            }

            var cargos = await _cargoRepository.GetAllWithSpecAsync(spec);

            if (cargos == null || !cargos.Any())
            {
                _logger.LogWarning("Hic kargo bulunamadi.");
                return new GetAllCargosQueryResult
                {
                    Items = new List<GetAllCargosQueryResponse>(),
                    TotalCount = runCountQuery ? 0 : -1,
                    PageIndex = useKeyset ? 1 : pageIndex,
                    PageSize = pageSize,
                    HasMore = false,
                    NextCursorCreatedDate = null,
                    NextCursorId = null
                };
            }

            bool hasMore = false;
            if ((useKeyset || fetchOneMore) && cargos.Count > pageSize)
            {
                hasMore = true;
                cargos = cargos.Take(pageSize).ToList();
            }
            else if (!useKeyset && totalCount >= 0)
            {
                hasMore = (pageIndex * pageSize) < totalCount;
            }

            var response = cargos.Select(c => c.ToGetAllCargosQueryResponse()).ToList();

            DateTime? nextCursorCreatedDate = null;
            string? nextCursorId = null;
            if (useKeyset && response.Count > 0)
            {
                var last = response[^1];
                nextCursorCreatedDate = last.CreatedDate;
                nextCursorId = last.Id;
            }

            return new GetAllCargosQueryResult
            {
                Items = response,
                TotalCount = runCountQuery ? totalCount : -1,
                PageIndex = useKeyset ? 1 : pageIndex,
                PageSize = pageSize,
                HasMore = hasMore,
                NextCursorCreatedDate = nextCursorCreatedDate,
                NextCursorId = nextCursorId
            };
        }
    }
}
