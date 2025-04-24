using CelHost.Models.HealthPolicyModels;
using Si.Utilites.OperateResult;

namespace CelHost.Services
{
    public interface IHealthCheckServiceImpl
    {
        Task<OperateResult> AddHealthPolicy(HealthPolicyAddModel healthPolicyAdd);
        Task<OperateResult> DeleteHealthPolicy(int id);
        Task<OperateResult> QueryHealthPolicy(HealthPolicyQueryModel healthPolicyQueryModel);
        Task<OperateResult> UpdateHealthPolicy(HealthPolicyUpdateModel healthPolicyUpdateModel);
    }
}