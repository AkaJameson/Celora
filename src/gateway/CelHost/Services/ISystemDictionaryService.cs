using CelHost.Models.SystemDictModels;
using Si.Utilites.OperateResult;

namespace CelHost.Server.Services
{
    public interface ISystemDictionaryService
    {
        Task<OperateResult> AddItemToDictionary(DictAdd addItem);
        Task<OperateResult> DeleteDataDictionary(DeleteDict deleteDict);
        Task<OperateResult> QueryDataDictionary(DictQuery query);
        Task<OperateResult> UpdateDataDictionary(DictUpdate updateItem);
    }
}