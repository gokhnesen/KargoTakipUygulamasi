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
    [Route("api/[controller]")]
    public class CargoController : BaseController
    {

        /// <summary>
        /// Yeni kargo olusturur
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(CreateCargoCommandResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> Create([FromBody] CreateCargoCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Id'ye gore kargo getirir
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(GetCargoByIdQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(string id)
        {
            var query = new GetCargoByIdQuery { Id = id };
            var result = await Mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Tum kargolari listeler. Tum parametreler opsiyoneldir.
        /// Parametresiz cagrildiginda tum kargolar tarihe gore azalan sirada listelenir.
        /// Filtreler: status, trackingNumber, senderName, receiverName, receiverEmail, startDate, endDate
        /// Sayfalama: pageIndex, pageSize
        /// Siralama: orderBy (date/trackingnumber/status), isDescending
        /// </summary>
        /// <param name="query">Filtreleme ve sayfalama parametreleri</param>
        /// <returns>Filtrelenmis kargo listesi ve sayfalama bilgileri</returns>
        [HttpGet]
        [ProducesResponseType(typeof(GetAllCargosQueryResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] GetAllCargosQuery query)
        {
            var result = await Mediator.Send(query);
            return Ok(result);
        }


        /// <summary>
        /// Kargo hareketi olusturur ve durum gunceller
        /// </summary>
        [HttpPost("movements")]
        [ProducesResponseType(typeof(CreateCargoMovementCommandResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateMovement([FromBody] CreateCargoMovementCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Kargonun tum hareketlerini getirir
        /// </summary>
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
        /// Takip numarasi ile kargo takibi yapar
        /// </summary>
        [HttpGet("track/{trackingNumber}")]
        [ProducesResponseType(typeof(TrackCargoQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> TrackCargo(string trackingNumber)
        {
            var query = new TrackCargoQuery { TrackingNumber = trackingNumber };
            var result = await Mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Kargo hareketini gunceller
        /// </summary>
        [HttpPut("movements/update")]
        [ProducesResponseType(typeof(UpdateCargoMovementCommandResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateMovement([FromBody] UpdateCargoMovementCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(result);
        }
    }
}
