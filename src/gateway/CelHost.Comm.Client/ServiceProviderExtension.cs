using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using System.Text;

namespace CelHost.Comm.Client
{
    public static class ServiceProviderExtension
    {
        public static void UseCelHost(this IEndpointRouteBuilder routeBuilder)
        {
            routeBuilder.MapGet("/HealthCheck", () =>
            {
                return OperateResult.Successed();
            });
        }
        public static void UseCelHost(this WebApplication webApplication,Action<ProxyConfiguration> configure)
        {
            var config = new ProxyConfiguration();
            configure?.Invoke(config);
            config.Validate();
            var httpClient = new HttpClient();
            var message = new HttpRequestMessage(HttpMethod.Post, config.HostAddress)
            {
                Content = new StringContent(JsonConvert.SerializeObject(config), Encoding.UTF8, "application/json")
            };
            httpClient.SendAsync(message);
            webApplication.MapGet("/HealthCheck", () =>
            {
                return OperateResult.Successed();
            });
        }

    }
}
