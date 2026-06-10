using ElectroTech.Application.Features.Users.Commands;
using ElectroTech.Application.Features.Users.Queries;
using ElectroTech.Web.Abstractions;
using Entities.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElectroTech.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class UsersController : BaseController<UsersController>
{
    // GET /Admin/Users
    public IActionResult Index() => View();

    // GET /Admin/Users/Roles
    public IActionResult Roles() => View();

    // POST /Admin/Users/LoadAll — DataTables
    [HttpPost]
    public async Task<IActionResult> LoadAll(UserSearch model)
    {
        try
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();

            if (string.IsNullOrEmpty(model.keyword))
                model.keyword = searchValue;

            int pageSize = length != null ? Convert.ToInt32(length) : 10;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int currentPage = skip >= 0 ? (skip / pageSize) + 1 : 1;

            var response = await _mediator.Send(new GetAllPaginatedUsersQuery
            {
                model = new UserSearch
                {
                    keyword = model.keyword,
                    isActive = model.isActive,
                    pageSize = pageSize,
                    currentPage = currentPage,
                    skip = skip
                }
            });

            if (response.Succeeded)
            {
                var total = response.Data.TotalItemCount;
                return Json(new
                {
                    draw = draw,
                    recordsFiltered = total,
                    recordsTotal = total,
                    data = response.Data.Items
                });
            }

            return Json(new { draw, recordsFiltered = 0, recordsTotal = 0, data = "" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            throw;
        }
    }
    // POST /Admin/Users/LoadAllRoles — DataTables cho trang Roles
    [HttpPost]
    public async Task<IActionResult> LoadAllRoles(UserSearch model)
    {
        try
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();

            if (string.IsNullOrEmpty(model.keyword))
                model.keyword = searchValue;

            int pageSize = length != null ? Convert.ToInt32(length) : 10;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int currentPage = skip >= 0 ? (skip / pageSize) + 1 : 1;

            var response = await _mediator.Send(new GetAllPaginatedUsersQuery
            {
                model = new UserSearch
                {
                    keyword = model.keyword,
                    pageSize = pageSize,
                    currentPage = currentPage,
                    skip = skip
                }
            });

            if (response.Succeeded)
            {
                var total = response.Data.TotalItemCount;
                return Json(new
                {
                    draw = draw,
                    recordsFiltered = total,
                    recordsTotal = total,
                    data = response.Data.Items
                });
            }

            return Json(new { draw, recordsFiltered = 0, recordsTotal = 0, data = "" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            throw;
        }
    }

    // POST /Admin/Users/ToggleActive — AJAX
    [HttpPost]
    public async Task<IActionResult> ToggleActive(string id)
    {
        var response = await _mediator.Send(new ToggleUserActiveCommand { Id = id });
        return Json(new { success = response.Succeeded, message = response.Message });
    }

    // POST /Admin/Users/ToggleLockout — AJAX
    [HttpPost]
    public async Task<IActionResult> ToggleLockout(string id)
    {
        var response = await _mediator.Send(new ToggleUserLockoutCommand { Id = id });
        return Json(new { success = response.Succeeded, message = response.Message });
    }

    // POST /Admin/Users/AssignRole — AJAX
    // POST /Admin/Users/AssignRole
    [HttpPost]
    public async Task<IActionResult> AssignRole(string id, string role)
    {
        try
        {
            var response = await _mediator.Send(
                new AssignRoleCommand { Id = id, Role = role });
            return Json(new { success = response.Succeeded, message = response.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            return Json(new { success = false, message = ex.Message });
        }
    }

    // POST /Admin/Users/RemoveRole
    [HttpPost]
    public async Task<IActionResult> RemoveRole(string id, string role)
    {
        try
        {
            var response = await _mediator.Send(
                new RemoveRoleCommand { Id = id, Role = role });
            return Json(new { success = response.Succeeded, message = response.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            return Json(new { success = false, message = ex.Message });
        }
    }
}