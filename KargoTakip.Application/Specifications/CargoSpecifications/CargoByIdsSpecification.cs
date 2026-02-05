using KargoTakip.Domain.Entities;
using System.Collections.Generic;
using System.Linq;

namespace KargoTakip.Application.Specifications.CargoSpecifications
{
    /// <summary>
    /// Verilen ID listesiyle tek sorguda IN clause kullanarak yükleme; GetIdsWithSpecAsync sonrası tek round-trip ile N+1 önlenir.
    /// </summary>
    public class CargoByIdsSpecification : BaseSpecification<Cargo>
    {
        public CargoByIdsSpecification(IReadOnlyList<string> ids, bool includeMovements = false)
            : base(c => ids.Contains(c.Id))
        {
            if (ids == null || ids.Count == 0)
                return;

            AddInclude(c => c.Sender);
            AddInclude(c => c.Receiver);
            if (includeMovements)
            {
                AddInclude(c => c.Movements);
                ApplySplitQuery();
            }

            ApplyOrderByDescending(c => c.CreatedDate);
            ApplyThenByDescending(c => c.Id);
            ApplyPaging(0, ids.Count);
        }
    }
}
