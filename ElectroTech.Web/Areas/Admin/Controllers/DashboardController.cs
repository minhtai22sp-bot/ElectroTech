using ElectroTech.Web.Abstractions;
using Entities.ViewModel;
using Enums;
using Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElectroTech.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class DashboardController : BaseController<DashboardController>
{
    private readonly IOrderRepository _orderRepo;
    private readonly IProductRepository _productRepo;
    private readonly IReviewRepository _reviewRepo;

    public DashboardController(
        IOrderRepository orderRepo,
        IProductRepository productRepo,
        IReviewRepository reviewRepo)
    {
        _orderRepo = orderRepo;
        _productRepo = productRepo;
        _reviewRepo = reviewRepo;
    }

   
    public async Task<IActionResult> Index()
    {
        var today = DateTime.Today;

       
        var todayOrders = await _orderRepo.GetByDateAsync(today);
        var todayRevenue = todayOrders
            .Where(o => o.Status == OrderStatus.Delivered)
            .Sum(o => o.TotalAmount);

        var pendingOrders = await _orderRepo.CountByStatusAsync(OrderStatus.Pending);
        var lowStockCount = await _productRepo.CountLowStockAsync(threshold: 5);
        var pendingReviews = await _reviewRepo.CountPendingAsync();

      
        var orderStats = new[]
        {
            new { label = "Chờ xác nhận", value = await _orderRepo.CountByStatusAsync(OrderStatus.Pending),    color = "#f59e0b" },
            new { label = "Đã xác nhận",  value = await _orderRepo.CountByStatusAsync(OrderStatus.Confirmed),  color = "#3b82f6" },
            new { label = "Đang xử lý",   value = await _orderRepo.CountByStatusAsync(OrderStatus.Processing), color = "#f97316" },
            new { label = "Đang giao",    value = await _orderRepo.CountByStatusAsync(OrderStatus.Shipped),    color = "#8b5cf6" },
            new { label = "Đã giao",      value = await _orderRepo.CountByStatusAsync(OrderStatus.Delivered),  color = "#22c55e" },
            new { label = "Đã hủy",       value = await _orderRepo.CountByStatusAsync(OrderStatus.Cancelled),  color = "#ef4444" },
        };

        ViewBag.TodayRevenue = todayRevenue;
        ViewBag.PendingOrders = pendingOrders;
        ViewBag.LowStockCount = lowStockCount;
        ViewBag.PendingReviews = pendingReviews;
        ViewBag.OrderStats = orderStats;

        return View();
    }


    [HttpGet]
    public async Task<IActionResult> Stats([FromQuery] int days = 7)
    {
        var today = DateTime.Today;
        var result = new List<object>();
        for (int i = days - 1; i >= 0; i--)
        {
            var date = today.AddDays(-i);
            var orders = await _orderRepo.GetByDateAsync(date);
            result.Add(new
            {
                label = date.ToString("dd/MM"),   // ← đổi "date" thành "label"
                revenue = orders.Where(o => o.Status == OrderStatus.Delivered)
                                .Sum(o => o.TotalAmount),
                orders = orders.Count,
                delivered = orders.Count(o => o.Status == OrderStatus.Delivered),
            });
        }

        // ── Thêm statusDist ──────────────────────────────────────────────
        var allStatuses = new[]
        {
        OrderStatus.Pending, OrderStatus.Confirmed, OrderStatus.Processing,
        OrderStatus.Shipped, OrderStatus.Delivered, OrderStatus.Cancelled
    };

        var statusDist = new List<object>();
        foreach (var s in allStatuses)
        {
            var count = await _orderRepo.CountByStatusAsync(s);
            if (count > 0)
                statusDist.Add(new { status = s.ToString(), count });
        }

        return Ok(new { last7 = result, statusDist });
    }


    [HttpGet]
    public async Task<IActionResult> TopProducts()
    {
        var top = await _orderRepo.GetTopSellingProductsAsync(limit: 5);
        return Ok(top);
    }
}