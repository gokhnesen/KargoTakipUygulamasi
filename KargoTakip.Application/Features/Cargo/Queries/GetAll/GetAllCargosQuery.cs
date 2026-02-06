using KargoTakip.Application.Helpers;
using KargoTakip.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;

namespace KargoTakip.Application.Features.Cargo.Queries.GetAll
{
    public class GetAllCargosQuery : BasePagedRequest, IRequest<GetAllCargosQueryResult>
    {
        public int? Status { get; set; }
        public string? TrackingNumber { get; set; }
        public string? SearchTerm { get; set; }
        public string? SenderName { get; set; }
        public string? ReceiverName { get; set; }
        public string? ReceiverEmail { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string? OrderBy { get; set; } = "date";
        public bool IsDescending { get; set; } = true;

        public bool UseKeysetPagination { get; set; } = true;
        public DateTime? CursorCreatedDate { get; set; }
        public string? CursorId { get; set; }

        public bool IncludeTotalCount { get; set; } = true;

        public CargoStatus? GetCargoStatus()
            => Status.HasValue ? (CargoStatus)Status.Value : null;
    }

    public class GetAllCargosQueryResult
    {
        public List<GetAllCargosQueryResponse> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public bool HasMore { get; set; }
        public DateTime? NextCursorCreatedDate { get; set; }
        public string? NextCursorId { get; set; }
        public int TotalPages => TotalCount < 0 || PageSize <= 0 ? 1 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}