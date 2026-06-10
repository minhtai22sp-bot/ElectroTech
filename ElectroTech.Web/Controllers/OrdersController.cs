using ElectroTech.Application.Features.Orders.Queries;
using ElectroTech.Web.Abstractions;
using Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ElectroTech.Web.Controllers
{
    [Authorize]
    public class OrdersController : BaseController<OrdersController>
    {
        private readonly IReviewRepository _reviewRepo;

        public OrdersController(IReviewRepository reviewRepo)
        {
            _reviewRepo = reviewRepo;
        }

        // GET /Orders
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var response = await _mediator.Send(new GetUserOrdersQuery { UserId = userId });
            var orders = response.Data ?? new List<Entities.Order>();

            if (Guid.TryParse(userId, out var uid))
            {
                var reviewedProductIds = new HashSet<int>();

                foreach (var order in orders)
                {
                    foreach (var item in order.OrderItems ?? new List<Entities.OrderItem>())
                    {
                        if (item.ProductId == null) continue;
                        var already = await _reviewRepo.HasReviewedAsync(item.ProductId.Value, uid);
                        if (already) reviewedProductIds.Add(item.ProductId.Value);
                    }
                } // ← đóng foreach order

                ViewBag.ReviewedProductIds = reviewedProductIds;
            } // ← đóng if

            return View(orders);
        }

        // GET /Orders/Detail/{id}
        public async Task<IActionResult> Detail(int id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var response = await _mediator.Send(new GetOrderByIdQuery
            {
                Id = id,
                UserId = userId
            });

            if (!response.Succeeded)
            {
                _notify.Error(response.Message);
                return RedirectToAction(nameof(Index));
            }

            return View(response.Data);
        }
    }
}