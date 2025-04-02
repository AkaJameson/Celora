using Microsoft.EntityFrameworkCore;
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
    }
}
