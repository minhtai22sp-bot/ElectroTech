using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IReviewRepository
    {
        Task<List<Review>> GetByProductIdAsync(int productId);
        Task<bool> HasReviewedAsync(int productId, Guid userId);
        Task AddAsync(Review review);
    }
}
