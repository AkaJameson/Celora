using CelHost.Data;
using CelHost.Database;
using CelHost.Models.HealthPolicyModels;
using Si.EntityFramework.Extension.UnitofWorks.Abstractions;
using Si.Utilites.OperateResult;

namespace CelHost.ServicesImpl
{
    public class HealthCheckServiceImpl
    {
        private readonly IUnitOfWork<HostContext> _unitofWork;
        public HealthCheckServiceImpl(IUnitOfWork<HostContext> unitofWork)
        {
            _unitofWork = unitofWork;
        }

        public async Task<OperateResult> AddHealthPolicy(HealthPolicyAddModel healthPolicyAdd)
        {
            var exists = await _unitofWork.GetRepository<HealthCheckOption>().ExistsAsync(p => p.Name == healthPolicyAdd.Name);
            if (exists)
            {
                return OperateResult.Failed("名称已存在");
            }
            var options = new HealthCheckOption()
            {
                Name = healthPolicyAdd.Name,
                Interval = healthPolicyAdd.Interval,
                Timeout = healthPolicyAdd.TimeOut,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                ActivePath = healthPolicyAdd.Path
            };
            await _unitofWork.GetRepository<HealthCheckOption>().AddAsync(options);
            await _unitofWork.CommitAsync();
            return OperateResult.Successed("添加成功");
        }

    }
}
