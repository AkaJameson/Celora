# Si.CoreHub 模块化系统

Si.CoreHub 模块化系统是一个轻量级的模块加载和管理框架，允许将应用程序功能划分为独立的模块，实现松耦合的插件式架构。

## 功能特点

- **松耦合架构**：每个模块独立开发、测试和部署
- **热插拔**：支持在不重启应用的情况下动态加载/卸载模块
- **配置隔离**：每个模块有自己独立的配置文件
- **生命周期管理**：完整控制模块的初始化、启动和关闭
- **依赖注入集成**：与ASP.NET Core的依赖注入系统无缝集成
- **路由集成**：自动注册模块中的控制器和路由

## 快速入门

### 1. 创建模块

模块必须继承 `PackBase` 类并实现其抽象方法：

```csharp
public class MyModule : PackBase
{
    // 模块元数据（可选覆盖）
    public override string Name => "MyModule";
    public override string Version => "1.0.0";
    public override string Description => "这是一个示例模块";
    public override string Author => "John Doe";

    // 初始化模块
    public override void Initialize()
    {
        // 模块初始化代码
    }

    // 注册服务
    public override void ConfigurationServices(WebApplicationBuilder builder, IServiceCollection services)
    {
        // 注册模块需要的服务
        services.AddScoped<IMyService, MyService>();
    }

    // 配置中间件和路由
    public override void Configuration(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        // 配置模块的中间件和路由
        routes.MapGet("/mymodule/hello", () => "Hello from MyModule!");
    }

    // 模块启动（所有模块加载完成后）
    public override void Startup(IServiceProvider serviceProvider)
    {
        // 所有模块加载完成后的启动代码
    }

    // 模块关闭
    public override void Shutdown()
    {
        // 释放资源和清理
    }
}
```

### 2. 创建模块配置文件

在模块目录下创建与模块同名的JSON配置文件：

```json
{
  "MyModule": {
    "Setting1": "Value1",
    "Setting2": 123,
    "ConnectionStrings": {
      "DefaultConnection": "..."
    }
  }
}
```

### 3. 注册模块系统

在应用启动代码中注册模块系统：

```csharp
var builder = WebApplication.CreateBuilder(args);

// 注册模块系统
builder.AddPackages(options =>
{
    options.FilePath = Path.Combine(AppContext.BaseDirectory, "Modules");
    options.EnableHotReload = true;
    options.IgnoreFailedModules = true;
});

var app = builder.Build();

// 使用模块系统
app.UsePackages(app, app.Services.GetRequiredService<IServiceProvider>());

app.Run();
```

### 4. 访问模块配置

在模块中访问配置：

```csharp
// 直接在模块类中
var connectionString = GetConfiguration("ConnectionStrings:DefaultConnection");

// 通过依赖注入
public class MyService
{
    private readonly IPackConfiguration<MyModule> _configuration;

    public MyService(IPackConfiguration<MyModule> configuration)
    {
        _configuration = configuration;
        var setting = _configuration["Setting1"];
        
        // 或者绑定到类
        var options = _configuration.GetOptions<MyOptions>("MyModule");
    }
}
```

## 模块目录结构

模块应该按照以下目录结构组织：

```
Modules/
  ├── ModuleA/
  │     ├── ModuleA.dll          # 模块程序集
  │     ├── ModuleA.json         # 模块配置文件
  │     └── Resources/           # 本地化资源目录（可选）
  │          ├── Messages.zh-CN.json
  │          └── Messages.en-US.json  
  └── ModuleB/
        ├── ModuleB.dll
        ├── ModuleB.json
        └── ...
```

## 高级功能

### 模块间通信

模块可以通过依赖注入系统共享服务进行通信：

```csharp
// 在模块A中注册服务
public override void ConfigurationServices(WebApplicationBuilder builder, IServiceCollection services)
{
    services.AddSingleton<ISharedService, SharedService>();
}

// 在模块B中使用服务
public class ModuleBService
{
    private readonly ISharedService _sharedService;

    public ModuleBService(ISharedService sharedService)
    {
        _sharedService = sharedService;
    }
}
```

### 模块依赖管理

如果模块之间有依赖关系，可以通过在初始化时检查依赖模块是否存在：

```csharp
public override void Initialize()
{
    // 检查依赖模块
    var dependentModule = PackLoader.GetPackInstance("DependentModule");
    if (dependentModule == null)
    {
        throw new Exception("缺少依赖模块: DependentModule");
    }
}
```

### 模块配置热更新

监听模块配置变更：

```csharp
public class ConfigAwareService
{
    private MyOptions _options;
    private readonly IDisposable _changeSubscription;

    public ConfigAwareService(IPackConfiguration<MyModule> configuration)
    {
        // 初始化配置
        _options = configuration.GetOptions<MyOptions>("MySettings");
        
        // 监听配置变更
        _changeSubscription = configuration.OnChange("MySettings", section =>
        {
            // 更新本地配置
            var newOptions = new MyOptions();
            section.Bind(newOptions);
            _options = newOptions;
        });
    }
    
    public void Dispose()
    {
        _changeSubscription?.Dispose();
    }
}
```

## 最佳实践

1. **保持模块独立**：模块应该是自包含的，避免直接依赖其他模块的内部实现
2. **使用接口通信**：通过定义清晰的接口实现模块间通信
3. **合理划分模块**：模块应该表示应用程序的一个完整功能或领域
4. **异常处理**：模块内部异常不应导致整个应用程序崩溃
5. **合理使用配置**：敏感配置应加密存储，依赖特定环境的配置应适当命名 