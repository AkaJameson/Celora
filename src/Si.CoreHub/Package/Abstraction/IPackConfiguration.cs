using Microsoft.Extensions.Configuration;

namespace Si.CoreHub.Package.Abstraction
{
    /// <summary>
    /// 模块配置接口，提供对模块特定配置的访问
    /// </summary>
    /// <typeparam name="T">模块类型，用于定位模块配置位置</typeparam>
    public interface IPackConfiguration<T> : IConfiguration
    {
        /// <summary>
        /// 获取指定配置节并绑定到指定类型
        /// </summary>
        /// <typeparam name="TOptions">配置类型</typeparam>
        /// <param name="sectionName">配置节名称</param>
        /// <returns>配置对象实例</returns>
        TOptions GetOptions<TOptions>(string sectionName) where TOptions : class, new();

        /// <summary>
        /// 获取模块名称
        /// </summary>
        string ModuleName { get; }

        /// <summary>
        /// 监听配置变更
        /// </summary>
        /// <param name="sectionName">配置节名称</param>
        /// <param name="onChange">变更回调</param>
        /// <returns>释放观察的操作</returns>
        IDisposable OnChange(string sectionName, Action<IConfigurationSection> onChange);
    }
}
