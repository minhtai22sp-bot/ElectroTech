using ElectroTech.Application.Features.Categories.Queries;
using ElectroTech.Application.Features.Products.Queries;
using ElectroTech.Web.Abstractions;
using Entities.ViewModel;
using Library;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ElectroTech.Web.Models;
using System.Diagnostics;

namespace ElectroTech.Web.Controllers;

public class HomeController : BaseController<HomeController>
{
    public async Task<IActionResult> Index()
    {
        var model = new ProductSearch
        {
            pageSize = 8,
            currentPage = 1,
            skip = 0,
            isFeatured = true,
            isActive = true
        };

        var response = await _mediator.Send(new GetAllPaginatedListQuery { model = model });

        // ← Thêm danh mục vào ViewBag
        var cats = await _mediator.Send(new GetAllCategoryQuery());
        ViewBag.Categories = cats.Data ?? new List<Entities.Categories>();

        if (response.Succeeded && response.Data != null)
            return View(response.Data);

        return View(new PaginatedList<ProductIndexModel>(
            new List<ProductIndexModel>(), 0, 1, 8
        ));
    }

    public IActionResult Privacy() => View();

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}