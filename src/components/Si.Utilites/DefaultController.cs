using Microsoft.AspNetCore.Mvc;

namespace Si.Utilites
{
    [ResponseCache(Duration = 60, VaryByHeader = "User-Agent")]
    [Route("api/[controller]/[action]")]
    public class DefaultController:ControllerBase
    {
    }
}
