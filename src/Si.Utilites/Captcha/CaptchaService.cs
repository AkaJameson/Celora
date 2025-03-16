using Microsoft.Extensions.Caching.Memory;
using Si.Utilites.Captcha.Entitys;

namespace Si.Utilites.Captcha
{
    public class CaptchaService : ICaptchaService
    {
        private readonly IMemoryCache _cache;
        private readonly CaptchaGenerator _generator;
        private readonly CaptchaOptions _options;
        public CaptchaService(
            IMemoryCache cache,
            CaptchaGenerator generator,
            CaptchaOptions options = null)
        {
            _cache = cache;
            _generator = generator;
            _options = options;
        }

        public CaptchaResult GenerateCaptcha(string id = null)
        {
            var captcha = _generator.GenerateCaptcha();
            var cacheId = id ?? Guid.NewGuid().ToString("N");

            _cache.Set(cacheId, captcha.Code, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromSeconds(_options.ExpirationSeconds)
            });

            return new CaptchaResult
            {
                Id = cacheId,
                Code = captcha.Code,
                ImageBytes = captcha.ImageBytes
            };
        }

        public bool Validate(string id, string code)
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(code))
                return false;

            if (!_cache.TryGetValue(id, out string storedCode))
                return false;

            var isValid = string.Equals(storedCode, code, StringComparison.OrdinalIgnoreCase);
            if (isValid) Remove(id);
            return isValid;
        }

        public void Remove(string id)
        {
            _cache.Remove(id);
        }
    }
}
