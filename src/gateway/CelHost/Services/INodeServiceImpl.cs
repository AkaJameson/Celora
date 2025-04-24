using CelHost.Models.NodeModels;
using Si.Utilites.OperateResult;

namespace CelHost.Services
{
    public interface INodeServiceImpl
    {
        Task<OperateResult> AddClusterNode(NodeAddModel nodeAddModel);
        Task<OperateResult> DeleteClusterNode(int nodeId);
        Task<OperateResult> UpdateClusterNode(NodeUpdateModel nodeUpdateModel);
    }
}