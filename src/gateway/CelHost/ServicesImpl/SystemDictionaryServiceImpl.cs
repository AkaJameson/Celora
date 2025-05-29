using CelHost.Models.SystemDictModels;
using CelHost.Server.Data;
using CelHost.Server.Database;
using CelHost.Server.Dto;
using CelHost.Server.Services;
using Microsoft.EntityFrameworkCore;
using Si.EntityFramework.Extension.UnitofWorks.Abstractions;
using Si.Utilites.OperateResult;

namespace CelHost.Server.ServicesImpl
{
    public class SystemDictionaryServiceImpl : ISystemDictionaryService
    {
        private readonly HostContext hostContext;
        private readonly IUnitOfWork<HostContext> unitOfWork;
        public SystemDictionaryServiceImpl(HostContext hostContext)
        {
            this.hostContext = hostContext;
        }
        /// <summary>
        /// 查询逐级字典
        /// </summary>
        /// <returns></returns>
        public async Task<OperateResult> QueryDataDictionary(DictQuery query)
        {
            var parentTypes = unitOfWork.GetRepository<SystemDict>().Where(p => !p.ParentId.HasValue)
                                                  .Skip(((Math.Max(1, query.PageIndex) - 1) * query.PageSize))
                                                  .Take(query.PageSize).ToList();
            var systemDict = new SystemDictDto();
            systemDict.Items.AddRange(parentTypes.Select(p => new DictItem
            {
                Id = p.Id,
                TypeCode = p.typeCode,
                TypeName = p.typeName,
                Remark = p.remark ?? string.Empty
            }));
            foreach (var parent in parentTypes)
            {
                var parentItem = systemDict.Items.FirstOrDefault(i => i.TypeCode == parent.typeCode);
                if (parentItem != null)
                {
                    await FindToLastDataItem(parentItem.Child, parent.Id);
                }
            }
            return OperateResult.Successed(systemDict);

        }
        private async Task FindToLastDataItem(List<DictItem> items, int parentId)
        {
            // Fetch child items for the given parent
            var childTypes = await unitOfWork.GetRepository<SystemDict>()
                .Where(p => p.ParentId == parentId)
                .ToListAsync();

            // Map children to DTO and add to the items list
            foreach (var child in childTypes)
            {
                var childItem = new DictItem
                {
                    Id = child.Id,
                    TypeCode = child.typeCode,
                    TypeName = child.typeName,
                    Remark = child.remark ?? string.Empty
                };

                items.Add(childItem);

                await FindToLastDataItem(childItem.Child, child.Id);
            }
        }

        private async Task DelectToLastDataItem(List<int> ids, int parentId)
        {
            var childTypes = await unitOfWork.GetRepository<SystemDict>()
                .Where(p => p.ParentId == parentId)
                .ToListAsync();

            foreach (var child in childTypes)
            {
                ids.Add(child.Id);
                await DelectToLastDataItem(ids, child.Id);
            }
        }
        /// <summary>
        /// 删除字典
        /// </summary>
        /// <param name="deleteDict"></param>
        /// <returns></returns>
        public async Task<OperateResult> DeleteDataDictionary(DeleteDict deleteDict)
        {
            var preDelectIds = await unitOfWork.GetRepository<SystemDict>().Where(p => deleteDict.Id.Contains(p.Id)).Select(p => p.Id).ToListAsync();
            //已添加到待删除列表
            var underDelectIds = new List<int>();
            foreach (var id in preDelectIds)
            {
                if (!underDelectIds.Contains(id))
                {
                    await DelectToLastDataItem(underDelectIds, id);
                }
            }
            var delectEntity = await unitOfWork.GetRepository<SystemDict>().Where(p => underDelectIds.Contains(p.Id)).ToListAsync();
            await unitOfWork.GetRepository<SystemDict>().DeleteRangeAsync(delectEntity);
            await unitOfWork.CommitAsync();
            return OperateResult.Successed();
        }
        /// <summary>
        /// 添加字典
        /// </summary>
        /// <param name="addItem"></param>
        /// <returns></returns>
        public async Task<OperateResult> AddItemToDictionary(DictAdd addItem)
        {
            if (addItem.parentId.HasValue)
            {
                var parent = await unitOfWork.GetRepository<SystemDict>().FirstOrDefaultAsync(p => p.Id == addItem.parentId);
                if (parent == null)
                {
                    return OperateResult.Failed("未找到父级字典");
                }
            }
            if (await unitOfWork.GetRepository<SystemDict>().ExistsAsync(p => p.typeCode == addItem.typeCode || p.typeName == addItem.typeName))
            {
                return OperateResult.Failed("字典已存在");
            }
            await unitOfWork.GetRepository<SystemDict>().AddAsync(new SystemDict
            {
                typeCode = addItem.typeCode,
                typeName = addItem.typeName,
                remark = addItem.remark,
                ParentId = addItem.parentId
            });
            await unitOfWork.CommitAsync();
            return OperateResult.Successed();
        }

        public async Task<OperateResult> UpdateDataDictionary(DictUpdate updateItem)
        {
            var entity = await unitOfWork.GetRepository<SystemDict>().FirstOrDefaultAsync(p => p.Id == updateItem.Id);
            if (entity == null)
            {
                return OperateResult.Failed("未找到字典");
            }
            if (!string.IsNullOrWhiteSpace(updateItem.typeCode))
            {
                if (await unitOfWork.GetRepository<SystemDict>().ExistsAsync(p => p.Id != updateItem.Id && p.typeCode == updateItem.typeCode))
                    return OperateResult.Failed("字典编码已存在");
                entity.typeCode = updateItem.typeCode;
            }
            if (!string.IsNullOrWhiteSpace(updateItem.typeName))
            {
                if (await unitOfWork.GetRepository<SystemDict>().ExistsAsync(p => p.Id != updateItem.Id && p.typeName == updateItem.typeName))
                    return OperateResult.Failed("字典名称已存在");
                entity.typeName = updateItem.typeName;
            }
            if (!string.IsNullOrWhiteSpace(updateItem.remark))
            {
                entity.remark = updateItem.remark;
            }
            await unitOfWork.GetRepository<SystemDict>().UpdateAsync(entity);
            await unitOfWork.CommitAsync();
            return OperateResult.Successed();
        }
    }
}
