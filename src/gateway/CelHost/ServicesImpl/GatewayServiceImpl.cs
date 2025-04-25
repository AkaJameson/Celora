using CelHost.Data;
using CelHost.Database;
using CelHost.Models.Gateway;
using CelHost.Services;
using CelHost.Utils;
using Microsoft.EntityFrameworkCore;
using Si.EntityFramework.Extension.Extensions;
using Si.EntityFramework.Extension.UnitofWorks.Abstractions;
using Si.Utilites.OperateResult;
using System.Security.Claims;

namespace CelHost.ServicesImpl
{
    public class GatewayServiceImpl : IGatewayServiceImpl
    {
        private readonly IUnitOfWork<HostContext> unitOfWork;
        private readonly IHttpContextAccessor httpContext;
        public GatewayServiceImpl(IUnitOfWork<HostContext> unitOfWork, IHttpContextAccessor httpContext)
        {
            this.unitOfWork = unitOfWork;
            this.httpContext = httpContext;
        }
        /// <summary>
        /// 添加
        /// </summary>
        /// <returns></returns>
        public async Task<OperateResult> AddGateWay(CascadeAddModel cascadeModel)
        {
            if (await unitOfWork.GetRepository<Cascade>().ExistsAsync(p => p.Name == cascadeModel.Name || p.Url == cascadeModel.Source))
            {
                return OperateResult.Successed("已存在网关信息");
            }
            if (!cascadeModel.Source.UrlMatch())
            {
                return OperateResult.Successed("地址格式错误");
            }
            var (Key, IV) = StableAesCrypto.GenerateKeyAndIV();
            cascadeModel.UserName = StableAesCrypto.Encrypt(cascadeModel.UserName, Key, IV);
            cascadeModel.Password = StableAesCrypto.Encrypt(cascadeModel.Password, Key, IV);
            var cascade = new Cascade
            {
                IV = IV,
                Key = Key,
                UserName = cascadeModel.UserName,
                Password = cascadeModel.Password,
                Name = cascadeModel.Name,
                Url = cascadeModel.Source
            };
            await unitOfWork.GetRepository<Cascade>().AddAsync(cascade);
            await unitOfWork.CommitAsync();
            return OperateResult.Successed("添加成功");
        }
        /// <summary>
        /// 更新
        /// </summary>
        /// <returns></returns>
        public async Task<OperateResult> UpdateGateway(CascadeUpdateModel cascadeUpdateModel)
        {
            var cascade = await unitOfWork.GetRepository<Cascade>().FirstOrDefaultAsync(p => p.Id == cascadeUpdateModel.Id);
            if (cascade == null)
            {
                return OperateResult.Failed("未查找到网关");
            }
            if (!string.IsNullOrEmpty(cascadeUpdateModel.Name) && !await unitOfWork.GetRepository<Cascade>().ExistsAsync(p => p.Name == cascadeUpdateModel.Name && p.Id != cascadeUpdateModel.Id))
            {
                cascade.Name = cascadeUpdateModel.Name;
            }
            if (!string.IsNullOrEmpty(cascadeUpdateModel.Source) && !cascadeUpdateModel.Source.UrlMatch())
            {
                return OperateResult.Failed("地址格式错误");
            }
            if (!string.IsNullOrEmpty(cascadeUpdateModel.Source) &&
                !await unitOfWork.GetRepository<Cascade>().ExistsAsync(p => p.Url == cascadeUpdateModel.Source && p.Id != cascadeUpdateModel.Id))
            {
                cascade.Url = cascadeUpdateModel.Source;
            }
            if (!string.IsNullOrEmpty(cascadeUpdateModel.UserName) &&
             !await unitOfWork.GetRepository<Cascade>().ExistsAsync(p => p.UserName == cascadeUpdateModel.UserName && p.Id != cascadeUpdateModel.Id))
            {
                var encryptUserName = StableAesCrypto.Encrypt(cascadeUpdateModel.UserName, cascade.Key, cascade.IV);
                cascade.UserName = encryptUserName;
            }
            if (!string.IsNullOrEmpty(cascadeUpdateModel.Password) &&
                !await unitOfWork.GetRepository<Cascade>().ExistsAsync(p => p.Password == cascadeUpdateModel.Password && p.Id != cascadeUpdateModel.Id))
            {
                var encryptPassword = StableAesCrypto.Encrypt(cascadeUpdateModel.Password, cascade.Key, cascade.IV);

                cascade.Password = cascadeUpdateModel.Password;
            }
            await unitOfWork.GetRepository<Cascade>().UpdateAsync(cascade);
            await unitOfWork.CommitAsync();

            return OperateResult.Successed("修改成功");
        }

        public async Task<OperateResult> QueryGateway(CascadeQueryModel cascadeQueryModel)
        {
            var query = unitOfWork.GetRepository<Cascade>().Query();
            if (!string.IsNullOrEmpty(cascadeQueryModel.Name))
            {
                query.Where(p => EF.Functions.Like(p.Name, $"%{cascadeQueryModel.Name}%"));
            }
            var result = await query.ToPagedListAsync(cascadeQueryModel.PageIndex, cascadeQueryModel.PageSize);
            return OperateResult.Successed(new
            {
                Total = result.Total,
                Items = result.Items.Select(p => new
                {
                    Id = p.Id,
                    p.Name,
                    userName = p.UserName.Mask(),
                    passWord = p.Password.Mask(),
                    Path = StableAesCrypto.Encrypt(p.Url, p.Key, p.IV).Mask()
                })
            });
        }
        public async Task<OperateResult> GatewayDeltail(GatewayDetalModel gatewayDetalModel)
        {
            var userId = httpContext?.HttpContext?.User.Claims.FirstOrDefault(p => p.Type == ClaimTypes.Sid);
            if (userId == null || !int.TryParse(userId.Value, out var Id))
            {
                return OperateResult.Failed("用户错误");
            }
            var user = await unitOfWork.GetRepository<User>().FirstOrDefaultAsync(p => p.Id == Id);
            if (user == null)
            {
                return OperateResult.Failed("用户未找到");
            }
            var encryptPassword = StableAesCrypto.Encrypt(gatewayDetalModel.Password, user.Key, user.IV);
            if (user.Password != encryptPassword)
            {
                return OperateResult.Failed("密码错误");
            }
            var gateway = await unitOfWork.GetRepository<Cascade>().GetByIdAsync(gatewayDetalModel.GatewayId);
            if (gateway == null)
            {
                return OperateResult.Failed("未找到网关");
            }
            return OperateResult.Successed(new
            {
                userName = StableAesCrypto.Decrypt(gateway.UserName, gateway.Key, gateway.IV),
                password = StableAesCrypto.Decrypt(gateway.Password, gateway.Key, gateway.IV),
                Path = StableAesCrypto.Decrypt(gateway.Url, gateway.Key, gateway.IV)
            });
        }
    }
}
