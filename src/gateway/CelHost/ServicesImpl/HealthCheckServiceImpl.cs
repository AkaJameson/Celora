using CelHost.Data;
using CelHost.Database;
using CelHost.Models.HealthPolicyModels;
using CelHost.Services;
using Microsoft.EntityFrameworkCore;
using Si.EntityFramework.Extension.Extensions;
using Si.EntityFramework.Extension.UnitofWorks.Abstractions;
using Si.Utilites.OperateResult;

namespace CelHost.ServicesImpl
{
    public class HealthCheckServiceImpl : IHealthCheckServiceImpl
    {
        private readonly IUnitOfWork<HostContext> _unitofWork;
        public HealthCheckServiceImpl(IUnitOfWork<HostContext> unitofWork)
        {
            _unitofWork = unitofWork;
        }
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="healthPolicyAdd"></param>
        /// <returns></returns>
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
        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="healthPolicyUpdateModel"></param>
        /// <returns></returns>
        public async Task<OperateResult> UpdateHealthPolicy(HealthPolicyUpdateModel healthPolicyUpdateModel)
        {
            var healthCheckOption = await _unitofWork.GetRepository<HealthCheckOption>().FirstOrDefaultAsync(p => p.Id == healthPolicyUpdateModel.Id);
            if (healthCheckOption == null)
            {
                return OperateResult.Failed("未找到配置");
            }
            if (!string.IsNullOrWhiteSpace(healthPolicyUpdateModel.Name))
            {
                if (await _unitofWork.GetRepository<HealthCheckOption>().ExistsAsync(p => p.Name == healthPolicyUpdateModel.Name && p.Id != healthPolicyUpdateModel.Id))
                {
                    return OperateResult.Failed("名称已存在");
                }
                healthCheckOption.Name = healthPolicyUpdateModel.Name;
            }
            if (healthPolicyUpdateModel.Interval.HasValue && healthPolicyUpdateModel.Interval != 0)
            {
                healthCheckOption.Interval = healthPolicyUpdateModel.Interval.Value;
            }
            if (healthPolicyUpdateModel.TimeOut.HasValue && healthPolicyUpdateModel.TimeOut != 0)
            {
                healthCheckOption.Timeout = healthPolicyUpdateModel.TimeOut.Value;
            }
            await _unitofWork.GetRepository<HealthCheckOption>().UpdateAsync(healthCheckOption);
            await _unitofWork.CommitAsync();
            return OperateResult.Successed();
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<OperateResult> DeleteHealthPolicy(int id)
        {
            await _unitofWork.GetRepository<HealthCheckOption>().DeleteAsync(id);
            return OperateResult.Successed();
        }
        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="healthPolicyQueryModel"></param>
        /// <returns></returns>
        public async Task<OperateResult> QueryHealthPolicy(HealthPolicyQueryModel healthPolicyQueryModel)
        {
            if (healthPolicyQueryModel.PageSize <= 0 || healthPolicyQueryModel.PageIndex <= 0)
            {
                return OperateResult.Failed("参数错误");
            }
            var query = _unitofWork.GetRepository<HealthCheckOption>().Query();
            if (!string.IsNullOrEmpty(healthPolicyQueryModel.Name))
            {
                query = query.Where(p => EF.Functions.Like(p.Name, $"%{healthPolicyQueryModel.Name}%"));
            }
            var result = await query.ToPagedListAsync(healthPolicyQueryModel.PageIndex, healthPolicyQueryModel.PageSize);
            return OperateResult.Successed(result);
        }
    }
}
