// 📁 Repositories/IRepository.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using Postgrest.Models;

namespace kutuphane.Repositories
{
    public interface IRepository<T> where T : BaseModel, new()
    {
        Task<List<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task<bool> AddAsync(T entity);
        Task<bool> UpdateAsync(T entity);
        Task<bool> DeleteAsync(T entity);
    }
}