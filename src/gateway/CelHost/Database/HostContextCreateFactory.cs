using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CelHost.Database
{
    public class HostContextCreateFactory : IDesignTimeDbContextFactory<HostContext>
    {
        public HostContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<HostContext>();
            optionsBuilder.UseSqlite("Data Source=./Database.db");

            return new HostContext(optionsBuilder.Options);
        }
    }
}
