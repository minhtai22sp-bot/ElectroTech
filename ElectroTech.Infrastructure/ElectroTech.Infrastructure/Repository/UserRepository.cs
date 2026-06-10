using Entities.ViewModel;
using Entities;
using Interfaces;
using Library;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ElectroTech.Infrastructure.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserRepository(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ApplicationUser> GetByIdAsync(string id)
            => await _userManager.FindByIdAsync(id);

        public async Task<PaginatedList<UserIndexModel>> GetAllPaginatedAsync(UserSearch model)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(model.keyword))
                query = query.Where(u =>
                    u.FullName.Contains(model.keyword) ||
                    u.Email.Contains(model.keyword) ||
                    u.PhoneNumber.Contains(model.keyword));

            if (model.isActive.HasValue)
                query = query.Where(u => u.IsActive == model.isActive.Value);

            query = query.OrderByDescending(u => u.CreatedOn);

            var users = await query
                .Skip(model.skip)
                .Take(model.pageSize)
                .ToListAsync();

            var total = await query.CountAsync();

            // Lấy roles cho từng user
            var result = new List<UserIndexModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                result.Add(new UserIndexModel
                {
                    Id = user.Id,
                    FullName = user.FullName ?? user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    IsActive = user.IsActive,
                    LockoutForever = user.LockoutForever,
                    Roles = string.Join(", ", roles),
                    CreatedOn = user.CreatedOn
                });
            }

            return new PaginatedList<UserIndexModel>(result, total,
                model.currentPage, model.pageSize);
        }

        public async Task<bool> ToggleActiveAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null) return false;
            user.IsActive = !user.IsActive;
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> ToggleLockoutAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null) return false;
            user.LockoutForever = !user.LockoutForever;
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<List<string>> GetRolesAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null) return new List<string>();
            return (await _userManager.GetRolesAsync(user)).ToList();
        }

        public async Task<bool> AssignRoleAsync(string id, string role)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null) return false;
            var result = await _userManager.AddToRoleAsync(user, role);
            return result.Succeeded;
        }

        public async Task<bool> RemoveRoleAsync(string id, string role)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null) return false;
            var result = await _userManager.RemoveFromRoleAsync(user, role);
            return result.Succeeded;
        }
    }
}
