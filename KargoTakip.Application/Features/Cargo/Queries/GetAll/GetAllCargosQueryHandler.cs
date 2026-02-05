using KargoTakip.Application.Helpers;
using KargoTakip.Application.Interfaces;
using KargoTakip.Application.Interfaces.Cargo;
using KargoTakip.Application.Mappings;
using KargoTakip.Application.Services;
using KargoTakip.Application.Specifications.CargoSpecifications;
using KargoTakip.Domain.Entities;
using KargoTakip.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace KargoTakip.Application.Features.Cargo.Queries.GetAll
{
    public class GetAllCargosQueryHandler : IRequestHandler<GetAllCargosQuery, GetAllCargosQueryResult>
    {

        private readonly IGenericRepository<Domain.Entities.Cargo> _cargoRepository;
        private readonly ICargoSearchManager _cargoSearchManager;
        private readonly ILogger<GetAllCargosQueryHandler> _logger;
        private readonly CargoStatusHelper _statusHelper;

        public GetAllCargosQueryHandler(
            IGenericRepository<Domain.Entities.Cargo> cargoRepository,
            ICargoSearchManager cargoSearchManager,
            ILogger<GetAllCargosQueryHandler> logger,
            CargoStatusHelper statusHelper)
        {
            _cargoRepository = cargoRepository;
            _cargoSearchManager = cargoSearchManager;
            _logger = logger;
            _statusHelper = statusHelper;
        }

        public async Task<GetAllCargosQueryResult> Handle(GetAllCargosQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Tum kargolar listeleniyor. Filtreler: Status={Status}, TrackingNumber={TrackingNumber}, Email={Email}, Page={Page}",
                request.Status, request.TrackingNumber, request.ReceiverEmail, request.EffectivePageIndex);

            var pageIndex = request.EffectivePageIndex;
            var pageSize = request.EffectivePageSize;

            var normalizedOrderBy = string.IsNullOrWhiteSpace(request.OrderBy) ? "date" : request.OrderBy.Trim();
            var useKeyset = request.UseKeysetPagination &&
                            string.Equals(normalizedOrderBy, "date", System.StringComparison.OrdinalIgnoreCase);

            var filterParams = new CargoFilterParams
            {
                Status = request.GetCargoStatus(),
                TrackingNumber = request.TrackingNumber,
                SearchTerm = request.SearchTerm,
                SenderName = request.SenderName,
                ReceiverName = request.ReceiverName,
                ReceiverEmail = request.ReceiverEmail,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                OrderBy = normalizedOrderBy,
                IsDescending = request.IsDescending,
                PageIndex = useKeyset ? PaginationHelper.DefaultPageIndex : pageIndex,
                PageSize = useKeyset ? (pageSize + 1) : pageSize,
                IncludeMovements = request.IncludeMovements,
                UseKeysetPagination = useKeyset,
                CursorCreatedDate = request.CursorCreatedDate,
                CursorId = request.CursorId
            };

            var spec = new CargoFilterSpecification(filterParams);

            var totalCount = -1;
            List<Domain.Entities.Cargo>? cargos = null;
            var usedElasticsearch = false;

            if (!string.IsNullOrWhiteSpace(request.SearchTerm) && !useKeyset)
            {
                var skip = request.Skip;
                var esFilter = new CargoSearchFilter
                {
                    Status = request.GetCargoStatus(),
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    SenderName = request.SenderName,
                    ReceiverName = request.ReceiverName,
                    ReceiverEmail = request.ReceiverEmail
                };
                var esResult = await _cargoSearchManager.SearchAsync(request.SearchTerm!.Trim(), esFilter, skip, pageSize, cancellationToken);
                // ES yanit verip 0 sonuc dondugunde (index bos vb.) veritabanina dusuyoruz; ayni SearchTerm ile DB'de aranir
                if (esResult != null && esResult.Ids.Count > 0)
                {
                    usedElasticsearch = true;
                    totalCount = request.IncludeTotalCount ? esResult.TotalCount : -1;
                    var esIds = esResult.Ids.ToList();
                    var byIdsSpec = new CargoByIdsSpecification(esResult.Ids, includeMovements: request.IncludeMovements);
                    cargos = await _cargoRepository.GetAllWithSpecAsync(byIdsSpec);
                    if (cargos != null)
                        cargos = cargos.OrderBy(c => esIds.IndexOf(c.Id)).ToList();
                }
            }

            if (!usedElasticsearch)
            {
                var runCountQuery = request.IncludeTotalCount && !useKeyset;
                if (runCountQuery)
                {
                    var countSpec = new CargoFilterSpecification(new CargoFilterParams
                    {
                        Status = request.GetCargoStatus(),
                        TrackingNumber = request.TrackingNumber,
                        SearchTerm = request.SearchTerm,
                        SenderName = request.SenderName,
                        ReceiverName = request.ReceiverName,
                        ReceiverEmail = request.ReceiverEmail,
                        StartDate = request.StartDate,
                        EndDate = request.EndDate,
                        PageSize = 0,
                        IncludeMovements = false,
                        UseKeysetPagination = false,
                        IsCount = true
                    });
                    totalCount = await _cargoRepository.CountAsync(countSpec);
                }

                var trackingTerm = !string.IsNullOrWhiteSpace(request.TrackingNumber) ? request.TrackingNumber.Trim() : null;
                var useTwoPhaseTracking = !useKeyset
                    && !string.IsNullOrWhiteSpace(trackingTerm)
                    && string.IsNullOrWhiteSpace(request.SenderName)
                    && string.IsNullOrWhiteSpace(request.ReceiverName)
                    && string.IsNullOrWhiteSpace(request.ReceiverEmail);

                if (useTwoPhaseTracking)
                {
                    var filterParamsIdsOnly = new CargoFilterParams
                    {
                        Status = request.GetCargoStatus(),
                        TrackingNumber = trackingTerm,
                        SearchTerm = null,
                        SenderName = null,
                        ReceiverName = null,
                        ReceiverEmail = null,
                        StartDate = request.StartDate,
                        EndDate = request.EndDate,
                        OrderBy = normalizedOrderBy,
                        IsDescending = request.IsDescending,
                        PageIndex = pageIndex,
                        PageSize = pageSize,
                        IncludeMovements = false,
                        UseKeysetPagination = false,
                        SkipIncludes = true
                    };
                    var specIdsOnly = new CargoFilterSpecification(filterParamsIdsOnly);
                    var ids = await _cargoRepository.GetIdsWithSpecAsync(specIdsOnly);
                    if (ids == null || ids.Count == 0)
                        cargos = new List<Domain.Entities.Cargo>();
                    else
                    {
                        var byIdsSpec = new CargoByIdsSpecification(ids, includeMovements: request.IncludeMovements);
                        cargos = await _cargoRepository.GetAllWithSpecAsync(byIdsSpec);
                        if (cargos != null && cargos.Count > 0)
                            cargos = cargos.OrderBy(c => ids.IndexOf(c.Id)).ToList();
                    }
                }
                else
                {
                    cargos = await _cargoRepository.GetAllWithSpecAsync(spec);
                }
            }

            _logger.LogInformation("Veritabanindan {Count} adet kargo getirildi (Toplam: {TotalCount}).", cargos?.Count ?? 0, totalCount);


            if (cargos == null || !cargos.Any())
            {
                _logger.LogWarning("Hic kargo bulunamadi.");
                return new GetAllCargosQueryResult
                {
                    Items = new List<GetAllCargosQueryResponse>(),
                    TotalCount = request.IncludeTotalCount && !useKeyset ? 0 : -1,
                    PageIndex = useKeyset ? 1 : pageIndex,
                    PageSize = pageSize,
                    HasMore = false,
                    NextCursorCreatedDate = null,
                    NextCursorId = null
                };
            }

            bool hasMore = false;
            if (useKeyset && cargos.Count > pageSize)
            {
                hasMore = true;
                cargos = cargos.Take(pageSize).ToList();
            }
            else if (!useKeyset && totalCount >= 0)
            {
                hasMore = (pageIndex * pageSize) < totalCount;
            }

            var response = cargos.Select(c => c.ToGetAllCargosQueryResponse(request.IncludeMovements)).ToList();

            _logger.LogInformation("{Count} adet kargo basariyla listelendi.", response.Count);

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
                TotalCount = request.IncludeTotalCount && !useKeyset ? totalCount : -1,
                PageIndex = useKeyset ? 1 : pageIndex,
                PageSize = pageSize,
                HasMore = hasMore,
                NextCursorCreatedDate = nextCursorCreatedDate,
                NextCursorId = nextCursorId
            };
        }
    }
}