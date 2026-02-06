using KargoTakip.Domain.Entities;
using System.Threading.Tasks;

namespace KargoTakip.Application.Interfaces.Cargo
{
    public interface ICargoRepository : IGenericRepository<Domain.Entities.Cargo>
    {
        Task<Domain.Entities.Cargo?> GetCargoByIdWithDetailsAsync(string id, CancellationToken cancellationToken = default);
    }
}