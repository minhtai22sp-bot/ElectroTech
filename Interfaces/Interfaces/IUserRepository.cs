using Entities.ViewModel;
using Entities;
using Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IUserRepository
    {
        Task<ApplicationUser> GetByIdAsync(string id);
        Task<PaginatedList<UserIndexModel>> GetAllPaginatedAsync(UserSearch model);
        Task<bool> ToggleActiveAsync(string id);
        Task<bool> ToggleLockoutAsync(string id);
        Task<List<string>> GetRolesAsync(string id);
        Task<bool> AssignRoleAsync(string id, string role);
        Task<bool> RemoveRoleAsync(string id, string role);
    }
}
