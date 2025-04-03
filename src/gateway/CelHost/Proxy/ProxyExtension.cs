using CelHost.Data;

namespace CelHost.Proxy
{
    public static class ProxyExtension
    {
        public static void InitializeServiceProxy(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<HostContext>();
        }

        public static void InitializeHostServiceProxy(this WebApplication app)
        {

        }
    }
}
