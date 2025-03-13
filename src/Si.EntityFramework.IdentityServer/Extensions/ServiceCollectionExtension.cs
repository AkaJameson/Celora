using Microsoft.Extensions.DependencyInjection;
using Si.EntityFrame.IdentityServer.Tools;
using Si.EntityFramework.IdentityServer.Configuration;

namespace Si.EntityFramework.IdentityServer.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddSiIdentityServer(this IServiceCollection services, Action<IdentityOptions> configure = null)
        {
            var options = new IdentityOptions();
            configure?.Invoke(options);
            services.AddScoped<RbacConfigReader>();
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
        }
    }
}
