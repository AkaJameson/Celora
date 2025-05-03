using CelHost.Models.Gateway;
using Si.Utilites.OperateResult;

namespace CelHost.Server.Services
{
    public interface IGatewayServiceImpl
    {
        Task<OperateResult> AddGateWay(CascadeAddModel cascadeModel);
        Task<OperateResult> GatewayDeltail(GatewayDetalModel gatewayDetalModel);
        Task<OperateResult> QueryGateway(CascadeQueryModel cascadeQueryModel);
        Task<OperateResult> UpdateGateway(CascadeUpdateModel cascadeUpdateModel);
    }
}