using System.Collections.Concurrent;

namespace CelHost.Middlewares
{
    /// <summary>
    /// 黑名单记录
    /// </summary>
    public class BlackListRecord
    {
        private int Constant = 10;
        private ConcurrentDictionary<string, int> RequestRecords = new();
        public BlackListRecord(int constant)
        {
            Constant = constant;
        }
    }
}
