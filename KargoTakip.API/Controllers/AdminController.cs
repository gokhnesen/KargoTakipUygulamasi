using KargoTakip.Application.Interfaces;
using KargoTakip.Application.Interfaces.Cargo;
using KargoTakip.Application.Specifications.CargoSpecifications;
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
        private const int ReindexBatchSize = 500;

        private readonly CargoDbContext _context;
        private readonly DataSeeder _seeder;
        private readonly IGenericRepository<Cargo> _cargoRepository;
        private readonly ICargoSearchService _cargoSearchService;

        public AdminController(
            CargoDbContext context,
            DataSeeder seeder,
            IGenericRepository<Cargo> cargoRepository,
            ICargoSearchService cargoSearchService)
        {
            _context = context;
            _seeder = seeder;
            _cargoRepository = cargoRepository;
            _cargoSearchService = cargoSearchService;
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

        /// <summary>
        /// Veritabanindaki tum kargolari Elasticsearch "cargos" index'ine yazar (reindex).
        /// Elasticsearch baglantisi yoksa veya hata olursa uygun mesaj doner.
        /// </summary>
        [HttpPost("reindex")]
        public async Task<IActionResult> Reindex([FromQuery] int batchSize = ReindexBatchSize)
        {
            if (batchSize < 1 || batchSize > 2000)
            {
                return BadRequest("batchSize must be between 1 and 2000");
            }

            var totalIndexed = 0;
            var page = 1;

            while (true)
            {
                var filterParams = new CargoFilterParams
                {
                    PageIndex = page,
                    PageSize = batchSize,
                    IncludeMovements = false,
                    UseKeysetPagination = false,
                    OrderBy = "date",
                    IsDescending = true
                };
                var spec = new CargoFilterSpecification(filterParams);
                var cargos = await _cargoRepository.GetAllWithSpecAsync(spec);

                if (cargos == null || cargos.Count == 0)
                    break;

                foreach (var c in cargos)
                {
                    var doc = new CargoSearchDocument
                    {
                        Id = c.Id,
                        TrackingNumber = c.TrackingNumber ?? string.Empty,
                        SenderFullName = c.Sender?.FullName ?? string.Empty,
                        SenderAddress = c.Sender?.FullAddress ?? string.Empty,
                        SenderPhone = c.Sender?.PhoneNumber ?? string.Empty,
                        ReceiverFullName = (c.Receiver != null ? $"{c.Receiver.Name} {c.Receiver.Surname}".Trim() : string.Empty),
                        ReceiverEmail = c.Receiver?.Email ?? string.Empty,
                        ReceiverAddress = c.Receiver?.FullAddress ?? string.Empty,
                        ReceiverPhone = c.Receiver?.PhoneNumber ?? string.Empty,
                        Status = (int)c.Status,
                        CreatedDate = c.CreatedDate
                    };
                    await _cargoSearchService.IndexAsync(doc);
                    totalIndexed++;
                }

                if (cargos.Count < batchSize)
                    break;
                page++;
            }

            return Ok(new
            {
                message = $"Reindex tamamlandi. Toplam {totalIndexed:N0} kargo Elasticsearch'e yazildi.",
                totalIndexed
            });
        }
    }
}
