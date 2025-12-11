using System;
using System.Collections.Generic;
using System.Text;

namespace KargoTakip.Domain.Entities
{
    public class CargoReceiver : BaseEntity
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string FullAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
    }
}
