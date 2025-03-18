using Microsoft.AspNetCore.Http;

namespace Si.EntityFramework.IdentityServer.Middleware
{
    public class SessionsMiddleware
    {
        private readonly RequestDelegate _next;
        public SessionsMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

        }
    }
}
