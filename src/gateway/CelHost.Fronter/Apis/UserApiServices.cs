using CelHost.Fronter.Apis.Models;
using CelHost.Models.UserInfoModels;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using System.Net.Http.Json;
using System.Security.Principal;

namespace CelHost.Fronter.Apis
{
    public class UserApiServices
    {
        private readonly HttpClient _httpClient;
        public UserApiServices(HttpClient httpClient)
        {
            _httpClient = httpClient;
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
                return null;
            }
        }
        public async Task<OperateResult> Login(string account,string password)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "api/User/Login")
            {
                Content = JsonContent.Create(new LoginModel { Account = account, Password = password })
            };
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<OperateResult>();
            }
            else
            {
                return null;
            }
        }
        public async Task<OperateResult> Logout()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "api/User/Logout");
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<OperateResult>();
            }
            else
            {
                return null;
            }
        }

    }
}
