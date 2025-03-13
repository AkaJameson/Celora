using PaperTrail.Utilities.Captcha.Entitys;

namespace PaperTrail.Utilities.Captcha
{
    public interface ICaptchaService
    {
        CaptchaResult GenerateCaptcha(string id = null);
        bool Validate(string id, string code);
        void Remove(string id);
    }
}
