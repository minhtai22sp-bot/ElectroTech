using ElectroTech.Application.Features.Brand.Commands;
using ElectroTech.Application.Features.Brands.Commands;
using ElectroTech.Web.Abstractions;
using Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElectroTech.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class BrandController : BaseController<BrandController>
{
    private readonly IBrandRepository _brandRepo;

    public BrandController(IBrandRepository brandRepo)
        => _brandRepo = brandRepo;

    // GET /Admin/Brand
    public async Task<IActionResult> Index()
    {
        var brands = await _brandRepo.GetAllAsync();
        return View(brands?.ToList());
    }

    // GET /Admin/Brand/Create
    public IActionResult Create() => View();

    // POST /Admin/Brand/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBrandCommand cmd)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value.Errors.Any())
                .ToDictionary(x => x.Key,
                    x => x.Value.Errors.Select(e => e.ErrorMessage).ToList());
            ViewBag.DebugErrors = errors;
            _logger.LogWarning("Brand Create ModelState invalid: {Errors}",
                string.Join(", ", errors.Select(e => $"{e.Key}: {string.Join(", ", e.Value)}")));
            return View();
        }

        if (string.IsNullOrWhiteSpace(cmd.Name))
        {
            _notify.Error("Tên thương hiệu không được để trống.");
            return View();
        }

        try
        {
            var result = await _mediator.Send(cmd);
            _logger.LogInformation("Brand Create result: Succeeded={S}, Message={M}",
                result.Succeeded, result.Message);

            if (result.Succeeded)
            {
                _notify.Success("Tạo thương hiệu thành công!");
                return RedirectToAction(nameof(Index));
            }
            _notify.Error(result.Message ?? "Lỗi không xác định.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Brand Create exception");
            _notify.Error("Lỗi: " + ex.Message);
        }

        return View();
    }

    // GET /Admin/Brand/Edit/{id}
    public async Task<IActionResult> Edit(int id)
    {
        var brand = await _brandRepo.GetByIdAsync(id);
        if (brand is null)
        {
            _notify.Error("Không tìm thấy thương hiệu.");
            return RedirectToAction(nameof(Index));
        }
        return View(brand);
    }

    // POST /Admin/Brand/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateBrandCommand cmd)
    {
        _logger.LogInformation("Brand Edit POST: Id={Id}, Name={Name}",
            cmd.Id, cmd.Name);

        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value.Errors.Any())
                .ToDictionary(x => x.Key,
                    x => x.Value.Errors.Select(e => e.ErrorMessage).ToList());
            _logger.LogWarning("Brand Edit ModelState invalid: {Errors}",
                string.Join(", ", errors.Select(e => $"{e.Key}: {string.Join(", ", e.Value)}")));

            var brand = await _brandRepo.GetByIdAsync(cmd.Id);
            ViewBag.DebugErrors = errors;
            return View(brand);
        }

        if (string.IsNullOrWhiteSpace(cmd.Name))
        {
            _notify.Error("Tên thương hiệu không được để trống.");
            return RedirectToAction(nameof(Edit), new { id = cmd.Id });
        }

        try
        {
            var result = await _mediator.Send(cmd);
            _logger.LogInformation("Brand Edit result: Succeeded={S}, Message={M}",
                result.Succeeded, result.Message);

            if (result.Succeeded)
            {
                _notify.Success("Cập nhật thương hiệu thành công!");
                return RedirectToAction(nameof(Index));
            }
            _notify.Error(result.Message ?? "Lỗi không xác định.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Brand Edit exception");
            _notify.Error("Lỗi: " + ex.Message);
        }

        return RedirectToAction(nameof(Edit), new { id = cmd.Id });
    }

    // POST /Admin/Brand/Delete
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _mediator.Send(new DeleteBrandCommand { Id = id });
            if (result.Succeeded)
                _notify.Success("Đã xóa thương hiệu.");
            else
                _notify.Error(result.Message);
        }
        catch (Exception ex)
        {
            _notify.Error("Lỗi: " + ex.Message);
        }
        return RedirectToAction(nameof(Index));
    }

    // POST /Admin/Brand/UploadImage
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
            Directory.GetCurrentDirectory(), "wwwroot", "uploads", "brands");
        if (!Directory.Exists(uploadDir))
            Directory.CreateDirectory(uploadDir);

        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadDir, fileName);
        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return Json(new { success = true, url = $"/uploads/brands/{fileName}" });
    }
}