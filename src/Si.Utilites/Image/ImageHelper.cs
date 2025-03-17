using SkiaSharp;
using System.Collections.ObjectModel;

namespace Si.Utilites.Image;

/// <summary>
/// 图片处理工具类
/// </summary>
public class ImageHelper
{
    /// <summary>
    /// 允许的图片扩展名
    /// </summary>
    public static readonly ReadOnlyCollection<string> AllowedExtensions = new(new[]
    {
        ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".webp", ".tiff", ".ico"
    });

    #region 生成缩略图

    /// <summary>
    /// 生成缩略图
    /// </summary>
    /// <param name="originalImagePath">原图路径</param>
    /// <param name="thumbnailPath">缩略图路径</param>
    /// <param name="width">目标宽度</param>
    /// <param name="height">目标高度</param>
    /// <param name="mode">缩放模式：HW=指定高宽缩放, W=指定宽等比缩放, H=指定高等比缩放, Cut=裁剪</param>
    public static void MakeThumbnail(string originalImagePath, string thumbnailPath, int width, int height, string mode)
    {
        using var original = SKBitmap.Decode(originalImagePath);

        // 计算目标尺寸
        var (targetWidth, targetHeight, x, y, sourceWidth, sourceHeight) =
            CalculateTargetSize(original.Width, original.Height, width, height, mode);

        // 创建目标图片
        using var target = new SKBitmap(targetWidth, targetHeight);
        using var canvas = new SKCanvas(target);

        // 设置高质量缩放
        canvas.Clear(SKColors.Transparent);
        var paint = new SKPaint
        {
            IsAntialias = true,
            FilterQuality = SKFilterQuality.High
        };

        // 绘制图片
        var sourceRect = SKRect.Create(x, y, sourceWidth, sourceHeight);
        var destRect = SKRect.Create(0, 0, targetWidth, targetHeight);
        canvas.DrawBitmap(original, sourceRect, destRect, paint);

        // 保存图片
        using var image = SKImage.FromBitmap(target);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 100);
        using var stream = File.OpenWrite(thumbnailPath);
        data.SaveTo(stream);
    }

    /// <summary>
    /// 计算目标尺寸
    /// </summary>
    private static (int width, int height, int x, int y, int sourceWidth, int sourceHeight)
        CalculateTargetSize(int originalWidth, int originalHeight, int targetWidth, int targetHeight, string mode)
    {
        int x = 0, y = 0;
        int sourceWidth = originalWidth;
        int sourceHeight = originalHeight;

        switch (mode)
        {
            case "W": // 指定宽，高按比例
                targetHeight = originalHeight * targetWidth / originalWidth;
                break;

            case "H": // 指定高，宽按比例
                targetWidth = originalWidth * targetHeight / originalHeight;
                break;

            case "Cut": // 裁剪
                if ((double)originalWidth / originalHeight > (double)targetWidth / targetHeight)
                {
                    sourceHeight = originalHeight;
                    sourceWidth = originalHeight * targetWidth / targetHeight;
                    x = (originalWidth - sourceWidth) / 2;
                }
                else
                {
                    sourceWidth = originalWidth;
                    sourceHeight = originalWidth * targetHeight / targetWidth;
                    y = (originalHeight - sourceHeight) / 2;
                }
                break;
        }

        return (targetWidth, targetHeight, x, y, sourceWidth, sourceHeight);
    }

    #endregion

    #region 水印处理

    /// <summary>
    /// 添加图片水印
    /// </summary>
    /// <param name="imagePath">原图路径</param>
    /// <param name="watermarkPath">水印图片路径</param>
    /// <param name="position">位置(LT=左上,T=上,RT=右上,LC=左中,C=中,RC=右中,LB=左下,B=下,RB=右下)</param>
    /// <param name="opacity">透明度(0-255)</param>
    public static void AddImageWatermark(string imagePath, string watermarkPath, string position = "RB", byte opacity = 255)
    {
        using var original = SKBitmap.Decode(imagePath);
        using var watermark = SKBitmap.Decode(watermarkPath);

        // 计算水印位置
        var (x, y) = CalculateWatermarkPosition(position, original.Width, original.Height,
            watermark.Width, watermark.Height);

        // 创建画布
        using var canvas = new SKCanvas(original);

        // 设置透明度
        using var paint = new SKPaint
        {
            IsAntialias = true,
            FilterQuality = SKFilterQuality.High,
            ColorF = new SKColorF(1, 1, 1, opacity / 255f)
        };

        // 绘制水印
        canvas.DrawBitmap(watermark, x, y, paint);

        // 保存图片
        using var image = SKImage.FromBitmap(original);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 100);
        using var stream = File.OpenWrite(imagePath);
        data.SaveTo(stream);
    }

    /// <summary>
    /// 添加文字水印
    /// </summary>
    /// <param name="imagePath">原图路径</param>
    /// <param name="text">水印文字</param>
    /// <param name="fontSize">字体大小</param>
    /// <param name="color">颜色</param>
    /// <param name="position">位置</param>
    /// <param name="opacity">透明度(0-255)</param>
    public static void AddTextWatermark(string imagePath, string text, float fontSize,
        SKColor color, string position = "RB", byte opacity = 255)
    {
        using var original = SKBitmap.Decode(imagePath);

        // 创建字体
        using var paint = new SKPaint
        {
            TextSize = fontSize,
            IsAntialias = true,
            Color = color,
            TextAlign = SKTextAlign.Left,
            ColorF = new SKColorF(color.Red / 255f, color.Green / 255f,
                color.Blue / 255f, opacity / 255f)
        };

        // 计算文字大小
        var textBounds = new SKRect();
        paint.MeasureText(text, ref textBounds);

        // 计算水印位置
        var (x, y) = CalculateWatermarkPosition(position, original.Width, original.Height,
            (int)textBounds.Width, (int)textBounds.Height);

        // 创建画布并绘制文字
        using var canvas = new SKCanvas(original);
        canvas.DrawText(text, x, y + textBounds.Height, paint);

        // 保存图片
        using var image = SKImage.FromBitmap(original);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 100);
        using var stream = File.OpenWrite(imagePath);
        data.SaveTo(stream);
    }

    /// <summary>
    /// 计算水印位置
    /// </summary>
    private static (float x, float y) CalculateWatermarkPosition(string position,
        int imageWidth, int imageHeight, int watermarkWidth, int watermarkHeight)
    {
        float x = 10, y = 10;

        switch (position)
        {
            case "LT": // 左上
                break;
            case "T":  // 上中
                x = (imageWidth - watermarkWidth) / 2f;
                break;
            case "RT": // 右上
                x = imageWidth - watermarkWidth - 10;
                break;
            case "LC": // 左中
                y = (imageHeight - watermarkHeight) / 2f;
                break;
            case "C":  // 中心
                x = (imageWidth - watermarkWidth) / 2f;
                y = (imageHeight - watermarkHeight) / 2f;
                break;
            case "RC": // 右中
                x = imageWidth - watermarkWidth - 10;
                y = (imageHeight - watermarkHeight) / 2f;
                break;
            case "LB": // 左下
                y = imageHeight - watermarkHeight - 10;
                break;
            case "B":  // 下中
                x = (imageWidth - watermarkWidth) / 2f;
                y = imageHeight - watermarkHeight - 10;
                break;
            case "RB": // 右下
                x = imageWidth - watermarkWidth - 10;
                y = imageHeight - watermarkHeight - 10;
                break;
        }

        return (x, y);
    }

    #endregion

    #region 图片效果

    /// <summary>
    /// 调整亮度
    /// </summary>
    /// <param name="imagePath">图片路径</param>
    /// <param name="brightness">亮度值(-255到255)</param>
    public static void AdjustBrightness(string imagePath, int brightness)
    {
        using var bitmap = SKBitmap.Decode(imagePath);
        using var canvas = new SKCanvas(bitmap);

        var matrix = new float[]{
            1, 0, 0, 0, brightness,
            0, 1, 0, 0, brightness,
            0, 0, 1, 0, brightness,
            0, 0, 0, 1, 0
        };

        using var paint = new SKPaint
        {
            ColorFilter = SKColorFilter.CreateColorMatrix(matrix)
        };

        canvas.DrawBitmap(bitmap, 0, 0, paint);

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 100);
        using var stream = File.OpenWrite(imagePath);
        data.SaveTo(stream);
    }

    /// <summary>
    /// 转换为灰度图
    /// </summary>
    public static void ToGrayscale(string imagePath)
    {
        using var bitmap = SKBitmap.Decode(imagePath);
        using var canvas = new SKCanvas(bitmap);

        var matrix = new float[] {
            0.21f, 0.72f, 0.07f, 0, 0,
            0.21f, 0.72f, 0.07f, 0, 0,
            0.21f, 0.72f, 0.07f, 0, 0,
            0, 0, 0, 1, 0
        };

        using var paint = new SKPaint
        {
            ColorFilter = SKColorFilter.CreateColorMatrix(matrix)
        };

        canvas.DrawBitmap(bitmap, 0, 0, paint);

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 100);
        using var stream = File.OpenWrite(imagePath);
        data.SaveTo(stream);
    }

    /// <summary>
    /// 图片反色
    /// </summary>
    public static void Invert(string imagePath)
    {
        using var bitmap = SKBitmap.Decode(imagePath);
        using var canvas = new SKCanvas(bitmap);

        var matrix = new float[] {
            -1,  0,  0,  0, 255,
             0, -1,  0,  0, 255,
             0,  0, -1,  0, 255,
             0,  0,  0,  1,   0
        };

        using var paint = new SKPaint
        {
            ColorFilter = SKColorFilter.CreateColorMatrix(matrix)
        };

        canvas.DrawBitmap(bitmap, 0, 0, paint);

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 100);
        using var stream = File.OpenWrite(imagePath);
        data.SaveTo(stream);
    }

    /// <summary>
    /// 浮雕效果
    /// </summary>
    public static void Emboss(string imagePath)
    {
        using var bitmap = SKBitmap.Decode(imagePath);
        using var target = new SKBitmap(bitmap.Width, bitmap.Height);

        for (int x = 0; x < bitmap.Width - 1; x++)
        {
            for (int y = 0; y < bitmap.Height - 1; y++)
            {
                var color1 = bitmap.GetPixel(x, y);
                var color2 = bitmap.GetPixel(x + 1, y + 1);

                var r = Math.Abs(color1.Red - color2.Red + 128);
                var g = Math.Abs(color1.Green - color2.Green + 128);
                var b = Math.Abs(color1.Blue - color2.Blue + 128);

                r = Math.Min(255, Math.Max(0, r));
                g = Math.Min(255, Math.Max(0, g));
                b = Math.Min(255, Math.Max(0, b));

                target.SetPixel(x, y, new SKColor((byte)r, (byte)g, (byte)b));
            }
        }

        using var image = SKImage.FromBitmap(target);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 100);
        using var stream = File.OpenWrite(imagePath);
        data.SaveTo(stream);
    }

    #endregion

    #region 图片变换

    /// <summary>
    /// 水平翻转
    /// </summary>
    public static void FlipHorizontal(string imagePath)
    {
        using var bitmap = SKBitmap.Decode(imagePath);
        using var target = new SKBitmap(bitmap.Width, bitmap.Height);
        using var canvas = new SKCanvas(target);

        var matrix = SKMatrix.CreateScale(-1, 1, bitmap.Width / 2f, 0);
        canvas.SetMatrix(matrix);
        canvas.DrawBitmap(bitmap, 0, 0);

        using var image = SKImage.FromBitmap(target);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 100);
        using var stream = File.OpenWrite(imagePath);
        data.SaveTo(stream);
    }

    /// <summary>
    /// 垂直翻转
    /// </summary>
    public static void FlipVertical(string imagePath)
    {
        using var bitmap = SKBitmap.Decode(imagePath);
        using var target = new SKBitmap(bitmap.Width, bitmap.Height);
        using var canvas = new SKCanvas(target);

        var matrix = SKMatrix.CreateScale(1, -1, 0, bitmap.Height / 2f);
        canvas.SetMatrix(matrix);
        canvas.DrawBitmap(bitmap, 0, 0);

        using var image = SKImage.FromBitmap(target);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 100);
        using var stream = File.OpenWrite(imagePath);
        data.SaveTo(stream);
    }

    /// <summary>
    /// 旋转图片
    /// </summary>
    /// <param name="degrees">旋转角度</param>
    public static void Rotate(string imagePath, float degrees)
    {
        using var bitmap = SKBitmap.Decode(imagePath);

        // 计算旋转后的尺寸
        var radians = degrees * Math.PI / 180;
        var sin = Math.Abs(Math.Sin(radians));
        var cos = Math.Abs(Math.Cos(radians));
        var newWidth = (int)(bitmap.Width * cos + bitmap.Height * sin);
        var newHeight = (int)(bitmap.Width * sin + bitmap.Height * cos);

        using var target = new SKBitmap(newWidth, newHeight);
        using var canvas = new SKCanvas(target);

        canvas.Clear(SKColors.Transparent);
        canvas.Translate(newWidth / 2f, newHeight / 2f);
        canvas.RotateDegrees(degrees);
        canvas.Translate(-bitmap.Width / 2f, -bitmap.Height / 2f);

        canvas.DrawBitmap(bitmap, 0, 0);

        using var image = SKImage.FromBitmap(target);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 100);
        using var stream = File.OpenWrite(imagePath);
        data.SaveTo(stream);
    }

    #endregion

    #region 图片压缩

    /// <summary>
    /// 压缩图片
    /// </summary>
    /// <param name="sourcePath">源图路径</param>
    /// <param name="targetPath">目标图路径</param>
    /// <param name="quality">质量(1-100)</param>
    /// <param name="maxWidth">最大宽度</param>
    /// <param name="maxHeight">最大高度</param>
    public static void Compress(string sourcePath, string targetPath, int quality = 75,
        int? maxWidth = null, int? maxHeight = null)
    {
        using var bitmap = SKBitmap.Decode(sourcePath);

        // 计算新尺寸
        var (newWidth, newHeight) = CalculateNewSize(bitmap.Width, bitmap.Height, maxWidth, maxHeight);

        // 如果需要调整尺寸
        if (newWidth != bitmap.Width || newHeight != bitmap.Height)
        {
            using var resized = bitmap.Resize(new SKImageInfo(newWidth, newHeight),
                SKFilterQuality.High);
            using var image = SKImage.FromBitmap(resized);
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, quality);
            using var stream = File.OpenWrite(targetPath);
            data.SaveTo(stream);
        }
        else
        {
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, quality);
            using var stream = File.OpenWrite(targetPath);
            data.SaveTo(stream);
        }
    }

    /// <summary>
    /// 计算新尺寸
    /// </summary>
    private static (int width, int height) CalculateNewSize(int width, int height,
        int? maxWidth, int? maxHeight)
    {
        if (!maxWidth.HasValue && !maxHeight.HasValue)
            return (width, height);

        var ratioX = (double)(maxWidth ?? width) / width;
        var ratioY = (double)(maxHeight ?? height) / height;
        var ratio = Math.Min(ratioX, ratioY);

        if (ratio >= 1)
            return (width, height);

        return ((int)(width * ratio), (int)(height * ratio));
    }

    #endregion
}