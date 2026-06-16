using Entities;
namespace Interfaces;
public interface IReviewRepository
{
    Task<double?> GetAverageApprovedRatingAsync();
    Task<List<Review>> GetByProductIdAsync(int productId);
    Task<List<Review>> GetApprovedByProductAsync(int productId);
    Task<List<Review>> GetAllAsync();
    Task<Review?> GetByIdAsync(int id);
    Task<bool> HasReviewedAsync(int productId, Guid userId);
    Task<bool> HasReviewedAsync(int productId, Guid userId, int orderId);
    Task<int> CountPendingAsync();
    Task AddAsync(Review review);
    Task UpdateAsync(Review review);
    Task DeleteAsync(int id);
}