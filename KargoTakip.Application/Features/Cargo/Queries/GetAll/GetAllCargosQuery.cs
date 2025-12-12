using MediatR;
using System.Collections.Generic;

namespace KargoTakip.Application.Features.Cargo.Queries.GetAll
{
    public class GetAllCargosQuery : IRequest<List<GetAllCargosQueryResponse>>
    {
    }
}