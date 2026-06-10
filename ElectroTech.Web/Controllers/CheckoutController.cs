using ElectroTech.Application.Features.Cart.Queries;
using ElectroTech.Application.Features.Orders.Commands;
using ElectroTech.Web.Abstractions;
using Entities;
using Enums;
using Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ElectroTech.Web.Controllers
{
    [Authorize(AuthenticationSchemes = CookieAuthenticationCustomer.AuthenticationScheme)]
    public class CheckoutController : BaseController<CheckoutController>
    {
        private readonly IOrderRepository _orderRepo;

        public CheckoutController(IOrderRepository orderRepo)
            => _orderRepo = orderRepo;

        // GET /Checkout
        public async Task<IActionResult> Index()
        {
            var cartResponse = await _mediator.Send(new GetCartQuery());
            var items = cartResponse.Data;
            if (items == null || !items.Any())
            {
                _notify.Warning("Gio hang trong. Vui long them san pham truoc.");
                return RedirectToAction("Index", "Cart");
            }
            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(
            string fullName, string email, string phone,
            string shippingAddress, string? note,
            string paymentMethod = "COD")
        {
            var cartResponse = await _mediator.Send(new GetCartQuery());
            var items = cartResponse.Data;
            if (items == null || !items.Any())
            {
                _notify.Error("Giỏ hàng trống.");
                return RedirectToAction("Index", "Cart");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            if (string.IsNullOrWhiteSpace(email))
                email = User.FindFirstValue(ClaimTypes.Email) ?? "";

            var method = paymentMethod == "BankTransfer"
                ? PaymentMethod.BankTransfer
                : PaymentMethod.COD;

            var command = new CreateOrderCommand
            {
                UserId = userId,
                FullName = fullName,
                Email = email,
                Phone = phone,
                ShippingAddress = shippingAddress,
                Note = note,
                PaymentMethod = method,
                Items = items
            };

            var response = await _mediator.Send(command);
            if (!response.Succeeded)
            {
                _notify.Error(response.Message);
                return RedirectToAction(nameof(Index));
            }

            _notify.Success("Đặt hàng thành công! Cảm ơn bạn.");
            return RedirectToAction(nameof(OrderSuccess), new { orderId = response.Data });
        }

        public async Task<IActionResult> OrderSuccess(int orderId)
        {
            var order = await _orderRepo.GetByIdAsync(orderId);
            if (order is null)
                return RedirectToAction("Index", "Home");

            return View(order);
        }
    }
}