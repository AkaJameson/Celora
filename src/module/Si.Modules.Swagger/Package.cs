using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Si.Package.Abstraction;
using Si.Package.Core;
using Si.Package.Entitys;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Collections.Concurrent;
using System.Reflection;

namespace Si.Modules.Swagger
{
    public class Package : PackBase
    {
        public override string Name => "Swagger文档管理";
        public override string Version => "1.0.0";
        public override string Description => "Swagger文档管理插件";
        public override string Author => "SimonJameson";
        public override string Contact => "jingjuexin@Gamil.com";
        public override void ConfigurationServices(WebApplicationBuilder builder, IServiceCollection services)
        {
            // 1. 首先注册Swagger基础服务
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                // 配置XML注释
                var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml");
                foreach (var xmlFile in xmlFiles)
                {
                    c.IncludeXmlComments(xmlFile);
                }

                // 配置JWT认证
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
                });
        }

        public override void Configuration(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var configuration = serviceProvider.GetRequiredService<IPackConfiguration<Package>>();
            // 2. 配置Swagger中间件
            app.UseSwagger(c =>
            {
                c.RouteTemplate = "swagger/{documentname}/swagger.json";
            });

            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = $"{configuration.GetValue<string>("RoutePrefix")}";
                c.DocumentTitle = $"{configuration.GetValue<string>("DocumentTitle")}";
                c.DefaultModelsExpandDepth(-1);
                c.DefaultModelRendering(ModelRendering.Model);
                c.DocExpansion(DocExpansion.None);
                c.EnableDeepLinking();
                c.DisplayOperationId();
                c.DisplayRequestDuration();
                c.EnableFilter();
            });
        }

        public override void Startup(IServiceProvider serviceProvider)
        {
            // 3. 在Startup中，所有模块都已加载完成，此时我们可以获取所有模块信息
            var modules = PackLoader.GetAllPackInstances();
            var configuration = serviceProvider.GetRequiredService<IPackConfiguration<Package>>();
            // 获取Swagger服务
            var swaggerGenOptions = serviceProvider.GetRequiredService<IOptions<SwaggerGenOptions>>().Value;

            // 为每个模块创建Swagger文档
            foreach (var module in modules)
            {
                var moduleInfo = new OpenApiInfo
                {
                    Title = module.Name,
                    Version = module.Version,
                    Description = module.Description,
                    Contact = new OpenApiContact
                    {
                        Name = module.Author,
                        Email = module.Contact
                    },
                    License = new OpenApiLicense
                    {
                        Name = configuration.GetSection("OpenApiLicense").GetValue<string>("Name") ?? "MIT",
                        Url = new Uri($"{configuration.GetSection("OpenApiLicense").GetValue<string>("Url") ?? "https://opensource.org/licenses/MIT"}")
                    }
                };

                // 注册模块的Swagger文档
                swaggerGenOptions.SwaggerDoc(module.Name, moduleInfo);

                // 配置API分组
                swaggerGenOptions.DocInclusionPredicate((docName, apiDesc) =>
                {
                    if (!apiDesc.TryGetMethodInfo(out var methodInfo))
                    {
                        return false;
                    }

                    // 获取控制器或方法上的ApiGroup特性
                    var groupAttribute = methodInfo.DeclaringType?.GetCustomAttribute<ApiGroupAttribute>()
                        ?? methodInfo.GetCustomAttribute<ApiGroupAttribute>();

                    // 如果没有ApiGroup特性，则使用模块名称
                    var groupName = groupAttribute?.GroupName ?? methodInfo.DeclaringType?.Assembly.GetName().Name;

                    return docName == groupName;
                });
            }
        }
    }
}
