using System;
using System.Collections.Generic;
using System.Text;

namespace KargoTakip.Domain.Enums
{
    public enum CargoStatus
    {
        Received = 1,
        Processing = 2,
        InTransit = 3,
        OutForDelivery = 4,
        Delivered = 5,
        Undelivered = 6,
    }
}
