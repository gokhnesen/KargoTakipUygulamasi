using KargoTakip.Application.Features.Cargo.Commands;
using KargoTakip.Application.Features.Cargo.Queries;
using KargoTakip.Application.Features.Cargo.Queries.GetAll;
using KargoTakip.Application.Features.Cargo.Queries.GetById;
using KargoTakip.Application.Features.CargoMovement.Commands;
using KargoTakip.Application.Features.CargoMovement.Queries.GetCargoMovements;
using KargoTakip.Application.Features.CargoMovement.Queries.TrackCargo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KargoTakip.API.Controllers
{
    [Route("api/[controller]")]
    public class CargoController : BaseController
    {

        [HttpPost]
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
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var query = new GetAllCargosQuery();
            var result = await Mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Kargoya yeni hareket ekler ve alıcıya bildirim gönderir
        /// </summary>
        /// <param name="command">Kargo hareket bilgileri</param>
        /// <returns>Oluşturulan hareket bilgisi</returns>
        [HttpPost("movements")]
        [Authorize]
        [ProducesResponseType(typeof(CreateCargoMovementCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateMovement([FromBody] CreateCargoMovementCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Belirli bir kargonun tüm hareketlerini getirir
        /// </summary>
        /// <param name="cargoId">Kargo ID</param>
        /// <returns>Kargo hareketleri listesi</returns>
        [HttpGet("{cargoId}/movements")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCargoMovements(string cargoId)
        {
            var query = new GetCargoMovementsQuery { CargoId = cargoId };
            var result = await Mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Takip numarası ile kargo hareketlerini getirir
        /// </summary>
        /// <param name="trackingNumber">Takip numarası</param>
        /// <returns>Kargo hareketleri listesi</returns>
        [HttpGet("track/{trackingNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> TrackCargo(string trackingNumber)
        {
            var query = new TrackCargoQuery { TrackingNumber = trackingNumber };
            var result = await Mediator.Send(query);
            return Ok(result);
        }
    }
}
