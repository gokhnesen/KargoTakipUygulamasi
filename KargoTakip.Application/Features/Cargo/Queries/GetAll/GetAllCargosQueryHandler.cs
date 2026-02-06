using KargoTakip.Application.Helpers;
using KargoTakip.Application.Interfaces;
using KargoTakip.Application.Mappings;
using KargoTakip.Application.Services;
using KargoTakip.Application.Specifications.CargoSpecifications;
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
    public class GetAllCargosQueryHandler : IRequestHandler<GetAllCargosQuery, GetAllCargosQueryResult>
    {

        private readonly IGenericRepository<Domain.Entities.Cargo> _cargoRepository;
        private readonly ILogger<GetAllCargosQueryHandler> _logger;
        private readonly CargoStatusHelper _statusHelper;

        public GetAllCargosQueryHandler(
            IGenericRepository<Domain.Entities.Cargo> cargoRepository,
            ILogger<GetAllCargosQueryHandler> logger,
            CargoStatusHelper statusHelper)
        {
            _cargoRepository = cargoRepository;
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

            var hasSearchTerm = !string.IsNullOrWhiteSpace(request.SearchTerm);
            var fetchOneMore = !useKeyset && (!request.IncludeTotalCount || hasSearchTerm);
            var requestedPageSize = (useKeyset || fetchOneMore) ? (pageSize + 1) : pageSize;
            var searchTermTrimmed = request.SearchTerm?.Trim() ?? string.Empty;
            // Sayi (rakam) iceriyorsa sadece TrackingNumber kriteri kullanilir: JOIN yok, IX_Cargos_TrackingNumber ile cok daha hizli.
            var searchTermLooksLikeTracking = hasSearchTerm && searchTermTrimmed.Length <= 40 && searchTermTrimmed.Any(char.IsDigit)
                && searchTermTrimmed.All(ch => char.IsLetterOrDigit(ch) || ch == '-' || ch == '.' || ch == ' ');

            // URL ?TrackingNumber=rtm32 veya frontend searchTerm=rtm32 gonderdiginde ayni hizli yolu kullan: TrackingNumber kriteri.
            var trackingNumberFromRequest = !string.IsNullOrWhiteSpace(request.TrackingNumber) ? request.TrackingNumber!.Trim() : null;
            var useSearchTermAsTracking = searchTermLooksLikeTracking && trackingNumberFromRequest == null;
            var effectiveTrackingNumber = trackingNumberFromRequest ?? (useSearchTermAsTracking ? searchTermTrimmed : null);
            // TrackingNumber doluysa veya searchTerm tracking olarak kullanildiysa SearchTerm kriteri ekleme (tek filtre, hizli).
            var effectiveSearchTerm = (useSearchTermAsTracking || (effectiveTrackingNumber != null && hasSearchTerm && searchTermLooksLikeTracking)) ? null : request.SearchTerm;

            // Liste cevabinda sadece LastLocation doner; Movements yuklenmez (performans).
            var filterParams = new CargoFilterParams
            {
                Status = request.GetCargoStatus(),
                TrackingNumber = effectiveTrackingNumber,
                SearchTerm = effectiveSearchTerm,
                SearchTermTrackingOnly = !useSearchTermAsTracking && searchTermLooksLikeTracking,
                SenderName = request.SenderName,
                ReceiverName = request.ReceiverName,
                ReceiverEmail = request.ReceiverEmail,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                OrderBy = normalizedOrderBy,
                IsDescending = request.IsDescending,
                PageIndex = useKeyset ? PaginationHelper.DefaultPageIndex : pageIndex,
                PageSize = requestedPageSize,
                IncludeMovements = false,
                UseKeysetPagination = useKeyset,
                CursorCreatedDate = request.CursorCreatedDate,
                CursorId = request.CursorId
            };

            var spec = new CargoFilterSpecification(filterParams);

            var totalCount = -1;
            List<Domain.Entities.Cargo>? cargos;

            // IncludeTotalCount true ise toplam/filtre sonucu sayisi hesaplanir (keyset kullanilsa da).
            var runCountQuery = request.IncludeTotalCount;
            if (runCountQuery)
            {
                var countSpec = new CargoFilterSpecification(new CargoFilterParams
                {
                    Status = request.GetCargoStatus(),
                    TrackingNumber = effectiveTrackingNumber,
                    SearchTerm = effectiveSearchTerm,
                    SearchTermTrackingOnly = filterParams.SearchTermTrackingOnly,
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

            cargos = await _cargoRepository.GetAllWithSpecAsync(spec);

            _logger.LogInformation("Veritabanindan {Count} adet kargo getirildi (Toplam: {TotalCount}).", cargos?.Count ?? 0, totalCount);


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