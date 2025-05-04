using CelHost.Server.Dto;
using CelHost.Server.Options;
using CelHost.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CelHost.Server.ServicesImpl
{
    public class FileServiceImpl : IFileServiceImpl
    {
        private readonly string _basePath;

        public FileServiceImpl(IOptions<FileServiceOptions> options)
        {
            _basePath = Path.GetFullPath(options.Value.BasePath ?? throw new ArgumentNullException("BasePath"));
        }

        private string GetSafeFullPath(string relativePath)
        {
            var fullPath = Path.GetFullPath(Path.Combine(_basePath, relativePath ?? string.Empty));

            if (!fullPath.StartsWith(_basePath, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("非法访问路径");

            return fullPath;
        }

        public List<FileInfoDto> GetFileList(string relativePath = "")
        {
            var result = new List<FileInfoDto>();
            string targetPath;

            try
            {
                targetPath = GetSafeFullPath(relativePath);
            }
            catch
            {
                return result;
            }

            if (!Directory.Exists(targetPath))
                return result;

            foreach (var dir in Directory.GetDirectories(targetPath))
            {
                var info = new DirectoryInfo(dir);
                result.Add(new FileInfoDto
                {
                    Name = info.Name,
                    RelativePath = Path.Combine(relativePath, info.Name).Replace("\\", "/"),
                    IsDirectory = true,
                    Size = 0,
                    LastModified = info.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }

            foreach (var file in Directory.GetFiles(targetPath))
            {
                var info = new FileInfo(file);
                result.Add(new FileInfoDto
                {
                    Name = info.Name,
                    RelativePath = Path.Combine(relativePath, info.Name).Replace("\\", "/"),
                    IsDirectory = false,
                    Size = info.Length,
                    LastModified = info.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }

            return result;
        }

        public FileStreamResult DownloadFile(string relativePath)
        {
            var fullPath = GetSafeFullPath(relativePath);

            if (!File.Exists(fullPath))
                throw new FileNotFoundException("文件不存在");

            var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            return new FileStreamResult(stream, "application/octet-stream")
            {
                FileDownloadName = Path.GetFileName(fullPath)
            };
        }
    }
}
