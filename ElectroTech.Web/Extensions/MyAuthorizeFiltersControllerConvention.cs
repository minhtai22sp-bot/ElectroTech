using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace ElectroTech.Web.Extensions
{
    public class MyAuthorizeFiltersControllerConvention : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            string apiArea;


            //if (controller.RouteValues.Any()
            //    && controller.RouteValues.TryGetValue("area", out apiArea)
            //    && apiArea.ToLower().Equals("admin"))
            //{
            //    var policy = new AuthorizationPolicyBuilder()
            //         .AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme)
            //        .RequireAuthenticatedUser().Build();
            //    controller.Filters.Add(new AuthorizeFilter(policy));

            //}
            //else
            //{
            //    if (controller.RouteValues.Any()
            //        && controller.RouteValues.TryGetValue("area", out apiArea)
            //        && apiArea.ToLower().Equals("sell"))
            //    {
            //        //.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme)
            //        var policy = new AuthorizationPolicyBuilder(CookieAuthenticationCustomer.AuthenticationScheme)
            //            .AddAuthenticationSchemes(CookieAuthenticationCustomer.AuthenticationScheme)
            //            .RequireAuthenticatedUser().Build();
            //        controller.Filters.Add(new AuthorizeFilter(policy));

            //        //var usercom =  _userrepository.GetUserAsync(System.Security.Claims.ClaimsPrincipal.Current).Result;
            //        //if (usercom!=null)
            //        //{
            //        //    if (!(usercom.IdType == (int)TypeCustomerEnum.Companypartner))
            //        //    {

            //        //    }
            //        //}

            //    }
            //    //  controller.Filters.Add(new AuthorizeFilter("defaultpolicy"));
            //}
           // controller.Filters.Add(new AuthorizeFilter(""));
        }
    }
}
