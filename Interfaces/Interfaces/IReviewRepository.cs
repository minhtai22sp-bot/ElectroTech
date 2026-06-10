using Entities;

namespace Interfaces;

public interface IReviewRepository
{
    Task<List<Review>> GetByProductIdAsync(int productId);
    Task<List<Review>> GetAllAsync();                     
    Task<Review?> GetByIdAsync(int id);                
    Task<bool> HasReviewedAsync(int productId, Guid userId);
    Task AddAsync(Review review);
    Task UpdateAsync(Review review);          
    Task DeleteAsync(int id);                  
}