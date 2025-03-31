using CelHost.Data;
using CelHost.Proxy.DynamicProvider;
using CelHost.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Yarp.ReverseProxy.Configuration;

try
{
    var builder = WebApplication.CreateBuilder(args);
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
    var proxyConfigProvider = new ProxyProvider();
    builder.Services.AddSingleton<IProxyConfigProvider>(proxyConfigProvider);
    builder.Services.AddSingleton(proxyConfigProvider);
    builder.Services.AddReverseProxy();
    builder.Services.AddAuthentication();
    builder.Services.AddAuthorization();
    builder.Services.AddControllers();
    var connectionStr = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<HostContext>(options =>
    {
        options.UseSqlite(connectionStr);
    });
    var app = builder.Build();
    var dbContext = app.Services.GetRequiredService<HostContext>();

    app.UseCors("AllowAll");
    app.UseRouting();
    app.MapReverseProxy();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")),
        RequestPath = "/"
    });
    app.Run();
}
catch(Exception ex)
{
    LogCenter.Error(ex.Message, ex);
}

