using KargoTakip.Domain.Enums;
using MediatR;

namespace KargoTakip.Application.Features.CargoMovement.Commands
{
    public class CreateCargoMovementCommand : IRequest<CreateCargoMovementCommandResponse>
    {
        public string CargoId { get; set; }
        public CargoStatus NewStatus { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public bool SendNotification { get; set; } = true;
    }
}