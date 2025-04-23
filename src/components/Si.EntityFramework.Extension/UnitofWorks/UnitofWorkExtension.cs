using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Si.EntityFramework.Extension.UnitofWorks.Abstractions;
using Si.EntityFramework.Extension.UnitofWorks.Implementations;

namespace Si.EntityFramework.Extension.UnitofWorks
{
    public static class UnitofWorkExtension
    {

        public static IUnitOfWork<T> GetUnitOfWork<T>(this T dbContext) where T : DbContext
        {
            return new UnitOfWork<T>(dbContext);
        }
        public static void AddUnitofWork(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            serviceCollection.AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>));
        }
    }
}
