using CelHost.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CelHost.Proxy.Core
{
    public static class ProxyExtension
    {
       
        public static void InitializeProxy(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<HostContext>();
            if(context == null)
            {
                throw new ArgumentNullException(nameof(HostContext));
            }


        }
    }
}
