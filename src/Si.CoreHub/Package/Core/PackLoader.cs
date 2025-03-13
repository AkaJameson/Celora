using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Si.CoreHub.Logging;
using Si.CoreHub.Package.Entitys;
using System.Collections.Concurrent;

namespace Si.CoreHub.Package.Core
{
    /// <summary>
    /// 模块加载器，负责加载模块和管理模块的生命周期
    /// </summary>
    public static class PackLoader
    {
        /// <summary>
        /// 已加载的模块实例集合
        /// </summary>
        private static readonly ConcurrentDictionary<string, PackBase> _packInstances = new ConcurrentDictionary<string, PackBase>();
        
        /// <summary>
        /// 模块配置集合
        /// </summary>
        private static readonly ConcurrentDictionary<string, IConfiguration> _packConfigurations = new ConcurrentDictionary<string, IConfiguration>();

        /// <summary>
        /// 加载模块到应用程序
        /// </summary>
        /// <param name="builder">Web应用程序构建器</param>
        /// <param name="moduleInfo">模块信息</param>
        public static void LoadPack(WebApplicationBuilder builder, ModuleInfo moduleInfo)
        {
            if (moduleInfo?.Assembly == null)
            {
                LogCenter.Write2Log(Loglevel.Warning, "尝试加载无效的模块");
                return;
            }

            try
            {
                // 查找模块中继承自PackBase的类型
                var packType = moduleInfo.Assembly.GetTypes()
                    .FirstOrDefault(t => !t.IsAbstract && typeof(PackBase).IsAssignableFrom(t));

                if (packType == null)
                {
                    LogCenter.Write2Log(Loglevel.Warning, $"模块 {moduleInfo.AssemblyName} 中未找到继承自PackBase的类型");
                    return;
                }

                // 创建模块实例
                var packInstance = (PackBase)Activator.CreateInstance(packType);
                if (packInstance == null)
                {
                    LogCenter.Write2Log(Loglevel.Error, $"无法创建模块 {moduleInfo.AssemblyName} 的实例");
                    return;
                }

                // 初始化模块
                packInstance.Initialize();

                // 将模块实例添加到集合中
                string moduleName = moduleInfo.Assembly.GetName().Name;
                _packInstances.TryAdd(moduleName, packInstance);

                // 注册模块程序集到MVC
                var partManager = builder.Services.AddMvcCore();
                partManager.AddApplicationPart(moduleInfo.Assembly);

                // 加载模块配置
                LoadModuleConfiguration(moduleInfo);

                // 调用模块的服务配置方法
                packInstance.ConfigurationServices(builder, builder.Services);

                /*// 预加载模块的本地化资源
                LocalizerManager.LoadModuleResources(moduleInfo);*/

                LogCenter.Write2Log(Loglevel.Info, $"成功加载模块 {moduleInfo.AssemblyName}");
                moduleInfo.IsLoaded = true;
            }
            catch (Exception ex)
            {
                LogCenter.Write2Log(Loglevel.Error, $"加载模块 {moduleInfo.AssemblyName} 失败: {ex.Message}\n{ex.StackTrace}");
                moduleInfo.IsLoaded = false;
            }
        }

        /// <summary>
        /// 加载模块配置
        /// </summary>
        /// <param name="moduleInfo">模块信息</param>
        private static void LoadModuleConfiguration(ModuleInfo moduleInfo)
        {
            try
            {
                string moduleName = moduleInfo.Assembly.GetName().Name;

                // 如果配置文件不存在，则创建空配置
                if (!File.Exists(moduleInfo.ConfigFile))
                {
                    _packConfigurations.TryAdd(moduleName, new ConfigurationBuilder().Build());
                    LogCenter.Write2Log(Loglevel.Warning, $"模块 {moduleInfo.AssemblyName} 的配置文件不存在，使用空配置");
                    return;
                }

                // 加载配置文件
                var configBuilder = new ConfigurationBuilder()
                    .AddJsonFile(moduleInfo.ConfigFile, optional: true, reloadOnChange: true);
                var configuration = configBuilder.Build();

                // 注册配置
                RegisterConfiguration(moduleName, configuration);
                LogCenter.Write2Log(Loglevel.Info, $"加载模块 {moduleInfo.AssemblyName} 的配置文件成功");
            }
            catch (Exception ex)
            {
                LogCenter.Write2Log(Loglevel.Error, $"加载模块 {moduleInfo.AssemblyName} 的配置文件失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 使用模块中间件和路由
        /// </summary>
        /// <param name="app">应用程序构建器</param>
        /// <param name="serviceProvider">服务提供者</param>
        /// <param name="routes">路由构建器</param>
        public static void UsePack(IApplicationBuilder app, IServiceProvider serviceProvider, IEndpointRouteBuilder routes)
        {
            // 调用所有模块的Configuration方法
            foreach (var packInstance in _packInstances.Values)
            {
                try
                {
                    packInstance.Configuration(app, routes, serviceProvider);
                    LogCenter.Write2Log(Loglevel.Info, $"配置模块 {packInstance.Name} 成功");
                }
                catch (Exception ex)
                {
                    LogCenter.Write2Log(Loglevel.Error, $"配置模块 {packInstance.Name} 失败: {ex.Message}");
                }
            }

            // 启动所有模块
            foreach (var packInstance in _packInstances.Values)
            {
                try
                {
                    packInstance.Startup(serviceProvider);
                    LogCenter.Write2Log(Loglevel.Info, $"启动模块 {packInstance.Name} 成功");
                }
                catch (Exception ex)
                {
                    LogCenter.Write2Log(Loglevel.Error, $"启动模块 {packInstance.Name} 失败: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 获取模块配置
        /// </summary>
        /// <param name="packName">模块名称</param>
        /// <returns>模块配置</returns>
        public static IConfiguration GetConfiguration(string packName)
        {
            if (string.IsNullOrEmpty(packName))
            {
                return null;
            }

            _packConfigurations.TryGetValue(packName, out IConfiguration configuration);
            return configuration;
        }

        /// <summary>
        /// 注册模块配置
        /// </summary>
        /// <param name="packName">模块名称</param>
        /// <param name="configuration">配置</param>
        public static void RegisterConfiguration(string packName, IConfiguration configuration)
        {
            if (string.IsNullOrEmpty(packName) || configuration == null)
            {
                return;
            }

            _packConfigurations.TryAdd(packName, configuration);
        }

        /// <summary>
        /// 获取已加载模块实例
        /// </summary>
        /// <param name="packName">模块名称</param>
        /// <returns>模块实例，未找到返回null</returns>
        public static PackBase GetPackInstance(string packName)
        {
            if (string.IsNullOrEmpty(packName))
            {
                return null;
            }

            _packInstances.TryGetValue(packName, out PackBase packInstance);
            return packInstance;
        }

        /// <summary>
        /// 获取所有已加载的模块实例
        /// </summary>
        /// <returns>模块实例集合</returns>
        public static IEnumerable<PackBase> GetAllPackInstances()
        {
            return _packInstances.Values;
        }

        /// <summary>
        /// 关闭所有模块
        /// </summary>
        public static void ShutdownAllPacks()
        {
            foreach (var packInstance in _packInstances.Values)
            {
                try
                {
                    packInstance.Shutdown();
                    LogCenter.Write2Log(Loglevel.Info, $"关闭模块 {packInstance.Name} 成功");
                }
                catch (Exception ex)
                {
                    LogCenter.Write2Log(Loglevel.Error, $"关闭模块 {packInstance.Name} 失败: {ex.Message}");
                }
            }

            _packInstances.Clear();
            _packConfigurations.Clear();
        }
    }
}
