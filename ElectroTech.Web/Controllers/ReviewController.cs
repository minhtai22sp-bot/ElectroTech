using ElectroTech.Application.Features.Reviews.Commands;

using ElectroTech.Web.Abstractions;
using Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ElectroTech.Web.Controllers;

[Authorize(AuthenticationSchemes = CookieAuthenticationCustomer.AuthenticationScheme)]
public class ReviewController : BaseController<ReviewController>
{
    private readonly IOrderRepository _orderRepo;
    private readonly IReviewRepository _reviewRepo;

    public ReviewController(IOrderRepository orderRepo, IReviewRepository reviewRepo)
    {
        _orderRepo = orderRepo;
        _reviewRepo = reviewRepo;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        int productId, byte rating, string? title, string comment)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        if (!Guid.TryParse(userIdStr, out var userId))
        {
            _notify.Error("Phiên đăng nhập không hợp lệ.");
            return RedirectToAction("Detail", "Products", new { id = productId });
        }

        // Double-check ở controller trước khi gửi command
        var hasDelivered = await _orderRepo.HasDeliveredProductAsync(userId, productId);
        if (!hasDelivered)
        {
            _notify.Error("Bạn chỉ có thể đánh giá sau khi đơn hàng đã được giao.");
            return RedirectToAction("Detail", "Products", new { id = productId });
        }

        var alreadyReviewed = await _reviewRepo.HasReviewedAsync(productId, userId);
        if (alreadyReviewed)
        {
            _notify.Error("Bạn đã đánh giá sản phẩm này rồi.");
            return RedirectToAction("Detail", "Products", new { id = productId });
        }

        var response = await _mediator.Send(new CreateReviewCommand
        {
            ProductId = productId,
            UserId = userIdStr,
            Rating = rating,
            Title = title,
            Comment = comment
        });

        if (response.Succeeded)
            _notify.Success(response.Message);
        else
            _notify.Error(response.Message);

        return RedirectToAction("Detail", "Products", new { id = productId });
    }

    // GET: kiểm tra user có quyền đánh giá không (dùng cho JS check)
    [HttpGet]
    public async Task<IActionResult> CanReview(int productId)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        if (!Guid.TryParse(userIdStr, out var userId))
            return Json(new { canReview = false, reason = "Chưa đăng nhập" });

        var hasDelivered = await _orderRepo.HasDeliveredProductAsync(userId, productId);
        if (!hasDelivered)
            return Json(new { canReview = false, reason = "Bạn chưa mua sản phẩm này hoặc đơn hàng chưa được giao." });

        var alreadyReviewed = await _reviewRepo.HasReviewedAsync(productId, userId);
        if (alreadyReviewed)
            return Json(new { canReview = false, reason = "Bạn đã đánh giá sản phẩm này rồi." });

        return Json(new { canReview = true, reason = "" });
    }
}