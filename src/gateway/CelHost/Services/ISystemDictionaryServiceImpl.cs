using CelHost.Models.SystemDictModels;
using Si.Utilites.OperateResult;

namespace CelHost.Server.Services
{
    public interface ISystemDictionaryServiceImpl
    {
        Task<OperateResult> AddItem(DictAddItemModel dictAddModel);
        Task<OperateResult> AddNewItem(DictAddNewItemModel dictAddModel);
        Task<OperateResult> DeleteItem(DeleteItemModel itemModels);
        Task<OperateResult> DeleteTypes(DeleteTypeModel deletModel);
        Task<OperateResult> QueryDictionary(QueryDictModel queryDict);
        Task<OperateResult> UpdateItem(UpdateDictItem itemModel);
    }
}