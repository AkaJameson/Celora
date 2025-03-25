using System.Data;

namespace Si.Dapper.Sharding.Core
{
    /// <summary>
    /// 数据库连接接口
    /// </summary>
    public interface IDbConnection : IDisposable
    {
        /// <summary>
        /// 获取原生数据库连接
        /// </summary>
        IDbConnection DbConnection { get; }
        
        /// <summary>
        /// 数据库类型
        /// </summary>
        DatabaseType DbType { get; }
        
        /// <summary>
        /// 连接字符串
        /// </summary>
        string ConnectionString { get; }
        
        /// <summary>
        /// 打开连接
        /// </summary>
        void Open();
        
        /// <summary>
        /// 关闭连接
        /// </summary>
        void Close();
        
        /// <summary>
        /// 创建命令
        /// </summary>
        IDbCommand CreateCommand();
        
        /// <summary>
        /// 执行SQL语句
        /// </summary>
        int Execute(string sql, object? param = null, int? commandTimeout = null, CommandType? commandType = null);
        
        /// <summary>
        /// 执行查询
        /// </summary>
        IEnumerable<T> Query<T>(string sql, object? param = null, int? commandTimeout = null, CommandType? commandType = null);
        
        /// <summary>
        /// 查询单个结果
        /// </summary>
        T QueryFirstOrDefault<T>(string sql, object? param = null, int? commandTimeout = null, CommandType? commandType = null);
        
        /// <summary>
        /// 开始事务
        /// </summary>
        IDbTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
    }
} 