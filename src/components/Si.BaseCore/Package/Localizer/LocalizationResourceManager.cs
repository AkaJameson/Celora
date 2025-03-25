using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using Si.CoreHub.Logging;
using Si.CoreHub.Package.Core;
using Si.CoreHub.Package.Entitys;
using System.Collections.Concurrent;
using System.Globalization;

namespace Si.CoreHub.Package.Localizer
{
    /// <summary>
    /// 本地化资源管理器
    /// </summary>
    public class LocalizationResourceManager
    {
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, Dictionary<string, string>>> _resourcesCache = 
            new ConcurrentDictionary<string, ConcurrentDictionary<string, Dictionary<string, string>>>();
        
        private const string SHARED_MODULE_NAME = "shared";
        
        /// <summary>
        /// 加载模块的本地化资源
        /// </summary>
        public static void LoadModuleResources(ModuleInfo moduleInfo)
        {
            if (moduleInfo?.Assembly == null)
            {
                return;
            }

            string moduleName = moduleInfo.Assembly.GetName().Name;
            string resourcesPath = Path.Combine(Path.GetDirectoryName(moduleInfo.AssemblyPath), "Resources");
            
            if (!Directory.Exists(resourcesPath))
            {
                return;
            }

            var moduleDictionary = _resourcesCache.GetOrAdd(moduleName, _ => new ConcurrentDictionary<string, Dictionary<string, string>>());
            
            // 寻找所有JSON资源文件，约定文件名格式为：{模块名称}-{文化缩写}.json，如 ModuleA-zh-CN.json
            var resourceFiles = Directory.GetFiles(resourcesPath, $"{moduleName}.*.json");
            foreach (var resourceFile in resourceFiles)
            {
                try
                {
                    string fileName = Path.GetFileNameWithoutExtension(resourceFile);
                    string cultureName = fileName.Substring(moduleName.Length + 1); // 获取文化名称
                    
                    string json = File.ReadAllText(resourceFile);
                    var resourceDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    
                    if (resourceDictionary != null)
                    {
                        moduleDictionary[cultureName] = resourceDictionary;
                    }
                }
                catch (Exception ex)
                {
                    // 记录日志但不中断处理
                    LogCenter.Write2Log(Loglevel.Error,$"加载本地化资源失败: {resourceFile}, 错误: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// 加载共享本地化资源
        /// </summary>
        public static void LoadSharedResources(string rootPath)
        {
            string resourcesPath = Path.Combine(rootPath, "Resources");
            
            if (!Directory.Exists(resourcesPath))
            {
                return;
            }

            var sharedDictionary = _resourcesCache.GetOrAdd(SHARED_MODULE_NAME, _ => new ConcurrentDictionary<string, Dictionary<string, string>>());
            
            // 寻找所有共享JSON资源文件，约定文件名格式为：shared-{文化缩写}.json
            var resourceFiles = Directory.GetFiles(resourcesPath, "shared-*.json");
            foreach (var resourceFile in resourceFiles)
            {
                try
                {
                    string fileName = Path.GetFileNameWithoutExtension(resourceFile);
                    string cultureName = fileName.Substring(SHARED_MODULE_NAME.Length + 1); // 获取文化名称
                    
                    string json = File.ReadAllText(resourceFile);
                    var resourceDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    
                    if (resourceDictionary != null)
                    {
                        sharedDictionary[cultureName] = resourceDictionary;
                    }
                }
                catch (Exception ex)
                {
                    // 记录日志但不中断处理
                    Console.WriteLine($"加载共享本地化资源失败: {resourceFile}, 错误: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// 获取模块的本地化资源
        /// </summary>
        public static string GetLocalizedString(string moduleName, string key, CultureInfo culture)
        {
            // 首先查找模块特定资源
            if (_resourcesCache.TryGetValue(moduleName, out var moduleResources))
            {
                if (TryGetLocalizedString(moduleResources, key, culture, out string value))
                {
                    return value;
                }
            }
            
            // 如果模块资源中未找到，查找共享资源
            if (moduleName != SHARED_MODULE_NAME && _resourcesCache.TryGetValue(SHARED_MODULE_NAME, out var sharedResources))
            {
                if (TryGetLocalizedString(sharedResources, key, culture, out string value))
                {
                    return value;
                }
            }
            
            // 如果未找到任何翻译，返回key本身作为默认值
            return key;
        }
        
        /// <summary>
        /// 尝试获取本地化字符串
        /// </summary>
        private static bool TryGetLocalizedString(ConcurrentDictionary<string, Dictionary<string, string>> resources, 
            string key, CultureInfo culture, out string value)
        {
            value = null;
            
            // 尝试使用完整文化名称
            if (resources.TryGetValue(culture.Name, out var resourceDict) && 
                resourceDict.TryGetValue(key, out value))
            {
                return true;
            }
            
            // 尝试使用基础文化名称
            if (culture.Name != culture.TwoLetterISOLanguageName && 
                resources.TryGetValue(culture.TwoLetterISOLanguageName, out resourceDict) && 
                resourceDict.TryGetValue(key, out value))
            {
                return true;
            }
            
            // 尝试回退到父文化
            if (culture.Parent != CultureInfo.InvariantCulture && culture.Parent != null)
            {
                return TryGetLocalizedString(resources, key, culture.Parent, out value);
            }
            
            return false;
        }
        
        /// <summary>
        /// 获取模块的所有本地化字符串
        /// </summary>
        public static Dictionary<string, string> GetAllStrings(string moduleName, CultureInfo culture)
        {
            var result = new Dictionary<string, string>();
            
            // 共享资源
            if (_resourcesCache.TryGetValue(SHARED_MODULE_NAME, out var sharedResources))
            {
                if (sharedResources.TryGetValue(culture.Name, out var cultureStrings))
                {
                    foreach (var pair in cultureStrings)
                    {
                        result[pair.Key] = pair.Value;
                    }
                }
            }
            
            // 模块特定资源（覆盖共享资源中的同名项）
            if (_resourcesCache.TryGetValue(moduleName, out var moduleResources))
            {
                if (moduleResources.TryGetValue(culture.Name, out var cultureStrings))
                {
                    foreach (var pair in cultureStrings)
                    {
                        result[pair.Key] = pair.Value;
                    }
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// 清除缓存
        /// </summary>
        public static void ClearCache()
        {
            _resourcesCache.Clear();
        }

       
    }
} 