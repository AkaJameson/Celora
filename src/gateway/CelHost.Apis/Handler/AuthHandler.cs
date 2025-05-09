using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace CelHost.Apis.Handler
{
    public class AuthHandler : DelegatingHandler
    {
        private readonly NavigationManager _navigationManager;

        public AuthHandler(NavigationManager navigationManager)
        {
            _navigationManager = navigationManager;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || // 401
                response.StatusCode == System.Net.HttpStatusCode.Forbidden)     // 403
            {
                if (_navigationManager.Uri != _navigationManager.ToAbsoluteUri("/").ToString())
                {
                    _navigationManager.NavigateTo("/");
                }
            }

            return response;
        }
    }
}
