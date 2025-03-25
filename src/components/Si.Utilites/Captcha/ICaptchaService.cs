using Si.Utilites.Captcha.Entitys;

namespace Si.Utilites.Captcha
{
    public interface ICaptchaService
    {
        CaptchaResult GenerateCaptcha(string id = null);
        bool Validate(string id, string code);
        void Remove(string id);
    }
}
