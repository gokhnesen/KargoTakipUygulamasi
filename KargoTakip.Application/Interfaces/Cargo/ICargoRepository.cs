using KargoTakip.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KargoTakip.Application.Interfaces.Cargo
{
    public interface ICargoRepository : IGenericRepository<Domain.Entities.Cargo>
    {
        Task<List<Domain.Entities.Cargo>> GetAllCargosAsync();
        Task<Domain.Entities.Cargo> GetCargoByIdWithDetailsAsync(string id);
    }
}