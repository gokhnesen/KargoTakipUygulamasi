using KargoTakip.Application.Interfaces.Cargo;
using System.Threading;
using System.Threading.Tasks;

namespace KargoTakip.Application.Services
{
    /// <summary>
    /// SearchTerm aramasını ICargoSearchService (ES veya NoOp) üzerinden yönetir; handler doğrudan search service'e gitmez.
    /// </summary>
    public class CargoSearchManager : ICargoSearchManager
    {
        private readonly ICargoSearchService _cargoSearchService;

        public CargoSearchManager(ICargoSearchService cargoSearchService)
        {
            _cargoSearchService = cargoSearchService;
        }

        public Task<CargoSearchResult?> SearchAsync(
            string searchTerm,
            CargoSearchFilter filter,
            int skip,
            int take,
            CancellationToken cancellationToken = default)
        {
            return _cargoSearchService.SearchIdsAsync(searchTerm, filter, skip, take, cancellationToken);
        }
    }
}
