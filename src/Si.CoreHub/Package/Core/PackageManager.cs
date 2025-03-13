using Si.CoreHub.Logging;
using Si.CoreHub.Package.Entitys;
using System.Reflection;

namespace Si.CoreHub.Package.Core
{
    /// <summary>
    /// 模块管理器，负责发现和管理模块
    /// </summary>
    public class PackageManager
    {
        private readonly PackOptions _options;
        private readonly List<ModuleInfo> _modules = new List<ModuleInfo>();
        private readonly object _modulesLock = new object();
        private bool _initialized = false;

        /// <summary>
        /// 初始化模块管理器
        /// </summary>
        /// <param name="options">模块加载选项</param>
        public PackageManager(PackOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            
            // 注册程序集解析事件以支持模块依赖项解析
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
            
            // 使用日志中心写入日志
        }

        public PackOptions GetPackOptions()
        {
            return _options;
        }
        /// <summary>
        /// 模块加载事件
        /// </summary>
        public event EventHandler<ModuleInfo> ModuleLoaded;

        /// <summary>
        /// 获取所有已发现的模块信息
        /// </summary>
        /// <returns>模块信息列表</returns>
        /// <exception cref="DirectoryNotFoundException">模块目录不存在时抛出</exception>
        public List<ModuleInfo> GetPacks()
        {
            if (!_initialized)
            {
                DiscoverModules();
            }

            lock (_modulesLock)
            {
                // 返回模块列表的副本
                return _modules.ToList();
            }
        }

        /// <summary>
        /// 查找并加载模块
        /// </summary>
        /// <exception cref="DirectoryNotFoundException">模块目录不存在时抛出</exception>
        public void DiscoverModules()
        {
            lock (_modulesLock)
            {
                if (_initialized)
                {
                    return;
                }

                if (string.IsNullOrEmpty(_options.FilePath) || !Directory.Exists(_options.FilePath))
                {
                    LogCenter.Write2Log(Loglevel.Error, $"指定的模块路径 {_options.FilePath} 不存在");
                    throw new DirectoryNotFoundException($"指定的模块路径 {_options.FilePath} 不存在");
                }

                var moduleDirs = Directory.GetDirectories(_options.FilePath);
                LogCenter.Write2Log(Loglevel.Info, $"发现 {moduleDirs.Length} 个可能的模块目录");

                foreach (var moduleDir in moduleDirs)
                {
                    try
                    {
                        LoadModuleFromDirectory(moduleDir);
                    }
                    catch (Exception ex)
                    {
                        LogCenter.Write2Log(Loglevel.Error, $"加载模块目录 {moduleDir} 失败: {ex.Message}");
                        
                        if (!_options.IgnoreFailedModules)
                        {
                            throw;
                        }
                    }
                }

                _initialized = true;
                LogCenter.Write2Log(Loglevel.Info, $"成功加载 {_modules.Count} 个模块");
            }
        }

        /// <summary>
        /// 加载特定目录中的模块
        /// </summary>
        /// <param name="moduleDir">模块目录</param>
        private void LoadModuleFromDirectory(string moduleDir)
        {
            string moduleName = Path.GetFileName(moduleDir);
            string assemblyFileName = string.Format(_options.AssemblyFileFormat, moduleName);
            string assemblyPath = Path.Combine(moduleDir, assemblyFileName);
            string configFileName = string.Format(_options.ConfigurationFileFormat, moduleName);
            string configFilePath = Path.Combine(moduleDir, configFileName);

            if (!File.Exists(assemblyPath))
            {
                LogCenter.Write2Log(Loglevel.Warning, $"模块 {moduleName} 的程序集文件不存在: {assemblyPath}");
                return;
            }

            try
            {
                Assembly assembly = Assembly.LoadFrom(assemblyPath);
                
                // 创建模块信息
                var moduleInfo = new ModuleInfo
                {
                    AssemblyName = moduleName,
                    AssemblyPath = assemblyPath,
                    Assembly = assembly,
                    ConfigFile = File.Exists(configFilePath) ? configFilePath : null,
                    LoadTime = DateTime.Now,
                    IsLoaded = true
                };

                // 添加到模块列表
                lock (_modulesLock)
                {
                    _modules.Add(moduleInfo);
                }

                LogCenter.Write2Log(Loglevel.Info, $"成功加载模块: {moduleInfo}");
                
                // 触发模块加载事件
                ModuleLoaded?.Invoke(this, moduleInfo);
            }
            catch (Exception ex)
            {
                LogCenter.Write2Log(Loglevel.Error, $"加载模块 {moduleName} 失败: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// 处理程序集解析事件，查找依赖的程序集
        /// </summary>
        private Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            lock (_modulesLock)
            {
                foreach (var moduleInfo in _modules)
                {
                    try
                    {
                        string fileName = new AssemblyName(args.Name).Name + ".dll";
                        string baseDir = Path.GetDirectoryName(moduleInfo.AssemblyPath);
                        string assemblyPath = Path.Combine(baseDir, fileName);

                        if (File.Exists(assemblyPath))
                        {
                            return Assembly.LoadFrom(assemblyPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogCenter.Write2Log(Loglevel.Error, $"解析程序集 {args.Name} 失败: {ex.Message}");
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 卸载模块管理器，释放资源
        /// </summary>
        public void Unload()
        {
            // 移除程序集解析事件
            AppDomain.CurrentDomain.AssemblyResolve -= OnAssemblyResolve;
            
            // 清空模块列表
            lock (_modulesLock)
            {
                _modules.Clear();
                _initialized = false;
            }
            
            LogCenter.Write2Log(Loglevel.Info, "模块管理器已卸载");
        }

        /// <summary>
        /// 获取特定名称的模块
        /// </summary>
        /// <param name="moduleName">模块名称</param>
        /// <returns>模块信息，未找到返回null</returns>
        public ModuleInfo GetModule(string moduleName)
        {
            if (!_initialized)
            {
                DiscoverModules();
            }

            lock (_modulesLock)
            {
                return _modules.FirstOrDefault(m => string.Equals(m.AssemblyName, moduleName, StringComparison.OrdinalIgnoreCase));
            }
        }
    }
}
