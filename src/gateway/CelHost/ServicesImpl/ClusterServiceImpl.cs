using CelHost.Database;
using CelHost.Proxy.Abstraction;
using Si.Logging;
using Si.Utilites.OperateResult;

namespace CelHost.ServicesImpl
{
    public class ClusterServiceImpl
    {
        private readonly IProxyManager _proxyManager;
        private readonly HostContext _hostContext;
        private readonly ILogService _logger;

        public ClusterServiceImpl(
            IProxyManager proxyManager,
            HostContext hostContext,
            ILogService logger)
        {
            _proxyManager = proxyManager;
            _hostContext = hostContext;
            _logger = logger;
        }
      
        /// <summary>
        /// 修改集群状态
        /// </summary>
        /// <param name="clusterId"></param>
        /// <param name="isActive"></param>
        /// <returns></returns>
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
        #endregion
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
    }
}
