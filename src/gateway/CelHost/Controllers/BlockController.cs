using CelHost.Models;
using CelHost.Server.BlockList;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Si.Utilites;
using Si.Utilites.OperateResult;

namespace CelHost.Server.Controllers
{
    [Authorize]
    [ApiController]
    public class BlockController : DefaultController
    {
        private readonly IBlocklistService _blocklistService;
        public BlockController(IBlocklistService blocklistService)
        {
            _blocklistService = blocklistService;
        }
        [Authorize]
        [HttpPost("/Block")]
        public async Task<OperateResult> BlockIpAsync([FromBody] BlockModel blockModel)
        {
            if (!ModelState.IsValid)
            {
                return OperateResult.Failed("参数错误");
            }
            await _blocklistService.BlockAsync(blockModel.Ip, blockModel.Reason);
            return OperateResult.Successed();
        }
        [Authorize]
        [HttpPost("/UnBlock")]
        public async Task<OperateResult> UnBlockIpAsync([FromBody] BlockModel blockModel)
        {
            if (!ModelState.IsValid)
            {
                return OperateResult.Failed("参数错误");
            }
            await _blocklistService.UnblockAsync(blockModel.Ip);
            return OperateResult.Successed();
        }
    }
}
