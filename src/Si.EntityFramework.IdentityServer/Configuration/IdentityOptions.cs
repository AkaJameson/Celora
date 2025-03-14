using Microsoft.AspNetCore.Http;

namespace Si.EntityFramework.IdentityServer.Configuration
{
    /// <summary>
    /// Si身份认证选项
    /// </summary>
    public class IdentityOptions
    {
        public AuthorizationType AuthorizationType { get; set; }
        /// <summary>
        /// JWT设置
        /// </summary>
        public JwtSettings JwtSettings { get; set; } = new JwtSettings();

        /// <summary>
        /// Cookie设置
        /// </summary>
        public CookieSettings CookieSettings { get; set; } = new CookieSettings();

        /// <summary>
        /// 是否使用RBAC授权
        /// </summary>
        public bool UseRbacAuthorization { get; set; } = true;
        
        /// <summary>
        /// 排除的路径，不需要授权
        /// </summary>
        public List<PathString> ExcludedPaths { get; set; } = new List<PathString>
        {
            new PathString("/login"),
            new PathString("/register"),
            new PathString("/error"),
            new PathString("/health"),
            new PathString("/swagger")
        };

        /// <summary>
        /// 判断当前请求路径是否在排除的路径列表中
        /// </summary>
        public bool IsExcludedPath(HttpContext context)
        {
            var requestPath = context.Request.Path;

            return ExcludedPaths.Any(excluded => requestPath.StartsWithSegments(excluded, StringComparison.OrdinalIgnoreCase));
        }
    }
    public enum AuthorizationType
    {
        /// <summary>
        /// Cookies
        /// </summary>
        Cookies,
        /// <summary>
        /// JWTToken
        /// </summary>
        Jwt
    }
}
