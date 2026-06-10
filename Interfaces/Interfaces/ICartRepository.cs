using Entities;

namespace Interfaces;

public interface ICartRepository
{
    Task<List<CartItem>> GetItemsAsync(string userId);
    Task AddItemAsync(string userId, CartItem item);
    Task UpdateQuantityAsync(string userId, int productId, int quantity);
    Task RemoveItemAsync(string userId, int productId);
    Task ClearAsync(string userId);
    Task<int> GetCountAsync(string userId);
    Task<decimal> GetTotalAsync(string userId);

    // Merge cookie cart vào DB khi login
    Task MergeFromCookieAsync(string userId, List<CartItem> cookieItems);
}