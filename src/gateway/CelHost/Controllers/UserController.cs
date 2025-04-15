using CelHost.Models;
using CelHost.Services;
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
    }
}
