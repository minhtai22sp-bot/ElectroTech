using Entities;
using Enums;
using Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ElectroTech.Infrastructure.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IRepositoryAsync<Order> _repo;
        private readonly IUnitOfWork _uow;

        public OrderRepository(IRepositoryAsync<Order> repo, IUnitOfWork uow)
        {
            _repo = repo;
            _uow = uow;
        }

        public async Task<Order> GetByIdAsync(int id)
            => await _repo.Entities
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

        public async Task<List<Order>> GetAllAsync()
            => await _repo.Entities
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.CreatedOn)
                .ToListAsync();

        public async Task<List<Order>> GetByUserIdAsync(string userId)
        {
            if (!Guid.TryParse(userId, out var guid))
                return new List<Order>();

            return await _repo.Entities
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == guid)
                .OrderByDescending(o => o.CreatedOn)
                .ToListAsync();
        }

        public async Task<List<Order>> GetByDateAsync(DateTime date)
            => await _repo.Entities
                .Include(o => o.OrderItems)
                .Where(o => o.CreatedOn.Date == date.Date)
                .OrderByDescending(o => o.CreatedOn)
                .ToListAsync();

        public async Task<int> CountByStatusAsync(OrderStatus status)
            => await _repo.Entities
                .CountAsync(o => o.Status == status);

        public async Task<List<TopProductDto>> GetTopSellingProductsAsync(int limit)
            => await _repo.Entities
                .Where(o => o.Status == OrderStatus.Delivered)
                .SelectMany(o => o.OrderItems)
                .GroupBy(i => new { i.ProductId, i.ProductName, i.ProductImage })
                .Select(g => new TopProductDto
                {
                    Id = g.Key.ProductId ?? 0,
                    Name = g.Key.ProductName,
                    ThumbnailUrl = g.Key.ProductImage,
                    SoldCount = g.Sum(i => i.Quantity),
                    Revenue = g.Sum(i => i.TotalPrice)
                })
                .OrderByDescending(x => x.SoldCount)
                .Take(limit)
                .ToListAsync();

        public async Task AddAsync(Order order)
        {
            await _repo.AddAsync(order);
            await _uow.SaveChangesAsync();
        }

        public async Task UpdateAsync(Order order)
        {
            await _repo.UpdateAsync(order);
            await _uow.SaveChangesAsync();
        }

        public async Task<bool> HasDeliveredProductAsync(Guid userId, int productId)
            => await _repo.Entities
                .Where(o => o.UserId == userId && o.Status == OrderStatus.Delivered)
                .AnyAsync(o => o.OrderItems.Any(i => i.ProductId == productId));

        public async Task<int?> GetDeliveredOrderItemIdAsync(Guid userId, int productId)
        {
            var order = await _repo.Entities
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == userId && o.Status == OrderStatus.Delivered)
                .FirstOrDefaultAsync(o => o.OrderItems.Any(i => i.ProductId == productId));

            return order?.OrderItems
                .FirstOrDefault(i => i.ProductId == productId)?.Id;
        }
        public async Task<int> CountUniqueCustomersAsync()
    => await _repo.Entities
           .Select(o => o.UserId)
           .Distinct()
           .CountAsync();
        public async Task<int> CountTotalOrdersAsync()
        {
            var orders = await GetAllAsync();
            return orders.Count;
        }
    }
}