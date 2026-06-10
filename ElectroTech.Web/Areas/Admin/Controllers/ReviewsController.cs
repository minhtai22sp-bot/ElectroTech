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
        => HttpContext.RequestServices
            .GetRequiredService<IReviewRepository>();

 
    public IActionResult Index() => View();

   
    [HttpPost]
    public async Task<IActionResult> LoadAll()
    {
        try
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var search = Request.Form["search[value]"].FirstOrDefault();

            var reviews = await ReviewRepo.GetAllAsync();

            if (!string.IsNullOrEmpty(search))
                reviews = reviews.Where(r =>
                    (r.Product?.Name?.Contains(search,
                        StringComparison.OrdinalIgnoreCase) == true) ||
                    (r.Comment?.Contains(search,
                        StringComparison.OrdinalIgnoreCase) == true) ||
                    (r.Title?.Contains(search,
                        StringComparison.OrdinalIgnoreCase) == true))
                    .ToList();

            return Json(new
            {
                draw,
                recordsFiltered = reviews.Count,
                recordsTotal = reviews.Count,
                data = reviews.Select(r => new
                {
                    r.Id,
                    ProductName = r.Product?.Name ?? "—",
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
            throw;
        }
    }

    
    [HttpPost]
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