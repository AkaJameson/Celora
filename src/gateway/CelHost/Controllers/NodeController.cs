using CelHost.Models.NodeModels;
using CelHost.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Si.Utilites.OperateResult;
using Si.Utilites;

namespace CelHost.Controllers
{
    [ApiController]
    public class NodeController : DefaultController
    {
        private readonly INodeServiceImpl _nodeService;

        public NodeController(INodeServiceImpl nodeService)
        {
            _nodeService = nodeService;
        }

        [HttpPost]
        [Authorize]
        public async Task<OperateResult> AddClusterNode([FromBody] NodeAddModel model)
        {
            if (!ModelState.IsValid)
            {
                return OperateResult.Failed("参数错误");
            }
            return await _nodeService.AddClusterNode(model);
        }

        [HttpPost("{nodeId}")]
        [Authorize]
        public async Task<OperateResult> DeleteClusterNode(int nodeId)
        {
            if (!ModelState.IsValid)
            {
                return OperateResult.Failed("参数错误");
            }
            return await _nodeService.DeleteClusterNode(nodeId);
        }

        [HttpPost]
        [Authorize]
        public async Task<OperateResult> UpdateClusterNode([FromBody] NodeUpdateModel model)
        {
            if (!ModelState.IsValid)
            {
                return OperateResult.Failed("参数错误");
            }
            return await _nodeService.UpdateClusterNode(model);
        }
    }
}
