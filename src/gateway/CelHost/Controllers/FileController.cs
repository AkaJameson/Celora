using CelHost.Server.Services;
using CelHost.Server.ServicesImpl;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Si.Utilites;
using Si.Utilites.OperateResult;

namespace CelHost.Server.Controllers
{
    [ApiController]
    [Authorize]
    public class FileController : DefaultController
    {
        private readonly IFileServiceImpl _fileService;

        public FileController(IFileServiceImpl fileService)
        {
            _fileService = fileService;
        }

        [HttpGet]
        public OperateResult List([FromQuery] string path = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = _fileService.GetFileList(path, page, pageSize);
            return OperateResult.Successed(result);
        }

        [HttpGet]
        public IActionResult Download([FromQuery] string filePath)
        {
            try
            {
                return _fileService.DownloadFile(filePath);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid("非法路径访问");
            }
            catch (FileNotFoundException)
            {
                return NotFound("文件不存在");
            }
        }
    }
}
