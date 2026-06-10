using AspNetCoreHero.Results;
using Entities;
using Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            private readonly IReviewRepository _repo;

            public Handler(IReviewRepository repo) => _repo = repo;

            public async Task<IResult<bool>> Handle(
                CreateReviewCommand cmd, CancellationToken ct)
            {
                try
                {
                    if (!Guid.TryParse(cmd.UserId, out var userId))
                        return await Result<bool>.FailAsync("User không hợp lệ.");

                    // Kiểm tra đã review chưa
                    if (await _repo.HasReviewedAsync(cmd.ProductId, userId))
                        return await Result<bool>.FailAsync(
                            "Bạn đã đánh giá sản phẩm này rồi.");

                    var review = new Review
                    {
                        ProductId = cmd.ProductId,
                        UserId = userId,
                        Rating = cmd.Rating,
                        Title = cmd.Title,
                        Comment = cmd.Comment,
                        IsApproved = true, // Auto approve
                        IsVerifiedPurchase = false
                    };

                    await _repo.AddAsync(review);
                    return await Result<bool>.SuccessAsync(true, "Cảm ơn bạn đã đánh giá!");
                }
                catch (Exception ex)
                {
                    return await Result<bool>.FailAsync($"Lỗi: {ex.Message}");
                }
            }
        }
    }
}
