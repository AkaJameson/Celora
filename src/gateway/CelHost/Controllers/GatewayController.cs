using CelHost.Models.Gateway;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Si.Utilites.OperateResult;
using Si.Utilites;
using CelHost.Server.Services;

namespace CelHost.Server.Controllers
{
    [ApiController]
    public class GatewayController : DefaultController
    {
        private readonly IGatewayServiceImpl _gatewayService;

        public GatewayController(IGatewayServiceImpl gatewayService)
        {
            _gatewayService = gatewayService;
        }

        [HttpPost]
        [Authorize]
        public async Task<OperateResult> AddGateWay([FromBody] CascadeAddModel model)
        {
            if (!ModelState.IsValid)
            {
                return OperateResult.Failed("参数错误");
            }
            return await _gatewayService.AddGateWay(model);
        }

        [HttpPost("detail")]
        [Authorize]
        public async Task<OperateResult> GatewayDetail([FromBody] GatewayDetalModel model)
        {
            if (!ModelState.IsValid)
            {
                return OperateResult.Failed("参数错误");
            }
            return await _gatewayService.GatewayDeltail(model);
        }

        [HttpPost("query")]
        [Authorize]
        public async Task<OperateResult> QueryGateway([FromBody] CascadeQueryModel model)
        {
            if (!ModelState.IsValid)
            {
                return OperateResult.Failed("参数错误");
            }
            return await _gatewayService.QueryGateway(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<OperateResult> UpdateGateway([FromBody] CascadeUpdateModel model)
        {
            if (!ModelState.IsValid)
            {
                return OperateResult.Failed("参数错误");
            }
            return await _gatewayService.UpdateGateway(model);
        }
    }
}
