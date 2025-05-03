using CelHost.Apis;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace CelHost.Admin
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");
            builder.Services.AddAllApis();
            builder.Services.AddBootstrapBlazor();
            var apiAddress = builder.Configuration.GetValue<string>("ApiAddress");
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiAddress) });

            await builder.Build().RunAsync();
        }
    }
}
