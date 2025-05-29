using CelHost.Models.SystemDictModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Si.Utilites.OperateResult;
using Si.Utilites;
using CelHost.Server.Services;

namespace CelHost.Server.Controllers
{
    [ApiController]
    public class SystemDictionaryController : DefaultController
    {
        private readonly ISystemDictionaryService _systemDictionaryService;

        public SystemDictionaryController(ISystemDictionaryService systemDictionaryService)
        {
            _systemDictionaryService = systemDictionaryService;
        }
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("item")]
        [Authorize]
        public async Task<OperateResult> AddItem([FromBody] DictAdd model)
        {
            if (!ModelState.IsValid)
            {
                return OperateResult.Failed("参数错误");
            }
            return await _systemDictionaryService.AddItemToDictionary(model);
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public async Task<OperateResult> DeleteItem([FromBody] DeleteDict model)
        {
            if (!ModelState.IsValid)
            {
                return OperateResult.Failed("参数错误");
            }
            return await _systemDictionaryService.DeleteDataDictionary(model);
        }
        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("query")]
        [Authorize]
        public async Task<OperateResult> QueryDictionary([FromBody] DictQuery model)
        {
            if (!ModelState.IsValid)
            {
                return OperateResult.Failed("参数错误");
            }
            return await _systemDictionaryService.QueryDataDictionary(model);
        }
        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public async Task<OperateResult> UpdateItem([FromBody] DictUpdate model)
        {
            if (!ModelState.IsValid)
            {
                return OperateResult.Failed("参数错误");
            }
            return await _systemDictionaryService.UpdateDataDictionary(model);
        }
    }
}
