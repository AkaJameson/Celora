# Si.Dapper.Sharding

基于Dapper的分库分表组件，支持SQLite、MySQL、PostgreSQL和SQL Server四种数据库。

## 功能特点

- 支持多种数据库：SQLite、MySQL、PostgreSQL和SQL Server
- 支持分库分表策略：哈希路由、取模路由和日期路由
- 支持跨分片查询和操作
- 支持分布式事务
- 支持依赖注入
- 支持动态建表（按需建表、按日期建表）

## 快速开始

### 安装

```bash
dotnet add package Si.Dapper.Sharding
```

### 示例

#### 1. 配置服务

```csharp
// 在Startup.cs中配置
public void ConfigureServices(IServiceCollection services)
{
    // 添加分库分表服务
    services.AddDapperSharding(Configuration);
    
    // 配置数据库
    var databaseConfigs = new Dictionary<string, DatabaseConfig>
    {
        ["db_0"] = new DatabaseConfig 
        { 
            ConnectionString = "Data Source=localhost;Initial Catalog=db_0;User Id=root;Password=123456;", 
            DbType = DatabaseType.MySQL 
        },
        ["db_1"] = new DatabaseConfig 
        { 
            ConnectionString = "Data Source=localhost;Initial Catalog=db_1;User Id=root;Password=123456;", 
            DbType = DatabaseType.MySQL 
        }
    };
    
    // 添加哈希分片策略
    services.AddHashSharding(
        databaseNames: new[] { "db_0", "db_1" },
        tableShardCount: 4,
        databaseConfigs: databaseConfigs
    );
    
    // 或者添加取模分片策略
    // services.AddModSharding(
    //     databaseNames: new[] { "db_0", "db_1" },
    //     tableShardCount: 4,
    //     databaseConfigs: databaseConfigs
    // );
}
```

#### 2. 注入和使用

```csharp
public class UserService
{
    private readonly IShardingDbContext _shardingContext;
    private readonly IShardingRouter _router;
    
    public UserService(IShardingDbContext shardingContext, IShardingRouter router)
    {
        _shardingContext = shardingContext;
        _router = router;
    }
    
    public async Task<User> GetUserById(long userId)
    {
        // 获取特定分片的连接
        var connection = _shardingContext.GetConnection(userId);
        
        // 获取分表名
        var tableName = _router.GetTableName(userId, "user");
        
        // 查询数据
        return connection.QueryFirstOrDefault<User>($"SELECT * FROM {tableName} WHERE id = @Id", new { Id = userId });
    }
    
    public async Task<IEnumerable<User>> GetAllUsers()
    {
        // 执行跨分片查询
        return _shardingContext.QueryAcrossShards<User>("SELECT * FROM user_0 UNION ALL SELECT * FROM user_1 UNION ALL SELECT * FROM user_2 UNION ALL SELECT * FROM user_3");
    }
    
    public async Task CreateUser(User user)
    {
        // 获取分表名
        var tableName = _router.GetTableName(user.Id, "user");
        
        // 开始事务
        using var transaction = _shardingContext.BeginTransaction();
        try
        {
            var connection = _shardingContext.GetConnection(user.Id);
            connection.Execute($"INSERT INTO {tableName} (id, name, age) VALUES (@Id, @Name, @Age)", user);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
```

## 动态建表

Si.Dapper.Sharding支持动态建表，可以根据需要在运行时自动创建分片表。

### 1. 定义表结构

首先需要定义表结构，继承`BaseTableDefinition`类：

```csharp
public class OrderTableDefinition : BaseTableDefinition
{
    public OrderTableDefinition()
    {
        // 定义主键
        var idColumn = new TableColumnDefinition
        {
            Name = "id",
            AllowNull = false,
            IsAutoIncrement = true,
            TypeMapping = new Dictionary<DatabaseType, string>
            {
                { DatabaseType.SQLite, "INTEGER" },
                { DatabaseType.MySQL, "BIGINT" },
                { DatabaseType.PostgreSQL, "BIGINT" },
                { DatabaseType.SQLServer, "BIGINT" }
            }
        };
        
        // 设置主键
        SetPrimaryKey(idColumn);
        
        // 添加其他字段
        AddColumn(new TableColumnDefinition
        {
            Name = "order_no",
            AllowNull = false,
            TypeMapping = new Dictionary<DatabaseType, string>
            {
                { DatabaseType.SQLite, "TEXT" },
                { DatabaseType.MySQL, "VARCHAR(50)" },
                { DatabaseType.PostgreSQL, "VARCHAR(50)" },
                { DatabaseType.SQLServer, "NVARCHAR(50)" }
            },
            Comment = "订单号"
        });
        
        // 添加索引
        AddIndex(new TableIndexDefinition
        {
            Name = "idx_order_no",
            Columns = new List<string> { "order_no" },
            IsUnique = true
        });
    }
    
    public override string BaseTableName => "order";
}
```

### 2. 配置动态分片路由

然后配置动态分片路由：

```csharp
// 配置表定义
var tableDefinitions = new Dictionary<string, ITableDefinition>
{
    ["order"] = new OrderTableDefinition()
};

// 添加动态哈希分片路由
services.AddDynamicHashSharding(
    databaseNames: new[] { "db_0", "db_1" },
    tableShardCount: 4,
    databaseConfigs: databaseConfigs,
    tableDefinitions: tableDefinitions
);

// 或者添加动态取模分片路由
// services.AddDynamicModSharding(
//     databaseNames: new[] { "db_0", "db_1" },
//     tableShardCount: 4,
//     databaseConfigs: databaseConfigs,
//     tableDefinitions: tableDefinitions
// );
```

### 3. 按日期分表

Si.Dapper.Sharding支持按日期分表，适用于日志、订单等按时间分表的场景：

```csharp
// 添加按月分表
services.AddDateSharding(
    databaseNames: new[] { "db_0", "db_1" },
    databaseConfigs: databaseConfigs,
    tableDefinitions: tableDefinitions,
    shardingPeriod: DateShardingPeriod.Month,
    historyTableCount: 3,  // 保留3个月历史表
    futureTableCount: 1    // 预创建1个月未来表
);

// 使用时，以日期作为分片键
var shardKey = order.CreateTime;
var tableName = _router.GetTableName(shardKey, "order");
```

### 4. 预创建表

对于日期分表，可以预先创建未来的表：

```csharp
// 获取DateShardingRouter
var router = serviceProvider.GetRequiredService<DateShardingRouter>();

// 预创建未来6个月的表
router.PrecreateFutureTables("order", 6);
```

## 高级用法

### 自定义分片路由

您可以通过实现`IShardingRouter`接口来创建自定义的分片路由策略：

```csharp
public class CustomShardingRouter : IShardingRouter
{
    // 实现接口方法
}
```

### 读写分离

可以在数据库配置中设置读写分离：

```csharp
var databaseConfigs = new Dictionary<string, DatabaseConfig>
{
    ["db_0"] = new DatabaseConfig 
    { 
        ConnectionString = "主库连接字符串", 
        DbType = DatabaseType.MySQL,
        ReadOnlyConnectionStrings = new[] { "从库1连接字符串", "从库2连接字符串" }
    }
};
```

## 许可证

MIT 