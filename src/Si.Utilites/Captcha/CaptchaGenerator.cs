using Si.Utilites.Captcha.Entitys;
using SkiaSharp;

public class CaptchaGenerator
{
    private readonly CaptchaOptions _options;

    public CaptchaGenerator(CaptchaOptions options =null)
    {
        _options = options?? new CaptchaOptions();
    }
    // 生成验证码（返回图片字节和验证码文本）
    public CaptchaResult GenerateCaptcha()
    {
        using var surface = SKSurface.Create(new SKImageInfo(_options.Width, _options.Height));
        var canvas = surface.Canvas;

        // 绘制背景
        canvas.Clear(_options.BackgroundColor);

        // 生成随机验证码
        var captchaCode = GenerateRandomCode(_options.CodeLength);

        // 绘制干扰元素
        DrawNoise(canvas);
        DrawInterferenceLines(canvas);

        // 绘制验证码文本
        DrawText(canvas, captchaCode);

        // 转换为图片字节
        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return new CaptchaResult { ImageBytes = data.ToArray(), Code = captchaCode };
    }

    private string GenerateRandomCode(int length)
    {
        var chars = _options.Characters
            .Where(c => !_options.ExcludedCharacters.Contains(c))
            .ToArray();

        return new string(Enumerable.Range(0, length)
            .Select(_ => chars[Random.Shared.Next(chars.Length)])
            .ToArray());
    }

    private void DrawText(SKCanvas canvas, string text)
    {
        using var paint = new SKPaint
        {
            IsAntialias = true,
            TextAlign = SKTextAlign.Center,
            Typeface = SKTypeface.FromFamilyName(_options.FontFamily, SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
        };

        var textWidth = text.Length * _options.FontSize;
        var startX = (float)(_options.Width - textWidth) / 2;

        for (int i = 0; i < text.Length; i++)
        {
            // 随机设置字符属性
            paint.Color = _options.TextColors[Random.Shared.Next(_options.TextColors.Count)];
            paint.TextSize = _options.FontSize * (float)(0.8 + Random.Shared.NextDouble() * 0.4);

            // 计算字符位置
            var x = startX + i * _options.FontSize;
            var y = _options.Height / 2f + _options.FontSize / 2 - 4;

            // 添加随机旋转
            canvas.Save();
            canvas.Translate(x, y);
            canvas.RotateDegrees(Random.Shared.Next(-20, 20));

            // 绘制字符
            canvas.DrawText(text[i].ToString(), 0, 0, paint);
            canvas.Restore();

            // 添加字符间距
            startX += (float)(_options.FontSize * 0.2);
        }
    }

    private void DrawNoise(SKCanvas canvas)
    {
        using var paint = new SKPaint
        {
            Color = SKColors.Gray.WithAlpha((byte)0x60),
            IsAntialias = true
        };

        for (int i = 0; i < _options.NoiseCount; i++)
        {
            var x = Random.Shared.Next(_options.Width);
            var y = Random.Shared.Next(_options.Height);
            canvas.DrawCircle(x, y, 1, paint);
        }
    }

    private void DrawInterferenceLines(SKCanvas canvas)
    {
        using var paint = new SKPaint
        {
            StrokeWidth = 2,
            IsAntialias = true,
            PathEffect = SKPathEffect.CreateDash(new[] { 5f, 5f }, 0)
        };

        for (int i = 0; i < _options.InterferenceLineCount; i++)
        {
            paint.Color = _options.TextColors[Random.Shared.Next(_options.TextColors.Count)].WithAlpha((byte)0x80);

            var start = new SKPoint(Random.Shared.Next(_options.Width), Random.Shared.Next(_options.Height));
            var end = new SKPoint(Random.Shared.Next(_options.Width), Random.Shared.Next(_options.Height));
            canvas.DrawLine(start, end, paint);
        }
    }
}
