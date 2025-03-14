using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Si.EntityFrame.IdentityServer.Tools;
using Si.EntityFramework.IdentityServer.Configuration;
using Si.EntityFramework.IdentityServer.Extensions;

namespace Si.EntityFramework.IdentityServer.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    class RoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _role;
        public RoleAttribute(string permission)
        {
            _role = permission;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var options = context.HttpContext.RequestServices.GetRequiredService<IdentityOptions>();
            if (options.IsExcludedPath(context.HttpContext))
            {
                return;
            }
            bool hasRole = false;
            switch (options.AuthorizationType)
            {
                case AuthorizationType.Jwt:
                    {
                        var jwtManager = context.HttpContext.RequestServices.GetRequiredService<JwtManager>();
                        var principal = context.HttpContext.GetPrincipalFromAuthorizationHeader(jwtManager);
                        hasRole = principal?.IsInRole(_role) ?? false;
                        break;
                    }
                case AuthorizationType.Cookies:
                    {
                        var cookiesManager = context.HttpContext.RequestServices.GetRequiredService<CookieManager>();
                        if (cookiesManager != null && cookiesManager.IsAuthenticated(context.HttpContext))
                        {
                            hasRole = cookiesManager.IsInRole(context.HttpContext, _role);

                        }
                        else 
                            hasRole = false;
                        break;
                    }
            }
            if (!hasRole)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
