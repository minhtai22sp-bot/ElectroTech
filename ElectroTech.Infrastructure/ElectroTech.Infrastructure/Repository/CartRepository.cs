using Entities;
using Interfaces;
using Library;
using Microsoft.EntityFrameworkCore;

namespace ElectroTech.Infrastructure.Repository;

public class CartRepository : ICartRepository
{
    private readonly IRepositoryAsync<CartItem> _repo;
    private readonly IUnitOfWork _unitOfWork;

    public CartRepository(
        IRepositoryAsync<CartItem> repo,
        IUnitOfWork unitOfWork)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<CartItem>> GetItemsAsync(string userId)
        => await _repo.Entities
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.CreatedOn)
            .ToListAsync();

    public async Task AddItemAsync(string userId, CartItem item)
    {
        var existing = await _repo.Entities
            .FirstOrDefaultAsync(c => c.UserId == userId
                                   && c.ProductId == item.ProductId);
        if (existing != null)
        {
            existing.Quantity += item.Quantity;
            await _repo.UpdateAsync(existing);
        }
        else
        {
            item.UserId = userId;
            await _repo.AddAsync(item);
        }
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateQuantityAsync(string userId, int productId, int quantity)
    {
        var item = await _repo.Entities
            .FirstOrDefaultAsync(c => c.UserId == userId
                                   && c.ProductId == productId);
        if (item is null) return;

        if (quantity <= 0)
            await _repo.DeleteAsync(item);
        else
        {
            item.Quantity = quantity;
            await _repo.UpdateAsync(item);
        }
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveItemAsync(string userId, int productId)
    {
        var item = await _repo.Entities
            .FirstOrDefaultAsync(c => c.UserId == userId
                                   && c.ProductId == productId);
        if (item is null) return;

        await _repo.DeleteAsync(item);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ClearAsync(string userId)
    {
        var items = await _repo.Entities
            .Where(c => c.UserId == userId)
            .ToListAsync();

        foreach (var item in items)
            await _repo.DeleteAsync(item);

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<int> GetCountAsync(string userId)
        => await _repo.Entities
            .Where(c => c.UserId == userId)
            .SumAsync(c => c.Quantity);

    public async Task<decimal> GetTotalAsync(string userId)
        => await _repo.Entities
            .Where(c => c.UserId == userId)
            .SumAsync(c => c.Price * c.Quantity);

    // Merge cookie cart vào DB sau khi login
    public async Task MergeFromCookieAsync(string userId, List<CartItem> cookieItems)
    {
        foreach (var cookieItem in cookieItems)
        {
            var existing = await _repo.Entities
                .FirstOrDefaultAsync(c => c.UserId == userId
                                       && c.ProductId == cookieItem.ProductId);
            if (existing != null)
            {
                // Cộng thêm quantity từ cookie
                existing.Quantity += cookieItem.Quantity;
                await _repo.UpdateAsync(existing);
            }
            else
            {
                cookieItem.UserId = userId;
                await _repo.AddAsync(cookieItem);
            }
        }
        await _unitOfWork.SaveChangesAsync();
    }
}