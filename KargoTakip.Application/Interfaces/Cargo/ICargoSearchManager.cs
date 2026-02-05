using KargoTakip.Domain.Enums;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KargoTakip.Application.Interfaces.Cargo
{
    /// <summary>
    /// SearchTerm ile kargo aramasını yönetir; arama her zaman bu manager üzerinden yapılır (ES veya DB fallback manager içinde karar verilir).
    /// </summary>
    public interface ICargoSearchManager
    {
        /// <summary>
        /// Aramaya uyan kargo ID'lerini ve toplam sayıyı döner. Null dönerse arama yapılamadı (DB ile devam edilir).
        /// </summary>
        Task<CargoSearchResult?> SearchAsync(
            string searchTerm,
            CargoSearchFilter filter,
            int skip,
            int take,
            CancellationToken cancellationToken = default);
    }
}
