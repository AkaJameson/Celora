using CelHost.Data;
using CelHost.Database;
using CelHost.Dto;
using CelHost.Services;
using Microsoft.EntityFrameworkCore;

namespace CelHost.ServicesImpl
{
    public class SystemDictionaryServiceImpl : ISystemDictionaryService
    {
        private readonly HostContext _dbContext;

        public SystemDictionaryServiceImpl(HostContext dbContext)
        {
            _dbContext = dbContext;
        }

        // 创建字典项
        public async Task<SystemDict> CreateDictAsync(SystemDict dict)
        {
            // 检查唯一性
            if (await _dbContext.SystemDict.AnyAsync(s =>
                s.typeCode == dict.typeCode &&
                s.itemCode == dict.itemCode))
            {
                throw new InvalidOperationException("字典项编码在指定类型下已存在");
            }

            dict.CreateTime = DateTime.Now;
            _dbContext.SystemDict.Add(dict);
            await _dbContext.SaveChangesAsync();
            return dict;
        }

        // 更新字典项
        public async Task<SystemDict> UpdateDictAsync(SystemDict dict)
        {
            var existing = await _dbContext.SystemDict.FindAsync(dict.PkId);
            if (existing == null) throw new KeyNotFoundException("字典项不存在");

            // 检查编码是否被修改且冲突
            if (existing.typeCode != dict.typeCode || existing.itemCode != dict.itemCode)
            {
                if (await _dbContext.SystemDict.AnyAsync(s =>
                    s.PkId != dict.PkId &&
                    s.typeCode == dict.typeCode &&
                    s.itemCode == dict.itemCode))
                {
                    throw new InvalidOperationException("修改后的字典项编码在指定类型下已存在");
                }
            }

            _dbContext.Entry(existing).CurrentValues.SetValues(dict);
            await _dbContext.SaveChangesAsync();
            return existing;
        }

        // 删除字典项
        public async Task DeleteDictAsync(int pkId)
        {
            var dict = await _dbContext.SystemDict.FindAsync(pkId);
            if (dict == null) return;

            // 检查是否存在子项
            if (await _dbContext.SystemDict.AnyAsync(s => s.superTypeCode == dict.typeCode))
            {
                throw new InvalidOperationException("该类型存在子项，无法删除");
            }

            _dbContext.SystemDict.Remove(dict);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<SystemDictTree>> GetTypeTreeAsync(string superTypeCode = null)
        {
            var query = _dbContext.SystemDict
                .Where(s => s.superTypeCode == superTypeCode)
                .OrderBy(s => s.order);

            var types = await query.Select(s => new SystemDictTree
            {
                PkId = s.PkId,
                TypeCode = s.typeCode,
                TypeName = s.typeName,
                ItemCode = s.itemCode,
                ItemName = s.itemName,
                ItemValue = s.itemValue,
                Order = s.order,
                SuperTypeCode = s.superTypeCode
            }).ToListAsync();

            foreach (var type in types)
            {
                type.Children = await GetTypeTreeAsync(type.TypeCode);
            }
            return types;
        }
        // 分页查询
        public async Task<(IEnumerable<SystemDict> Items, int Total)> GetPagedListAsync(
            string typeCode = null,
            string itemName = null,
            string superTypeCode = null,
            int page = 1,
            int pageSize = 20)
        {
            var query = _dbContext.SystemDict.AsQueryable();

            if (!string.IsNullOrEmpty(typeCode))
                query = query.Where(s => s.typeCode == typeCode);

            if (!string.IsNullOrEmpty(itemName))
                query = query.Where(s => s.itemName.Contains(itemName));

            if (!string.IsNullOrEmpty(superTypeCode))
                query = query.Where(s => s.superTypeCode == superTypeCode);

            int total = await query.CountAsync();
            var items = await query
                .OrderBy(s => s.order)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        // 根据编码获取字典项
        public async Task<SystemDict> GetByCodeAsync(string typeCode, string itemCode)
        {
            return await _dbContext.SystemDict
                .FirstOrDefaultAsync(s => s.typeCode == typeCode && s.itemCode == itemCode);
        }

        // 获取类型下所有选项
        public async Task<List<SystemDict>> GetTypeOptionsAsync(string typeCode)
        {
            return await _dbContext.SystemDict
                .Where(s => s.typeCode == typeCode)
                .OrderBy(s => s.order)
                .ToListAsync();
        }
    }
}
