using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace ElectroTech.Web.Permission
{
    public class PermissionPolicyProvider : IAuthorizationPolicyProvider
    {
        public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }

        public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            // There can only be one policy provider in ASP.NET Core.
            // We only handle permissions related policies, for the rest
            /// we will use the default provider.
            FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        {
            return FallbackPolicyProvider.GetDefaultPolicyAsync();
        }

        // Dynamically creates a policy with a requirement that contains the permission.
        // The policy name must match the permission that is needed.
        public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            // if (policyName.StartsWith("Permissions", StringComparison.OrdinalIgnoreCase))
            //{
            var policy = new AuthorizationPolicyBuilder().AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme);

            policy.AddRequirements(new PermissionRequirement(policyName));
            return Task.FromResult(policy.Build());
            // }

            // Policy is not for permissions, try the default provider.
            //  return FallbackPolicyProvider.GetPolicyAsync(policyName);
        }

        public Task<AuthorizationPolicy> GetFallbackPolicyAsync()
        {
            return Task.FromResult<AuthorizationPolicy>(null);
           // return FallbackPolicyProvider.GetPolicyAsync(null);// mở dòng này khi nào cần login admin thôi
            //return FallbackPolicyProvider.GetDefaultPolicyAsync();// dongf nay thì bắt buộc login khi vào web
        }
    }
}
