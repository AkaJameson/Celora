using Microsoft.AspNetCore.Http;

namespace Si.EntityFramework.IdentityServer.Configuration
{
    /// <summary>
    /// Si身份认证中间件选项
    /// </summary>
    public class IdentityMiddlewareOptions
    {
        /// <summary>
        /// 是否使用RBAC授权
        /// </summary>
        public bool UseRbacAuthorization { get; set; } = true;

        /// <summary>
        /// 是否要求所有端点都需要授权
        /// </summary>
        public bool RequireAuthorizationForAllEndpoints { get; set; } = false;

        /// <summary>
        /// 是否要求未映射端点都需要授权
        /// </summary>
        public bool RequireAuthorizationForUnmappedEndpoints { get; set; } = false;

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
    }
}
