using KargoTakip.Application.Specifications;
using KargoTakip.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace KargoTakip.Persistence.Repositories
{
    public class SpecificationEvaluator<T> where T : BaseEntity
    {
        public static IQueryable<T> GetQuery(IQueryable<T> inputQuery, ISpecification<T> spec)
        {
            var query = inputQuery;

            if (spec.Criteria != null)
            {
                query = query.Where(spec.Criteria);
            }

            query = spec.Includes.Aggregate(query, (current, include) => current.Include(include));

            if (spec.IsSplitQuery)
            {
                query = query.AsSplitQuery();
            }

            if (spec.OrderBy != null)
            {
                var orderedQuery = query.OrderBy(spec.OrderBy);

                if (spec.ThenBy != null)
                    orderedQuery = orderedQuery.ThenBy(spec.ThenBy);
                else if (spec.ThenByDescending != null)
                    orderedQuery = orderedQuery.ThenByDescending(spec.ThenByDescending);

                query = orderedQuery;
            }
            else if (spec.OrderByDescending != null)
            {
                var orderedQuery = query.OrderByDescending(spec.OrderByDescending);

                if (spec.ThenBy != null)
                    orderedQuery = orderedQuery.ThenBy(spec.ThenBy);
                else if (spec.ThenByDescending != null)
                    orderedQuery = orderedQuery.ThenByDescending(spec.ThenByDescending);

                query = orderedQuery;
            }

            if (spec.IsPagingEnabled)
            {
                query = query.Skip(spec.Skip).Take(spec.Take);
            }

            return query;
        }
    }
}
