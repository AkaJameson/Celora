using CelHost.BlockList;
using CelHost.Database;
using CelHost.Hosts;
using CelHost.Hubs;
using CelHost.Models;
using CelHost.Proxy;
using CelHost.Services;
using CelHost.ServicesImpl;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Si.EntityFramework.Extension.Extensions;
using Si.EntityFramework.Extension.UnitofWorks;
using Si.Logging;
using Si.Utilites;
using Si.Utilites.OperateResult;
using System.Text;
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
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1",new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Gateway", Version = "v1" });
    });
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
            // 封禁处理
            var ip = context.HttpContext.Connection.RemoteIpAddress?.ToString();
            if (!string.IsNullOrEmpty(ip))
            {
                var monitor = context.HttpContext.RequestServices
                    .GetRequiredService<BlackListMonitor>();
                await monitor.RecordViolation(ip);
            }
        };
    });
    builder.AddProxyInjection();
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("JwtSettings");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"])),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
        options.Events = new JwtBearerEvents()
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Cookies["Access-Token"];
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            }
        };
    });
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddAuthorization();
    builder.Services.AddControllers();
    builder.Services.AddScoped<ILogService, LogService>();
    builder.Services.AddScoped<IBlocklistService, BlocklistService>();
    builder.Services.AddScoped<BlackListMonitor>();
    builder.Services.AddScoped<DestinationHealthCheck>();
    builder.Services.AddUnitofWork();
    builder.Services.AddHttpContextAccessor();
    var connectionStr = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<HostContext>(options =>
    {
        options.UseSqlite(connectionStr);
    });
    builder.Services.AddSignalR();
    builder.Services.AddHostedService<HealthMonitorWorker>();
    #region 注册模块
    builder.Services.AddScoped<IUserService, UserServiceImpl>();
    builder.Services.AddScoped<ISystemDictionaryServiceImpl, SystemDictionaryServiceImpl>();
    builder.Services.AddScoped<IHealthCheckServiceImpl, HealthCheckServiceImpl>();
    builder.Services.AddScoped<IClusterServiceImpl, ClusterServiceImpl>();
    builder.Services.AddScoped<INodeServiceImpl, NodeServiceImpl>();
    builder.Services.AddScoped<IGatewayServiceImpl, GatewayServiceImpl>();
    #endregion
    var app = builder.Build();
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        });
    }
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
    app.UseMiddleware<BlockListMiddleware>();
    app.UseAuthentication();
    app.UseAuthorization();
    Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "wwwroot"));
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "wwwroot")),
        RequestPath = ""
    });
    var healthRoute = app.Configuration.GetValue<string>("HealthStateHubRoute:Route");
    app.MapHub<HealthHub>(healthRoute);
    app.MapControllers();
    app.Run();
}
catch (Exception ex)
{
    Logger.Error(ex.Message + ex.ToString());
}


