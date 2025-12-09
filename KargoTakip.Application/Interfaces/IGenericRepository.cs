using KargoTakip.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace KargoTakip.Application.Interfaces
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        Task<List<T>> GetAllAsync();
        Task<T> GetByIdAsync(Guid id);
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task<bool> DeleteAsync(Guid id); 

    }
}
