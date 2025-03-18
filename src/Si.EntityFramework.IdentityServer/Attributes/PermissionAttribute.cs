using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Si.EntityFramework.IdentityServer.Attributes
{

    class PermissionAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly string _permission;

        public PermissionAttribute(string permission)
        {
            _permission = permission;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
           
        }
    }
}
