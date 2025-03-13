using System.ComponentModel.DataAnnotations;

namespace Si.EntityFrame.IdentityServer.Models
{
    /// <summary>
    /// 用户注册模型
    /// </summary>
    public class RegisterModel
    {
        /// <summary>
        /// 用户名
        /// </summary>
        [Required(ErrorMessage = "用户名不能为空")]
        [StringLength(20, MinimumLength = 4, ErrorMessage = "用户名长度必须在4-20个字符之间")]
        public string Username { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [Required(ErrorMessage = "密码不能为空")]
        [StringLength(30, MinimumLength = 6, ErrorMessage = "密码长度必须在6-30个字符之间")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&])[A-Za-z\d@$!%*#?&]{6,}$", 
            ErrorMessage = "密码必须包含数字、字母和特殊字符")]
        public string Password { get; set; }

        /// <summary>
        /// 确认密码
        /// </summary>
        [Required(ErrorMessage = "确认密码不能为空")]
        [Compare("Password", ErrorMessage = "两次输入的密码不一致")]
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        [EmailAddress(ErrorMessage = "邮箱格式不正确")]
        public string Email { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        [RegularExpression(@"^1[3-9]\d{9}$", ErrorMessage = "手机号格式不正确")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// 默认角色名称
        /// </summary>
        public string DefaultRoleName { get; set; }
    }
}