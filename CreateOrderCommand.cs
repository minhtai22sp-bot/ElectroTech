using ElectroTech.Domain.Entities;
using ElectroTech.Domain.Enums;
using ElectroTech.Domain.Exceptions;
using ElectroTech.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ElectroTech.Application.Commands.Orders;

public record CreateOrderCommand(
    string UserId,
    string FullName,
    string Phone,
    string Address,
    string? CouponCode,
    string PaymentMethod,
    List<OrderItemDto> Items
) : IRequest<int>;

public record OrderItemDto(int ProductId, int Quantity);

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, int>
{
    private readonly IUnitOfWork _uow;
    private readonly IEmailService _email;

    public CreateOrderCommandHandler(IUnitOfWork uow, IEmailService email)
    {
        _uow = uow;
        _email = email;
    }

    public async Task<int> Handle(CreateOrderCommand cmd, CancellationToken ct)
    {
        // 1. Validate & snapshot sản phẩm
        var productIds = cmd.Items.Select(i => i.ProductId).ToList();
        var products = await _uow.Products
            .GetQueryable()
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(ct);

        var orderItems = new List<OrderItem>();
        foreach (var item in cmd.Items)
        {
            var product = products.FirstOrDefault(p => p.Id == item.ProductId)
                ?? throw new NotFoundException(nameof(Product), item.ProductId);

            if (product.Stock < item.Quantity)
                throw new InsufficientStockException(product.Name, product.Stock, item.Quantity);

            product.Stock -= item.Quantity;
            _uow.Products.Update(product);

            orderItems.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                ProductPrice = product.Price,
                Quantity = item.Quantity
            });
        }

        // 2. Áp coupon
        decimal discount = 0;
        if (!string.IsNullOrEmpty(cmd.CouponCode))
        {
            var coupon = await _uow.Repository<Coupon>()
                .GetQueryable()
                .FirstOrDefaultAsync(c =>
                    c.Code == cmd.CouponCode &&
                    c.IsActive &&
                    c.StartDate <= DateTime.UtcNow &&
                    c.EndDate >= DateTime.UtcNow, ct)
                ?? throw new NotFoundException("Coupon", cmd.CouponCode);

            var subtotal = orderItems.Sum(i => i.ProductPrice * i.Quantity);
            discount = coupon.DiscountType == DiscountType.Percentage
                ? subtotal * coupon.Value / 100
                : coupon.Value;
        }

        // 3. Tạo order
        var order = new Order
        {
            UserId = cmd.UserId,
            FullName = cmd.FullName,
            Phone = cmd.Phone,
            Address = cmd.Address,
            PaymentMethod = Enum.Parse<PaymentMethod>(cmd.PaymentMethod),
            PaymentStatus = PaymentStatus.Pending,
            Status = OrderStatus.Pending,
            Discount = discount,
            Items = orderItems,
            CreatedAt = DateTime.UtcNow
        };

        await _uow.Orders.AddAsync(order, ct);
        await _uow.SaveChangesAsync(ct);

        // 4. Gửi email xác nhận (fire & forget)
        _ = _email.SendOrderConfirmationAsync(order, ct);

        return order.Id;
    }
}