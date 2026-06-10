using ElectroTech.Application.Features.Orders.Queries;
using ElectroTech.Web.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ElectroTech.Web.Controllers
{

    [Authorize]
    public class OrdersController : BaseController<OrdersController>
    {
        // GET /Orders
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var response = await _mediator.Send(new GetUserOrdersQuery { UserId = userId });
            return View(response.Data ?? new List<Entities.Order>());
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
