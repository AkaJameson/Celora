using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Si.EntityFrame.IdentityServer.Tools;
using Si.EntityFramework.IdentityServer.Configuration;
using Si.EntityFramework.IdentityServer.Middleware;
using Si.EntityFramework.IdentityServer.Models;
using Si.EntityFramework.IdentityServer.Services;
using Si.EntityFramework.IdentityServer.ServicesImpl;

namespace Si.EntityFramework.IdentityServer.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static void AddSiIdentityServer(this IServiceCollection services
            , Action<IdentityOptions> configure = null)
        {
            var options = new IdentityOptions();
            configure?.Invoke(options);
            services.AddScoped<RbacConfigReader>();
            services.AddSingleton(options);
            services.AddScoped<Session>();
            if(options.AuthorizationType == AuthorizationType.Jwt)
            {
                services.AddSingleton(provider =>
                {
                    return new JwtManager(options.JwtSettings);
                });
            }
            else
            {
                // 注册Cookie管理器
                services.AddSingleton(provider =>
                {
                    return new CookieManager(options.CookieSettings);
                });
            }
            services.AddScoped(typeof(IRolePermissionService<>), typeof(RolePermissionService<>));
            services.AddScoped(typeof(IUserRefreshTokenService<>), typeof(UserRefreshTokenService<>));
        }
        /// <summary>
        /// 用户信息查询
        /// </summary>
        /// <param name="app"></param>
        public static void UseSession(this WebApplication app)
        {
            app.UseMiddleware<IdentityServerMiddleware>();
        }
        /// <summary>
        /// 初始化权限
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="configPath"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static async Task InitIdentityServer(this IServiceProvider serviceProvider, string configPath, InitMode mode)
        {
            var configReader = serviceProvider.GetRequiredService<RbacConfigReader>();

            await configReader.InitializeFromXmlAsync(configPath, mode);
        }
    }
    /// <summary>
    /// 权限初始化
    /// </summary>
    public enum InitMode
    {
        /// <summary>
        /// 首次启动配置
        /// </summary>
        F,
        /// <summary>
        /// 重置
        /// </summary>
        R

    }
}
