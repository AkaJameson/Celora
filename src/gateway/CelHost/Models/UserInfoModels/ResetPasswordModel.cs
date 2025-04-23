namespace CelHost.Models.UserInfoModels
{
    public class ResetPasswordModel
    {
        public string Account { get; set; }
        public string OldPassword { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
