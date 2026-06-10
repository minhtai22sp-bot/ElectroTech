using ElectroTech.Infrastructure.DbContexts;
using Interfaces;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ElectroTech.Infrastructure.Repository
{
    public class RepositoryAsync<T> : IRepositoryAsync<T> where T : class
    {

        private ApplicationDbContext _ApplicationDbContext;
        private readonly DbSet<T> dbSet;
        // protected AuditableContext _dbContext { get; }
        protected AuditableLogContext _dbContext { get; }
        public RepositoryAsync(ApplicationDbContext ApplicationDbContext, AuditableLogContext dbContext)
        {
            _ApplicationDbContext = ApplicationDbContext;
            _dbContext = dbContext;
        }
        protected IDbFactory DbFactory
        {
            get;
            set;
        }
        public IQueryable<T> Entities => _ApplicationDbContext.Set<T>();


        protected ApplicationDbContext DbContext
        {
            get { return _ApplicationDbContext ?? (_ApplicationDbContext = DbFactory.Init()); }
        }
        protected RepositoryAsync(IDbFactory dbFactory)
        {

            DbFactory = dbFactory;
            dbSet = DbContext.Set<T>();
        }
        public async Task<T> AddAsync(T entity)
        {
            var add = await _ApplicationDbContext.Set<T>().AddAsync(entity);
            return add.Entity;
        }

        public void ClearChangeTracker()
        {
            _ApplicationDbContext.ChangeTracker.Clear();
        }
        public void DetachAllEntities()
        {
            var undetachedEntriesCopy = _ApplicationDbContext.ChangeTracker.Entries()
                .Where(e => e.State != EntityState.Detached)
                .ToList();

            foreach (var entry in undetachedEntriesCopy)
            {
                entry.State = EntityState.Detached;
            }

        }
        public T Add(T entity)
        {
            var add = _ApplicationDbContext.Set<T>().Add(entity);
            return add.Entity;
        }
        public async Task<Task> AddRangeAsync(IEnumerable<T> entity)
        {
            await _ApplicationDbContext.Set<T>().AddRangeAsync(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(T entity)
        {
            _ApplicationDbContext.Set<T>().Remove(entity);
            return Task.CompletedTask;
        }
        public void Delete(T entity)
        {
            _ApplicationDbContext.Set<T>().Remove(entity);
        }
        public Task DeleteRangeAsync(IEnumerable<T> entity)
        {
            _ApplicationDbContext.Set<T>().RemoveRange(entity);
            return Task.CompletedTask;
        }

        public async Task<List<T>> GetAllAsync()
        {
            return await _ApplicationDbContext.Set<T>().ToListAsync();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _ApplicationDbContext.Set<T>().FindAsync(id);
        }
        public async Task<T> GetByIdAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null)
        {
            IQueryable<T> query = _ApplicationDbContext.Set<T>();
            if (include != null)
            {
                query = include(query);
            }
            if (predicate != null)
            {
                query = query.Where(predicate);
            }
            return await query.SingleOrDefaultAsync();
        }
    
        public async Task<T> GetByIdAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IIncludableQueryable<List<T>, object>> include = null)
        {
            IQueryable<T> query = _ApplicationDbContext.Set<T>();
            if (include != null)
            {
                query = (IQueryable<T>)include(query);
            }
            if (predicate != null)
            {
                query = query.Where(predicate);
            }
            return await query.SingleOrDefaultAsync();
        }
        public T GetById(int id)
        {
            return _ApplicationDbContext.Set<T>().Find(id);
        }
        public async Task<T> GetFirstAsync()
        {
            return await _ApplicationDbContext.Set<T>().FirstOrDefaultAsync();
        }
        public async Task<T> GetFirstAsNoTrackingAsync()
        {
            return await _ApplicationDbContext.Set<T>().AsNoTracking().FirstOrDefaultAsync();
        }
        public T GetFirstAsNoTracking()
        {
            return _ApplicationDbContext.Set<T>().AsNoTracking().FirstOrDefault();
        }
        public async Task<T> GetByIdAsync(string id)
        {
            return await _ApplicationDbContext.Set<T>().FindAsync(id);
        }

        public async Task<List<T>> GetPagedReponseAsync(int pageNumber, int pageSize)
        {
            return await _ApplicationDbContext
                .Set<T>()
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        public Task UpdateAsync(T entity)
        {
            _dbContext.Attach(entity);
            _ApplicationDbContext.Entry(entity).CurrentValues.SetValues(entity);
            return Task.CompletedTask;
        }
        public void Update(T entity)
        {
            _dbContext.Attach(entity);
            _ApplicationDbContext.Entry(entity).CurrentValues.SetValues(entity);
        }
        public Task UpdateRangeAsync(IEnumerable<T> entity)
        {

            foreach (var item in entity)
            {
                _dbContext.Attach(item);
                _ApplicationDbContext.Entry(item).CurrentValues.SetValues(item);
            }
            // _ApplicationDbContext.Set<T>().UpdateRange(entity);
            return Task.CompletedTask;
        }
        public void UpdateRange(IEnumerable<T> entity)
        {

            foreach (var item in entity)
            {
                _dbContext.Attach(item);
                _ApplicationDbContext.Entry(item).CurrentValues.SetValues(item);
            }
            // _ApplicationDbContext.Set<T>().UpdateRange(entity);
            // return Task.CompletedTask;
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>> expression)
        {
            return _ApplicationDbContext.Set<T>().Where<T>(expression);
        }

        public async Task<T> SingleByExpressionAsync(Expression<Func<T, bool>> expression)
        {
            return await _ApplicationDbContext.Set<T>().SingleOrDefaultAsync(expression);
        }
        public T SingleByExpression(Expression<Func<T, bool>> expression)
        {
            return _ApplicationDbContext.Set<T>().SingleOrDefault(expression);
        }

        public IQueryable<T> GetMultiPaging(Expression<Func<T, bool>> predicate, out int total, int index = 0, int size = 20)
        {
            int skipCount = index * size;
            IQueryable<T> _resetSet;

            //HANDLE INCLUDES FOR ASSOCIATED OBJECTS IF APPLICABLE

            _resetSet = predicate != null ? _ApplicationDbContext.Set<T>().Where<T>(predicate).AsQueryable() : _ApplicationDbContext.Set<T>().AsQueryable();
            _resetSet = skipCount == 0 ? _resetSet.Take(size) : _resetSet.Skip(skipCount).Take(size);
            total = _resetSet.Count();
            return _resetSet.AsQueryable();
        }

        public IQueryable<T> GetMultiListInclude(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IIncludableQueryable<List<T>, object>> include = null)
        {
            IQueryable<T> query = _ApplicationDbContext.Set<T>();
            if (include != null)
            {
                query = (IQueryable<T>)include(query);
            }
            query = query.Where<T>(predicate);
            //return _ApplicationDbContext.Set<T>().Where<T>(predicate).AsQueryable<T>();
            return query;
        }
        public IQueryable<T> GetListInclude(Func<IQueryable<T>, IIncludableQueryable<List<T>, object>> include = null)
        {
            IQueryable<T> query = _ApplicationDbContext.Set<T>();
            if (include != null)
            {
                query = (IQueryable<T>)include(query);
            }
            return query;
        }
        public IQueryable<T> GetMulti(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null)
        {
            IQueryable<T> query = _ApplicationDbContext.Set<T>();
            if (include != null)
            {
                query = include(query);
            }
            query = query.Where<T>(predicate);
            //return _ApplicationDbContext.Set<T>().Where<T>(predicate).AsQueryable<T>();
            return query;
        }

        public IEnumerable<T> GetAllEnumerable()
        {
            return _ApplicationDbContext.Set<T>();
        }
        public IQueryable<T> GetAllQueryable()
        {
            return _ApplicationDbContext.Set<T>();
        }


        public IQueryable<T> GetListInclude(Func<IQueryable<T>, IIncludableQueryable<T, object>> include)
        {
            IQueryable<T> query = _ApplicationDbContext.Set<T>();
            if (include != null)
            {
                query = (IQueryable<T>)include(query);
            }
            return query;
        }
    }
}
