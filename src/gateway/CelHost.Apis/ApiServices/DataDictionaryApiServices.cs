using CelHost.Apis.Models;
using CelHost.Models.SystemDictModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CelHost.Apis.ApiServices
{
    [Api]
    public class DataDictionaryApiServices
    {
        private readonly HttpClient _httpClient;
        public DataDictionaryApiServices(IHttpClientFactory clientFactory)
        {
            _httpClient = clientFactory.CreateClient("client");
        }
        /// <summary>
        /// 查询数据字典
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<OperateResult<SystemDictDto>> GetDataDictionary(DictQuery query)
        {
            var httpMessage = new HttpRequestMessage(HttpMethod.Post, "/api/SystemDictionary/query")
            {
                Content = new StringContent(JsonConvert.SerializeObject(query))
            };
            var response = await _httpClient.SendAsync(httpMessage);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<OperateResult<SystemDictDto>>(result);
            }
            else
            {
                return new OperateResult<SystemDictDto>();
            }
        }
        /// <summary>
        /// 添加数据字典项
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<OperateResult> AddItem(DictAdd item)
        {
            var httpMessage = new HttpRequestMessage(HttpMethod.Post, "/api/SystemDictionary/item")
            {
                Content = new StringContent(JsonConvert.SerializeObject(item))
            };
            var response = await _httpClient.SendAsync(httpMessage);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<OperateResult>(result);
            }
            else
            {
                return new OperateResult();
            }
        }
        /// <summary>
        /// 更新数据字典项
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<OperateResult> UpdateItem(DictUpdate item)
        {
            var httpMessage = new HttpRequestMessage(HttpMethod.Post, "/api/SystemDictionary/UpdateItem");
            httpMessage.Content = new StringContent(JsonConvert.SerializeObject(item));
            var response = await _httpClient.SendAsync(httpMessage);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<OperateResult>(result);
            }
            else
            {
                return new OperateResult();
            }
        }
        /// <summary>
        /// 删除数据字典项
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<OperateResult> DeleteItem(DeleteDict item)
        {
            var httpMessage = new HttpRequestMessage(HttpMethod.Post, "/api/SystemDictionary/DeleteItem");
            httpMessage.Content = new StringContent(JsonConvert.SerializeObject(item));
            var response = await _httpClient.SendAsync(httpMessage);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<OperateResult>(result);
            }
            else
            {
                return new OperateResult();
            }
        }

    }
}
