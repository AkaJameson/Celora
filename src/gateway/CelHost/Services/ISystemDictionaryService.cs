using CelHost.Data;
using CelHost.Dto;

namespace CelHost.Services
{
    public interface ISystemDictionaryService
    {
        Task<SystemDict> CreateDictAsync(SystemDict dict);
        Task DeleteDictAsync(int pkId);
        Task<SystemDict> GetByCodeAsync(string typeCode, string itemCode);
        Task<(IEnumerable<SystemDict> Items, int Total)> GetPagedListAsync(string typeCode = null, string itemName = null, string superTypeCode = null, int page = 1, int pageSize = 20);
        Task<List<SystemDict>> GetTypeOptionsAsync(string typeCode);
        Task<List<SystemDictTree>> GetTypeTreeAsync(string superTypeCode = null);
        Task<SystemDict> UpdateDictAsync(SystemDict dict);
    }
}