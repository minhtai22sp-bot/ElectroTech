using Entities;
using Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ElectroTech.Infrastructure.Repository
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly IRepositoryAsync<Categories> _repo;
        private readonly IUnitOfWork _unitOfWork;

        public CategoryRepository(
            IRepositoryAsync<Categories> repo,
            IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Categories>> GetAllAsync()
            => await _repo.Entities
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();

        public async Task<Categories?> GetByIdAsync(int id)
            => await _repo.Entities
                .FirstOrDefaultAsync(c => c.Id == id);

        public async Task<Categories> AddAsync(Categories category)
        {
            await _repo.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();
            return category;
        }

        public async Task UpdateAsync(Categories category)
        {
            await _repo.UpdateAsync(category);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(Categories category)
        {
            await _repo.DeleteAsync(category);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<bool> SlugExistsAsync(string slug, int? excludeId = null)
            => await _repo.Entities
                .AnyAsync(c => c.Slug == slug
                            && (excludeId == null || c.Id != excludeId));
    }
}