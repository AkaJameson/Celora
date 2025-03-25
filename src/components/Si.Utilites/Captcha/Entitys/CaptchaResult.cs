namespace Si.Utilites.Captcha.Entitys
{
    public class CaptchaResult
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public byte[] ImageBytes { get; set; }
    }
}
