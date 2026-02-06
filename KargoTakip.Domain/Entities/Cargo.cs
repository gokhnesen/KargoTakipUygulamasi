using KargoTakip.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace KargoTakip.Domain.Entities
{
    public class Cargo : BaseEntity
    {
        public string TrackingNumber { get; set; }
        public CargoSender Sender { get; set; }
        public string SenderId { get; set; }
        public CargoReceiver Receiver { get; set; }
        public string ReceiverId { get; set; }
        public ICollection<CargoMovement> Movements { get; set; }

        public CargoStatus Status { get; set; }
        /// <summary>Son hareketin konumu; liste ekraninda Movements yuklenmeden gosterilir.</summary>
        public string? LastLocation { get; set; }
    }
}
