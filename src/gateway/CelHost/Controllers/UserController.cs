using CelHost.Models.UserInfoModels;
using CelHost.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Si.Utilites;
using Si.Utilites.OperateResult;

namespace CelHost.Controllers
{
    public class UserController:DefaultController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="loginModel"></param>
        /// <returns></returns>
       
        [HttpPost]
        public async Task<OperateResult> LoginAsync([FromBody] LoginModel loginModel)
        {
            return await _userService.Login(loginModel);
        }

        /// <summary>
        /// 重置密码
        /// </summary>
        /// <param name="resetPasswordModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public async Task<OperateResult> ResetPassword([FromBody] ResetPsdModel resetPasswordModel)
        {
            return await _userService.ResetPassword(resetPasswordModel);
        }
        /// <summary>
        /// 初始化用户（通过文件导入）
        /// </summary>
        /// <param name="file">用户数据文件</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public async Task<OperateResult> InitUser([FromForm] IFormFile file)
        {
            return await _userService.InitUser(file);
        }
    }
}
