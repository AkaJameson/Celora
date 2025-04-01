using CelHost.Data;
using CelHost.Proxy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Si.EntityFramework.AutoMigration;
using Si.Logging;
using Yarp.ReverseProxy.Configuration;

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
builder.Services.AddReverseProxy()
                .LoadFromMemory(new List<RouteConfig>(), new List<ClusterConfig>());
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
app.InitializeProxy();
app.MapReverseProxy();
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


