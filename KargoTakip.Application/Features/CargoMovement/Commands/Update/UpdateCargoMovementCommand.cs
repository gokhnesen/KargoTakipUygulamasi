using KargoTakip.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace KargoTakip.Application.Features.CargoMovement.Commands.UpdateCargoMovement
{
    public class UpdateCargoMovementCommand : IRequest<UpdateCargoMovementCommandResponse>
    {
        public string CargoId { get; set; }
        public CargoStatus NewStatus { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
    }
}