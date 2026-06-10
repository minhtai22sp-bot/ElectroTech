using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface ICategoryRepository
    {
        Task<List<Categories>> GetAllAsync();
        Task<Categories?> GetByIdAsync(int id);
        Task<Categories> AddAsync(Categories category);
        Task UpdateAsync(Categories category);
        Task DeleteAsync(Categories category);
        Task<bool> SlugExistsAsync(string slug, int? excludeId = null);
    }
}
