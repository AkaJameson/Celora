using Microsoft.Extensions.Localization;
using Si.CoreHub.Utility;
using System.Collections.Concurrent;
using System.Reflection;

namespace Si.CoreHub.Package.Localizer
{
    /// <summary>
    /// 模块字符串本地化器工厂
    /// </summary>
    public class ModuleStringLocalizerFactory : IStringLocalizerFactory
    {
        private readonly ConcurrentDictionary<string, IStringLocalizer> _localizerCache = 
            new ConcurrentDictionary<string, IStringLocalizer>();
            
        public IStringLocalizer Create(Type resourceSource)
        {
            if (resourceSource == null)
            {
                throw new ArgumentNullException(nameof(resourceSource));
            }

            var assembly = resourceSource.Assembly;
            var moduleName = assembly.GetName().Name;
            
            return _localizerCache.GetOrAdd(moduleName, _ => new ModuleStringLocalizer(moduleName));
        }
        
        public IStringLocalizer Create(string baseName, string location)
        {
            if (string.IsNullOrEmpty(location))
            {
                throw new ArgumentNullException(nameof(location));
            }
            
            return _localizerCache.GetOrAdd(location, _ => new ModuleStringLocalizer(location));
        }
    }
} 