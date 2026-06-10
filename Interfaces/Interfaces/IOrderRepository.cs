using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IOrderRepository
    {
        Task<Order> GetByIdAsync(int id);
        Task<List<Order>> GetByUserIdAsync(string userId);
        Task AddAsync(Order order);
        Task UpdateAsync(Order order);
        Task<List<Order>> GetAllAsync();
        Task<bool> HasDeliveredProductAsync(Guid userId, int productId);
        Task<int?> GetDeliveredOrderItemIdAsync(Guid userId, int productId);
    }
}
