namespace CelHost.BlockList
{
    public interface IBlocklistService
    {
        Task BlockAsync(string ip, string reason);
        Task<bool> CheckIsBlockedAsync(string ip);
        Task UnblockAsync(string ip);
    }
}