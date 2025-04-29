using CelHost.Models.SystemDictModels;
using CelHost.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Si.Utilites.OperateResult;
using Si.Utilites;

namespace CelHost.Controllers
{
    [ApiController]
    public class SystemDictionaryController : DefaultController
    {
        private readonly ISystemDictionaryServiceImpl _systemDictionaryService;

        public SystemDictionaryController(ISystemDictionaryServiceImpl systemDictionaryService)
        {
            _systemDictionaryService = systemDictionaryService;
        }

        [HttpPost("item")]
        [Authorize]
        public async Task<OperateResult> AddItem([FromBody] DictAddItemModel model)
        {
            if (!ModelState.IsValid)
            {
                return OperateResult.Failed("参数错误");
            }
            return await _systemDictionaryService.AddItem(model);
        }

        [HttpPost("new-item")]
        [Authorize]
        public async Task<OperateResult> AddNewItem([FromBody] DictAddNewItemModel model)
        {
            if (!ModelState.IsValid)
            {
                return OperateResult.Failed("参数错误");
            }
            return await _systemDictionaryService.AddNewItem(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<OperateResult> DeleteItem([FromBody] DeleteItemModel model)
        {
            if (!ModelState.IsValid)
            {
                return OperateResult.Failed("参数错误");
            }
            return await _systemDictionaryService.DeleteItem(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<OperateResult> DeleteTypes([FromBody] DeleteTypeModel model)
        {
            if (!ModelState.IsValid)
            {
                return OperateResult.Failed("参数错误");
            }
            return await _systemDictionaryService.DeleteTypes(model);
        }

        [HttpPost("query")]
        [Authorize]
        public async Task<OperateResult> QueryDictionary([FromBody] QueryDictModel model)
        {
            if (!ModelState.IsValid)
            {
                return OperateResult.Failed("参数错误");
            }
            return await _systemDictionaryService.QueryDictionary(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<OperateResult> UpdateItem([FromBody] UpdateDictItem model)
        {
            if (!ModelState.IsValid)
            {
                return OperateResult.Failed("参数错误");
            }
            return await _systemDictionaryService.UpdateItem(model);
        }
    }
}
