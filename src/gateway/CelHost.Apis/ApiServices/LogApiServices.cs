using CelHost.Apis.Models;
using System.Net.Http.Json;

namespace CelHost.Apis.ApiServices
{
    [Api]
    public class LogApiServices
    {
        private readonly HttpClient _httpClient;
        public LogApiServices(IHttpClientFactory clientFactory)
        {
            _httpClient = clientFactory.CreateClient("client");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<List<FileInfoDto>> GetFileListAsync(string path = "", int pageIndex = 1, int pageSize = 10)
        {
            string url = $"api/files/list?path={Uri.EscapeDataString(path)}&page={pageIndex}&pageSize={pageSize}";
            return await _httpClient.GetFromJsonAsync<List<FileInfoDto>>(url);
        }
        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="relativePath"></param>
        /// <returns></returns>
        public async Task<byte[]> DownloadFileAsync(string relativePath)
        {
            string url = $"api/files/download?filePath={Uri.EscapeDataString(relativePath)}";
            return await _httpClient.GetByteArrayAsync(url);
        }


    }
}
