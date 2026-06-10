using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepositoryAsync<T> Repository<T>() where T : class;
        void CreateTransaction();
        Task CreateTransactionAsync();
        void Commit();
        Task CommitAsync();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken());
        void SaveChanges();
        void Rollback();
        Task RollbackAsync();
    }
}
