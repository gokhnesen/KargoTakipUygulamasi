using KargoTakip.Application.Interfaces;
using KargoTakip.Application.Interfaces.Cargo;
using KargoTakip.Domain.Entities;
using KargoTakip.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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

        public async Task<List<Domain.Entities.Cargo>> GetAllCargosAsync()
        {
            return await _context.Cargos
                .Include(c => c.Sender)
                .Include(c => c.Receiver)
                .Include(c => c.Movements)
                .Where(c => !c.IsDeleted)
                .OrderByDescending(c => c.CreatedDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Domain.Entities.Cargo> GetCargoByIdWithDetailsAsync(string id)
        {
            return await _context.Cargos
                .Include(c => c.Sender)
                .Include(c => c.Receiver)
                .Include(c => c.Movements.OrderByDescending(m => m.CreatedDate))
                .Where(c => c.Id == id && !c.IsDeleted)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }
    }
}