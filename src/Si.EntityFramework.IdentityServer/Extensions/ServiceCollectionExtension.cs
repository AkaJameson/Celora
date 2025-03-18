using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Si.EntityFramework.IdentityServer.Configuration;
using Si.EntityFramework.IdentityServer.Middleware;
using Si.EntityFramework.IdentityServer.Models;
using Si.EntityFramework.IdentityServer.Services;
using Si.EntityFramework.IdentityServer.ServicesImpl;
using System.Text;

namespace Si.EntityFramework.IdentityServer.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static void AddIdentityServer(this IServiceCollection services,Action<JwtSettings> config)
        {
            var jwtSetting = new JwtSettings();
            config(jwtSetting);
            services.AddSingleton(jwtSetting);
            services.AddScoped<RbacConfigReader>();
            services.AddScoped<Session>();
            services.AddScoped(typeof(IRolePermissionService<>), typeof(RolePermissionService<>));
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false; // 是否要求使用 https
                options.SaveToken = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtSetting.Issuer,
                    ValidAudience = jwtSetting.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSetting.SecretKey)),
                    ClockSkew = TimeSpan.Zero // 可以设置允许的时间误差
                };
            });
            services.AddAuthorization();
        }
        /// <summary>
        /// 初始化权限
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="configPath"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static async Task UseIdentityServer(this IApplicationBuilder app, string configPath, InitMode mode)
        {
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMiddleware<SessionsMiddleware>();
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
