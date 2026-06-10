using ElectroTech.Application.Features.Categories.Queries;
using ElectroTech.Application.Features.Products.Commands;
using ElectroTech.Application.Features.Products.Queries;
using ElectroTech.Web.Abstractions;
using Entities;
using Entities.ViewModel;
using Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElectroTech.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class ProductController : BaseController<ProductController>
{
    private IProductRepository ProductRepo
        => HttpContext.RequestServices
            .GetRequiredService<IProductRepository>();

    // GET /Admin/Product
    public IActionResult Index() => View();

    // POST /Admin/Product/LoadAll
    [HttpPost]
    public async Task<IActionResult> LoadAll(ProductSearch model)
    {
        try
        {
            var draw = HttpContext.Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();

            if (string.IsNullOrEmpty(model.keyword))
                model.keyword = searchValue;

            int pageSize = length != null ? Convert.ToInt32(length) : 10;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int currentPage = skip >= 0 ? (skip / pageSize) + 1 : 1;

            var response = await _mediator.Send(new GetAllPaginatedListQuery
            {
                model = new ProductSearch
                {
                    keyword = model.keyword,
                    pageSize = pageSize,
                    currentPage = currentPage,
                    skip = skip
                }
            });

            if (response.Succeeded)
                return Json(new
                {
                    draw,
                    recordsFiltered = response.Data.TotalItemCount,
                    recordsTotal = response.Data.TotalItemCount,
                    data = response.Data.Items
                });

            return Json(new { draw, recordsFiltered = 0, recordsTotal = 0, data = "" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            throw;
        }
    }

    // GET /Admin/Product/Create
    public async Task<IActionResult> Create()
    {
        await LoadCategories();
        return View(new CreateProductViewModel());
    }

    // POST /Admin/Product/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProductViewModel vm)
    {
        // Debug errors
        if (!ModelState.IsValid)
        {
            var debugErrors = ModelState
                .Where(x => x.Value.Errors.Any())
                .ToDictionary(
                    x => x.Key,
                    x => x.Value.Errors
                        .Select(e => e.ErrorMessage +
                            (e.Exception != null ? $" [{e.Exception.Message}]" : ""))
                        .ToList());

            ViewBag.DebugErrors = debugErrors;

            foreach (var kvp in debugErrors)
                foreach (var err in kvp.Value)
                    _logger.LogWarning("ModelState [{Key}]: {Err}", kvp.Key, err);

            await LoadCategories();
            return View(vm);
        }

        var response = await _mediator.Send(new CreateProductCommand
        {
            Name = vm.Name,
            Code = vm.Code,
            Brand = vm.Brand,
            CategoryId = vm.CategoryId,
            Description = vm.Description,
            Price = vm.GetPrice(),
            OriginalPrice = vm.GetOriginalPrice(),
            Stock = vm.Stock,
            ThumbnailUrl = vm.ThumbnailUrl,
            IsFeatured = vm.IsFeatured,
            IsActive = vm.IsActive,
            Specs = (vm.Specs ?? new())
           .Where(s => !string.IsNullOrWhiteSpace(s.SpecKey)
                    && !string.IsNullOrWhiteSpace(s.SpecValue))
           .ToList(),
            ImageUrls = (vm.ImageUrls ?? new())
           .Where(u => !string.IsNullOrWhiteSpace(u))
           .ToList()
        });

        if (response.Succeeded)
        {
            _notify.Success("Tạo sản phẩm thành công!");
            return RedirectToAction(nameof(Index));
        }

        _notify.Error(response.Message);
        await LoadCategories();
        return View(vm);
    }

    // GET /Admin/Product/Edit/{id}
    public async Task<IActionResult> Edit(int id)
    {
        ModelState.Clear();

        // Dùng GetById trực tiếp — trả về full Product entity
        var product = await ProductRepo.GetById(id);
        if (product is null)
        {
            _notify.Error("Không tìm thấy sản phẩm.");
            return RedirectToAction(nameof(Index));
        }

        var specs = await ProductRepo.GetSpecsAsync(id);
        var images = await ProductRepo.GetImagesAsync(id);

        var enUS = System.Globalization.CultureInfo.GetCultureInfo("en-US");

        var vm = new EditProductViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Code = product.Code,
            Brand = product.Brand,
            CategoryId = product.CategoryId,
            Description = product.Description,
            PriceInput = ((long)product.Price).ToString("N0", enUS),
            OriginalPriceInput = product.OriginalPrice.HasValue
                                 ? ((long)product.OriginalPrice.Value).ToString("N0", enUS)
                                 : "",
            Stock = product.Stock,
            ThumbnailUrl = product.ThumbnailUrl,
            IsFeatured = product.IsFeatured,
            IsActive = product.IsActive,
            Specs = specs.Select(s => new ProductSpecViewModel
            {
                Id = s.Id,
                GroupName = s.GroupName ?? "",
                SpecKey = s.SpecKey,
                SpecValue = s.SpecValue,
                DisplayOrder = s.DisplayOrder
            }).ToList(),
            ImageUrls = images.Select(i => i.ImageUrl).ToList()
        };

        await LoadCategories();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(52428800)]          // ← thêm dòng này
    [RequestFormLimits(ValueCountLimit = 10000)]  // ← và dòng này
    public async Task<IActionResult> Edit(EditProductViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            var debugErrors = ModelState
                .Where(x => x.Value.Errors.Any())
                .ToDictionary(
                    x => x.Key,
                    x => x.Value.Errors
                        .Select(e => e.ErrorMessage +
                            (e.Exception != null ? $" [{e.Exception.Message}]" : ""))
                        .ToList());

            ViewBag.DebugErrors = debugErrors;

            foreach (var kvp in debugErrors)
                foreach (var err in kvp.Value)
                    _logger.LogWarning("ModelState [{Key}]: {Err}", kvp.Key, err);

            await LoadCategories();
            return View(vm);
        }

        // Edit POST
        var response = await _mediator.Send(new UpdateProductCommand
        {
            Id = vm.Id,
            Name = vm.Name,
            Code = vm.Code,
            Brand = vm.Brand,
            CategoryId = vm.CategoryId,
            Description = vm.Description,
            Price = vm.GetPrice(),
            OriginalPrice = vm.GetOriginalPrice(),
            Stock = vm.Stock,
            ThumbnailUrl = vm.ThumbnailUrl,
            IsFeatured = vm.IsFeatured,
            IsActive = vm.IsActive,
            Specs = (vm.Specs ?? new())
                .Where(s => !string.IsNullOrWhiteSpace(s.SpecKey)
                         && !string.IsNullOrWhiteSpace(s.SpecValue))
                .ToList(),
            ImageUrls = (vm.ImageUrls ?? new())
                .Where(u => !string.IsNullOrWhiteSpace(u))
                .ToList()
        });
        if (response.Succeeded)
        {
            _notify.Success("Cập nhật sản phẩm thành công!");
            return RedirectToAction(nameof(Index));
        }

        _notify.Error(response.Message);
        await LoadCategories();
        return View(vm);
    }

    // POST /Admin/Product/Delete
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var response = await _mediator.Send(new DeleteProductCommand { Id = id });

        if (response.Succeeded)
            _notify.Success("Đã xóa sản phẩm.");
        else
            _notify.Error(response.Message);

        return RedirectToAction(nameof(Index));
    }

    // POST /Admin/Product/ToggleActive
    [HttpPost]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var product = await ProductRepo.GetById(id);
        if (product is null)
            return Json(new { success = false, message = "Không tìm thấy sản phẩm." });

        var response = await _mediator.Send(new UpdateProductCommand
        {
            Id = product.Id,
            Name = product.Name,
            Code = product.Code,
            Brand = product.Brand,
            CategoryId = product.CategoryId,
            Price = product.Price,
            OriginalPrice = product.OriginalPrice,
            Stock = product.Stock,
            Description = product.Description,
            ThumbnailUrl = product.ThumbnailUrl,
            IsFeatured = product.IsFeatured,
            IsActive = !product.IsActive
        });

        return Json(new { success = response.Succeeded, isActive = !product.IsActive });
    }

    // POST /Admin/Product/UploadImage
    [HttpPost]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return Json(new { success = false, message = "Không có file." });

        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        var ext = Path.GetExtension(file.FileName).ToLower();

        if (!allowed.Contains(ext))
            return Json(new
            {
                success = false,
                message = "Chỉ chấp nhận jpg, png, webp, gif."
            });

        if (file.Length > 5 * 1024 * 1024)
            return Json(new
            {
                success = false,
                message = "File quá lớn (tối đa 5MB)."
            });

        try
        {
            var uploadDir = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot", "uploads", "products");

            if (!Directory.Exists(uploadDir))
                Directory.CreateDirectory(uploadDir);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadDir, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return Json(new
            {
                success = true,
                url = $"/uploads/products/{fileName}"
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    // Helper
    private async Task LoadCategories()
    {
        var cats = await _mediator.Send(new GetAllCategoryQuery());
        ViewBag.Categories = cats.Data ?? new List<Categories>();
    }
}