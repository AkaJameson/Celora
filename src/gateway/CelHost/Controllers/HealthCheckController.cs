using CelHost.Models.HealthPolicyModels;
using CelHost.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Si.Utilites.OperateResult;
using Si.Utilites;

namespace CelHost.Controllers
{
    [ApiController]
    public class HealthCheckController : DefaultController
    {
        private readonly IHealthCheckServiceImpl _healthCheckService;

        public HealthCheckController(IHealthCheckServiceImpl healthCheckService)
        {
            _healthCheckService = healthCheckService;
        }

        [HttpPost]
        [Authorize]
        public async Task<OperateResult> AddHealthPolicy([FromBody] HealthPolicyAddModel model)
        {
            if (!ModelState.IsValid)
            {
                return OperateResult.Failed("参数错误");
            }
            return await _healthCheckService.AddHealthPolicy(model);
        }

        [HttpPost("{id}")]
        [Authorize]
        public async Task<OperateResult> DeleteHealthPolicy(int id)
        {
            if (!ModelState.IsValid)
            {
                return OperateResult.Failed("参数错误");
            }
            return await _healthCheckService.DeleteHealthPolicy(id);
        }

        [HttpPost("query")]
        [Authorize]
        public async Task<OperateResult> QueryHealthPolicy([FromBody] HealthPolicyQueryModel model)
        {
            if (!ModelState.IsValid)
            {
                return OperateResult.Failed("参数错误");
            }
            return await _healthCheckService.QueryHealthPolicy(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<OperateResult> UpdateHealthPolicy([FromBody] HealthPolicyUpdateModel model)
        {
            if (!ModelState.IsValid)
            {
                return OperateResult.Failed("参数错误");
            }
            return await _healthCheckService.UpdateHealthPolicy(model);
        }
    }
}
