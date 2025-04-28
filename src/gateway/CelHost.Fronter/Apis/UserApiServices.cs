using CelHost.Fronter.Apis.Models;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Json;

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
    }
}
