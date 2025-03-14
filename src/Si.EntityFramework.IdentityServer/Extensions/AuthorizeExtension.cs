using Microsoft.AspNetCore.Http;
using Si.EntityFrame.IdentityServer.Tools;
using System.Security.Claims;

namespace Si.EntityFramework.IdentityServer.Extensions
{
    public static class AuthorizeExtension
    {
        /// <summary>
        /// 从Authorization头获取身份
        /// </summary>
        public static ClaimsPrincipal GetPrincipalFromAuthorizationHeader(this HttpContext context,JwtManager jwtManager)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (jwtManager == null)
                throw new ArgumentNullException(nameof(jwtManager));
            // 获取Authorization头
            if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader))
                return null;

            var headerValue = authHeader.ToString();
            if (string.IsNullOrEmpty(headerValue))
                return null;

            // 处理Bearer令牌
            if (headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = headerValue.Substring("Bearer ".Length).Trim();
                if (jwtManager == null)
                {
                    throw new InvalidOperationException("JWTManager未注册");
                }
                if (!string.IsNullOrEmpty(token))
                {
                    return jwtManager.ValidateToken(token);
                }
            }
            return null;
        }
    }
}
