using AspNetCoreHero.Results;
using Entities;
using Enums;
using Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace ElectroTech.Application.Features.Reviews.Commands
{
    public class CreateReviewCommand : IRequest<IResult<bool>>
    {
        public int ProductId { get; set; }
        public string UserId { get; set; } = "";

        [Range(1, 5)]
        public byte Rating { get; set; }

        [MaxLength(200)]
        public string? Title { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Comment { get; set; } = "";

        public class Handler : IRequestHandler<CreateReviewCommand, IResult<bool>>
        {
            private readonly IReviewRepository _reviewRepo;
            private readonly IOrderRepository _orderRepo;

            public Handler(IReviewRepository reviewRepo, IOrderRepository orderRepo)
            {
                _reviewRepo = reviewRepo;
                _orderRepo = orderRepo;
            }

            public async Task<IResult<bool>> Handle(
                CreateReviewCommand cmd, CancellationToken ct)
            {
                try
                {
                    if (!Guid.TryParse(cmd.UserId, out var userId))
                        return await Result<bool>.FailAsync("User không hợp lệ.");

                    // ✅ 1. Kiểm tra đã mua và đã giao hàng chưa
                    var hasDelivered = await _orderRepo
                        .HasDeliveredProductAsync(userId, cmd.ProductId);

                    if (!hasDelivered)
                        return await Result<bool>.FailAsync(
                            "Bạn chỉ có thể đánh giá sản phẩm sau khi đơn hàng đã được giao.");

                    // ✅ 2. Kiểm tra đã review chưa
                    if (await _reviewRepo.HasReviewedAsync(cmd.ProductId, userId))
                        return await Result<bool>.FailAsync(
                            "Bạn đã đánh giá sản phẩm này rồi.");

                    // ✅ 3. Lấy OrderItemId để đánh dấu verified purchase
                    var orderItemId = await _orderRepo
                        .GetDeliveredOrderItemIdAsync(userId, cmd.ProductId);

                    var review = new Review
                    {
                        ProductId = cmd.ProductId,
                        UserId = userId,
                        Rating = cmd.Rating,
                        Title = cmd.Title,
                        Comment = cmd.Comment,
                        IsApproved = true,
                        IsVerifiedPurchase = true,  // ✅ Đã xác nhận mua hàng
                        OrderItemId = orderItemId
                    };

                    await _reviewRepo.AddAsync(review);
                    return await Result<bool>.SuccessAsync(
                        true, "Cảm ơn bạn đã đánh giá sản phẩm!");
                }
                catch (Exception ex)
                {
                    return await Result<bool>.FailAsync($"Lỗi: {ex.Message}");
                }
            }
        }
    }
}