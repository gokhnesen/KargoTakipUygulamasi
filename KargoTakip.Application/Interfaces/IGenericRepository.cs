using KargoTakip.Application.Specifications;
using KargoTakip.Domain.Entities;
using System.Linq.Expressions;

namespace KargoTakip.Application.Interfaces
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
        Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<T?> GetValueByExpressionAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default);
        Task<List<T>> GetAllWithSpecAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);
        Task<List<string>> GetIdsWithSpecAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);
        Task<int> CountAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);
    }
}
