using KargoTakip.Application.Features.Cargo.Commands.Create;
using KargoTakip.Application.Features.Cargo.Queries;
using KargoTakip.Application.Features.Cargo.Queries.GetAll;
using KargoTakip.Application.Features.Cargo.Queries.GetById;
using KargoTakip.Application.Features.CargoMovement.Commands.Create;
using KargoTakip.Application.Features.CargoMovement.Commands.UpdateCargoMovement;
using KargoTakip.Application.Features.CargoMovement.Queries.GetCargoMovements;
using KargoTakip.Application.Features.CargoMovement.Queries.TrackCargo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KargoTakip.API.Controllers
{
    public class CargoController : BaseController
    {


        [HttpPost]
        [ProducesResponseType(typeof(CreateCargoCommandResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> Create([FromBody] CreateCargoCommand command)
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
        public async Task<IActionResult> GetAll([FromQuery] GetAllCargosQuery query)
        {
            var result = await Mediator.Send(query);
            return Ok(result);
        }


        [HttpPost("movements")]
        public async Task<IActionResult> CreateMovement([FromBody] CreateCargoMovementCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("{cargoId}/movements")]
        public async Task<IActionResult> GetCargoMovements(string cargoId)
        {
            var query = new GetCargoMovementsQuery { CargoId = cargoId };
            var result = await Mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("track/{trackingNumber}")]
        public async Task<IActionResult> TrackCargo(string trackingNumber)
        {
            var query = new TrackCargoQuery { TrackingNumber = trackingNumber };
            var result = await Mediator.Send(query);
            return Ok(result);
        }

        [HttpPut("movements/update")]
        public async Task<IActionResult> UpdateMovement([FromBody] UpdateCargoMovementCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(result);
        }
    }
}
