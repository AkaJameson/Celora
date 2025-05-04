using CelHost.Apis.Models;
using CelHost.Models.UserInfoModels;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using System.Net.Http;
using System.Net.Http.Json;

namespace CelHost.Apis.ApiServices
{
    [Api]
    public class UserApiServices
    {
        private readonly HttpClient _httpClient;
        public UserApiServices(IHttpClientFactory clientFactory)
        {
            _httpClient = clientFactory.CreateClient("client");
        }


        public async Task<OperateResult> InitUser(IBrowserFile selectedFile)
        {
            var content = new MultipartFormDataContent();
            var stream = selectedFile.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024); // 10MB limit
            content.Add(new StreamContent(stream), "file", selectedFile.Name);
            var response = await _httpClient.PostAsync("api/User/InitUser", content);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<OperateResult>();
            }
            else
            {
                return new OperateResult();
            }
        }
        public async Task<OperateResult> Login(string account, string password)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "api/User/Login")
            {
                Content = JsonContent.Create(new LoginModel { Account = account, Password = password })
            };
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<OperateResult>();
            }
            else
            {
                return new OperateResult();
            }
        }
        public async Task<OperateResult> Logout()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "api/User/Logout");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<OperateResult>();
            }
            else
            {
                return new OperateResult();
            }
        }

        public async Task<OperateResult> CheckLogin()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "api/User/CheckLogin");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<OperateResult>();
            }
            else
            {
                return new OperateResult();
            }
        }

    }
}
