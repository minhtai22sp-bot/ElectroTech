using Interfaces;
using System.Security.Claims;

namespace ElectroTech.Web.Service
{
    public class AuthenticatedUserService : IAuthenticatedUserService
    {
        public AuthenticatedUserService(IHttpContextAccessor httpContextAccessor)
        {
            //UserId = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier) == null ? null : httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier).Value;
            UserId = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Username = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;
        }

        public string UserId { get; }
        public string Username { get; }
        public string _comId { get; }
        public int? ComId
        {
            get
            {
                if (!string.IsNullOrEmpty(_comId))
                {
                    return int.Parse(_comId);
                }
                return null;
            }
        }
    }
}
