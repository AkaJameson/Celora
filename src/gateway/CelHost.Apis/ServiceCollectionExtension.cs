using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CelHost.Apis
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddAllApis(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            var apiServiceTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.GetCustomAttribute<ApiAttribute>() != null)
                .ToList();

            foreach (var implementationType in apiServiceTypes)
            {
                services.AddScoped(implementationType); // 注册为自身类型
            }

            return services;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ApiAttribute : Attribute
    {
    }
   
}
