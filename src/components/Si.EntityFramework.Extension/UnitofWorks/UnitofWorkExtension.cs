using Si.EntityFramework.Extension.Data.Context;
using Si.EntityFramework.Extension.UnitofWorks.Abstractions;
using Si.EntityFramework.Extension.UnitofWorks.Implementations;

namespace Si.EntityFramework.Extension.UnitofWorks
{
    public static class UnitofWorkExtension
    {

        public static IUnitOfWork<T> GetUnitOfWork<T>(this T dbContext) where T : ApplicationDbContext
        {
            return new UnitOfWork<T>(dbContext);
        }
    }
}
