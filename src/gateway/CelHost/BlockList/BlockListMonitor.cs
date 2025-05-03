using System.Collections.Concurrent;

namespace CelHost.Server.BlockList
{
    public class BlackListMonitor
    {
        private readonly ConcurrentDictionary<string, int> _violationRecords = new();
        private readonly IBlocklistService _blocklistService;
        private readonly int _threshold;

        public BlackListMonitor(IBlocklistService blocklistService,
            IConfiguration configuration)
        {
            _blocklistService = blocklistService;
            _threshold = configuration.GetValue("RateLimiter:BlockThreshold", 5);
        }

        public async Task RecordViolation(string ip)
        {
            var count = _violationRecords.AddOrUpdate(ip, 1, (_, v) => v + 1);

            if (count >= _threshold)
            {
                await _blocklistService.BlockAsync(ip, $"触发限流阈值 {_threshold} 次");
                _violationRecords.TryRemove(ip, out _); // 重置计数器
            }
        }
    }
}
