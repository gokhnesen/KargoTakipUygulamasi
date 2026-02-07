using KargoTakip.Domain.Enums;
using System;
using System.Collections.Generic;

namespace KargoTakip.Application.Features.Cargo.Queries.GetAll
{

    public class GetAllCargosQueryResponse
    {
        public List<GetAllCargosQueryItem> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public bool HasMore { get; set; }
        public int TotalPages => TotalCount < 0 || PageSize <= 0 ? 1 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    }


    public class GetAllCargosQueryItem
    {
        public string Id { get; set; }
        public string TrackingNumber { get; set; }
        public string SenderName { get; set; }
        public string SenderAddress { get; set; }
        public string SenderPhoneNumber { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverSurname { get; set; }
        public string ReceiverAddress { get; set; }
        public string ReceiverPhoneNumber { get; set; }
        public string ReceiverEmail { get; set; }
        public CargoStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? LastLocation { get; set; }
    }
}
