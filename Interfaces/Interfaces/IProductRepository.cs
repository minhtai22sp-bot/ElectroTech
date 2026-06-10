using AspNetCoreHero.Results;
using Entities;
using Entities.ViewModel;
using Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IProductRepository
    {
        IQueryable<Product> Entities { get; }
        Task<Product> GetById(int id);
        Task AddAsync(Product entity);
        Task UpdateAsync(Product entity);
        Task DeleteAsync(int id);
        Task<PaginatedList<ProductIndexModel>> GetAllPaginatedAsync(ProductSearch model);
        Task<List<ProductSpec>> GetSpecsAsync(int productId);
        Task<List<ProductImage>> GetImagesAsync(int productId);
        Task SaveSpecsAsync(int productId, List<ProductSpec> specs);
        Task SaveImagesAsync(int productId, List<ProductImage> images);
    }
}
