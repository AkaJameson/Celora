using System.Collections.Generic;
using System.Threading.Tasks;

namespace Si.Dapper.Sharding.Core
{
    /// <summary>
    /// 表管理器接口
    /// </summary>
    public interface ITableManager
    {
        /// <summary>
        /// 检查表是否存在
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="dbName">数据库名</param>
        /// <returns>表是否存在</returns>
        bool TableExists(string tableName, string dbName);
        
        /// <summary>
        /// 异步检查表是否存在
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="dbName">数据库名</param>
        /// <returns>表是否存在</returns>
        Task<bool> TableExistsAsync(string tableName, string dbName);
        
        /// <summary>
        /// 创建表
        /// </summary>
        /// <param name="tableDefinition">表定义</param>
        /// <param name="tableName">表名</param>
        /// <param name="dbName">数据库名</param>
        /// <returns>是否创建成功</returns>
        bool CreateTable(ITableDefinition tableDefinition, string tableName, string dbName);
        
        /// <summary>
        /// 异步创建表
        /// </summary>
        /// <param name="tableDefinition">表定义</param>
        /// <param name="tableName">表名</param>
        /// <param name="dbName">数据库名</param>
        /// <returns>是否创建成功</returns>
        Task<bool> CreateTableAsync(ITableDefinition tableDefinition, string tableName, string dbName);
        
        /// <summary>
        /// 创建表（如果不存在）
        /// </summary>
        /// <param name="tableDefinition">表定义</param>
        /// <param name="tableName">表名</param>
        /// <param name="dbName">数据库名</param>
        /// <returns>是否创建成功</returns>
        bool CreateTableIfNotExists(ITableDefinition tableDefinition, string tableName, string dbName);
        
        /// <summary>
        /// 异步创建表（如果不存在）
        /// </summary>
        /// <param name="tableDefinition">表定义</param>
        /// <param name="tableName">表名</param>
        /// <param name="dbName">数据库名</param>
        /// <returns>是否创建成功</returns>
        Task<bool> CreateTableIfNotExistsAsync(ITableDefinition tableDefinition, string tableName, string dbName);
        
        /// <summary>
        /// 获取所有表名
        /// </summary>
        /// <param name="dbName">数据库名</param>
        /// <returns>表名列表</returns>
        IEnumerable<string> GetAllTables(string dbName);
        
        /// <summary>
        /// 异步获取所有表名
        /// </summary>
        /// <param name="dbName">数据库名</param>
        /// <returns>表名列表</returns>
        Task<IEnumerable<string>> GetAllTablesAsync(string dbName);
    }
} 