using Entities;
using Interfaces;
using Microsoft.EntityFrameworkCore;
namespace ElectroTech.Infrastructure.Repository
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly IRepositoryAsync<Review> _repo;
        private readonly IUnitOfWork _unitOfWork;
        public ReviewRepository(IRepositoryAsync<Review> repo, IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _unitOfWork = unitOfWork;
        }
        public async Task<List<Review>> GetByProductIdAsync(int productId)
            => await _repo.Entities
                .Where(r => r.ProductId == productId && r.IsApproved)
                .OrderByDescending(r => r.CreatedOn)
                .ToListAsync();
        public async Task<List<Review>> GetApprovedByProductAsync(int productId)
            => await _repo.Entities
                .Where(r => r.ProductId == productId && r.IsApproved)
                .ToListAsync();
        public async Task<bool> HasReviewedAsync(int productId, Guid userId)
            => await _repo.Entities
                .AnyAsync(r => r.ProductId == productId && r.UserId == userId);
        public async Task<bool> HasReviewedAsync(int productId, Guid userId, int orderId)
            => await _repo.Entities
                .AnyAsync(r => r.ProductId == productId
                            && r.UserId == userId
                            && r.OrderId == orderId);
        public async Task<int> CountPendingAsync()
            => await _repo.Entities
                .CountAsync(r => !r.IsApproved);
        public async Task AddAsync(Review review)
        {
            await _repo.AddAsync(review);
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task<List<Review>> GetAllAsync()
            => await _repo.Entities
                .OrderByDescending(r => r.CreatedOn)
                .ToListAsync();
        public async Task<Review?> GetByIdAsync(int id)
            => await _repo.Entities
                .Include(r => r.Product)
                .FirstOrDefaultAsync(r => r.Id == id);
        public async Task UpdateAsync(Review review)
        {
            await _repo.UpdateAsync(review);
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task DeleteAsync(int id)
        {
            var review = await _repo.Entities.FirstOrDefaultAsync(r => r.Id == id);
            if (review != null)
            {
                await _repo.DeleteAsync(review);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task<double?> GetAverageApprovedRatingAsync()
    => await _repo.Entities
           .Where(r => r.IsApproved)
           .AverageAsync(r => (double?)r.Rating);
    }
}