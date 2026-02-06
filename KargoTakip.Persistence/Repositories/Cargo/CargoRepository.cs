using KargoTakip.Application.Interfaces;
using KargoTakip.Application.Interfaces.Cargo;
using KargoTakip.Domain.Entities;
using KargoTakip.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace KargoTakip.Persistence.Repositories.Cargo
{
    public class CargoRepository : GenericRepository<Domain.Entities.Cargo>, ICargoRepository
    {
        private readonly CargoDbContext _context;

        public CargoRepository(CargoDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Domain.Entities.Cargo?> GetCargoByIdWithDetailsAsync(string id, CancellationToken cancellationToken = default)
        {
            var cargo = await _context.Cargos
                .Include(c => c.Sender)
                .Include(c => c.Receiver)
                .Include(c => c.Movements)
                .Where(c => c.Id == id && !c.IsDeleted)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (cargo?.Movements != null)
            {
                cargo.Movements = cargo.Movements.OrderByDescending(m => m.CreatedDate).ToList();
            }

            return cargo;
        }
    }
}