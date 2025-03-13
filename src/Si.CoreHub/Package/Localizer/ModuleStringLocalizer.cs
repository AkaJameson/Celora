using Microsoft.Extensions.Localization;
using System.Globalization;

namespace Si.CoreHub.Package.Localizer
{
    /// <summary>
    /// 模块字符串本地化器
    /// </summary>
    public class ModuleStringLocalizer : IStringLocalizer
    {
        private readonly string _moduleName;

        public ModuleStringLocalizer(string moduleName)
        {
            _moduleName = moduleName;
        }

        public LocalizedString this[string name]
        {
            get
            {
                string value = LocalizationResourceManager.GetLocalizedString(_moduleName, name, CultureInfo.CurrentUICulture);
                bool notFound = value == name; // 如果返回值等于key，则认为未找到
                return new LocalizedString(name, value, notFound);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                string format = LocalizationResourceManager.GetLocalizedString(_moduleName, name, CultureInfo.CurrentUICulture);
                bool notFound = format == name;
                string value = notFound ? name : string.Format(format, arguments);
                return new LocalizedString(name, value, notFound);
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            var resources = LocalizationResourceManager.GetAllStrings(_moduleName, CultureInfo.CurrentUICulture);
            return resources.Select(r => new LocalizedString(r.Key, r.Value, false));
        }
    }

    /// <summary>
    /// 泛型模块字符串本地化器
    /// </summary>
    public class ModuleStringLocalizer<T> : IStringLocalizer<T>
    {
        private readonly IStringLocalizer _localizer;

        public ModuleStringLocalizer(IStringLocalizerFactory factory)
        {
            _localizer = factory.Create(typeof(T));
        }

        public LocalizedString this[string name] => _localizer[name];

        public LocalizedString this[string name, params object[] arguments] => _localizer[name, arguments];

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => _localizer.GetAllStrings(includeParentCultures);
    }
}