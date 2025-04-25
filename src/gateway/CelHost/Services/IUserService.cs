using CelHost.Models.UserInfoModels;
using Si.Utilites.OperateResult;

namespace CelHost.Services
{
    public interface IUserService
    {
        Task<OperateResult> Login(LoginModel loginModel);
        Task<OperateResult> ResetPassword(ResetPsdModel resetPasswordModel);
        Task<OperateResult> InitUser(IFormFile file);
    }
}