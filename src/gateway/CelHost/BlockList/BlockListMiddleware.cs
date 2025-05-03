using Si.Utilites.OperateResult;

namespace CelHost.Server.BlockList
{
    public class BlockListMiddleware
    {
        private readonly RequestDelegate _next;
        public BlockListMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            var blocklistService = context.RequestServices.GetRequiredService<IBlocklistService>();
            var ip = context.Connection.RemoteIpAddress?.ToString();

            if (!string.IsNullOrEmpty(ip) && await blocklistService.CheckIsBlockedAsync(ip))
            {
                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(
                    OperateResult.Failed("请求IP已被封禁"));
                return;
            }

            await _next(context);
        }
    }
}
