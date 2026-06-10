using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public partial interface IRepositoryAsync<T> where T : class
    {
        IQueryable<T> Entities { get; }
        T GetById(int id);
        T GetFirstAsNoTracking();
        Task<T> GetFirstAsNoTrackingAsync();
        Task<T> GetFirstAsync();
        Task<T> GetByIdAsync(int id);
        Task<T> GetByIdAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null);
        Task<T> GetByIdAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IIncludableQueryable<List<T>, object>> include = null);
        Task<T> GetByIdAsync(string id);
        Task<List<T>> GetAllAsync();
        Task<List<T>> GetPagedReponseAsync(int pageNumber, int pageSize);
        Task<T> AddAsync(T entity);
        T Add(T entity);
        Task<Task> AddRangeAsync(IEnumerable<T> entity);
        Task UpdateAsync(T entity);
        void Update(T entity);
        Task UpdateRangeAsync(IEnumerable<T> entity);
        void UpdateRange(IEnumerable<T> entity);
        void Delete(T entity);
        Task DeleteAsync(T entity);
        Task DeleteRangeAsync(IEnumerable<T> entity);
    }
}
