using CelHost.Data;
using CelHost.Database;
using CelHost.Models.ClusterModels;
using CelHost.Proxy.Abstraction;
using CelHost.Utils;
using Microsoft.EntityFrameworkCore;
using Si.EntityFramework.Extension.Extensions;
using Si.EntityFramework.Extension.UnitofWorks.Abstractions;
using Si.Logging;
using Si.Utilites.OperateResult;

namespace CelHost.ServicesImpl
{
    public class ClusterServiceImpl : IClusterServiceImpl
    {
        private readonly IProxyManager _proxyManager;
        private readonly IUnitOfWork<HostContext> unitOfWork;
        private readonly ILogService _logger;

        public ClusterServiceImpl(
            IProxyManager proxyManager,
            IUnitOfWork<HostContext> hostContext,
            ILogService logger)
        {
            _proxyManager = proxyManager;
            unitOfWork = hostContext;
            _logger = logger;
        }
        /// <summary>
        /// 更新配置
        /// </summary>
        /// <returns></returns>
        public async Task<OperateResult> RefactorClusters()
        {
            return await OperateResult.WrapAsync(async () =>
            {
                await TriggerConfigUpdate();
                return "配置重载成功";
            });
        }
        /// <summary>
        /// 修改集群状态
        /// </summary>
        /// <param name="clusterId"></param>
        /// <param name="isActive"></param>
        /// <returns></returns>
        public async Task<OperateResult> ToggleClusterStatus(int clusterId, bool isActive)
        {
            var cluster = await unitOfWork.GetRepository<Cluster>().GetByIdAsync(clusterId);
            if (cluster == null) return OperateResult.Failed("集群不存在");
            cluster.IsActive = isActive;
            await unitOfWork.CommitAsync();
            await TriggerConfigUpdate();
            return OperateResult.Successed("集群状态更新成功");
        }
        /// <summary>
        /// 添加Cluster
        /// </summary>
        /// <param name="clusterAddModel"></param>
        /// <returns></returns>
        public async Task<OperateResult> AddCluster(ClusterAddModel clusterAddModel)
        {
            if (!clusterAddModel.PrefixPath.RouteMatch())
            {
                return OperateResult.Failed("路径格式不正确");
            }
            if (await unitOfWork.GetRepository<Cluster>().ExistsAsync(p => p.ClusterName == clusterAddModel.Name))
            {
                return OperateResult.Failed("名称已存在");
            }
            if (await unitOfWork.GetRepository<Cluster>().ExistsAsync(p => p.RouteId == clusterAddModel.PrefixPath))
            {
                return OperateResult.Failed("路径已存在");
            }
            var cluster = new Cluster()
            {
                RouteId = Guid.NewGuid().ToString("N"),
                ClusterName = clusterAddModel.Name,
                Path = clusterAddModel.PrefixPath,
                LoadBalancingPolicy = clusterAddModel.LoadBalancePolicyName,
                RateLimitPolicyName = clusterAddModel.RateLimitPolicyName,
                IsActive = clusterAddModel.IsActive,
                HealthCheck = clusterAddModel.HealthCheck
            };
            if (clusterAddModel.HealthCheck)
            {
                cluster.HealthCheckId = clusterAddModel.HealthCheckId;
            }
            await unitOfWork.GetRepository<Cluster>().AddAsync(cluster);
            await unitOfWork.CommitAsync();
            return OperateResult.Successed("集群添加成功");
        }
        /// <summary>
        /// 删除Cluster
        /// </summary>
        /// <param name="clusterId"></param>
        /// <returns></returns>
        public async Task<OperateResult> DeleteCluster(int clusterId)
        {
            await unitOfWork.GetRepository<Cluster>().DeleteAsync(clusterId);
            await unitOfWork.CommitAsync();
            return OperateResult.Successed("集群删除成功");
        }
        /// <summary>
        /// 更新Cluster
        /// </summary>
        /// <param name="clusterUpdateModel"></param>
        /// <returns></returns>
        public async Task<OperateResult> UpdateCluster(ClusterUpdateModel clusterUpdateModel)
        {
            var cluster = await unitOfWork.GetRepository<Cluster>().FirstOrDefaultAsync(p => p.Id == clusterUpdateModel.Id);
            if (!string.IsNullOrEmpty(clusterUpdateModel.Path) && clusterUpdateModel.Path.RouteMatch())
            {
                if (await unitOfWork.GetRepository<Cluster>().ExistsAsync(p => p.Path == clusterUpdateModel.Path && p.Id != clusterUpdateModel.Id))
                {
                    return OperateResult.Failed("路径已存在");
                }
                cluster.Path = clusterUpdateModel.Path;
            }
            if (!string.IsNullOrEmpty(clusterUpdateModel.Name))
            {
                if (await unitOfWork.GetRepository<Cluster>().ExistsAsync(p => p.ClusterName == clusterUpdateModel.Name && p.Id != clusterUpdateModel.Id))
                {
                    return OperateResult.Failed("名称已存在");
                }
                cluster.ClusterName = clusterUpdateModel.Name;
            }
            if (!string.IsNullOrEmpty(clusterUpdateModel.LoadBlancePolicyName))
            {
                cluster.LoadBalancingPolicy = clusterUpdateModel.LoadBlancePolicyName;
            }
            if (!string.IsNullOrEmpty(clusterUpdateModel.RateLimitPolicyName))
            {
                cluster.RateLimitPolicyName = clusterUpdateModel.RateLimitPolicyName;
            }
            if (clusterUpdateModel.IsActive.HasValue)
            {
                cluster.IsActive = clusterUpdateModel.IsActive.Value;
            }
            if (clusterUpdateModel.IsHealthCheck.HasValue)
            {
                cluster.HealthCheck = clusterUpdateModel.IsHealthCheck.Value;
            }
            if (clusterUpdateModel.HealthCheckId.HasValue)
            {
                var healthCheckOption = await unitOfWork.GetRepository<HealthCheckOption>().FirstOrDefaultAsync(p => p.Id == clusterUpdateModel.HealthCheckId.Value);
                if (healthCheckOption == null)
                {
                    return OperateResult.Failed("未找到配置");
                }
                cluster.HealthCheckId = clusterUpdateModel.HealthCheckId.Value;
            }
            await unitOfWork.GetRepository<Cluster>().UpdateAsync(cluster);
            await unitOfWork.CommitAsync();
            return OperateResult.Successed("集群更新成功");
        }
        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="clusterQueryModel"></param>
        /// <returns></returns>
        public async Task<OperateResult> QueryClusters(ClusterQueryModel clusterQueryModel)
        {
            var query = unitOfWork.GetRepository<Cluster>().Query();
            if (!string.IsNullOrEmpty(clusterQueryModel.Name))
            {
                query.Where(p => EF.Functions.Like(p.ClusterName, $"%{clusterQueryModel.Name}%"));
            }
            query.Include(p => p.Nodes).Include(p => p.HealthCheck);
            var result = await query.ToPagedListAsync(clusterQueryModel.PageIndex, clusterQueryModel.PageSize);
            return OperateResult.Successed(new
            {
                Total = result.Total,
                Items = result.Items.Select(p => new
                {
                    Id = p.Id,
                    Name = p.ClusterName,
                    Prefix = p.Path,
                    RateLimitPolicy = p.RateLimitPolicyName,
                    LoadBlancePolicy = p.LoadBalancingPolicy,
                    IsActive = p.IsActive,
                    HealthCheck = p.HealthCheck,
                    CheckOption = new
                    {
                        Name = p.CheckOption.Name,
                        Interval = p.CheckOption.Interval,
                        Timeout = p.CheckOption.Timeout,
                        CheckPath = p.CheckOption.ActivePath,
                        CreateTime = p.CheckOption.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                        UpdateTime = p.CheckOption.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    },
                    Nodes = p.Nodes.Select(p => new
                    {
                        Address = p.Address,
                        IsActive = p.IsActive,
                    })
                })
            });
        }

        /// <summary>
        /// 更新配置(Internal)
        /// </summary>
        /// <returns></returns>
        private async Task TriggerConfigUpdate()
        {
            try
            {
                await Task.Run(() => _proxyManager.UpdateProxyConfig());
            }
            catch (Exception ex)
            {
                _logger.Error("配置更新触发失败", ex);
            }
        }
    }
}
