using CelHost.Server.Dto;
using Microsoft.AspNetCore.Mvc;

namespace CelHost.Server.Services
{
    public interface IFileServiceImpl
    {
        FileStreamResult DownloadFile(string relativePath);
        List<FileInfoDto> GetFileList(string relativePath = "", int page = 1, int pageSize = 20);
    }
}