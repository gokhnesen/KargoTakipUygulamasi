using KargoTakip.Domain.Enums;

namespace KargoTakip.Domain.Entities
{
    public class CargoMovement : BaseEntity
    {
        public int CargoId { get; set; }
        public CargoStatus NewStatus { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }

        public Cargo Cargo { get; set; }
    }
}