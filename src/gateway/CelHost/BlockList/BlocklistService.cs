using CelHost.Server.Data;
using CelHost.Server.Database;
using Microsoft.EntityFrameworkCore;

namespace CelHost.Server.BlockList
{
    public class BlocklistService : IBlocklistService
    {
        private readonly HostContext _context;
        private readonly TimeSpan _initialBanDuration = TimeSpan.FromMinutes(30);
        private const int MaxTemporaryBans = 3;

        public BlocklistService(HostContext context)
        {
            _context = context;
        }

        public async Task<bool> CheckIsBlockedAsync(string ip)
        {
            var record = await _context.blocklistRecords
                .FirstOrDefaultAsync(r => r.BlockIp == ip);

            if (record == null) return false;

            return record.IsPermanent || record.ExpireTime > DateTime.UtcNow;
        }

        public async Task BlockAsync(string ip, string reason)
        {
            var existing = await _context.Set<BlocklistRecord>()
                .FirstOrDefaultAsync(r => r.BlockIp == ip);

            if (existing == null)
            {
                var newRecord = new BlocklistRecord
                {
                    BlockIp = ip,
                    BlockReason = reason,
                    BlockCount = 1,
                    EffectiveTime = DateTime.UtcNow,
                    ExpireTime = DateTime.UtcNow.Add(_initialBanDuration),
                    LastViolationTime = DateTime.UtcNow,
                    IsPermanent = false
                };
                _context.Set<BlocklistRecord>().Add(newRecord);
            }
            else
            {
                // 更新封禁逻辑
                existing.BlockCount++;
                existing.LastViolationTime = DateTime.UtcNow;
                existing.BlockReason = $"{existing.BlockReason}; {reason}";

                if (existing.IsPermanent)
                {
                    // 已经是永久封禁无需修改
                }
                else if (existing.BlockCount >= MaxTemporaryBans)
                {
                    existing.IsPermanent = true;
                    existing.ExpireTime = DateTime.MaxValue;
                }
                else
                {
                    // 封禁时间指数级增长
                    var multiplier = Math.Pow(2, existing.BlockCount - 1);
                    existing.ExpireTime = DateTime.UtcNow.Add(
                        TimeSpan.FromTicks(_initialBanDuration.Ticks * (long)multiplier));
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task UnblockAsync(string ip)
        {
            var record = await _context.Set<BlocklistRecord>()
                .FirstOrDefaultAsync(r => r.BlockIp == ip);

            if (record != null)
            {
                _context.Set<BlocklistRecord>().Remove(record);
                await _context.SaveChangesAsync();
            }
        }
    }
}
