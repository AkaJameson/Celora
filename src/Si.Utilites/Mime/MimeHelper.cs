using System.Collections.ObjectModel;

namespace Si.Utilites.Mime;

/// <summary>
/// MIME类型工具类
/// </summary>
public static class MimeHelper
{
    /// <summary>
    /// MIME类型映射字典
    /// </summary>
    private static readonly ReadOnlyDictionary<string, string> MimeTypes = new(new Dictionary<string, string>
    {
        // 文本文件
        { ".txt", "text/plain" },
        { ".csv", "text/csv" },
        { ".html", "text/html" },
        { ".htm", "text/html" },
        { ".css", "text/css" },
        { ".js", "text/javascript" },
        { ".json", "application/json" },
        { ".xml", "application/xml" },
        { ".md", "text/markdown" },
        { ".log", "text/plain" },
        { ".ini", "text/plain" },
        { ".conf", "text/plain" },
        { ".rtf", "application/rtf" },

        // 图片文件
        { ".jpg", "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".png", "image/png" },
        { ".gif", "image/gif" },
        { ".bmp", "image/bmp" },
        { ".ico", "image/x-icon" },
        { ".tiff", "image/tiff" },
        { ".tif", "image/tiff" },
        { ".svg", "image/svg+xml" },
        { ".webp", "image/webp" },
        { ".heic", "image/heic" },
        { ".heif", "image/heif" },

        // 音频文件
        { ".mp3", "audio/mpeg" },
        { ".wav", "audio/wav" },
        { ".ogg", "audio/ogg" },
        { ".m4a", "audio/mp4" },
        { ".wma", "audio/x-ms-wma" },
        { ".aac", "audio/aac" },
        { ".mid", "audio/midi" },
        { ".midi", "audio/midi" },
        { ".flac", "audio/flac" },

        // 视频文件
        { ".mp4", "video/mp4" },
        { ".avi", "video/x-msvideo" },
        { ".wmv", "video/x-ms-wmv" },
        { ".mov", "video/quicktime" },
        { ".flv", "video/x-flv" },
        { ".mkv", "video/x-matroska" },
        { ".webm", "video/webm" },
        { ".m4v", "video/mp4" },
        { ".mpeg", "video/mpeg" },
        { ".mpg", "video/mpeg" },
        { ".3gp", "video/3gpp" },

        // 文档文件
        { ".doc", "application/msword" },
        { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        { ".xls", "application/vnd.ms-excel" },
        { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
        { ".ppt", "application/vnd.ms-powerpoint" },
        { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
        { ".pdf", "application/pdf" },
        { ".odt", "application/vnd.oasis.opendocument.text" },
        { ".ods", "application/vnd.oasis.opendocument.spreadsheet" },
        { ".odp", "application/vnd.oasis.opendocument.presentation" },

        // 压缩文件
        { ".zip", "application/zip" },
        { ".rar", "application/x-rar-compressed" },
        { ".7z", "application/x-7z-compressed" },
        { ".tar", "application/x-tar" },
        { ".gz", "application/gzip" },
        { ".bz2", "application/x-bzip2" },

        // 字体文件
        { ".ttf", "font/ttf" },
        { ".otf", "font/otf" },
        { ".woff", "font/woff" },
        { ".woff2", "font/woff2" },
        { ".eot", "application/vnd.ms-fontobject" },

        // 可执行文件和二进制文件
        { ".exe", "application/x-msdownload" },
        { ".dll", "application/x-msdownload" },
        { ".bin", "application/octet-stream" },
        { ".iso", "application/x-iso9660-image" },
        { ".img", "application/octet-stream" },
        { ".msi", "application/x-msdownload" },

        // 开发相关
        { ".cs", "text/plain" },
        { ".java", "text/plain" },
        { ".py", "text/plain" },
        { ".cpp", "text/plain" },
        { ".h", "text/plain" },
        { ".hpp", "text/plain" },
        { ".sql", "text/plain" },
        { ".php", "text/plain" },
        { ".rb", "text/plain" },
        { ".go", "text/plain" },
        { ".swift", "text/plain" },
        { ".kt", "text/plain" },
        { ".ts", "text/plain" },
        { ".jsx", "text/plain" },
        { ".tsx", "text/plain" },

        // 其他常见文件类型
        { ".apk", "application/vnd.android.package-archive" },
        { ".ipa", "application/octet-stream" },
        { ".swf", "application/x-shockwave-flash" },
        { ".torrent", "application/x-bittorrent" },
        { ".psd", "image/vnd.adobe.photoshop" },
        { ".ai", "application/postscript" },
        { ".eps", "application/postscript" }
    });

    /// <summary>
    /// 获取文件的MIME类型
    /// </summary>
    /// <param name="fileName">文件名或文件路径</param>
    /// <returns>MIME类型</returns>
    public static string GetMimeType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return MimeTypes.TryGetValue(extension, out var mimeType) ? mimeType : "application/octet-stream";
    }

    /// <summary>
    /// 获取MIME类型对应的文件扩展名（不包含点号）
    /// </summary>
    /// <param name="mimeType">MIME类型</param>
    /// <returns>文件扩展名（不包含点号），如果未找到则返回null</returns>
    public static string? GetExtension(string mimeType)
    {
        var extension = MimeTypes.FirstOrDefault(x => x.Value.Equals(mimeType, StringComparison.OrdinalIgnoreCase)).Key;
        return extension?.TrimStart('.');
    }

    /// <summary>
    /// 获取所有支持的MIME类型
    /// </summary>
    /// <returns>MIME类型列表</returns>
    public static IEnumerable<string> GetSupportedMimeTypes()
    {
        return MimeTypes.Values.Distinct();
    }

    /// <summary>
    /// 获取所有支持的文件扩展名（包含点号）
    /// </summary>
    /// <returns>文件扩展名列表</returns>
    public static IEnumerable<string> GetSupportedExtensions()
    {
        return MimeTypes.Keys;
    }

    /// <summary>
    /// 判断文件扩展名是否受支持
    /// </summary>
    /// <param name="extension">文件扩展名（可以包含或不包含点号）</param>
    /// <returns>是否支持</returns>
    public static bool IsSupportedExtension(string extension)
    {
        extension = extension.StartsWith(".") ? extension : "." + extension;
        return MimeTypes.ContainsKey(extension.ToLowerInvariant());
    }

    /// <summary>
    /// 判断MIME类型是否受支持
    /// </summary>
    /// <param name="mimeType">MIME类型</param>
    /// <returns>是否支持</returns>
    public static bool IsSupportedMimeType(string mimeType)
    {
        return MimeTypes.Values.Contains(mimeType, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 获取指定MIME类型分类的所有扩展名
    /// </summary>
    /// <param name="category">MIME类型分类（如：image, audio, video等）</param>
    /// <returns>文件扩展名列表</returns>
    public static IEnumerable<string> GetExtensionsByCategory(string category)
    {
        return MimeTypes.Where(x => x.Value.StartsWith($"{category}/"))
                       .Select(x => x.Key);
    }

    /// <summary>
    /// 判断文件是否为图片
    /// </summary>
    public static bool IsImage(string fileName)
    {
        var mimeType = GetMimeType(fileName);
        return mimeType.StartsWith("image/");
    }

    /// <summary>
    /// 判断文件是否为音频
    /// </summary>
    public static bool IsAudio(string fileName)
    {
        var mimeType = GetMimeType(fileName);
        return mimeType.StartsWith("audio/");
    }

    /// <summary>
    /// 判断文件是否为视频
    /// </summary>
    public static bool IsVideo(string fileName)
    {
        var mimeType = GetMimeType(fileName);
        return mimeType.StartsWith("video/");
    }

    /// <summary>
    /// 判断文件是否为文本文件
    /// </summary>
    public static bool IsText(string fileName)
    {
        var mimeType = GetMimeType(fileName);
        return mimeType.StartsWith("text/");
    }

    /// <summary>
    /// 判断文件是否为压缩文件
    /// </summary>
    public static bool IsCompressed(string fileName)
    {
        var mimeType = GetMimeType(fileName);
        return mimeType.Contains("zip") || mimeType.Contains("compressed") || 
               mimeType.Contains("tar") || mimeType.Contains("gzip") || 
               mimeType.Contains("bzip");
    }

    /// <summary>
    /// 判断文件是否为文档
    /// </summary>
    public static bool IsDocument(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return new[] { ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", 
                      ".pdf", ".odt", ".ods", ".odp" }.Contains(extension);
    }

    /// <summary>
    /// 判断文件是否为可执行文件
    /// </summary>
    public static bool IsExecutable(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return new[] { ".exe", ".msi", ".dll" }.Contains(extension);
    }

    /// <summary>
    /// 判断文件是否为字体文件
    /// </summary>
    public static bool IsFont(string fileName)
    {
        var mimeType = GetMimeType(fileName);
        return mimeType.StartsWith("font/") || 
               mimeType.Contains("fontobject");
    }
} 