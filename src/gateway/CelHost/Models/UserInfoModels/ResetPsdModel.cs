using System.ComponentModel.DataAnnotations;

namespace CelHost.Models.UserInfoModels
{
    public class ResetPsdModel
    {
        [Required]
        public string Account { get; set; }
        [Required]
        public string OldPassword { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string ConfirmPassword { get; set; }
    }
}
