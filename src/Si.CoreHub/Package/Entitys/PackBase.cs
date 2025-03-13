using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Si.CoreHub.Package.Abstraction;

namespace Si.CoreHub.Package.Entitys
{
    /// <summary>
    /// 模块基类，所有模块必须继承此类
    /// </summary>
    public abstract class PackBase
    {
        /// <summary>
        /// 模块名称
        /// </summary>
        public virtual string Name => GetType().Assembly.GetName().Name;

        /// <summary>
        /// 模块版本
        /// </summary>
        public virtual string Version => GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0";

        /// <summary>
        /// 模块描述
        /// </summary>
        public virtual string Description => "";

        /// <summary>
        /// 模块作者
        /// </summary>
        public virtual string Author => "";

        /// <summary>
        /// 模块初始化，此时模块正在准备加载
        /// </summary>
        public virtual void Initialize() { }

        /// <summary>
        /// 配置服务，注册模块所需的服务
        /// </summary>
        /// <param name="builder">Web应用程序构建器</param>
        /// <param name="services">服务集合</param>
        public abstract void ConfigurationServices(WebApplicationBuilder builder, IServiceCollection services);

        /// <summary>
        /// 配置应用程序，注册模块的中间件和路由
        /// </summary>
        /// <param name="app">应用程序构建器</param>
        /// <param name="routes">路由构建器</param>
        /// <param name="serviceProvider">服务提供者</param>
        public abstract void Configuration(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider);

        /// <summary>
        /// 模块启动，此时所有模块都已加载完成
        /// </summary>
        /// <param name="serviceProvider">服务提供者</param>
        public virtual void Startup(IServiceProvider serviceProvider) { }

        /// <summary>
        /// 模块停止，应用程序正在关闭
        /// </summary>
        public virtual void Shutdown() { }

        /// <summary>
        /// 获取模块配置
        /// </summary>
        /// <param name="configSection">配置节点名称</param>
        /// <returns>配置节</returns>
        protected IConfiguration GetConfiguration(string configSection = null)
        {
            var config = Si.CoreHub.Package.Core.PackLoader.GetConfiguration(Name);
            if (string.IsNullOrEmpty(configSection))
            {
                return config;
            }
            return config?.GetSection(configSection);
        }

        /// <summary>
        /// 获取模块信息
        /// </summary>
        /// <returns>模块信息字符串</returns>
        public override string ToString()
        {
            return $"模块: {Name}, 版本: {Version}, 描述: {Description}, 作者: {Author}";
        }
    }
}
