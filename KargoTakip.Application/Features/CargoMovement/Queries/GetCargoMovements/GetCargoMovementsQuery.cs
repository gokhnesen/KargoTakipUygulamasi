using MediatR;

namespace KargoTakip.Application.Features.CargoMovement.Queries.GetCargoMovements
{
    public class GetCargoMovementsQuery : IRequest<GetCargoMovementsQueryResponse>
    {
        public string CargoId { get; set; }
    }
}