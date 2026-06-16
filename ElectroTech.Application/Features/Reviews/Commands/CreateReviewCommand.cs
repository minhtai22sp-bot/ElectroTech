using AspNetCoreHero.Results;
using Entities;
using Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace ElectroTech.Application.Features.Reviews.Commands
{
    public class CreateReviewCommand : IRequest<IResult<bool>>
    {
        public int ProductId { get; set; }
        public string UserId { get; set; } = "";
        public string UserName { get; set; } = "";
        public int? OrderId { get; set; }        
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
            private readonly IProductRepository _productRepo;

            public Handler(
                IReviewRepository reviewRepo,
                IOrderRepository orderRepo,
                IProductRepository productRepo)
            {
                _reviewRepo = reviewRepo;
                _orderRepo = orderRepo;
                _productRepo = productRepo;
            }

            public async Task<IResult<bool>> Handle(
                CreateReviewCommand cmd, CancellationToken ct)
            {
                try
                {
                    if (!Guid.TryParse(cmd.UserId, out var userId))
                        return await Result<bool>.FailAsync("User không hợp lệ.");

                    var hasDelivered = await _orderRepo
                        .HasDeliveredProductAsync(userId, cmd.ProductId);
                    if (!hasDelivered)
                        return await Result<bool>.FailAsync(
                            "Bạn chỉ có thể đánh giá sản phẩm sau khi đơn hàng đã được giao.");

                    bool alreadyReviewed;
                    if (cmd.OrderId.HasValue)
                        alreadyReviewed = await _reviewRepo.HasReviewedAsync(cmd.ProductId, userId, cmd.OrderId.Value);
                    else
                        alreadyReviewed = await _reviewRepo
                            .HasReviewedAsync(cmd.ProductId, userId);

                    if (alreadyReviewed)
                        return await Result<bool>.FailAsync("Bạn đã đánh giá sản phẩm này rồi.");

                    var orderItemId = await _orderRepo
                        .GetDeliveredOrderItemIdAsync(userId, cmd.ProductId);

                    var review = new Review
                    {
                        ProductId = cmd.ProductId,
                        UserId = userId,
                        UserName = cmd.UserName,
                        OrderId = cmd.OrderId,         
                        Rating = cmd.Rating,
                        Title = cmd.Title,
                        Comment = cmd.Comment,
                        IsApproved = true,
                        IsVerifiedPurchase = true,
                        OrderItemId = orderItemId
                    };

                    await _reviewRepo.AddAsync(review);

                    var product = await _productRepo.GetById(cmd.ProductId);
                    if (product != null)
                    {
                        var allReviews = await _reviewRepo
                            .GetApprovedByProductAsync(cmd.ProductId);

                        product.ReviewCount = allReviews.Count;
                        product.Rating = allReviews.Count > 0
                            ? Math.Round((decimal)allReviews.Average(r => r.Rating), 1)
                            : 0;

                        await _productRepo.UpdateAsync(product);
                    }

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