using AutoMigrationTest;
using Microsoft.EntityFrameworkCore;
using Si.EntityFramework.AutoMigration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<TestDbContext>(option => option.UseSqlite(@"Data Source=C:\Users\15788\AppData\Local\Programs\IotCenter\Windows\database\Database.db"));
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
    await dbContext.AutoMigrationAsync();
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthorization();

app.MapControllers();

app.Run();
