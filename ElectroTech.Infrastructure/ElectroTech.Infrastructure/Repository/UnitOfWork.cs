using ElectroTech.Infrastructure.DbContexts;
using Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectroTech.Infrastructure.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private Hashtable _repositories;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly ApplicationDbContext _dbContext;
        private bool disposed;
        private IDbContextTransaction _objTran;
        public UnitOfWork(ApplicationDbContext dbContext, IAuthenticatedUserService authenticatedUserService)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _authenticatedUserService = authenticatedUserService;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public Task RollbackAsync()
        {
            _objTran.Rollback();
            _objTran.Dispose();
            return Task.CompletedTask;
        }
        public void Rollback()
        {
            _objTran.Rollback();
            _objTran.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    //dispose managed resources
                    _dbContext.Dispose();
                }
            }
            //dispose unmanaged resources
            disposed = true;
        }

        public void CreateTransaction()
        {
            _objTran = _dbContext.Database.BeginTransaction();
        }
        public async Task CreateTransactionAsync()
        {
            _objTran = await _dbContext.Database.BeginTransactionAsync();
        }

        public void Commit()
        {
            _objTran.Commit();
        }
        public async Task CommitAsync()
        {
            await _objTran.CommitAsync();
        }

        public void SaveChanges()
        {
            _dbContext.SaveChanges();
        }

        public IRepositoryAsync<T> Repository<T>() where T : class
        {
            if (_repositories == null)
                _repositories = new Hashtable();

            var type = typeof(T).Name;

            if (!_repositories.ContainsKey(type))
            {
                var repositoryType = typeof(RepositoryAsync<>);

                var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(T)), _dbContext);

                _repositories.Add(type, repositoryInstance);
            }

            return (IRepositoryAsync<T>)_repositories[type];
        }


    }
}
