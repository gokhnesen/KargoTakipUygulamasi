using KargoTakip.Application.Interfaces.CargoMovement;
using KargoTakip.Domain.Entities;
using KargoTakip.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KargoTakip.Persistence.Repositories.CargoMovementRepository
{
    public class CargoMovementRepository : GenericRepository<CargoMovement>, ICargoMovementRepository
    {
        private readonly CargoDbContext _context;

        public CargoMovementRepository(CargoDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<CargoMovement>> GetMovementsByCargoIdAsync(string cargoId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<CargoMovement>()
                .Where(m => m.CargoId == cargoId && !m.IsDeleted)
                .OrderByDescending(m => m.CreatedDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<CargoMovement?> GetLatestMovementAsync(string cargoId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<CargoMovement>()
                .Where(m => m.CargoId == cargoId && !m.IsDeleted)
                .OrderByDescending(m => m.CreatedDate)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}