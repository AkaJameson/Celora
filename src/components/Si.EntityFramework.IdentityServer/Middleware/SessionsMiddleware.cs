using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Si.EntityFramework.IdentityServer.Entitys;
using Si.EntityFramework.IdentityServer.Models;
using System.Security.Claims;

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
            var session = context.RequestServices.GetRequiredService<Session>();
            var user = context.User;
            // 解析用户信息到Session
            session.userId = int.Parse(user.FindFirst("Id")?.Value ?? "0");
            session.Name = user.FindFirst("Name")?.Value ?? string.Empty;
            session.Account = user.FindFirst("Account")?.Value ?? string.Empty;
            session.Phone = user.FindFirst("Phone")?.Value ?? string.Empty;
            session.Roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            await _next(context);
        }
    }
}
