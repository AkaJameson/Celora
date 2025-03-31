using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace CelHost.Comm.Client
{
    public static class ServiceProviderExtension
    {
        public static IServiceProvider UseCelHost(this IEndpointRouteBuilder routeBuilder)
        {
            routeBuilder.Map("/HealthCheck", () =>
            {

            });
        }

    }
}
