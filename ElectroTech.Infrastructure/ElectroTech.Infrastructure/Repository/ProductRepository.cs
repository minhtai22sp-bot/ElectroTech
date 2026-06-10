using Entities;
using Entities.ViewModel;
using Interfaces;
using Library;
using Microsoft.EntityFrameworkCore;

namespace ElectroTech.Infrastructure.Repository;

public class ProductRepository : IProductRepository
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepositoryAsync<Product> _productreporitory;
    private readonly IRepositoryAsync<ProductSpec> _specRepo;
    private readonly IRepositoryAsync<ProductImage> _imageRepo;

    public ProductRepository(
        IUnitOfWork unitOfWork,
        IRepositoryAsync<Product> productreporitory,
        IRepositoryAsync<ProductSpec> specRepo,
        IRepositoryAsync<ProductImage> imageRepo)
    {
        _unitOfWork = unitOfWork;
        _productreporitory = productreporitory;
        _specRepo = specRepo;
        _imageRepo = imageRepo;
    }

    public IQueryable<Product> Entities => _productreporitory.Entities;

    public async Task<Product> GetById(int id)
    {
        return await _productreporitory.Entities
            .Include(x => x.Categories)
            .Include(x => x.ProductImages)
            .Include(x => x.ProductSpecs)
            .Include(x => x.Reviews)
            .Where(x => x.Id == id)
            .SingleOrDefaultAsync();
    }

    public async Task AddAsync(Product entity)
    {
        await _productreporitory.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateAsync(Product entity)
    {
        await _productreporitory.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await _productreporitory.Entities
            .Where(x => x.Id == id)
            .ExecuteDeleteAsync();
    }

    public async Task<PaginatedList<ProductIndexModel>> GetAllPaginatedAsync(ProductSearch model)
    {
        var query = _productreporitory.Entities
            .Include(x => x.Categories)
            .AsQueryable();

        if (!string.IsNullOrEmpty(model.keyword))
            query = query.Where(x =>
                x.Name.Contains(model.keyword) ||
                x.Code.Contains(model.keyword) ||
                x.Brand.Contains(model.keyword));

        if (model.isFeatured.HasValue)
            query = query.Where(x => x.IsFeatured == model.isFeatured.Value);

        if (model.isActive.HasValue)
            query = query.Where(x => x.IsActive == model.isActive.Value);

        if (model.categoryId.HasValue)
            query = query.Where(x => x.CategoryId == model.categoryId.Value);

        if (model.minPrice.HasValue)
            query = query.Where(x => x.Price >= model.minPrice.Value);

        if (model.maxPrice.HasValue)
            query = query.Where(x => x.Price <= model.maxPrice.Value);

        var result = query.Select(x => new ProductIndexModel
        {
            Id = x.Id,
            Name = x.Name,
            Slug = x.Slug,
            Code = x.Code,
            Brand = x.Brand,
            Price = x.Price,
            OriginalPrice = x.OriginalPrice,
            Stock = x.Stock,
            ThumbnailUrl = x.ThumbnailUrl,
            Rating = x.Rating,
            ReviewCount = x.ReviewCount,
            IsFeatured = x.IsFeatured,
            IsActive = x.IsActive,
            CategoryId = x.CategoryId,
            CategoryName = x.Categories != null ? x.Categories.Name : ""
        });

        return await PaginatedList<ProductIndexModel>.ToPagedListAsync(
            result, model.currentPage, model.pageSize);
    }

    // ── Specs ──────────────────────────────────────────────────────────────────
    public async Task<List<ProductSpec>> GetSpecsAsync(int productId)
        => await _specRepo.Entities
            .Where(s => s.ProductId == productId)
            .OrderBy(s => s.DisplayOrder)   // ✅ đã sửa từ SortOrder → DisplayOrder
            .ToListAsync();

    public async Task SaveSpecsAsync(int productId, List<ProductSpec> specs)
    {
        // Xóa specs cũ
        var old = await _specRepo.Entities
            .Where(s => s.ProductId == productId)
            .ToListAsync();

        foreach (var s in old)
            await _specRepo.DeleteAsync(s);

        // Thêm specs mới
        foreach (var s in specs)
        {
            s.ProductId = productId;
            await _specRepo.AddAsync(s);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    // ── Images ─────────────────────────────────────────────────────────────────
    public async Task<List<ProductImage>> GetImagesAsync(int productId)
        => await _imageRepo.Entities
            .Where(i => i.ProductId == productId)
            .OrderBy(i => i.DisplayOrder)   // ✅ đã sửa từ SortOrder → DisplayOrder
            .ToListAsync();

    public async Task SaveImagesAsync(int productId, List<ProductImage> images)
    {
        // Xóa images cũ
        var old = await _imageRepo.Entities
            .Where(i => i.ProductId == productId)
            .ToListAsync();

        foreach (var i in old)
            await _imageRepo.DeleteAsync(i);

        // Thêm images mới
        foreach (var img in images)
        {
            img.ProductId = productId;
            await _imageRepo.AddAsync(img);
        }

        await _unitOfWork.SaveChangesAsync();
    }
}