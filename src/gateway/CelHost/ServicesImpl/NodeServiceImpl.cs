using CelHost.Data;
using CelHost.Database;
using CelHost.Models.NodeModels;
using CelHost.Services;
using CelHost.Utils;
using Microsoft.EntityFrameworkCore;
using Si.EntityFramework.Extension.UnitofWorks.Abstractions;
using Si.Utilites.OperateResult;

namespace CelHost.ServicesImpl
{
    public class NodeServiceImpl : INodeServiceImpl
    {
        private readonly IUnitOfWork<HostContext> unitOfWork;
        public NodeServiceImpl(IUnitOfWork<HostContext> unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        /// <summary>
        /// 添加节点
        /// </summary>
        /// <returns></returns>
        public async Task<OperateResult> AddClusterNode(NodeAddModel nodeAddModel)
        {
            var cluster = await unitOfWork.GetRepository<Cluster>().Where(p => p.Id == nodeAddModel.ClusterId).Include(p => p.Nodes).FirstOrDefaultAsync();
            if (cluster == null)
            {
                return OperateResult.Failed("集群不存在");
            }
            if (cluster.Nodes.Any(p => p.Address == nodeAddModel.Address || p.Name == nodeAddModel.Name))
            {
                return OperateResult.Failed("节点已存在");
            }
            if (!nodeAddModel.Address.UrlMatch())
            {
                return OperateResult.Failed("地址格式错误");
            }
            cluster.Nodes.Add(new ClusterNode
            {
                Name = nodeAddModel.Name,
                ClusterId = nodeAddModel.ClusterId,
                Address = nodeAddModel.Address,
                IsActive = nodeAddModel.IsActive
            });
            await unitOfWork.GetRepository<Cluster>().UpdateAsync(cluster);
            await unitOfWork.CommitAsync();
            return OperateResult.Successed("添加成功");
        }
        /// <summary>
        /// 更新节点
        /// </summary>
        /// <param name="nodeUpdateModel"></param>
        /// <returns></returns>
        public async Task<OperateResult> UpdateClusterNode(NodeUpdateModel nodeUpdateModel)
        {
            var node = await unitOfWork.GetRepository<ClusterNode>().FirstOrDefaultAsync(p => p.Id == nodeUpdateModel.NodeId && p.ClusterId == nodeUpdateModel.ClusterId);
            if (node == null)
            {
                return OperateResult.Failed("节点不存在");
            }
            if (!string.IsNullOrEmpty(nodeUpdateModel.Name))
            {
                node.Name = nodeUpdateModel.Name;
            }
            if (!string.IsNullOrEmpty(nodeUpdateModel.Address) && nodeUpdateModel.Address.UrlMatch())
            {
                node.Address = nodeUpdateModel.Address;
            }
            if (nodeUpdateModel.isActive.HasValue)
            {
                node.IsActive = nodeUpdateModel.isActive.Value;
            }
            await unitOfWork.GetRepository<ClusterNode>().UpdateAsync(node);
            await unitOfWork.CommitAsync();
            return OperateResult.Successed("更新成功");
        }
        /// <summary>
        /// 删除节点
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public async Task<OperateResult> DeleteClusterNode(int nodeId)
        {
            await unitOfWork.GetRepository<ClusterNode>().DeleteAsync(nodeId);
            await unitOfWork.CommitAsync();
            return OperateResult.Successed("删除成功");
        }

    }
}
