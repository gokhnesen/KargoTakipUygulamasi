using KargoTakip.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace KargoTakip.Application.Features.Cargo.Commands.Create
{
    public class CreateCargoCommandResponse
    {
        public string Id { get; set; }
        public string TrackingNumber { get; set; }
        public string SenderName { get; set; }
        public string SenderAddress { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverAddress { get; set; }
        public CargoStatus CargoStatus { get; set; } = CargoStatus.Received;
    }
}
