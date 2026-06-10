using Entities;
using Enums;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectroTech.Infrastructure.Identity.Seeds
{
    public static class DefaultRoles
    {
        public static async Task SeedAsync(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            //Seed Roles
            var role = new ApplicationRole(Roles.SuperAdmin.ToString());
            await roleManager.CreateAsync(role);
            //await roleManager.CreateAsync(new IdentityRole(Roles.Admin.ToString()));
            //await roleManager.CreateAsync(new IdentityRole(Roles.Basic.ToString()));
        }
    }
}
