namespace Si.EntityFramework.Extension.Core.Utils
{
    /// <summary>
    /// 长雪花ID生成器 (64位)
    /// </summary>
    public class LongIdGenerator
    {
        // 位分配
        private const int TimestampBits = 41;    // 时间戳占41位，可表示69年
        private const int DatacenterIdBits = 5;  // 数据中心ID占5位，最多32个数据中心
        private const int WorkerIdBits = 5;      // 工作机器ID占5位，每个数据中心最多32个工作节点
        private const int SequenceBits = 12;     // 序列号占12位，每毫秒最多生成4096个ID

        // 位移量
        private const int WorkerIdShift = SequenceBits;
        private const int DatacenterIdShift = SequenceBits + WorkerIdBits;
        private const int TimestampShift = SequenceBits + WorkerIdBits + DatacenterIdBits;

        // 最大值
        private const int MaxDatacenterId = -1 ^ (-1 << DatacenterIdBits);  // 31
        private const int MaxWorkerId = -1 ^ (-1 << WorkerIdBits);          // 31
        private const int MaxSequence = -1 ^ (-1 << SequenceBits);          // 4095

        // 起始时间戳 (2020-01-01 00:00:00)
        private const long Epoch = 1577836800000L;

        // 字段
        private readonly int _datacenterId;
        private readonly int _workerId;
        private int _sequence = 0;
        private long _lastTimestamp = -1L;
        private readonly object _lock = new object();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="datacenterId">数据中心ID(0-31)</param>
        /// <param name="workerId">工作机器ID(0-31)</param>
        public LongIdGenerator(int datacenterId, int workerId)
        {
            if (datacenterId < 0 || datacenterId > MaxDatacenterId)
            {
                throw new ArgumentException($"Datacenter ID must be between 0 and {MaxDatacenterId}");
            }

            if (workerId < 0 || workerId > MaxWorkerId)
            {
                throw new ArgumentException($"Worker ID must be between 0 and {MaxWorkerId}");
            }

            _datacenterId = datacenterId;
            _workerId = workerId;
        }

        /// <summary>
        /// 生成下一个ID
        /// </summary>
        /// <returns>64位雪花ID</returns>
        public long NextId()
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

                // 组装ID (64位)
                return (
                    ((timestamp - Epoch) << TimestampShift) |
                    (_datacenterId << DatacenterIdShift) |
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