using ElectroTech.Application.Features.Products.Queries;
using ElectroTech.Web.Abstractions;
using Entities.ViewModel;
using Interfaces;
using Library;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ElectroTech.Web.Controllers;

public class ProductsController : BaseController<ProductsController>
{
    private readonly IOrderRepository _orderRepo;
    private readonly IReviewRepository _reviewRepo;

    public ProductsController(IOrderRepository orderRepo, IReviewRepository reviewRepo)
    {
        _orderRepo = orderRepo;
        _reviewRepo = reviewRepo;
    }

    public async Task<IActionResult> Index(ProductSearch model)
    {
        model.pageSize = 12;
        model.currentPage = model.currentPage > 0 ? model.currentPage : 1;
        model.skip = (model.currentPage - 1) * model.pageSize;

       
        var response = await _mediator.Send(new GetAllPaginatedListQuery { model = model });
        var statsResponse = await _mediator.Send(new GetSidebarStatsQuery { Keyword = model.keyword });

        if (statsResponse.Succeeded)
            ViewBag.SidebarStats = statsResponse.Data;
        ViewBag.CategoryId = model.categoryId;
        ViewBag.SearchQuery = model.keyword;
        ViewBag.SortBy = model.sortBy;
        ViewBag.InStock = model.inStock;
        ViewBag.OnSale = model.onSale;
        ViewBag.Featured = model.featured;
        ViewBag.Brand = model.brand;
        ViewBag.MinPrice = model.minPrice;
        ViewBag.MaxPrice = model.maxPrice;

        if (response.Succeeded)
            return View(response.Data);

        return View(new PaginatedList<ProductIndexModel>(
            new List<ProductIndexModel>(), 0, 1, model.pageSize));
    }

    public async Task<IActionResult> Detail(int id, int? orderId)
    {
        var response = await _mediator.Send(new GetIdProductQuery { Id = id });
        if (!response.Succeeded || response.Data is null)
            return NotFound();

        ViewBag.ShowReviewForm = false;
        ViewBag.AlreadyReviewed = false;
        ViewBag.OrderId = null;

        if (orderId.HasValue)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            if (Guid.TryParse(userIdStr, out var userId))
            {
                var hasDelivered = await _orderRepo.HasDeliveredProductAsync(userId, id);

                var alreadyReviewed = await _reviewRepo.HasReviewedAsync(id, userId, orderId.Value);

                ViewBag.ShowReviewForm = hasDelivered && !alreadyReviewed;
                ViewBag.AlreadyReviewed = hasDelivered && alreadyReviewed;
                ViewBag.OrderId = orderId.Value;
            }
        }

        return View("Details", response.Data);
    }
}