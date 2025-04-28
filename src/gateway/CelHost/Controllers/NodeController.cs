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
            return await _nodeService.AddClusterNode(model);
        }

        [HttpPost("{nodeId}")]
        [Authorize]
        public async Task<OperateResult> DeleteClusterNode(int nodeId)
        {
            return await _nodeService.DeleteClusterNode(nodeId);
        }

        [HttpPost]
        [Authorize]
        public async Task<OperateResult> UpdateClusterNode([FromBody] NodeUpdateModel model)
        {
            return await _nodeService.UpdateClusterNode(model);
        }
    }
}
