using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Si.EntityFramework.IdentityServer.Configuration;
using Si.EntityFramework.IdentityServer.Models;

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
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }
            var permissionDict = RoleBasedPermissionDictionary._instance;

            var session = context.HttpContext.RequestServices.GetRequiredService<Session>();

            if (!HasPermission(session.Roles, permissionDict))
            {
                context.Result = new ForbidResult();
            }
        }

        private bool HasPermission(IEnumerable<string> roles, RoleBasedPermissionDictionary permissionDict)
        {
            foreach (var role in roles)
            {
                if (permissionDict.TryGetValue(role, out var permissions) &&
                    permissions.Contains(_permission))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
