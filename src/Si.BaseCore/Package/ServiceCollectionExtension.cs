using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Si.CoreHub.Package.Abstraction;
using Si.CoreHub.Package.Core;
using Si.CoreHub.Package.Entitys;
using Si.CoreHub.Package.Localizer;

namespace Si.CoreHub.Package
{
    public static class ServiceCollectionExtension
    {
        /// <summary>
        /// 添加包
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options"></param>
        public static void AddPackages(this WebApplicationBuilder builder, Action<PackOptions> options)
        {
            var packOptions = new PackOptions();
            options(packOptions);
            PackageManager finder = new PackageManager(packOptions);
            //注入包管理器
            builder.Services.AddSingleton(finder);
            builder.Services.AddScoped(typeof(IPackConfiguration<>), typeof(PackConfiguration<>));
            foreach (var package in finder.GetPacks())
            {
                PackLoader.LoadPack(builder, package);
            }
            if (packOptions.EnableLocaizor)
            {
                builder.Services.AddModuleLocalization();
            }
        }

        /// <summary>
        /// 注册包
        /// </summary>
        /// <param name="app"></param>
        /// <param name="routes"></param>
        /// <param name="provider"></param>
        public static void UsePackages(this IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider provider, Action<RequestLocalizationOptions> localizorSetup = null)
        {
            var packManager = app.ApplicationServices.GetRequiredService<PackageManager>();
            if (packManager == null)
            {
                throw new ArgumentNullException(nameof(packManager));
            }
            if (packManager.GetPackOptions().EnableLocaizor)
            {
                UseModuleLocalization(app, packManager, localizorSetup);
            }
            PackLoader.UsePack(app, provider, routes);
        }
        /// <summary>
        /// 添加模块化本地化服务
        /// </summary>
        internal static IServiceCollection AddModuleLocalization(this IServiceCollection services)
        {
            // 注册本地化服务
            services.AddSingleton<IStringLocalizerFactory, ModuleStringLocalizerFactory>();
            services.AddScoped(typeof(IStringLocalizer<>), typeof(ModuleStringLocalizer<>));
            // 注册请求本地化中间件
            services.AddLocalization();

            return services;
        }
        /// <summary>
        /// 使用模块化本地化
        /// </summary>
        internal static IApplicationBuilder UseModuleLocalization(IApplicationBuilder app, PackageManager packageManager, Action<RequestLocalizationOptions> setupAction = null)
        {
            if (packageManager != null)
            {
                var modules = packageManager.GetPacks();
                foreach (var module in modules)
                {
                    LocalizationResourceManager.LoadModuleResources(module);
                }
            }

            // 加载共享资源
            string rootPath = AppContext.BaseDirectory;
            LocalizationResourceManager.LoadSharedResources(rootPath);

            // 配置请求本地化选项
            var options = new RequestLocalizationOptions()
                .AddSupportedCultures("zh-CN", "en-US")
                .AddSupportedUICultures("zh-CN", "en-US")
                .SetDefaultCulture("zh-CN");

            // 应用自定义配置
            setupAction?.Invoke(options);

            // 使用请求本地化中间件
            return app.UseRequestLocalization(options);
        }
    }
}
