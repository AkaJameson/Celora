using CelHost.Proxy.Abstraction;
using Si.Utilites.OperateResult;

namespace CelHost.ServicesImpl
{
    public class ClusterServiceImpl
    {
        private readonly IProxyManager _proxyManager;
        public ClusterServiceImpl(IProxyManager proxyManager)
        {
            _proxyManager = proxyManager;
        }
        /// <summary>
        /// 重构集群
        /// </summary>
        /// <returns></returns>
        public async Task<OperateResult> RefactorClusters()
        {

            return await Task.Run(() =>
             {
                 _proxyManager.UpdateProxyConfig();
                 return OperateResult.Successed("配置重载成功");
             });
        }
        /// <summary>
        /// 查询集群信息
        /// </summary>
        /// <returns></returns>
        //public async Task<OperateResult> QueryClusterInfo()
        //{

        //}
    }
}
