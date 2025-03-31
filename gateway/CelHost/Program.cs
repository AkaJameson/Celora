using CelHost.Data;
using CelHost.Proxy.DynamicProvider;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Si.EntityFramework.AutoMigration;
using Yarp.ReverseProxy.Configuration;

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
await app.AutoMigrationAsync<HostContext>(new AutoMigrationOptions
{
    BackupDatabase = false
});
app.UseCors("AllowAll");
app.UseRouting();
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
app.MapReverseProxy();
app.UseAuthentication();
app.UseAuthorization();
Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot"));
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot")),
    RequestPath = ""
});
app.MapControllers();
app.Run();


