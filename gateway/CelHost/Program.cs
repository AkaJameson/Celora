using Microsoft.AspNetCore.Cors.Infrastructure;

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
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddControllers();
var app = builder.Build();
app.UseCors("AllowAll");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

