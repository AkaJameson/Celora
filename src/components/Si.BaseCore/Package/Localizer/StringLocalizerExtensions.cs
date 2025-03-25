using Microsoft.Extensions.Localization;

namespace Si.CoreHub.Package.Localizer
{
    /// <summary>
    /// 字符串本地化器扩展方法
    /// </summary>
    public static class StringLocalizerExtensions
    {
        /// <summary>
        /// 获取共享本地化器
        /// </summary>
        public static IStringLocalizer GetSharedLocalizer(this IStringLocalizerFactory factory)
        {
            return factory.Create("shared", "shared");
        }
        
        /// <summary>
        /// 获取指定模块的本地化器
        /// </summary>
        public static IStringLocalizer ForModule(this IStringLocalizerFactory factory, string moduleName)
        {
            return factory.Create(moduleName, moduleName);
        }

        public static IStringLocalizer ForModule<T>(this IStringLocalizerFactory factory)
        {
            return factory.Create(typeof(T));
        }
    }
} 