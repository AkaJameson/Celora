using CelHost.Data;
using CelHost.Database;
using CelHost.Models.SystemDictModels;
using CelHost.Services;
using Microsoft.EntityFrameworkCore;
using Si.EntityFramework.Extension.Extensions;
using Si.EntityFramework.Extension.UnitofWorks.Abstractions;
using Si.Utilites.OperateResult;

namespace CelHost.ServicesImpl
{
    public class SystemDictionaryServiceImpl : ISystemDictionaryServiceImpl
    {
        private readonly HostContext hostContext;
        private readonly IUnitOfWork<HostContext> unitOfWork;
        public SystemDictionaryServiceImpl(HostContext hostContext)
        {
            this.hostContext = hostContext;
        }
        /// <summary>
        /// 数据字典查询
        /// </summary>
        /// <param name="queryDict"></param>
        /// <returns></returns>
        public async Task<OperateResult> QueryDictionary(QueryDictModel queryDict)
        {
            var query = hostContext.Set<SystemDict>().AsQueryable();
            if (queryDict.typeName != null)
            {
                query.Where(p => EF.Functions.Like(p.typeName, $"%{queryDict.typeName}%"));
            }
            var groupedQuery = query
            .GroupBy(p => p.typeName)
            .Select(g => new
            {
                TypeName = g.Key,
                Count = g.Count(),
                Items = g.ToList()
            });
            var pagedQuery = groupedQuery.PageBy(queryDict.PageIndex, queryDict.PageSize);
            var totalCount = await groupedQuery.CountAsync();
            var result = await pagedQuery.ToListAsync();

            return OperateResult.Successed(new
            {
                Data = result,
                Total = totalCount,
                queryDict.PageIndex,
                queryDict.PageSize
            });
        }
        /// <summary>
        /// 添加新属性
        /// </summary>
        /// <param name="dictAddModel"></param>
        /// <returns></returns>
        public async Task<OperateResult> AddNewItem(DictAddNewItemModel dictAddModel)
        {
            unitOfWork.BeginTransaction();
            var typeNameExists = await unitOfWork.GetRepository<SystemDict>().ExistsAsync(p => p.typeName == dictAddModel.typeName);
            if (typeNameExists)
                return OperateResult.Failed("字典类型已存在，禁止新增");
            var typeCode = GenerateItemCode();
            var systemDictItem = new SystemDict
            {
                typeCode = typeCode,
                typeName = dictAddModel.typeName,
                itemCode = GenerateItemCode(),
                itemName = dictAddModel.ItemName,
                itemValue = dictAddModel.ItemValue,
                itemDesc = dictAddModel.itemDesc,
                remark = dictAddModel.remark,
                order = 1,
                CreateTime = DateTime.Now
            };
            await unitOfWork.GetRepository<SystemDict>().AddAsync(systemDictItem);
            await unitOfWork.CommitTransactionAsync();
            return OperateResult.Successed("添加成功");
        }
        /// <summary>
        /// 添加子属性
        /// </summary>
        /// <param name="dictAddModel"></param>
        /// <returns></returns>
        public async Task<OperateResult> AddItem(DictAddItemModel dictAddModel)
        {
            unitOfWork.BeginTransaction();
            // 检查字典类型是否存在
            var typeExists = await unitOfWork.GetRepository<SystemDict>()
                .ExistsAsync(p => p.typeCode == dictAddModel.typeCode && p.typeName == dictAddModel.typeName);
            if (!typeExists)
                return OperateResult.Failed("字典中不存在该类型");
            bool itemExists = await unitOfWork.GetRepository<SystemDict>()
                .ExistsAsync(p => p.typeCode == dictAddModel.typeCode
                              && p.itemName == dictAddModel.itemName);
            if (itemExists)
                return OperateResult.Failed("当前类型下已存在同名属性");
            int maxOrder = await unitOfWork.GetRepository<SystemDict>()
                .Where(p => p.typeCode == dictAddModel.typeCode)
                .MaxAsync(p => (int?)p.order) ?? 0;

            var newItem = new SystemDict
            {
                typeCode = dictAddModel.typeCode,
                typeName = dictAddModel.typeName,
                itemCode = GenerateItemCode(),
                itemName = dictAddModel.itemName,
                itemValue = dictAddModel.itemValue,
                order = maxOrder + 1,
                CreateTime = DateTime.Now
            };
            await unitOfWork.GetRepository<SystemDict>().AddAsync(newItem);
            await unitOfWork.CommitTransactionAsync();
            return OperateResult.Successed("添加成功");
        }
        /// <summary>
        /// 删除类型
        /// </summary>
        /// <param name="deletModel"></param>
        /// <returns></returns>
        public async Task<OperateResult> DeleteTypes(DeleteTypeModel deletModel)
        {
            var types = unitOfWork.GetRepository<SystemDict>().Where(p => deletModel.Types.Contains(p.typeCode));
            await unitOfWork.GetRepository<SystemDict>().DeleteRangeAsync(types);
            await unitOfWork.CommitAsync();
            return OperateResult.Successed("删除成功");
        }
        /// <summary>
        /// 删除类型下属性
        /// </summary>
        /// <param name="itemModels"></param>
        /// <returns></returns>
        public async Task<OperateResult> DeleteItem(DeleteItemModel itemModels)
        {
            var items = unitOfWork.GetRepository<SystemDict>().Where(p => itemModels.Items.Contains(p.itemCode));
            await unitOfWork.GetRepository<SystemDict>().DeleteRangeAsync(items);
            await unitOfWork.CommitAsync();
            return OperateResult.Successed("删除成功");
        }
        /// <summary>
        /// 更新类型下属性
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        public async Task<OperateResult> UpdateItem(UpdateDictItem itemModel)
        {
            var item = await unitOfWork.GetRepository<SystemDict>().FirstOrDefaultAsync(p => p.itemCode == itemModel.itemCode);

            if (item == null)
            {
                return OperateResult.Failed("字典属性未找到");
            }
            if (!string.IsNullOrEmpty(itemModel.itemName))
            {
                var items = unitOfWork.GetRepository<SystemDict>().Where(p => p.typeCode == item.typeCode).Select(p => p.itemName).ToList();
                if (items.Contains(itemModel.itemName))
                {
                    return OperateResult.Failed("当前类型下已存在同名属性");
                }
                item.itemName = itemModel.itemName;

            }
            if (!string.IsNullOrEmpty(itemModel.itemValue))
            {
                item.itemValue = itemModel.itemValue;
            }
            await unitOfWork.CommitAsync();
            return OperateResult.Successed("更新成功");
        }
        private string GenerateItemCode()
        {
            // 根据实际业务需求实现编码规则，例如：
            return $"{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N").Substring(0, 4)}";
        }
    }
}
