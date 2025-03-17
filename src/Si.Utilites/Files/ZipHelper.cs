using Ionic.Zip;

namespace Si.Utilites.Files;

public static class ZipHelper
{
    /// <summary>
    /// 压缩文件或目录
    /// </summary>
    /// <param name="sourcePath">源文件或目录路径</param>
    /// <param name="zipPath">ZIP文件保存路径</param>
    /// <param name="password">密码（可选）</param>
    public static void Compress(string sourcePath, string zipPath, string? password = null)
    {
        if (Directory.Exists(sourcePath))
        {
            CompressDirectory(sourcePath, zipPath, password);
        }
        else if (File.Exists(sourcePath))
        {
            CompressFile(sourcePath, zipPath, password);
        }
        else
        {
            throw new FileNotFoundException("源文件或目录不存在", sourcePath);
        }
    }

    /// <summary>
    /// 解压ZIP文件
    /// </summary>
    /// <param name="zipPath">ZIP文件路径</param>
    /// <param name="extractPath">解压目标路径</param>
    /// <param name="password">密码（可选）</param>
    public static void Decompress(string zipPath, string extractPath, string? password = null)
    {
        if (!File.Exists(zipPath))
        {
            throw new FileNotFoundException("ZIP文件不存在", zipPath);
        }

        // 确保目标目录存在
        Directory.CreateDirectory(extractPath);

        using var zip = new ZipFile(zipPath);
        if (!string.IsNullOrEmpty(password))
        {
            zip.Password = password;
        }
        zip.ExtractAll(extractPath);
    }

    /// <summary>
    /// 压缩目录
    /// </summary>
    private static void CompressDirectory(string sourceDir, string zipPath, string? password)
    {
        using var zip = new ZipFile(zipPath);
        if (!string.IsNullOrEmpty(password))
        {
            zip.Password = password;
        }

        var files = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var relativePath = Path.GetRelativePath(sourceDir, file);
            zip.AddFile(file, Path.GetDirectoryName(relativePath));
        }
        zip.Save();
    }

    /// <summary>
    /// 压缩单个文件
    /// </summary>
    private static void CompressFile(string sourceFile, string zipPath, string? password)
    {
        using var zip = new ZipFile(zipPath);
        if (!string.IsNullOrEmpty(password))
        {
            zip.Password = password;
        }

        zip.AddFile(sourceFile);
        zip.Save();
    }

    /// <summary>
    /// 压缩字符串内容
    /// </summary>
    /// <param name="content">要压缩的内容</param>
    /// <param name="entryName">ZIP中的文件名</param>
    /// <param name="password">密码（可选）</param>
    /// <returns>压缩后的字节数组</returns>
    public static byte[] CompressString(string content, string entryName, string? password = null)
    {
        using var zip = new ZipFile();
        if (!string.IsNullOrEmpty(password))
        {
            zip.Password = password;
        }

        zip.AddEntry(entryName, content);
        using var memoryStream = new MemoryStream();
        zip.Save(memoryStream);
        return memoryStream.ToArray();
    }

    /// <summary>
    /// 解压字符串内容
    /// </summary>
    /// <param name="zipBytes">ZIP文件的字节数组</param>
    /// <param name="entryName">要解压的文件名</param>
    /// <param name="password">密码（可选）</param>
    /// <returns>解压后的字符串内容</returns>
    public static string DecompressString(byte[] zipBytes, string entryName, string? password = null)
    {
        using var memoryStream = new MemoryStream(zipBytes);
        using var zip = ZipFile.Read(memoryStream);
        
        if (!string.IsNullOrEmpty(password))
        {
            zip.Password = password;
        }

        var entry = zip[entryName];
        if (entry == null)
        {
            throw new FileNotFoundException($"ZIP文件中未找到指定的文件: {entryName}");
        }

        using var reader = new StreamReader(entry.OpenReader());
        return reader.ReadToEnd();
    }
} 