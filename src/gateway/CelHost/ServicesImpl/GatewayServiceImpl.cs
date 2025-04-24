using CelHost.Data;
using CelHost.Database;
using CelHost.Models.Gateway;
using CelHost.Utils;
using Si.EntityFramework.Extension.UnitofWorks.Abstractions;
using Si.Utilites.OperateResult;

namespace CelHost.ServicesImpl
{
    public class GatewayServiceImpl
    {
        private readonly IUnitOfWork<HostContext> unitOfWork;
        public GatewayServiceImpl(IUnitOfWork<HostContext> unitOfWork)
        {
            this.unitOfWork = unitOfWork;
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
            return OperateResult.Successed("修改成功");
        }
    }
}
