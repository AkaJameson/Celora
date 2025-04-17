using CelHost.Data;
using CelHost.Database;
using CelHost.Proxy.Abstraction;
using Si.Utilites.OperateResult;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CelHost.ServicesImpl
{
    public class ClusterServiceImpl
    {
        private readonly IProxyManager _proxyManager;
        private readonly HostContext _hostContext;
        private readonly ILogger<ClusterServiceImpl> _logger;

        public ClusterServiceImpl(
            IProxyManager proxyManager,
            HostContext hostContext,
            ILogger<ClusterServiceImpl> logger)
        {
            _proxyManager = proxyManager;
            _hostContext = hostContext;
            _logger = logger;
        }

        #region 集群配置管理
        public async Task<OperateResult> CreateCluster(Cluster cluster)
        {
            using var transaction = await _hostContext.Database.BeginTransactionAsync();
            // 验证路由ID唯一性
            if (await _hostContext.Clusters.AnyAsync(c => c.RouteId == cluster.RouteId))
                return OperateResult.Failed("路由ID已存在");

            // 关联健康检查配置
            if (cluster.HealthCheckId > 0)
            {
                cluster.CheckOption = await _hostContext.HealthCheckOptions
                    .FindAsync(cluster.HealthCheckId);
            }

            await _hostContext.Clusters.AddAsync(cluster);
            await _hostContext.SaveChangesAsync();
            await transaction.CommitAsync();

            await TriggerConfigUpdate();
            return OperateResult.Successed("集群创建成功");
        }

        public async Task<OperateResult> UpdateCluster(Cluster updatedCluster)
        {
            using var transaction = await _hostContext.Database.BeginTransactionAsync();
            var existingCluster = await _hostContext.Clusters
                .Include(c => c.Nodes)
                .Include(c => c.CheckOption)
                .FirstOrDefaultAsync(c => c.Id == updatedCluster.Id);

            if (existingCluster == null)
                return OperateResult.Failed("集群不存在");

            // 更新基础属性
            _hostContext.Entry(existingCluster).CurrentValues
                .SetValues(updatedCluster);

            // 处理健康检查配置
            if (updatedCluster.HealthCheckId != existingCluster.HealthCheckId)
            {
                existingCluster.CheckOption = await _hostContext.HealthCheckOptions
                    .FindAsync(updatedCluster.HealthCheckId);
            }

            // 节点同步处理
            SyncClusterNodes(existingCluster.Nodes, updatedCluster.Nodes);

            await _hostContext.SaveChangesAsync();
            await transaction.CommitAsync();

            await TriggerConfigUpdate();
            return OperateResult.Successed("集群更新成功");
        }

        public async Task<OperateResult> DeleteCluster(int clusterId)
        {
            using var transaction = await _hostContext.Database.BeginTransactionAsync();
            var cluster = await _hostContext.Clusters
                .Include(c => c.Nodes)
                .FirstOrDefaultAsync(c => c.Id == clusterId);

            if (cluster == null)
                return OperateResult.Failed("集群不存在");

            _hostContext.Clusters.Remove(cluster);
            await _hostContext.SaveChangesAsync();
            await transaction.CommitAsync();

            await TriggerConfigUpdate();
            return OperateResult.Successed("集群删除成功");
        }
        #endregion

        #region 状态管理
        public async Task<OperateResult> ToggleClusterStatus(int clusterId, bool isActive)
        {
            var cluster = await _hostContext.Clusters.FindAsync(clusterId);
            if (cluster == null) return OperateResult.Failed("集群不存在");

            cluster.IsActive = isActive;
            await _hostContext.SaveChangesAsync();
            await TriggerConfigUpdate();
            return OperateResult.Successed("集群状态更新成功");
        }

        public async Task<OperateResult> UpdateNodeHealthStatus(int nodeId, bool isHealthy)
        {
            var node = await _hostContext.ClusterNodes.FindAsync(nodeId);
            if (node == null) return OperateResult.Failed("节点不存在");

            node.IsActive = isHealthy;
            node.LastHealthCheck = DateTime.UtcNow;
            await _hostContext.SaveChangesAsync();

            // 限流更新：仅当节点状态变化时触发
            if (node.IsActive != isHealthy)
                await TriggerConfigUpdate();

            return OperateResult.Successed("节点健康状态更新成功");
        }
        #endregion

        #region 查询功能
        public async Task<OperateResult<Cluster>> GetCluster(Expression<Func<Cluster, bool>> predicate)
        {
            var cluster = await _hostContext.Clusters
                .Include(c => c.Nodes)
                .Include(c => c.CheckOption)
                .FirstOrDefaultAsync(predicate);

            return cluster == null
                ? OperateResult.Failed<Cluster>("集群未找到")
                : OperateResult.Successed<Cluster>(cluster);
        }

        public async Task<OperateResult<List<Cluster>>> GetClusters(
            Expression<Func<Cluster, bool>> predicate = null,
            bool includeInactive = false)
        {
            var query = _hostContext.Clusters
                .Include(c => c.Nodes)
                .Include(c => c.CheckOption)
                .AsQueryable();

            if (!includeInactive)
                query = query.Where(c => c.IsActive);

            if (predicate != null)
                query = query.Where(predicate);

            var results = await query.ToListAsync();
            return OperateResult<List<Cluster>>.Successed(results);
        }
        #endregion

        #region 辅助方法
        private void SyncClusterNodes(
            ICollection<ClusterNode> existingNodes,
            ICollection<ClusterNode> updatedNodes)
        {
            // 删除不存在的节点
            var removedNodes = existingNodes
                .Where(en => !updatedNodes.Any(un => un.Id == en.Id))
                .ToList();
            foreach (var node in removedNodes)
                _hostContext.ClusterNodes.Remove(node);

            // 更新或新增节点
            foreach (var updatedNode in updatedNodes)
            {
                var existingNode = existingNodes
                    .FirstOrDefault(n => n.Id == updatedNode.Id);

                if (existingNode != null)
                {
                    _hostContext.Entry(existingNode).CurrentValues
                        .SetValues(updatedNode);
                }
                else
                {
                    updatedNode.ClusterId = existingNodes.First().ClusterId;
                    existingNodes.Add(updatedNode);
                }
            }
        }

        private async Task TriggerConfigUpdate()
        {
            try
            {
                await Task.Run(() => _proxyManager.UpdateProxyConfig());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "配置更新触发失败");
            }
        }

        public async Task<OperateResult> RefactorClusters()
        {
            return await OperateResult.WrapAsync(async () =>
            {
                await TriggerConfigUpdate();
                return "配置重载成功";
            });
        }
        #endregion
    }
}
