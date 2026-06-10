using ElectroTech.Domain.Entities;
using ElectroTech.Domain.Enums;
using ElectroTech.Domain.Exceptions;
using ElectroTech.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ElectroTech.Application.Commands.Orders;

public record CancelOrderCommand(int OrderId, string UserId) : IRequest<bool>;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, bool>
{
    private readonly IUnitOfWork _uow;

    public CancelOrderCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<bool> Handle(CancelOrderCommand cmd, CancellationToken ct)
    {
        var order = await _uow.Orders
            .GetQueryable()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == cmd.OrderId, ct)
            ?? throw new NotFoundException(nameof(Order), cmd.OrderId);

        if (order.UserId != cmd.UserId)
            throw new BusinessRuleException("Bạn không có quyền huỷ đơn hàng này.");

        if (order.Status != OrderStatus.Pending)
            throw new BusinessRuleException("Chỉ đơn hàng Pending mới có thể huỷ.");

        // Hoàn kho
        foreach (var item in order.Items)
        {
            var product = await _uow.Products.GetByIdAsync(item.ProductId, ct);
            if (product is not null)
            {
                product.Stock += item.Quantity;
                _uow.Products.Update(product);
            }
        }

        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;
        _uow.Orders.Update(order);

        await _uow.SaveChangesAsync(ct);
        return true;
    }
}