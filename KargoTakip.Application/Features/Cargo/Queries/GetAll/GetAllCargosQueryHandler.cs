using KargoTakip.Application.Dtos;
using KargoTakip.Application.Interfaces;
using KargoTakip.Application.Interfaces.Cargo;
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
using System.Text.RegularExpressions;

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
                request.Status, request.TrackingNumber, request.ReceiverEmail, request.PageIndex);

            var pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
            var pageSize = request.PageSize <= 0 ? 50 : request.PageSize;
            pageSize = pageSize > 500 ? 500 : pageSize;

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
                PageIndex = useKeyset ? 1 : pageIndex,
                PageSize = useKeyset ? (pageSize + 1) : pageSize,
                IncludeMovements = request.IncludeMovements,
                UseKeysetPagination = useKeyset,
                CursorCreatedDate = request.CursorCreatedDate,
                CursorId = request.CursorId
            };

            var spec = new CargoFilterSpecification(filterParams);

            var totalCount = -1;
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

            var trackingTerm = !string.IsNullOrWhiteSpace(request.TrackingNumber)
                ? request.TrackingNumber.Trim()
                : (IsLikelyTrackingNumber(request.SearchTerm) ? request.SearchTerm!.Trim() : null);
            var useTwoPhaseTracking = !useKeyset
                && !string.IsNullOrWhiteSpace(trackingTerm)
                && string.IsNullOrWhiteSpace(request.SenderName)
                && string.IsNullOrWhiteSpace(request.ReceiverName)
                && string.IsNullOrWhiteSpace(request.ReceiverEmail);

            List<Domain.Entities.Cargo>? cargos;
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
                Movements = request.IncludeMovements
                    ? (c.Movements?.OrderByDescending(m => m.CreatedDate).Select(m => new CargoMovementDto
                    {
                        Id = m.Id,
                        NewStatus = m.NewStatus,
                        Location = m.Location,
                        Description = m.Description,
                        CreatedDate = m.CreatedDate
                    }).ToList() ?? new List<CargoMovementDto>())
                    : new List<CargoMovementDto>()
            }).ToList();

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

        private static bool IsLikelyTrackingNumber(string? term)
        {
            if (string.IsNullOrWhiteSpace(term)) return false;
            var t = term.Trim();
            if (t.Length < 2 || t.Length > 64) return false;
            return Regex.IsMatch(t, @"^[A-Za-z0-9\-]+$");
        }
    }
}