# Si.CoreHub

一个模块化、高扩展性的ASP.NET Core核心库，集成常用工具、日志、缓存、事件总线、模块化开发等功能，快速构建应用。

## 核心功能

## 🧩 模块化架构

动态程序集加载 - 支持按需加载DLL模块

热插拔配置 - 模块独立配置文件自动加载

生命周期管理- 通过PackBase基类实现模块初始化

## 📦 核心组件

服务定位器 - 全局服务访问 (ServiceLocator)

跨平台支持 - Linux命令执行 (LinuxRunCommand)

系统工具 - WMIC查询工具 (WmicUtils)

### 🚀 基础服务

日志系统 - 分级日志记录 + Serilog集成 (ILogHub)

内存缓存 - 支持绝对/滑动过期策略 (ICacheService)

事件总线 - 内存和RabbitMQ两种实现 (IEventBus)

### 🔧 扩展工具

枚举扩展 - 描述信息获取 (EnumExtension)

JSON工具 - 序列化/反序列化增强 (JsonExtensions)

Web扩展 - Kestrel配置/模块加载中间件

基础配置

```c#
var builder = WebApplication.CreateBuilder(args);

// 启用内存缓存
builder.Services.UseMemoryCache();

// 配置模块加载
builder.Services.AddPackages(options => {
    options.FilePath = "./Modules";
});

// 使用事件总线（RabbitMQ示例）
builder.Services.AddRabbitMqEventBus(opts => {
    opts.HostName = "rabbitmq.local";
    opts.Port = 5672;
});

var app = builder.Build();

public class SampleModule : PackBase
{
    public override void ConfigurationServices(IServiceCollection services)
    {
        services.AddScoped<IMyService, MyService>();
    }

public override void Configuration(IApplicationBuilder app, ...)
{
    // 注册模块路由
}

}
```

事件总线使用

```
var eventBus = ServiceLocator.GetService<IEventBus>();
await eventBus.PublishAsync(new UserCreatedEvent());

// 订阅事件
var subscription = eventBus.Subscribe<UserCreatedEvent>(async e => {
    // 处理事件
});
```

架构设计
Si.CoreHub
├── Core        - 核心组件
├── Logs        - 日志系统
├── Cache       - 缓存服务
├── EventBus    - 事件总线
├── Package     - 模块化支持
└── Extension   - 扩展工具

日志配置

```c#
// 初始化日志路径
LogSetting.Init("./Logs");

// 系统级日志配置
builder.Host.UseLog("./SystemLogs");
```

模块约定

每个模块独立文件夹

包含[模块名].dll和[模块名].json

继承PackBase实现初始化逻辑

许可证
MIT License © 2023 Si.CoreHub Team