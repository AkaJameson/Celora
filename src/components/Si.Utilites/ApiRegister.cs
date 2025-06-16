using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Si.Utilites
{
    public enum ServiceLifetimeOption
    {
        Singleton,
        Scoped,
        Transient
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ApiAttribute : Attribute
    {
        public ServiceLifetimeOption Lifetime { get; }
        public bool RegisterAsGeneric { get; }
        public bool RegisterAsInterface { get; }

        public ApiAttribute(ServiceLifetimeOption lifetime = ServiceLifetimeOption.Scoped,
                            bool registerAsGeneric = false,
                            bool registerAsInterface = true)
        {
            Lifetime = lifetime;
            RegisterAsGeneric = registerAsGeneric;
            RegisterAsInterface = registerAsInterface;
        }

    }
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAllApisByScan(this IServiceCollection services)
        {
            var apiTypes = AssemblyManager.FindTypesByAttribute<ApiAttribute>();

            foreach (var implementationType in apiTypes)
            {
                var attr = implementationType.GetCustomAttribute<ApiAttribute>();
                if (attr == null) continue;

                var serviceTypes = new List<Type>();

                if (attr.RegisterAsInterface)
                {
                    serviceTypes.AddRange(implementationType.GetInterfaces().Where(i => i != typeof(IDisposable)));
                }
                else
                {
                    serviceTypes.Add(implementationType);
                }

                if (!serviceTypes.Any())
                {
                    serviceTypes.Add(implementationType);
                }

                foreach (var serviceType in serviceTypes)
                {
                    RegisterService(services, serviceType, implementationType, attr);
                }
            }

            return services;
        }

        private static void RegisterService(IServiceCollection services, Type serviceType, Type implementationType, ApiAttribute attr)
        {
            var isGeneric = implementationType.IsGenericTypeDefinition;

            if (isGeneric && !attr.RegisterAsGeneric)
                return;

            switch (attr.Lifetime)
            {
                case ServiceLifetimeOption.Singleton:
                    if (isGeneric)
                        services.AddSingleton(serviceType, implementationType);
                    else
                        services.AddSingleton(serviceType, implementationType);
                    break;

                case ServiceLifetimeOption.Scoped:
                    if (isGeneric)
                        services.AddScoped(serviceType, implementationType);
                    else
                        services.AddScoped(serviceType, implementationType);
                    break;

                case ServiceLifetimeOption.Transient:
                    if (isGeneric)
                        services.AddTransient(serviceType, implementationType);
                    else
                        services.AddTransient(serviceType, implementationType);
                    break;
            }
        }
    }
}
