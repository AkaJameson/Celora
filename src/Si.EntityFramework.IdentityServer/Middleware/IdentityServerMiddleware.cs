using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Si.EntityFrame.IdentityServer.Tools;
using Si.EntityFramework.IdentityServer.Configuration;

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
                            
                            break;
                        }
                    case AuthorizationType.Cookies:
                        {

                            break;
                        }
                }
                await _next(context);
            }


        }
    }
}
