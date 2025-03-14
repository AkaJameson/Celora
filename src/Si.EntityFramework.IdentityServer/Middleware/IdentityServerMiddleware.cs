using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Si.EntityFrame.IdentityServer.Tools;
using Si.EntityFramework.IdentityServer.Configuration;
using Si.EntityFramework.IdentityServer.Extensions;
using Si.EntityFramework.IdentityServer.Models;
using System.Security.Claims;

namespace Si.EntityFramework.IdentityServer.Middleware
{
    public class IdentityServerMiddleware
    {
        private readonly RequestDelegate _next;
        public IdentityServerMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            var options = (IdentityOptions)context.RequestServices.GetRequiredService<IdentityOptions>();
            if (options==null)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("401 UnAuthorize");
                return;
            }
            else
            {
                var session = context.RequestServices.GetRequiredService<Session>();
                switch (options.AuthorizationType)
                {
                    case AuthorizationType.Jwt:
                        {
                            var jwtManager = (JwtManager)context.RequestServices.GetRequiredService<JwtManager>();
                            if(jwtManager==null)
                            {
                                context.Response.StatusCode = 401;
                                await context.Response.WriteAsync("401 UnAuthorize");
                                return;
                            }
                            var principals = context.GetPrincipalFromAuthorizationHeader(jwtManager);
                            session.userId = int.TryParse(principals.FindFirst("UserId").Value, out var userId) ? userId : 0;
                            session.Account = principals.FindFirst("Account").Value ?? string.Empty;
                            session.Name = principals.FindFirst("name").Value?? string.Empty;
                            session.Phone = principals.FindFirst("Phone").Value ?? string.Empty;
                            session.Roles = principals.FindAll(ClaimTypes.Role).Select(p => p.Value).AsEnumerable();
                            session.Permissions = principals.FindAll("permission").Select(p=> p.Value).AsEnumerable();
                            break;
                        }
                    case AuthorizationType.Cookies:
                        {
                            var cookiesManage = context.RequestServices.GetRequiredService<CookieManager>();
                            if (cookiesManage == null)
                            {
                                context.Response.StatusCode = 401;
                                await context.Response.WriteAsync("401 UnAuthorize");
                                return;
                            }
                            var principals = cookiesManage.GetPrincipalFromCookie(context);
                            session.userId = int.TryParse(principals.FindFirst("UserId").Value, out var userId) ? userId : 0;
                            session.Account = principals.FindFirst("Account").Value ?? string.Empty;
                            session.Name = principals.FindFirst("name").Value ?? string.Empty;
                            session.Phone = principals.FindFirst("Phone").Value ?? string.Empty;
                            session.Roles = principals.FindAll(ClaimTypes.Role).Select(p => p.Value).AsEnumerable();
                            session.Permissions = principals.FindAll("permission").Select(p => p.Value).AsEnumerable();
                            break;
                        }
                }
                await _next(context);
            }


        }
    }
}
