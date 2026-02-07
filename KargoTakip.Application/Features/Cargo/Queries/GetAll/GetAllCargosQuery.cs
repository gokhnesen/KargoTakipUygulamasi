using KargoTakip.Application.Helpers;
using KargoTakip.Domain.Enums;
using MediatR;
using System;

namespace KargoTakip.Application.Features.Cargo.Queries.GetAll
{
    public class GetAllCargosQuery : BasePagedRequest, IRequest<GetAllCargosQueryResponse>
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

        public bool IncludeTotalCount { get; set; } = true;

        public CargoStatus? GetCargoStatus()
            => Status.HasValue ? (CargoStatus)Status.Value : null;
    }
}