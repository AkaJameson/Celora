using Si.EntityFramework.Extension.Routing.Abstractions;
using Si.EntityFramework.Extension.Routing.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace Si.EntityFramework.Extension.Routing.Implementations.Selector
{
    /// <summary>
    /// 一致性Hash策略选择器
    /// </summary>
    public class ConsistentHashSelector : ILoadBalanceSelector
    {
        private readonly SortedDictionary<int, SlaveConnectionConfig> _ring = new SortedDictionary<int, SlaveConnectionConfig>();
        private readonly List<SlaveConnectionConfig> _slaves;
        private readonly int _virtualNodeCount;
        private readonly Func<string> _getHashKey;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="slaves">从库连接配置列表</param>
        /// <param name="virtualNodeCount">虚拟节点数量</param>
        /// <param name="getHashKey">获取Hash键值的函数</param>
        public ConsistentHashSelector(List<SlaveConnectionConfig> slaves, int virtualNodeCount, Func<string> getHashKey)
        {
            if (slaves == null || !slaves.Any())
                throw new ArgumentException("从库连接配置不能为空");

            _slaves = slaves;
            _virtualNodeCount = virtualNodeCount;
            _getHashKey = getHashKey;

            // 初始化哈希环
            InitializeRing();
        }

        /// <summary>
        /// 初始化哈希环
        /// </summary>
        private void InitializeRing()
        {
            foreach (var slave in _slaves)
            {
                if (string.IsNullOrEmpty(slave.Key))
                {
                    slave.Key = Guid.NewGuid().ToString("N");
                }

                // 为每个节点创建多个虚拟节点
                for (int i = 0; i < _virtualNodeCount; i++)
                {
                    string virtualKey = $"{slave.Key}:{i}";
                    int hashCode = GetHashCode(virtualKey);

                    // 避免哈希冲突
                    while (_ring.ContainsKey(hashCode))
                    {
                        hashCode = (hashCode + 1) % int.MaxValue;
                    }

                    _ring.Add(hashCode, slave);
                }
            }
        }

        /// <summary>
        /// 根据一致性Hash选择一个从库连接
        /// </summary>
        public SlaveConnectionConfig Select()
        {
            string key = _getHashKey();
            return GetSlaveByKey(key);
        }

        /// <summary>
        /// 根据键值在哈希环上查找节点
        /// </summary>
        private SlaveConnectionConfig GetSlaveByKey(string key)
        {
            if (_ring.Count == 0)
                throw new InvalidOperationException("哈希环为空");

            int hashCode = GetHashCode(key);

            // 在哈希环上顺时针查找第一个节点
            if (!_ring.ContainsKey(hashCode))
            {
                var keys = _ring.Keys.ToArray();

                // 找到第一个大于hashCode的键
                var nextKeys = keys.Where(k => k > hashCode).ToArray();

                if (nextKeys.Length > 0)
                {
                    hashCode = nextKeys[0];
                }
                else
                {
                    // 如果没有大于hashCode的键，则取第一个键（形成环）
                    hashCode = keys[0];
                }
            }

            return _ring[hashCode];
        }

        /// <summary>
        /// 计算字符串的哈希值
        /// </summary>
        private int GetHashCode(string key)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(key);
                byte[] hash = md5.ComputeHash(bytes);

                // 使用前4个字节作为哈希值
                return BitConverter.ToInt32(hash, 0);
            }
        }
    }
}
