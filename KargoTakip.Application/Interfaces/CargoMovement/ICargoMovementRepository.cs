using KargoTakip.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KargoTakip.Application.Interfaces.CargoMovement
{
    public interface ICargoMovementRepository : IGenericRepository<Domain.Entities.CargoMovement>
    {
        Task<List<Domain.Entities.CargoMovement>> GetMovementsByCargoIdAsync(string cargoId);
        Task<Domain.Entities.CargoMovement> GetLatestMovementAsync(string cargoId);
    }
}