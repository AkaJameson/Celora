namespace CelHost.Models.UserInfoModels
{
    public class UserLicenseModel
    {

        public string Account { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public DateTime ExpireAt { get; set; }
        public string Signature { get; set; }
    }
}
