using Microsoft.Extensions.DependencyModel;
using System.Reflection;

namespace Si.CoreHub.Utility
{
    /// <summary>
    /// 程序集管理器
    /// </summary>
    public static class AssemblyManager
    {
        private static readonly string[] Filters = { "dotnet-", "Microsoft.", "mscorlib", "netstandard", "System", "Windows" };
        private static Assembly[] _allAssemblies;
        private static Type[] _allTypes;

        static AssemblyManager()
        {
            AssemblyFilterFunc = name =>
            {
                return name.Name != null && !Filters.Any(m => name.Name.StartsWith(m));
            };
        }

        /// <summary>
        /// 设置 程序集过滤器
        /// </summary>
        public static Func<AssemblyName, bool> AssemblyFilterFunc { private get; set; }

        /// <summary>
        /// 获取 所有程序集
        /// </summary>
        public static Assembly[] AllAssemblies
        {
            get
            {
                if (_allAssemblies == null)
                {
                    Init();
                }

                return _allAssemblies;
            }
        }

        /// <summary>
        /// 获取 所有类型
        /// </summary>
        public static Type[] AllTypes
        {
            get
            {
                if (_allTypes == null)
                {
                    Init();
                }

                return _allTypes;
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public static void Init()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve!;
            _allAssemblies = DependencyContext.Default.GetDefaultAssemblyNames()
                  .Where(AssemblyFilterFunc).Select(Assembly.Load).ToArray();
            _allTypes = _allAssemblies.SelectMany(m => m.GetTypes()).ToArray();
        }
        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {

            var assemblyPath = Path.Combine("../", AppDomain.CurrentDomain.BaseDirectory, "dll");
            if (File.Exists(assemblyPath))
            {
                // 从文件加载程序集
                return Assembly.LoadFrom(assemblyPath);
            }
            assemblyPath = AppDomain.CurrentDomain.BaseDirectory;
            if (File.Exists(assemblyPath))
            {
                return Assembly.LoadFrom(assemblyPath);
            }
            return null;
        }
        /// <summary>
        /// 查找指定条件的类型
        /// </summary>
        public static Type[] FindTypes(Func<Type, bool> predicate)
        {
            return AllTypes.Where(predicate).ToArray();
        }

        /// <summary>
        /// 查找指定基类的实现类型
        /// </summary>
        public static Type[] FindTypesByBase<TBaseType>()
        {
            Type baseType = typeof(TBaseType);
            return FindTypesByBase(baseType);
        }

        /// <summary>
        /// 查找指定基类的实现类型
        /// </summary>
        public static Type[] FindTypesByBase(Type baseType)
        {
            return AllTypes.Where(type => baseType.IsAssignableFrom(type)).Distinct().ToArray();
        }

        /// <summary>
        /// 查找指定Attribute特性的实现类型
        /// </summary>
        public static Type[] FindTypesByAttribute<TAttribute>(bool inherit = true)
        {
            Type attributeType = typeof(TAttribute);
            return FindTypesByAttribute(attributeType, inherit);
        }

        /// <summary>
        /// 查找指定Attribute特性的实现类型
        /// </summary>
        public static Type[] FindTypesByAttribute(Type attributeType, bool inherit = true)
        {
            return AllTypes.Where(type => type.IsDefined(attributeType, inherit)).Distinct().ToArray();
        }
    }
}
