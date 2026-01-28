using KargoTakip.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;

namespace KargoTakip.Application.Features.Cargo.Queries.GetAll
{
    public class GetAllCargosQuery : IRequest<GetAllCargosQueryResult>
    {
        // Filter parameters
        public int? Status { get; set; }  // Nullable int for API binding
        public string? TrackingNumber { get; set; }
        public string? SenderName { get; set; }
        public string? ReceiverName { get; set; }
        public string? ReceiverEmail { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        
        // Sorting parameters
        public string? OrderBy { get; set; } = "date";
        public bool IsDescending { get; set; } = true;
        
        // Paging parameters
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 0; // 0 means no paging

        // Helper property to convert int to CargoStatus
        public CargoStatus? GetCargoStatus() 
            => Status.HasValue ? (CargoStatus)Status.Value : null;
    }

    public class GetAllCargosQueryResult
    {
        public List<GetAllCargosQueryResponse> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 1;
    }
}