using ElectroTech.Application.Features.Auth.Commands;

using ElectroTech.Web.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using Entities.ViewModel;
using Microsoft.AspNetCore.Authentication.Cookies;
namespace ElectroTech.Web.Controllers
{
    public class AccountController : BaseController<AccountController>
    {
        // GET /Account/Login
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(vm);

            var response = await _mediator.Send(new LoginCommand
            {
                Email = vm.Email,
                Password = vm.Password,
                Remember = vm.Remember
            });

            if (!response.Succeeded)
            {
                _notify.Error(response.Message);
                return View(vm);
            }

            _notify.Success("Đăng nhập thành công!");

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

     
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            // Bỏ qua ModelState check tạm thời
            try
            {
                var response = await _mediator.Send(new RegisterCommand
                {
                    FullName = vm.FullName,
                    Email = vm.Email,
                    Password = vm.Password,
                    ConfirmPassword = vm.ConfirmPassword,
                    PhoneNumber = vm.PhoneNumber,
                    Address = vm.Address
                });

                if (!response.Succeeded)
                {
                    // Hiện lỗi rõ ràng
                    TempData["DebugError"] = response.Message;
                    return View(vm);
                }

                TempData["Success"] = "Đăng ký thành công!";
                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                TempData["DebugError"] = ex.Message;
                return View(vm);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Xóa toàn bộ session data
            HttpContext.Session.Clear();

            // Xóa session cookie
            Response.Cookies.Delete(".AspNetCore.Session");
            Response.Cookies.Delete("ElectroTech_Cart");
            Response.Cookies.Delete("ElectroTech.CustomerAuth");
            Response.Cookies.Delete("ElectroTech.AdminAuth");

            await HttpContext.SignOutAsync("CookiesCustomer");
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            _notify.Success("Đã đăng xuất thành công.");
            return RedirectToAction("Index", "Home");
        }
    }
}
