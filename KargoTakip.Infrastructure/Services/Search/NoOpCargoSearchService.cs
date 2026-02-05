using KargoTakip.Application.Interfaces.Cargo;
using System.Threading;
using System.Threading.Tasks;

namespace KargoTakip.Infrastructure.Services.Search
{
    /// <summary>
    /// Elasticsearch yokken kullanilir; arama her zaman DB'de yapilir.
    /// </summary>
    public class NoOpCargoSearchService : ICargoSearchService
    {
        public Task<CargoSearchResult?> SearchIdsAsync(string searchTerm, CargoSearchFilter filter, int skip, int take, CancellationToken cancellationToken = default)
            => Task.FromResult<CargoSearchResult?>(null);

        public Task IndexAsync(CargoSearchDocument document, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task DeleteAsync(string cargoId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
