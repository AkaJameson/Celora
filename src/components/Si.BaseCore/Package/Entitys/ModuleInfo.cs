using System.Reflection;

namespace Si.CoreHub.Package.Entitys
{
    /// <summary>
    /// 模块信息类，包含模块的基本信息和元数据
    /// </summary>
    public class ModuleInfo
    {
        /// <summary>
        /// 初始化模块信息实例
        /// </summary>
        public ModuleInfo()
        {
        }

        /// <summary>
        /// 使用完整参数初始化模块信息实例
        /// </summary>
        /// <param name="assemblyName">程序集名称</param>
        /// <param name="assemblyPath">程序集路径</param>
        /// <param name="assembly">程序集实例</param>
        /// <param name="configFile">配置文件路径</param>
        public ModuleInfo(string assemblyName, string assemblyPath, Assembly assembly, string configFile)
        {
            AssemblyName = assemblyName ?? throw new ArgumentNullException(nameof(assemblyName));
            AssemblyPath = assemblyPath ?? throw new ArgumentNullException(nameof(assemblyPath));
            Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
            ConfigFile = configFile;
        }

        /// <summary>
        /// 模块名称
        /// </summary>
        public string AssemblyName { get; set; }
        /// <summary>
        /// 模块路径
        /// </summary>
        public string AssemblyPath { get; set; }
        /// <summary>
        /// 模块程序集
        /// </summary>
        public Assembly Assembly { get; set; }
        /// <summary>
        /// 配置文件路径
        /// </summary>
        public string ConfigFile { get; set; }

        /// <summary>
        /// 模块版本
        /// </summary>
        public string Version => Assembly?.GetName().Version?.ToString() ?? "未知版本";

        /// <summary>
        /// 模块加载时间
        /// </summary>
        public DateTime LoadTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 模块是否已加载成功
        /// </summary>
        public bool IsLoaded { get; set; }

        /// <summary>
        /// 获取格式化的模块信息
        /// </summary>
        /// <returns>格式化的模块信息字符串</returns>
        public override string ToString()
        {
            return $"模块: {AssemblyName}, 版本: {Version}, 加载状态: {(IsLoaded ? "已加载" : "未加载")}";
        }
    }
}
