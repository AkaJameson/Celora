using CelHost.Models;
using Si.Utilites.OperateResult;

namespace CelHost.Services
{
    public interface IUserService
    {
        Task<OperateResult> Login(LoginModel loginModel);
        Task<OperateResult> ResetPassword(ResetPasswordModel resetPasswordModel);
    }
}