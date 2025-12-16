using KargoTakip.Domain.Enums;
using System;

namespace KargoTakip.Application.Features.CargoMovement.Commands
{
    public class CreateCargoMovementCommandResponse
    {
        public string MovementId { get; set; }
        public string CargoId { get; set; }
        public string TrackingNumber { get; set; }
        public CargoStatus NewStatus { get; set; }
        public string Location { get; set; }
        public DateTime MovementDate { get; set; }
        public bool NotificationSent { get; set; }
    }
}