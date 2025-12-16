using MediatR;

namespace KargoTakip.Application.Features.CargoMovement.Queries.TrackCargo
{
    public class TrackCargoQuery : IRequest<TrackCargoQueryResponse>
    {
        public string TrackingNumber { get; set; }
    }
}