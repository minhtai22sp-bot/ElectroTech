using ElectroTech.Application.Features.Orders.Commands;
using ElectroTech.Application.Features.Orders.Queries;
using ElectroTech.Web.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElectroTech.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class OrdersController : BaseController<OrdersController>
{
    public IActionResult Index() => View();

    [HttpPost]
    public async Task<IActionResult> LoadAll(string? status = null)
    {
        try
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var search = Request.Form["search[value]"].FirstOrDefault();

            var response = await _mediator.Send(
                new GetAllOrdersQuery { Status = status });

            if (!response.Succeeded)
                return Json(new
                {
                    draw,
                    recordsFiltered = 0,
                    recordsTotal = 0,
                    data = ""
                });

            var data = response.Data;

            if (!string.IsNullOrEmpty(search))
                data = data.Where(o =>
                    (o.FullName?.Contains(search, StringComparison.OrdinalIgnoreCase) == true) ||
                    (o.Phone?.Contains(search) == true) ||
                    o.Id.ToString().Contains(search)).ToList();

            return Json(new
            {
                draw,
                recordsFiltered = data.Count,
                recordsTotal = data.Count,
                data = data.Select(o => new
                {
                    o.Id,
                    o.OrderCode,
                    o.FullName,
                    o.Phone,
                    o.Email,
                    o.ShippingAddress,
                    PaymentMethod = o.PaymentMethod.ToString(),
                    Status = o.Status.ToString(),
                    PaymentStatus = o.PaymentStatus.ToString(),
                    o.TotalAmount,
                    o.SubTotal,
                    CreatedOn = o.CreatedOn.ToString("dd/MM/yyyy HH:mm"),
                    ItemCount = o.OrderItems?.Count ?? 0
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            throw;
        }
    }

    public async Task<IActionResult> Detail(int id)
    {
        // Admin không cần filter theo UserId → truyền null
        var response = await _mediator.Send(
            new GetOrderByIdQuery { Id = id, UserId = null });

        if (!response.Succeeded)
        {
            _notify.Error("Không tìm thấy đơn hàng.");
            return RedirectToAction(nameof(Index));
        }

        return View(response.Data);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var response = await _mediator.Send(
            new UpdateOrderStatusCommand { Id = id, Status = status });

        return Json(new
        {
            success = response.Succeeded,
            message = response.Message
        });
    }

    [HttpPost]
    public async Task<IActionResult> UpdatePayment(int id, string paymentStatus)
    {
        var response = await _mediator.Send(
            new UpdatePaymentStatusCommand
            {
                Id = id,
                PaymentStatus = paymentStatus
            });

        return Json(new
        {
            success = response.Succeeded,
            message = response.Message
        });
    }
}