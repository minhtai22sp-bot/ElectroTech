using Entities;
using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
namespace ElectroTech.Infrastructure.Repository
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly IRepositoryAsync<Review> _repo;
        private readonly IUnitOfWork _unitOfWork;

        public ReviewRepository(
            IRepositoryAsync<Review> repo,
            IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Review>> GetByProductIdAsync(int productId)
            => await _repo.Entities
                .Where(r => r.ProductId == productId && r.IsApproved)
                .OrderByDescending(r => r.CreatedOn)
                .ToListAsync();

        public async Task<bool> HasReviewedAsync(int productId, Guid userId)
            => await _repo.Entities
                .AnyAsync(r => r.ProductId == productId
                            && r.UserId == userId);

        public async Task AddAsync(Review review)
        {
            await _repo.AddAsync(review);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
