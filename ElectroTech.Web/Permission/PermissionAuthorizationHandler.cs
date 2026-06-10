using Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ElectroTech.Web.Permission
{
    internal class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
       // private readonly IUserManagerRepository<ApplicationUser> _user;
       // private readonly ISignInManagerRepository<ApplicationUser> _signInManagerrRepository;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private IHttpContextAccessor _contextAccessor;
        private UserManager<ApplicationUser> _userManager;
        private RoleManager<ApplicationRole> _roleManager;
     //   private IRolesRepository rolesRepository;
        public PermissionAuthorizationHandler(UserManager<ApplicationUser> userManager, IHttpContextAccessor contextAccessor,
         //   ISignInManagerRepository<ApplicationUser> signInManagerrRepository,
            RoleManager<ApplicationRole> roleManager, SignInManager<ApplicationUser> signInManager
           // IUserManagerRepository<ApplicationUser> user, IRolesRepository rolesRepository
            )
        {
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));

            //_signInManagerrRepository = signInManagerrRepository;
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
           // _user = user;
         //   this.rolesRepository = rolesRepository;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (context.User == null)
            {
                context.Succeed(requirement);
                //context.Fail();
                //return;
            }
            var user = await _userManager.GetUserAsync(context.User);
            if (user == null)
            {
                context.Fail();
                return;
            }
            if (!user.IsActive || await _userManager.IsLockedOutAsync(user) || user.LockoutForever)
            {
                await _signInManager.SignOutAsync();
              //  await _signInManagerrRepository.SignOutAsync();
                return;
            }
            if (user.UserName.ToLower() == "superadmin" || user.Level == 2)
            {
                context.Succeed(requirement);
                return;
            }
           


            var userRoleNames = await _userManager.GetRolesAsync(user);
            var userRoles = await _roleManager.Roles.AsNoTracking().Where(x => userRoleNames.Contains(x.Name)).ToListAsync();

            var checkSupadmin = userRoles.Where(m => m.Name.ToLower().Contains("superadmin")).Count();
            if (checkSupadmin > 0)
            {
                context.Succeed(requirement);
                return;
            }
            context.Succeed(requirement);
            //foreach (var role in userRoles)
            //{
            //    var roleClaims = await rolesRepository.GetRoleClaimsAsync(role.Id, user.ComId);
            //    var permissions = roleClaims.Where(x => x.Type == CustomClaimTypes.Permission &&
            //        x.Value == requirement.Permission)
            //                              .Select(x => x.Value);
            //    if (permissions.Any())
            //    {
            //        context.Succeed(requirement);
            //        return;
            //    }
            //}

        }
    }
}
