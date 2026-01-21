using KargoTakip.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace KargoTakip.Application.Features.CargoMovement.Commands.UpdateCargoMovement
{
    public class UpdateCargoMovementCommandResponse
    {
        public string Id { get; set; }
        public string CargoId { get; set; }
        public string TrackingNumber { get; set; }
        public CargoStatus NewStatus { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}