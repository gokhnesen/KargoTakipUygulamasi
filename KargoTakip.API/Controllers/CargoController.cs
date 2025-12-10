using KargoTakip.Application.Features.Cargo.Commands;
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
    }
}
