using CelHost.Database;
using CelHost.Proxy.Abstraction;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Yarp.ReverseProxy.Configuration;

namespace CelHost.Proxy
{
    public static class ProxyInjection
    {
        public static void AddProxyInjection(this WebApplicationBuilder builder)
        {
            var iConfiguration = builder.Configuration;
            var connectionstring = iConfiguration.GetConnectionString("DefaultConnection");
            builder.Services.AddReverseProxy().LoadFromMemory(new List<RouteConfig>(), new List<ClusterConfig>());
            var dbContextOptions = new DbContextOptionsBuilder<HostContext>()
                .UseSqlite(connectionstring)
                .Options;
            using var dbContext = new HostContext(dbContextOptions);
            var rateLimitPolicies = dbContext.RateLimitPolicies.ToList();
            var rateLimitPolicyDict = rateLimitPolicies.ToDictionary(k => k.PolicyName, v => v);
           
                builder.Services.AddRateLimiter(options =>
                {
                    foreach (var item in rateLimitPolicyDict)
                    {
                        options.AddFixedWindowLimiter(item.Key, config =>
                        {
                            config.PermitLimit = item.Value.PermitLimit;
                            config.Window = TimeSpan.FromSeconds(item.Value.Window);
                            config.QueueProcessingOrder = item.Value.QueueProcessingOrder;
                            config.QueueLimit = item.Value.QueueLimit;
                        });
                    }
                });
            builder.Services.AddScoped<IProxyLoader, ProxyLoader>();
            builder.Services.AddScoped<IProxyManager, ProxyManager>();
        }
        public static void UseProxy(this WebApplication app)
        {
            using var sp = app.Services.CreateScope();
            sp.ServiceProvider.GetRequiredService<IProxyManager>().UpdateProxyConfig();
            app.MapReverseProxy();
        }
    }
}
