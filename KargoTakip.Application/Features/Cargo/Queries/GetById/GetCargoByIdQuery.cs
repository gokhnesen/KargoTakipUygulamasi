using MediatR;
using System;

namespace KargoTakip.Application.Features.Cargo.Queries.GetById
{
    public class GetCargoByIdQuery : IRequest<GetCargoByIdQueryResponse>
    {
        public string Id { get; set; }
    }
}