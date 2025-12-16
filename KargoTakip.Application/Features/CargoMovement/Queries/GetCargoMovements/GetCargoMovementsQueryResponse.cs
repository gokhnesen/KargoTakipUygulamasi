using KargoTakip.Domain.Entities;
using KargoTakip.Domain.Enums;
using System;
using System.Collections.Generic;

namespace KargoTakip.Application.Features.CargoMovement.Queries.GetCargoMovements
{
    public class GetCargoMovementsQueryResponse
    {
        public string CargoId { get; set; }
        public string TrackingNumber { get; set; }
        public List<Domain.Entities.CargoMovement> Movements { get; set; }
    }


}