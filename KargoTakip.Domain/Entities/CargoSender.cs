using System;
using System.Collections.Generic;
using System.Text;

namespace KargoTakip.Domain.Entities
{
    public class CargoSender : BaseEntity
    {
        public string FullName { get; set; }
        public string FullAddress { get; set; }
        public string PhoneNumber { get; set; }
    }
}
