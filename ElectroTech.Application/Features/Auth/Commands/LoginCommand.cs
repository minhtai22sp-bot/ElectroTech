using AspNetCoreHero.Results;
using Entities;
using Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace ElectroTech.Application.Features.Auth.Commands
{
    public class LoginCommand : IRequest<IResult<string>>
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool Remember { get; set; }

        public class Handler : IRequestHandler<LoginCommand, IResult<string>>
        {
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly IHttpContextAccessor _httpContext;
            private readonly ICartRepository _cartRepo;

            public Handler(
                UserManager<ApplicationUser> userManager,
                IHttpContextAccessor httpContext,
                ICartRepository cartRepo)
            {
                _userManager = userManager;
                _httpContext = httpContext;
                _cartRepo = cartRepo;
            }

            public async Task<IResult<string>> Handle(
                LoginCommand cmd, CancellationToken ct)
            {
                var user = await _userManager.FindByEmailAsync(cmd.Email);
                if (user is null || !await _userManager.CheckPasswordAsync(user, cmd.Password))
                    return await Result<string>.FailAsync("Email hoặc mật khẩu không đúng.");

                // Lấy cookie cart trước khi login
                var ctx = _httpContext.HttpContext!;
                var cartJson = ctx.Request.Cookies["ElectroTech_Cart"];
                List<CartItem> cookieItems = new();

                if (!string.IsNullOrEmpty(cartJson))
                {
                    try
                    {
                        cookieItems = System.Text.Json.JsonSerializer
                            .Deserialize<List<CartItem>>(cartJson) ?? new();
                    }
                    catch { }
                }

                // Sign in
                var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name,           user.UserName ?? ""),
            new(ClaimTypes.Email,          user.Email    ?? ""),
        };
                var roles = await _userManager.GetRolesAsync(user);
                foreach (var role in roles)
                    claims.Add(new Claim(ClaimTypes.Role, role));

                var identity = new ClaimsIdentity(claims, "CookiesCustomer");
                var principal = new ClaimsPrincipal(identity);

                await ctx.SignInAsync("CookiesCustomer", principal,
                    new AuthenticationProperties { IsPersistent = cmd.Remember });

                // Merge cookie cart vào DB
                if (cookieItems.Any())
                {
                    await _cartRepo.MergeFromCookieAsync(user.Id, cookieItems);
                    // Xóa cookie cart sau khi merge
                    ctx.Response.Cookies.Delete("ElectroTech_Cart");
                }

                return await Result<string>.SuccessAsync("Đăng nhập thành công.");
            }
        }
    }
}