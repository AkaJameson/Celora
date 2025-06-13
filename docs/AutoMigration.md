# Si.EntityFramework.AutoMigration

一个轻量级的Entity Framework Core自动迁移工具，无需依赖EF Core Migration。支持多种数据库，包括SQL Server、SQLite、MySQL和PostgreSQL。

## 特点

- 无需定义Migration类或执行Migration命令
- 在应用程序启动时自动检测并应用数据库结构变更
- 支持表的创建、修改表结构
- 可选择性地删除表和列
- 支持迁移历史记录
- 支持多种数据库

## 安装

```bash
dotnet add package Si.EntityFramework.AutoMigration
```

## 使用方法

### 基本用法

在应用程序启动时添加以下代码：

```csharp
using Si.EntityFramework.AutoMigration;

// 在Web应用的Program.cs或Startup.cs中
var app = builder.Build();

// 确保数据库结构与模型一致
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<YourDbContext>();
    dbContext.AutoMigration();
}

app.Run();
```

或者在控制台应用程序中：

```csharp
   public static class DbContextExtensions
   {
       public static void AddAutoMigrationProvider(this IServiceCollection services)
       {
           services.AddScoped<MigrationExecuter>();
           services.AddScoped<MigrationStepProcessor>();
       }
       /// <summary>
       /// 同步检查并更新数据库表结构
       /// </summary>
       /// <param name="context">数据库上下文</param>
       /// <param name="options">迁移选项</param>
       internal static void AutoMigration(this DbContext context, IServiceProvider sp, AutoMigrationOptions options = null)
       {
           AutoMigrationAsync(context, sp, options).GetAwaiter().GetResult();
       }

       /// <summary>
       /// 异步检查并更新数据库表结构
       /// </summary>
       /// <param name="context">数据库上下文</param>
       /// <param name="options">迁移选项</param>
       internal static async Task AutoMigrationAsync(this DbContext context, IServiceProvider sp, AutoMigrationOptions options = null)
       {
           options = options ?? new AutoMigrationOptions();
           var executer = sp.GetRequiredService<MigrationExecuter>();
           await executer.Migrate(context, options);
       }

       public static async Task AutoMigrationAsync<T>(this WebApplication app, AutoMigrationOptions options = null) where T : DbContext
       {
           using var scope = app.Services.CreateScope();
           var context = scope.ServiceProvider.GetRequiredService<T>();
           if (context == null)
           {
               throw new ArgumentNullException(nameof(context));
           }
           if (context == null)
           {
               throw new ArgumentNullException(nameof(context));
           }
           await AutoMigrationAsync(context, scope.ServiceProvider, options);
       }
       public static void AutoMigration<T>(this WebApplication app, AutoMigrationOptions options = null) where T : DbContext
       {
           using var scope = app.Services.CreateScope();
           var context = scope.ServiceProvider.GetRequiredService<T>();
           if (context == null)
           {
               throw new ArgumentNullException(nameof(context));
           }
           AutoMigration(context, scope.ServiceProvider, options);
       }
```

### 高级配置

你可以通过选项配置迁移行为：

```csharp
using Si.EntityFramework.AutoMigration;

dbContext.AutoMigration(new AutoMigrationOptions
{
    ThrowOnError = true,             // 出错时抛出异常
    DetailedErrors = true,           // 显示详细错误信息
    TrackHistory = true,             // 跟踪迁移历史
    AllowDropColumn = false,         // 不允许删除列
    AllowDropTable = false,          // 不允许删除表
    ScriptOnly = false,              // 不仅生成脚本而且执行它们
    ValidateConnection = true,       // 验证数据库连接
    HistoryTableName = "__AutoMigrationHistory" // 自定义历史表名
});
```

### 异步用法

```csharp
using Si.EntityFramework.AutoMigration;

// 异步方法中使用
await dbContext.AutoMigrationAsync();

// 或使用配置
await dbContext.AutoMigrationAsync(new AutoMigrationOptions
{
    // 配置...
});
```

## 工作原理

1. 通过`DbContext`的模型元数据获取定义的实体及其属性
2. 查询数据库系统表获取实际的表结构信息
3. 对比两者差异，生成需要执行的DDL语句
4. 在一个事务中执行所有变更
5. 记录变更历史（可选）

## 注意事项

- 此库目前支持以下数据类型变更：
  - 创建不存在的表
  - 添加新列
  - 修改列的数据类型（当安全时）
  - 修改列的可空性（仅从可空到非空）
  - 增加字符串列的长度

- 默认不支持以下操作（但可通过选项开启）：
  - 删除表
  - 删除列
  - 重命名表或列（始终被视为删除后新增）

- 不支持以下操作：
  - 重命名表或列
  - 添加或删除外键、索引等

## 支持的数据库

- SQL Server
- SQLite
- MySQL/MariaDB
- PostgreSQL

## 许可证

MIT

## 贡献

欢迎提交Issue和Pull Requests! 
