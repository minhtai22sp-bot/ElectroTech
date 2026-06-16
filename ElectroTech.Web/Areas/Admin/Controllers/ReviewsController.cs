using ElectroTech.Web.Abstractions;
using Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElectroTech.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class ReviewsController : BaseController<ReviewsController>
{
    private IReviewRepository ReviewRepo
        => HttpContext.RequestServices.GetRequiredService<IReviewRepository>();

    private IProductRepository ProductRepo
        => HttpContext.RequestServices.GetRequiredService<IProductRepository>();

    public IActionResult Index() => View();

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> LoadAll()
    {
        try
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var search = Request.Form["search[value]"].FirstOrDefault();

            var reviews = await ReviewRepo.GetAllAsync();

            var productDict = ProductRepo.Entities
                .ToDictionary(p => p.Id, p => p.Name);

            if (!string.IsNullOrEmpty(search))
                reviews = reviews.Where(r =>
                    (r.Comment?.Contains(search, StringComparison.OrdinalIgnoreCase) == true) ||
                    (r.Title?.Contains(search, StringComparison.OrdinalIgnoreCase) == true) ||
                    (r.UserName?.Contains(search, StringComparison.OrdinalIgnoreCase) == true))
                    .ToList();

            return Json(new
            {
                draw,
                recordsFiltered = reviews.Count,
                recordsTotal = reviews.Count,
                data = reviews.Select(r => new
                {
                    r.Id,
                    ProductName = productDict.TryGetValue(r.ProductId, out var name)
                        ? name : $"Sản phẩm #{r.ProductId}",
                    UserName = r.UserName ?? "Ẩn danh",
                    r.Rating,
                    r.Title,
                    r.Comment,
                    r.IsApproved,
                    r.IsVerifiedPurchase,
                    CreatedOn = r.CreatedOn.ToString("dd/MM/yyyy HH:mm")
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            return Json(new
            {
                draw = "",
                recordsFiltered = 0,
                recordsTotal = 0,
                data = new object[] { },
                error = ex.Message
            });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        var review = await ReviewRepo.GetByIdAsync(id);
        if (review is null) return NotFound();

        var productDict = ProductRepo.Entities
            .ToDictionary(p => p.Id, p => p.Name);

        ViewBag.ProductName = productDict.TryGetValue(review.ProductId, out var name)
            ? name : $"Sản phẩm #{review.ProductId}";

        return View(review);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var review = await ReviewRepo.GetByIdAsync(id);
        if (review is null)
            return Json(new { success = false, message = "Không tìm thấy." });

        review.IsApproved = true;
        await ReviewRepo.UpdateAsync(review);
        return Json(new { success = true, message = "Đã duyệt." });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id)
    {
        var review = await ReviewRepo.GetByIdAsync(id);
        if (review is null)
            return Json(new { success = false, message = "Không tìm thấy." });

        review.IsApproved = false;
        await ReviewRepo.UpdateAsync(review);
        return Json(new { success = true, message = "Đã ẩn." });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await ReviewRepo.DeleteAsync(id);
        _notify.Success("Đã xóa đánh giá.");
        return RedirectToAction(nameof(Index));
    }
}