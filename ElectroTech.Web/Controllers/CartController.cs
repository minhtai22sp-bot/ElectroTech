using ElectroTech.Application.Features.Cart.Commands;
using ElectroTech.Application.Features.Cart.Queries;
using ElectroTech.Application.Features.Products.Queries;
using ElectroTech.Web.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Entities;

namespace ElectroTech.Web.Controllers;

public class CartController : BaseController<CartController>
{
   
    [Authorize(AuthenticationSchemes = CookieAuthenticationCustomer.AuthenticationScheme)]
    public async Task<IActionResult> Index()
    {
        var response = await _mediator.Send(new GetCartQuery());
        return View(response.Data);
    }

    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, int quantity = 1)
    {
        
        if (User.Identity?.IsAuthenticated != true)
        {
            var returnUrl = Uri.EscapeDataString(Url.Action("Index", "Products") ?? "/");
            return Json(new
            {
                success = false,
                requireLogin = true,
                message = "Vui lòng đăng nhập để thêm vào giỏ hàng.",
                redirectUrl = $"/account/login?returnUrl={returnUrl}"
            });
        }

        var product = await _mediator.Send(new GetIdProductQuery { Id = productId });
        if (!product.Succeeded || product.Data is null)
            return Json(new { success = false, message = "Sản phẩm không tồn tại." });

        if (product.Data.Stock < quantity)
            return Json(new { success = false, message = $"Chỉ còn {product.Data.Stock} sản phẩm." });

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


    [HttpPost]
    [Authorize(AuthenticationSchemes = CookieAuthenticationCustomer.AuthenticationScheme)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateAjax(int productId, int quantity)
    {
        if (quantity < 1)
            await _mediator.Send(new RemoveFromCartCommand { ProductId = productId });
        else
            await _mediator.Send(new UpdateCartCommand { ProductId = productId, Quantity = quantity });

        var cart = await _mediator.Send(new GetCartQuery());
        var items = cart.Data ?? new List<CartItem>();
        var item = items.FirstOrDefault(i => i.ProductId == productId);
        var total = items.Sum(i => i.Subtotal);
        var count = items.Sum(i => i.Quantity);

        return Json(new { success = true, subtotal = item?.Subtotal ?? 0, total, count });
    }

   
    [HttpPost]
    [Authorize(AuthenticationSchemes = CookieAuthenticationCustomer.AuthenticationScheme)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveAjax(int productId)
    {
        await _mediator.Send(new RemoveFromCartCommand { ProductId = productId });

        var cart = await _mediator.Send(new GetCartQuery());
        var items = cart.Data ?? new List<CartItem>();

        return Json(new
        {
            success = true,
            total = items.Sum(i => i.Subtotal),
            count = items.Sum(i => i.Quantity)
        });
    }

   
    [HttpPost]
    [Authorize(AuthenticationSchemes = CookieAuthenticationCustomer.AuthenticationScheme)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClearAjax()
    {
        await _mediator.Send(new ClearCartCommand());
        return Json(new { success = true, total = 0, count = 0 });
    }

    
    [HttpPost]
    [Authorize(AuthenticationSchemes = CookieAuthenticationCustomer.AuthenticationScheme)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int productId, int quantity)
    {
        await _mediator.Send(new UpdateCartCommand { ProductId = productId, Quantity = quantity });
        return RedirectToAction(nameof(Index));
    }

   
    [HttpPost]
    [Authorize(AuthenticationSchemes = CookieAuthenticationCustomer.AuthenticationScheme)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int productId)
    {
        await _mediator.Send(new RemoveFromCartCommand { ProductId = productId });
        return RedirectToAction(nameof(Index));
    }

    
    [HttpPost]
    [Authorize(AuthenticationSchemes = CookieAuthenticationCustomer.AuthenticationScheme)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clear()
    {
        await _mediator.Send(new ClearCartCommand());
        return RedirectToAction(nameof(Index));
    }

   
    [HttpGet]
    public async Task<IActionResult> Count()
    {
        if (User.Identity?.IsAuthenticated != true)
            return Json(new { count = 0 });

        var response = await _mediator.Send(new GetCartQuery());
        var count = response.Data?.Sum(i => i.Quantity) ?? 0;
        return Json(new { count });
    }
}