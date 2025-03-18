using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Si.CoreHub.Package.Abstraction;

namespace Si.CoreHub.Package.Core
{
    /// <summary>
    /// 模块配置实现，为每个模块提供独立的配置访问
    /// </summary>
    /// <typeparam name="T">模块类型</typeparam>
    public class PackConfiguration<T> : IPackConfiguration<T>
    {
        private readonly IConfiguration _innerConfiguration;

        /// <summary>
        /// 初始化模块配置
        /// </summary>
        public PackConfiguration()
        {
            ModuleName = typeof(T).Assembly.GetName().Name;
            _innerConfiguration = PackLoader.GetConfiguration(ModuleName);
        }

        /// <summary>
        /// 获取或设置配置值
        /// </summary>
        /// <param name="key">配置键</param>
        /// <returns>配置值</returns>
        public string this[string key]
        {
            get => _innerConfiguration?[key];
            set
            {
                if (_innerConfiguration != null)
                {
                    _innerConfiguration[key] = value;
                }
            }
        }

        /// <summary>
        /// 获取子配置节
        /// </summary>
        /// <returns>子配置节集合</returns>
        public IEnumerable<IConfigurationSection> GetChildren()
        {
            return _innerConfiguration?.GetChildren() ?? Enumerable.Empty<IConfigurationSection>();
        }

        /// <summary>
        /// 获取重新加载令牌
        /// </summary>
        /// <returns>变更令牌</returns>
        public IChangeToken GetReloadToken()
        {
            return _innerConfiguration?.GetReloadToken() ?? new CancellationChangeToken(CancellationToken.None);
        }

        /// <summary>
        /// 获取指定配置节
        /// </summary>
        /// <param name="key">配置节名称</param>
        /// <returns>配置节</returns>
        public IConfigurationSection GetSection(string key)
        {
            return _innerConfiguration?.GetSection(key);
        }

        /// <summary>
        /// 获取模块名称
        /// </summary>
        public string ModuleName { get; }

        /// <summary>
        /// 获取指定配置节并绑定到指定类型
        /// </summary>
        /// <typeparam name="TOptions">配置类型</typeparam>
        /// <param name="sectionName">配置节名称</param>
        /// <returns>配置对象实例</returns>
        public TOptions GetOptions<TOptions>(string sectionName) where TOptions : class, new()
        {
            if (_innerConfiguration == null)
            {
                return new TOptions();
            }

            var section = _innerConfiguration.GetSection(sectionName);
            var options = new TOptions();
            section.Bind(options);
            return options;
        }

        /// <summary>
        /// 监听配置变更
        /// </summary>
        /// <param name="sectionName">配置节名称</param>
        /// <param name="onChange">变更回调</param>
        /// <returns>释放观察的操作</returns>
        public IDisposable OnChange(string sectionName, Action<IConfigurationSection> onChange)
        {
            if (_innerConfiguration == null)
            {
                return null;
            }

            var section = _innerConfiguration.GetSection(sectionName);
            var reloadToken = section.GetReloadToken();
            var registration = reloadToken.RegisterChangeCallback(state =>
            {
                var s = (IConfigurationSection)state;
                onChange?.Invoke(s);
                // 重新注册以监听后续变更
                OnChange(sectionName, onChange);
            }, section);

            return registration;
        }
    }
}
