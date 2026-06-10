using ElectroTech.Application.Features.Categories.commands;

using ElectroTech.Web.Abstractions;
using Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElectroTech.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class CategoryController : BaseController<CategoryController>
{
    private readonly ICategoryRepository _categoryRepo;

    public CategoryController(ICategoryRepository categoryRepo)
        => _categoryRepo = categoryRepo;

    // GET /Admin/Category
    public async Task<IActionResult> Index()
    {
        var cats = await _categoryRepo.GetAllAsync();
        return View(cats);
    }

    // GET /Admin/Category/Create
    public IActionResult Create() => View();

    // POST /Admin/Category/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCategoryCommand cmd)
    {
        // Debug: log toàn bộ ModelState
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value.Errors.Any())
                .ToDictionary(x => x.Key,
                    x => x.Value.Errors.Select(e => e.ErrorMessage).ToList());
            ViewBag.DebugErrors = errors;
            _logger.LogWarning("Category Create ModelState invalid: {Errors}",
                string.Join(", ", errors.Select(e => $"{e.Key}: {string.Join(", ", e.Value)}")));
            return View();
        }

        if (string.IsNullOrWhiteSpace(cmd.Name))
        {
            _notify.Error("Tên danh mục không được để trống.");
            return View();
        }

        try
        {
            var result = await _mediator.Send(cmd);
            _logger.LogInformation("Category Create result: Succeeded={S}, Message={M}",
                result.Succeeded, result.Message);

            if (result.Succeeded)
            {
                _notify.Success("Tạo danh mục thành công!");
                return RedirectToAction(nameof(Index));
            }
            _notify.Error(result.Message ?? "Lỗi không xác định.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Category Create exception");
            _notify.Error("Lỗi: " + ex.Message);
        }

        return View();
    }

    // GET /Admin/Category/Edit/{id}
    public async Task<IActionResult> Edit(int id)
    {
        var cat = await _categoryRepo.GetByIdAsync(id);
        if (cat is null)
        {
            _notify.Error("Không tìm thấy danh mục.");
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Id = cat.Id;
        ViewBag.Name = cat.Name;
        ViewBag.Description = cat.Description ?? "";
        ViewBag.ImageUrl = cat.ImageUrl ?? "";
        ViewBag.DisplayOrder = cat.DisplayOrder;
        ViewBag.IsActive = cat.IsActive;

        return View();
    }

    // POST /Admin/Category/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateCategoryCommand cmd)
    {
        _logger.LogInformation("Category Edit POST: Id={Id}, Name={Name}",
            cmd.Id, cmd.Name);

        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value.Errors.Any())
                .ToDictionary(x => x.Key,
                    x => x.Value.Errors.Select(e => e.ErrorMessage).ToList());
            _logger.LogWarning("Category Edit ModelState invalid: {Errors}",
                string.Join(", ", errors.Select(e => $"{e.Key}: {string.Join(", ", e.Value)}")));

            // Reload ViewBag để view hiển thị lại
            ViewBag.Id = cmd.Id;
            ViewBag.Name = cmd.Name;
            ViewBag.Description = cmd.Description ?? "";
            ViewBag.ImageUrl = cmd.ImageUrl ?? "";
            ViewBag.DisplayOrder = cmd.DisplayOrder;
            ViewBag.IsActive = cmd.IsActive;
            ViewBag.DebugErrors = errors;
            return View();
        }

        if (string.IsNullOrWhiteSpace(cmd.Name))
        {
            _notify.Error("Tên danh mục không được để trống.");
            return RedirectToAction(nameof(Edit), new { id = cmd.Id });
        }

        try
        {
            var result = await _mediator.Send(cmd);
            _logger.LogInformation("Category Edit result: Succeeded={S}, Message={M}",
                result.Succeeded, result.Message);

            if (result.Succeeded)
            {
                _notify.Success("Cập nhật danh mục thành công!");
                return RedirectToAction(nameof(Index));
            }
            _notify.Error(result.Message ?? "Lỗi không xác định.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Category Edit exception");
            _notify.Error("Lỗi: " + ex.Message);
        }

        return RedirectToAction(nameof(Edit), new { id = cmd.Id });
    }

    // POST /Admin/Category/Delete
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _mediator.Send(
                new DeleteCategoryCommand { Id = id });
            if (result.Succeeded)
                _notify.Success("Đã xóa danh mục.");
            else
                _notify.Error(result.Message);
        }
        catch (Exception ex)
        {
            _notify.Error("Lỗi: " + ex.Message);
        }
        return RedirectToAction(nameof(Index));
    }

    // POST /Admin/Category/UploadImage
    [HttpPost]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return Json(new { success = false, message = "Không có file." });

        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var ext = Path.GetExtension(file.FileName).ToLower();
        if (!allowed.Contains(ext))
            return Json(new { success = false, message = "Chỉ chấp nhận jpg, png, webp." });

        if (file.Length > 5 * 1024 * 1024)
            return Json(new { success = false, message = "File quá lớn (tối đa 5MB)." });

        var uploadDir = Path.Combine(
            Directory.GetCurrentDirectory(), "wwwroot", "uploads", "categories");
        if (!Directory.Exists(uploadDir))
            Directory.CreateDirectory(uploadDir);

        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadDir, fileName);
        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return Json(new { success = true, url = $"/uploads/categories/{fileName}" });
    }
}