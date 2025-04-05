using CelHost.Database;
using CelHost.Models;
using CelHost.Proxy;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Serilog.Core;
using Si.EntityFramework.AutoMigration;
using Si.Logging;
using Si.Utilites;
using Si.Utilites.OperateResult;
try
{

    var builder = WebApplication.CreateBuilder(args);
    builder.AddLogging();
    if (builder.Environment.IsDevelopment())
    {
        builder.Services.AddCors(option =>
        {
            option.AddPolicy("AllowAll", builder =>
            {
                builder.SetIsOriginAllowed(_ => true)
                       .AllowAnyHeader()
                       .AllowAnyMethod()
                       .AllowCredentials();
            });
        });
    }
    else
    {
        var hostUrls = builder.Configuration.GetValue<string>("AllowedHosts").Split(";").ToArray();
        builder.Services.AddCors(option =>
        {
            option.AddPolicy("AllowAll", builder =>
            {
                builder.WithOrigins(hostUrls)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
            });
        });
    }
    var rateLimit = builder.Configuration.GetSection("RateLimiter").Get<RateLimit>();
    builder.Services.AddRateLimiter(options =>
    {
        // 定义网关管理接口限流策略
        options.AddFixedWindowLimiter("GatewayApiLimit", opt =>
        {
            opt.Window = TimeSpan.FromSeconds(rateLimit?.WindowSize ?? 20);
            opt.PermitLimit = rateLimit?.MaxRequests ?? 100;
            opt.QueueLimit = rateLimit?.QueueLimit ?? 2;
        });

        // 自定义拒绝响应
        options.OnRejected = async (context, _) =>
        {
            context.HttpContext.Response.StatusCode = 429;
            context.HttpContext.Response.ContentType = "application/json";
            await context.HttpContext.Response.WriteAsJsonAsync(OperateResult.Failed("管理接口请求超限"));
        };
    });
    builder.AddProxyInjection();
    builder.Services.AddAuthentication();
    builder.Services.AddAuthorization();
    builder.Services.AddControllers();
    builder.Services.AddScoped<ILogService, LogService>();
    var connectionStr = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<HostContext>(options =>
    {
        options.UseSqlite(connectionStr);
    });
    var app = builder.Build();
    app.Services.RegisterShellScope(app.Configuration);
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";

            var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
            var ex = exceptionHandlerPathFeature?.Error;
            var logger = context.RequestServices.GetRequiredService<ILogService>();
            logger.Error(ex?.ToString() ?? "未知错误");

            await context.Response.WriteAsJsonAsync(new
            {
                code = 500,
                message = "服务器内部异常，请联系管理员。"
            });
        });
    });
    await app.AutoMigrationAsync<HostContext>(new AutoMigrationOptions
    {
        BackupDatabase = false
    });
    app.UseCors("AllowAll");
    app.UseRouting();
    app.UseProxy();
    app.Use(async (context, next) =>
    {
        if (context.Request.Path == "/")
        {
            context.Response.Redirect("/index.html");
        }
        else
        {
            await next();
        }
    });
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();
    Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "wwwroot"));
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "wwwroot")),
        RequestPath = ""
    });
    app.MapControllers();
    app.Run();
}
catch(Exception ex)
{
   Logger.Error(ex.Message + ex.ToString());
}


