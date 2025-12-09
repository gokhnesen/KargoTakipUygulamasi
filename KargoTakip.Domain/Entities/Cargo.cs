using KargoTakip.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace KargoTakip.Domain.Entities
{
    public class Cargo : BaseEntity
    {
        public string TrackingNumber { get; set; }
        public string SenderName { get; set; }
        public string SenderAddress { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverAddress { get; set; }

        public ICollection<CargoMovement> Movements { get; set; }

        public CargoStatus Status { get; set; }
    }
}
