using KargoTakip.Application.Interfaces;
using KargoTakip.Domain.Entities;
using KargoTakip.Persistence.DatabaseContext;
using KargoTakip.Persistence.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace KargoTakip.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AdminController : ControllerBase
    {
        private readonly CargoDbContext _context;
        private readonly DataSeeder _seeder;

        public AdminController(
            CargoDbContext context,
            DataSeeder seeder)
        {
            _context = context;
            _seeder = seeder;
        }

        [HttpPost("seed-data")]
        public async Task<IActionResult> SeedData([FromQuery] int totalCargos = 100_000, [FromQuery] int movementsPerCargo = 5)
        {
            if (!_context.Database.ProviderName.Contains("SqlServer"))
            {
                return BadRequest("This endpoint only works with SQL Server databases");
            }

            if (totalCargos < 1 || totalCargos > 5_000_000)
            {
                return BadRequest("totalCargos must be between 1 and 5,000,000");
            }

            await _seeder.SeedAsync(totalCargos, movementsPerCargo);
            return Ok(new
            {
                message = $"? Successfully seeded {totalCargos:N0} cargos with {movementsPerCargo} movements each",
                totalCargos,
                movementsPerCargo
            });
        }

        [HttpDelete("clear-data")]
        public async Task<IActionResult> ClearData()
        {
            if (!_context.Database.ProviderName.Contains("SqlServer"))
            {
                return BadRequest("This endpoint only works with SQL Server databases");
            }

            await _seeder.ClearAllCargoDataAsync();
            return Ok(new { message = "? All cargo data cleared successfully" });
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var cargoCount = await _context.Cargos.CountAsync();
            var senderCount = await _context.CargoSenders.CountAsync();
            var receiverCount = await _context.CargoReceivers.CountAsync();
            var movementCount = await _context.CargoMovements.CountAsync();

            return Ok(new
            {
                cargos = cargoCount,
                senders = senderCount,
                receivers = receiverCount,
                movements = movementCount,
                totalMovementsPerCargo = cargoCount > 0 ? movementCount / cargoCount : 0
            });
        }

    }
}
