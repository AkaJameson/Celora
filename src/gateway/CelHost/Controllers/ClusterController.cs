using CelHost.Models.ClusterModels;
using CelHost.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Si.Utilites.OperateResult;
using Si.Utilites;

namespace CelHost.Controllers
{
    [ApiController]
    public class ClusterController : DefaultController
    {
        private readonly IClusterServiceImpl _clusterService;

        public ClusterController(IClusterServiceImpl clusterService)
        {
            _clusterService = clusterService;
        }

        [HttpPost]
        [Authorize]
        public async Task<OperateResult> AddCluster([FromBody] ClusterAddModel model)
        {
            if (!ModelState.IsValid)
            {
                return OperateResult.Failed("参数错误");
            }
            return await _clusterService.AddCluster(model);
        }

        [HttpPost("{clusterId}")]
        [Authorize]
        public async Task<OperateResult> DeleteCluster(int clusterId)
        {
            if (!ModelState.IsValid)
            {
                return OperateResult.Failed("参数错误");
            }
            return await _clusterService.DeleteCluster(clusterId);
        }

        [HttpPost("query")]
        [Authorize]
        public async Task<OperateResult> QueryClusters([FromBody] ClusterQueryModel model)
        {
            if (!ModelState.IsValid)
            {
                return OperateResult.Failed("参数错误");
            }
            return await _clusterService.QueryClusters(model);
        }

        [HttpPost("refactor")]
        [Authorize]
        public async Task<OperateResult> RefactorClusters()
        {
            if (!ModelState.IsValid)
            {
                return OperateResult.Failed("参数错误");
            }
            return await _clusterService.RefactorClusters();
        }

        [HttpPost("toggle-status/{clusterId}")]
        [Authorize]
        public async Task<OperateResult> ToggleClusterStatus(int clusterId, [FromBody] bool isActive)
        {
            if (!ModelState.IsValid)
            {
                return OperateResult.Failed("参数错误");
            }
            return await _clusterService.ToggleClusterStatus(clusterId, isActive);
        }

        [HttpPost]
        [Authorize]
        public async Task<OperateResult> UpdateCluster([FromBody] ClusterUpdateModel model)
        {
            if (!ModelState.IsValid)
            {
                return OperateResult.Failed("参数错误");
            }
            return await _clusterService.UpdateCluster(model);
        }
    }
}
