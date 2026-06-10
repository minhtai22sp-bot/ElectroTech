using AspNetCoreHero.Results;
using Entities;
using Enums;
using Hangfire;
using Interfaces;
using MediatR;

namespace ElectroTech.Application.Features.Orders.Commands;

public class CreateOrderCommand : IRequest<IResult<int>>
{
    public string UserId { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string ShippingAddress { get; set; } = "";
    public string? Note { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.COD;
    public List<CartItem> Items { get; set; } = new();

    public class CreateOrderCommandHandler
        : IRequestHandler<CreateOrderCommand, IResult<int>>
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IProductRepository _productRepo;
        private readonly ICartRepository _cartRepo;
        private readonly IBackgroundJobClient _jobClient;
        private readonly IEmailService _emailService;

        public CreateOrderCommandHandler(
            IOrderRepository orderRepo,
            IProductRepository productRepo,
            ICartRepository cartRepo,
            IBackgroundJobClient jobClient,
            IEmailService emailService)
        {
            _orderRepo = orderRepo;
            _productRepo = productRepo;
            _cartRepo = cartRepo;
            _jobClient = jobClient;
            _emailService = emailService;
        }

        public async Task<IResult<int>> Handle(
            CreateOrderCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var orderItems = new List<OrderItem>();
                decimal subTotal = 0;

                foreach (var item in command.Items)
                {
                    var product = await _productRepo.GetById(item.ProductId);
                    if (product is null)
                        return await Result<int>.FailAsync(
                            $"Sản phẩm #{item.ProductId} không tồn tại.");

                    if (product.Stock < item.Quantity)
                        return await Result<int>.FailAsync(
                            $"Sản phẩm '{product.Name}' chỉ còn {product.Stock} trong kho.");

                    product.Stock -= item.Quantity;
                    await _productRepo.UpdateAsync(product);

                    var orderItem = new OrderItem
                    {
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        ProductImage = item.ImageUrl ?? string.Empty,
                        ProductCode = product.Code ?? string.Empty,
                        ProductSlug = string.Empty,
                        UnitPrice = item.Price,
                        Quantity = item.Quantity,
                        TotalPrice = item.Price * item.Quantity
                    };
                    orderItems.Add(orderItem);
                    subTotal += orderItem.TotalPrice;
                }

                var orderCode = $"ET{DateTime.Now:yyyyMMddHHmmss}";
                Guid? userId = Guid.TryParse(command.UserId, out var parsed)
                    ? parsed : null;

                var order = new Order
                {
                    OrderCode = orderCode,
                    UserId = userId,
                    FullName = command.FullName,
                    Email = command.Email ?? string.Empty,
                    Phone = command.Phone,
                    ShippingAddress = command.ShippingAddress,
                    Note = command.Note,
                    PaymentMethod = command.PaymentMethod,
                    Status = OrderStatus.Pending,
                    PaymentStatus = PaymentStatus.Unpaid,
                    SubTotal = subTotal,
                    DiscountAmount = 0,
                    ShippingFee = 0,
                    Tax = 0,
                    TotalAmount = subTotal,
                    OrderItems = orderItems
                };

                await _orderRepo.AddAsync(order);

                // Xóa giỏ hàng
                await _cartRepo.ClearAsync(command.UserId);

                // ✅ Gửi email xác nhận qua Hangfire background job
                if (!string.IsNullOrWhiteSpace(command.Email))
                {
                    var emailItems = command.Items
                        .Select(i => (i.ProductName, i.Quantity, i.Price))
                        .ToList();

                    _jobClient.Enqueue(() =>
                        _emailService.SendOrderConfirmationAsync(
                            command.Email,
                            command.FullName,
                            order.OrderCode,
                            order.TotalAmount,
                            emailItems,
                            command.ShippingAddress));
                }

                return await Result<int>.SuccessAsync(
                    order.Id, "Đặt hàng thành công.");
            }
            catch (Exception ex)
            {
                return await Result<int>.FailAsync($"Lỗi: {ex.Message}");
            }
        }
    }
}