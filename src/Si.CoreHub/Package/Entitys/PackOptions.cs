namespace Si.CoreHub.Package.Entitys
{
    /// <summary>
    /// 模块加载选项配置
    /// </summary>
    public class PackOptions
    {
        /// <summary>
        /// 初始化模块加载选项
        /// </summary>
        public PackOptions()
        {
        }

        /// <summary>
        /// 使用指定路径初始化模块加载选项
        /// </summary>
        /// <param name="filePath">模块文件夹路径</param>
        public PackOptions(string filePath)
        {
            FilePath = filePath;
        }

        /// <summary>
        /// 模块文件夹位置
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// 是否启用热加载（运行时监控模块变化）
        /// </summary>
        public bool EnableHotReload { get; set; } = false;

        /// <summary>
        /// 是否忽略加载失败的模块
        /// </summary>
        public bool IgnoreFailedModules { get; set; } = true;

        /// <summary>
        /// 是否预加载所有模块（否则延迟到使用时再加载）
        /// </summary>
        public bool PreloadAllModules { get; set; } = true;

        /// <summary>
        /// 是否启用本地化支持
        /// </summary>
        public bool EnableLocaizor { get; set; } = false;

        /// <summary>
        /// 模块配置文件名称格式
        /// 默认为 {模块名}.json
        /// </summary>
        public string ConfigurationFileFormat { get; set; } = "{0}.json";

        /// <summary>
        /// 模块程序集文件名称格式
        /// 默认为 {模块名}.dll
        /// </summary>
        public string AssemblyFileFormat { get; set; } = "{0}.dll";

       
    }
}
