using ElectroTech.Application.Features.Reviews.Commands;
using ElectroTech.Web.Abstractions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ElectroTech.Web.Controllers;

[Authorize(AuthenticationSchemes =
    CookieAuthenticationCustomer.AuthenticationScheme)]
public class ReviewController : BaseController<ReviewController>
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        int productId,
        byte rating,
        string? title,
        string comment)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        var response = await _mediator.Send(new CreateReviewCommand
        {
            ProductId = productId,
            UserId = userId,
            Rating = rating,
            Title = title,
            Comment = comment
        });

        if (response.Succeeded)
            _notify.Success(response.Message);
        else
            _notify.Error(response.Message);

        return RedirectToAction("Detail", "Products",
            new { id = productId });
    }
}