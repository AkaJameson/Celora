using System.Data;
using Si.Dapper.Sharding.Core;

namespace Si.Dapper.Sharding.Implementations
{
    /// <summary>
    /// 分布式事务实现
    /// </summary>
    public class ShardingTransaction : IShardingTransaction
    {
        private readonly Dictionary<string, IDbTransaction> _transactions = new Dictionary<string, IDbTransaction>();
        private bool _isDisposed;

        /// <summary>
        /// 添加事务
        /// </summary>
        /// <param name="key">事务键</param>
        /// <param name="transaction">事务</param>
        public void AddTransaction(string key, IDbTransaction transaction)
        {
            if (!_transactions.ContainsKey(key))
            {
                _transactions.Add(key, transaction);
            }
        }

        /// <summary>
        /// 提交事务
        /// </summary>
        public void Commit()
        {
            try
            {
                foreach (var transaction in _transactions.Values)
                {
                    transaction.Commit();
                }
            }
            catch
            {
                Rollback();
                throw;
            }
        }

        /// <summary>
        /// 回滚事务
        /// </summary>
        public void Rollback()
        {
            foreach (var transaction in _transactions.Values)
            {
                try
                {
                    transaction.Rollback();
                }
                catch
                {
                    // 忽略回滚异常
                }
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">是否释放托管资源</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    foreach (var transaction in _transactions.Values)
                    {
                        transaction.Dispose();
                    }
                    _transactions.Clear();
                }
                _isDisposed = true;
            }
        }
    }
} 