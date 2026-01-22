using KargoTakip.Domain.Enums;

namespace KargoTakip.Application.Dtos
{
    public class CargoMovementDto
    {
        public string Id { get; set; }
        public CargoStatus NewStatus { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}