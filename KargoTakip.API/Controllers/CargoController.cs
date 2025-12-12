using KargoTakip.Application.Features.Cargo.Commands;
using KargoTakip.Application.Features.Cargo.Queries;
using KargoTakip.Application.Features.Cargo.Queries.GetAll;
using KargoTakip.Application.Features.Cargo.Queries.GetById;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KargoTakip.API.Controllers
{
    [Route("api/[controller]")]
    public class CargoController : BaseController
    {

        [HttpPost]
        public async Task<IActionResult> Create([FromBody]CreateCargoCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var query = new GetCargoByIdQuery { Id = id };
            var result = await Mediator.Send(query);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var query = new GetAllCargosQuery();
            var result = await Mediator.Send(query);
            return Ok(result);
        }
    }
}
