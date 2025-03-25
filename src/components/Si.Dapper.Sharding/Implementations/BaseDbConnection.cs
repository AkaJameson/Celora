using System.Data;
using Dapper;
using Si.Dapper.Sharding.Core;

namespace Si.Dapper.Sharding.Implementations
{
    /// <summary>
    /// 数据库连接基类
    /// </summary>
    public abstract class BaseDbConnection : Core.IDbConnection
    {

        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="dbType">数据库类型</param>
        /// <param name="connectionString">连接字符串</param>
        protected BaseDbConnection(System.Data.IDbConnection connection, DatabaseType dbType, string connectionString)
        {
            _connection = connection;
            DbType = dbType;
            ConnectionString = connectionString;
        }

        protected readonly System.Data.IDbConnection _connection;
        /// <summary>
        /// 获取原生数据库连接
        /// </summary>
        public System.Data.IDbConnection DbConnection => _connection;

        /// <summary>
        /// 数据库类型
        /// </summary>
        public DatabaseType DbType { get; }

        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString { get; }

        Core.IDbConnection Core.IDbConnection.DbConnection => throw new NotImplementedException();

        /// <summary>
        /// 打开连接
        /// </summary>
        public void Open()
        {
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close()
        {
            if (_connection.State != ConnectionState.Closed)
            {
                _connection.Close();
            }
        }

        /// <summary>
        /// 创建命令
        /// </summary>
        public IDbCommand CreateCommand()
        {
            return _connection.CreateCommand();
        }

        /// <summary>
        /// 执行SQL语句
        /// </summary>
        public int Execute(string sql, object? param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _connection.Execute(sql, param, null, commandTimeout, commandType);
        }

        /// <summary>
        /// 执行查询
        /// </summary>
        public IEnumerable<T> Query<T>(string sql, object? param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _connection.Query<T>(sql, param, null, true, commandTimeout, commandType);
        }

        /// <summary>
        /// 查询单个结果
        /// </summary>
        public T QueryFirstOrDefault<T>(string sql, object? param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _connection.QueryFirstOrDefault<T>(sql, param, null, commandTimeout, commandType);
        }

        /// <summary>
        /// 开始事务
        /// </summary>
        public IDbTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }
            return _connection.BeginTransaction(isolationLevel);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _connection?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
} 