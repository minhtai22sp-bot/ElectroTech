using ElectroTech.Application.Features.Cart.Commands;
using ElectroTech.Application.Features.Cart.Queries;
using ElectroTech.Application.Features.Products.Queries;
using ElectroTech.Web.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Entities;

namespace ElectroTech.Web.Controllers;

public class CartController : BaseController<CartController>
{
    // GET /Cart
    public async Task<IActionResult> Index()
    {
        var response = await _mediator.Send(new GetCartQuery());
        return View(response.Data);
    }

    // POST /Cart/Add — AJAX thêm sản phẩm
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, int quantity = 1)
    {
        var product = await _mediator.Send(new GetIdProductQuery { Id = productId });
        if (!product.Succeeded || product.Data is null)
            return Json(new { success = false, message = "San pham khong ton tai." });

        if (product.Data.Stock < quantity)
            return Json(new { success = false, message = $"Chi con {product.Data.Stock} san pham." });

        var response = await _mediator.Send(new AddToCartCommand
        {
            ProductId = product.Data.Id,
            ProductName = product.Data.Name,
            Price = product.Data.Price,
            ImageUrl = product.Data.ThumbnailUrl,
            Quantity = quantity
        });

        return Json(new { success = true, count = response.Data });
    }

    // POST /Cart/UpdateAjax — AJAX cap nhat so luong
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateAjax(int productId, int quantity)
    {
        if (quantity < 1)
        {
            await _mediator.Send(new RemoveFromCartCommand { ProductId = productId });
        }
        else
        {
            await _mediator.Send(new UpdateCartCommand
            {
                ProductId = productId,
                Quantity = quantity
            });
        }

        var cart = await _mediator.Send(new GetCartQuery());
        var items = cart.Data ?? new List<CartItem>();
        var item = items.FirstOrDefault(i => i.ProductId == productId);
        var total = items.Sum(i => i.Subtotal);
        var count = items.Sum(i => i.Quantity);

        return Json(new
        {
            success = true,
            subtotal = item?.Subtotal ?? 0,
            total,
            count
        });
    }

    // POST /Cart/RemoveAjax — AJAX xoa 1 san pham
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveAjax(int productId)
    {
        await _mediator.Send(new RemoveFromCartCommand { ProductId = productId });

        var cart = await _mediator.Send(new GetCartQuery());
        var items = cart.Data ?? new List<CartItem>();
        var total = items.Sum(i => i.Subtotal);
        var count = items.Sum(i => i.Quantity);

        return Json(new { success = true, total, count });
    }

    // POST /Cart/ClearAjax — AJAX xoa gio hang
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClearAjax()
    {
        await _mediator.Send(new ClearCartCommand());
        return Json(new { success = true, total = 0, count = 0 });
    }

    // POST /Cart/Update — form submit (giu lai de tuong thich)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int productId, int quantity)
    {
        await _mediator.Send(new UpdateCartCommand { ProductId = productId, Quantity = quantity });
        return RedirectToAction(nameof(Index));
    }

    // POST /Cart/Remove — form submit (giu lai de tuong thich)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int productId)
    {
        await _mediator.Send(new RemoveFromCartCommand { ProductId = productId });
        return RedirectToAction(nameof(Index));
    }

    // POST /Cart/Clear — form submit (giu lai de tuong thich)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clear()
    {
        await _mediator.Send(new ClearCartCommand());
        return RedirectToAction(nameof(Index));
    }

    // GET /Cart/Count — AJAX badge
    [HttpGet]
    public async Task<IActionResult> Count()
    {
        var response = await _mediator.Send(new GetCartQuery());
        var count = response.Data?.Sum(i => i.Quantity) ?? 0;
        return Json(new { count });
    }
}