using ElectroTech.Application.Features.Categories.Queries;
using ElectroTech.Application.Features.Products.Queries;
using ElectroTech.Web.Abstractions;
using Entities;
using Entities.ViewModel;
using Interfaces;
using Library;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ElectroTech.Web.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ElectroTech.Web.Controllers;

public class HomeController : BaseController<HomeController>
{
    private readonly IProductRepository _productRepo;
    private readonly IReviewRepository _reviewRepo;
    private readonly IOrderRepository _orderRepo;

    public HomeController(
        IProductRepository productRepo,
        IReviewRepository reviewRepo,
        IOrderRepository orderRepo)
    {
        _productRepo = productRepo;
        _reviewRepo = reviewRepo;
        _orderRepo = orderRepo;
    }

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

        // Chạy tuần tự để tránh lỗi DbContext concurrency
        var response = await _mediator.Send(new GetAllPaginatedListQuery { model = model });
        var cats = await _mediator.Send(new GetAllCategoryQuery());
        var productCount = await _productRepo.Entities.CountAsync(p => p.IsActive);
        var customerCount = await _orderRepo.CountUniqueCustomersAsync();
        var avgRatingRaw = await _reviewRepo.GetAverageApprovedRatingAsync();
        var orderCount = await _orderRepo.CountTotalOrdersAsync();
        ViewBag.OrderCount = orderCount;
        ViewBag.Categories = cats.Data ?? new List<Entities.Categories>();
        ViewBag.ProductCount = productCount;
        ViewBag.CustomerCount = customerCount;
        ViewBag.AvgRating = avgRatingRaw.HasValue
                                    ? Math.Round((decimal)avgRatingRaw.Value, 1)
                                    : 4.8m;
        ViewBag.OrderCount = orderCount;
        if (response.Succeeded && response.Data != null)
            return View(response.Data);

        return View(new PaginatedList<ProductIndexModel>(
            new List<ProductIndexModel>(), 0, 1, 8));
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