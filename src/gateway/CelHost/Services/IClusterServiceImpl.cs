using CelHost.Models.ClusterModels;
using Si.Utilites.OperateResult;

namespace CelHost.Server.Services
{
    public interface IClusterServiceImpl
    {
        Task<OperateResult> AddCluster(ClusterAddModel clusterAddModel);
        Task<OperateResult> DeleteCluster(int clusterId);
        Task<OperateResult> QueryClusters(ClusterQueryModel clusterQueryModel);
        Task<OperateResult> RefactorClusters();
        Task<OperateResult> ToggleClusterStatus(int clusterId, bool isActive);
        Task<OperateResult> UpdateCluster(ClusterUpdateModel clusterUpdateModel);
    }
}