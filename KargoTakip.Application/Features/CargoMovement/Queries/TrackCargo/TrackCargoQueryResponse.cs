using KargoTakip.Domain.Enums;
using System;
using System.Collections.Generic;

namespace KargoTakip.Application.Features.CargoMovement.Queries.TrackCargo
{
    public class TrackCargoQueryResponse
    {
        public string TrackingNumber { get; set; }
        public CargoStatus CurrentStatus { get; set; }
        public string CurrentStatusText { get; set; }
        public string CurrentLocation { get; set; }
        public SenderInfo Sender { get; set; }
        public ReceiverInfo Receiver { get; set; }
        public List<MovementInfo> Movements { get; set; }
    }

    public class SenderInfo
    {
        public string FullName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class ReceiverInfo
    {
        public string FullName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
    }

    public class MovementInfo
    {
        public CargoStatus Status { get; set; }
        public string StatusText { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
    }
}