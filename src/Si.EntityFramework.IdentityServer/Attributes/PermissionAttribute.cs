using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Si.EntityFrame.IdentityServer.Tools;
using Si.EntityFramework.IdentityServer.Configuration;
using Si.EntityFramework.IdentityServer.Extensions;

namespace Si.EntityFramework.IdentityServer.Attributes
{
    
    class PermissionAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _permission;

        public PermissionAttribute(string permission)
        {
            _permission = permission;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var options = context.HttpContext.RequestServices.GetRequiredService<IdentityOptions>();
            if (options.IsExcludedPath(context.HttpContext))
            {
                return;
            }
            bool hasPermission = false;
            switch (options.AuthorizationType)
            {
                case AuthorizationType.Jwt:
                    {
                        var jwtManager = context.HttpContext.RequestServices.GetRequiredService<JwtManager>();
                        var principal = context.HttpContext.GetPrincipalFromAuthorizationHeader(jwtManager);
                        hasPermission = principal?.HasClaim("Permission",_permission) ?? false;
                        break;
                    }
                case AuthorizationType.Cookies:
                    {
                        var cookiesManager = context.HttpContext.RequestServices.GetRequiredService<CookieManager>();
                        if (cookiesManager != null && cookiesManager.IsAuthenticated(context.HttpContext))
                        {
                            hasPermission = cookiesManager.HasPermission(context.HttpContext, _permission);
                        }
                        else
                            hasPermission = false;
                        break;
                    }
            }
            if (!hasPermission)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
