namespace Si.EntityFramework.Extension.Core.Utils
{
    /// <summary>
    /// 短雪花ID生成器 (32位)
    /// </summary>
    public class ShortIdGenerator
    {
        // 位分配
        private const int TimestampBits = 22;    // 时间戳占22位，可表示约12天
        private const int WorkerIdBits = 5;      // 工作机器ID占5位，可表示32个节点
        private const int SequenceBits = 5;      // 序列号占5位，每毫秒最多生成32个ID

        // 位移量
        private const int WorkerIdShift = SequenceBits;
        private const int TimestampShift = SequenceBits + WorkerIdBits;

        // 最大值
        private const int MaxWorkerId = -1 ^ (-1 << WorkerIdBits);  // 31
        private const int MaxSequence = -1 ^ (-1 << SequenceBits);  // 31

        // 起始时间戳 (2023-01-01 00:00:00)
        private const long Epoch = 1672502400000L;

        // 字段
        private readonly int _workerId;
        private int _sequence = 0;
        private long _lastTimestamp = -1L;
        private readonly object _lock = new object();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="workerId">工作机器ID(0-31)</param>
        public ShortIdGenerator(int workerId)
        {
            if (workerId < 0 || workerId > MaxWorkerId)
            {
                throw new ArgumentException($"Worker ID must be between 0 and {MaxWorkerId}");
            }
            _workerId = workerId;
        }

        /// <summary>
        /// 生成下一个ID
        /// </summary>
        /// <returns>32位雪花ID</returns>
        public int NextId()
        {
            lock (_lock)
            {
                var timestamp = GetTimestamp();

                // 时钟回拨处理
                if (timestamp < _lastTimestamp)
                {
                    throw new InvalidOperationException(
                        $"Clock moved backwards. Refusing to generate ID for {_lastTimestamp - timestamp} milliseconds");
                }

                // 同一毫秒内，增加序列号
                if (timestamp == _lastTimestamp)
                {
                    _sequence = (_sequence + 1) & MaxSequence;
                    // 同一毫秒序列号用完，等待下一毫秒
                    if (_sequence == 0)
                    {
                        timestamp = WaitNextMillis(_lastTimestamp);
                    }
                }
                else
                {
                    _sequence = 0;
                }

                _lastTimestamp = timestamp;

                // 组装ID (32位)
                return (int)(
                    ((timestamp - Epoch) << TimestampShift) |
                    (_workerId << WorkerIdShift) |
                    _sequence
                );
            }
        }

        /// <summary>
        /// 等待下一个毫秒
        /// </summary>
        private long WaitNextMillis(long lastTimestamp)
        {
            var timestamp = GetTimestamp();
            while (timestamp <= lastTimestamp)
            {
                timestamp = GetTimestamp();
            }
            return timestamp;
        }

        /// <summary>
        /// 获取当前时间戳
        /// </summary>
        private long GetTimestamp()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}
