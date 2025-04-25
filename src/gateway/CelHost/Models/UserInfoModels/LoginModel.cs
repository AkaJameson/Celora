using System.ComponentModel.DataAnnotations;

namespace CelHost.Models.UserInfoModels
{
    public class LoginModel
    {
        [Required]
        public string Account { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
