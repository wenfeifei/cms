using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using SS.CMS.Data;
using SS.CMS.Models;
using SS.CMS.Repositories;
using SS.CMS.Services;
using SS.CMS.Utils;

namespace SS.CMS.Core.Repositories
{
    public partial class DepartmentRepository : IDepartmentRepository
    {
        private readonly IDistributedCache _cache;
        private readonly string _cacheKey;
        private readonly Repository<Department> _repository;
        private readonly ISettingsManager _settingsManager;

        public DepartmentRepository(IDistributedCache cache, ISettingsManager settingsManager)
        {
            _cache = cache;
            _cacheKey = _cache.GetKey(nameof(DepartmentRepository));
            _repository = new Repository<Department>(new Database(settingsManager.DatabaseType, settingsManager.DatabaseConnectionString));
            _settingsManager = settingsManager;
        }

        public IDatabase Database => _repository.Database;

        public string TableName => _repository.TableName;
        public List<TableColumn> TableColumns => _repository.TableColumns;

        private static class Attr
        {
            public const string Id = nameof(Department.Id);
            public const string ParentId = nameof(Department.ParentId);
            public const string ParentsPath = nameof(Department.ParentsPath);
            public const string ChildrenCount = nameof(Department.ChildrenCount);
            public const string Taxis = nameof(Department.Taxis);
            public const string IsLastNode = nameof(Department.IsLastNode);
        }

        private async Task<int> InsertAsync(Department parentInfo, Department departmentInfo)
        {
            if (parentInfo != null)
            {
                departmentInfo.ParentsPath = parentInfo.ParentsPath + "," + parentInfo.Id;
                departmentInfo.ParentsCount = parentInfo.ParentsCount + 1;

                var maxTaxis = await GetMaxTaxisByParentPathAsync(departmentInfo.ParentsPath);
                if (maxTaxis == 0)
                {
                    maxTaxis = parentInfo.Taxis;
                }
                departmentInfo.Taxis = maxTaxis + 1;
            }
            else
            {
                departmentInfo.ParentsPath = "0";
                departmentInfo.ParentsCount = 0;
                var maxTaxis = await GetMaxTaxisByParentPathAsync("0");
                departmentInfo.Taxis = maxTaxis + 1;
            }

            departmentInfo.ChildrenCount = 0;
            departmentInfo.IsLastNode = true;

            await _repository.IncrementAsync(Attr.Taxis, Q
                .Where(Attr.Taxis, ">=", departmentInfo.Taxis));

            departmentInfo.Id = await _repository.InsertAsync(departmentInfo);

            if (!string.IsNullOrEmpty(departmentInfo.ParentsPath))
            {
                await _repository.IncrementAsync(Attr.ChildrenCount, Q
                    .WhereIn(Attr.Id, TranslateUtils.StringCollectionToIntList(departmentInfo.ParentsPath)));
            }

            await _repository.UpdateAsync(Q
                .Set(Attr.IsLastNode, false)
                .Where(Attr.ParentId, departmentInfo.ParentId)
            );

            var topId = await _repository.GetAsync<int>(Q
                .Select(Attr.Id)
                .Where(Attr.ParentId, departmentInfo.ParentId)
                .OrderByDesc(Attr.Taxis));

            if (topId > 0)
            {
                await _repository.UpdateAsync(Q
                    .Set(Attr.IsLastNode, true)
                    .Where(Attr.Id, topId)
                );
            }

            await _cache.RemoveAsync(_cacheKey);

            return departmentInfo.Id;
        }

        private async Task UpdateSubtractChildrenCountAsync(string parentsPath, int subtractNum)
        {
            if (!string.IsNullOrEmpty(parentsPath))
            {
                await _repository.DecrementAsync(Attr.ChildrenCount, Q
                    .WhereIn(Attr.Id, TranslateUtils.StringCollectionToIntList(parentsPath)), subtractNum);

                await _cache.RemoveAsync(_cacheKey);
            }
        }

        private async Task TaxisSubtractAsync(int selectedId)
        {
            var departmentInfo = await _repository.GetAsync(selectedId);
            if (departmentInfo == null) return;

            var result = await _repository.GetAsync<(int Id, int ChildrenCount, string ParentsPath)?>(Q
                .Select(
                    Attr.Id,
                    Attr.ChildrenCount,
                    Attr.ParentsPath)
                .Where(Attr.ParentId, departmentInfo.ParentId)
                .WhereNot(Attr.Id, departmentInfo.Id)
                .Where(Attr.Taxis, "<", departmentInfo.Taxis)
                .OrderByDesc(Attr.Taxis));

            if (result == null) return;

            var lowerId = result.Value.Id;
            var lowerChildrenCount = result.Value.ChildrenCount;
            var lowerParentsPath = result.Value.ParentsPath;

            var lowerNodePath = string.Concat(lowerParentsPath, ",", lowerId);
            var selectedNodePath = string.Concat(departmentInfo.ParentsPath, ",", departmentInfo.Id);

            await SetTaxisSubtractAsync(selectedId, selectedNodePath, lowerChildrenCount + 1);
            await SetTaxisAddAsync(lowerId, lowerNodePath, departmentInfo.ChildrenCount + 1);

            await UpdateIsLastNodeAsync(departmentInfo.ParentId);
        }

        private async Task TaxisAddAsync(int selectedId)
        {
            var departmentInfo = await _repository.GetAsync(selectedId);
            if (departmentInfo == null) return;

            var dataInfo = await _repository.GetAsync(Q
                .Select(Attr.Id, Attr.ChildrenCount, Attr.ParentsPath)
                .Where(Attr.ParentId, departmentInfo.ParentId)
                .WhereNot(Attr.Id, departmentInfo.Id)
                .Where(Attr.Taxis, ">", departmentInfo.Taxis)
                .OrderBy(Attr.Taxis));

            if (dataInfo == null) return;

            var higherId = dataInfo.Id;
            var higherChildrenCount = dataInfo.ChildrenCount;
            var higherParentsPath = dataInfo.ParentsPath;

            var higherNodePath = string.Concat(higherParentsPath, ",", higherId);
            var selectedNodePath = string.Concat(departmentInfo.ParentsPath, ",", departmentInfo.Id);

            await SetTaxisAddAsync(selectedId, selectedNodePath, higherChildrenCount + 1);
            await SetTaxisSubtractAsync(higherId, higherNodePath, departmentInfo.ChildrenCount + 1);

            await UpdateIsLastNodeAsync(departmentInfo.ParentId);
        }

        private async Task SetTaxisAddAsync(int id, string parentsPath, int addNum)
        {
            await _repository.IncrementAsync(Attr.Taxis, Q
                .Where(Attr.Id, id)
                .OrWhere(Attr.ParentsPath, parentsPath)
                .OrWhereStarts(Attr.ParentsPath, $"{parentsPath},"), addNum);

            await _cache.RemoveAsync(_cacheKey);
        }

        private async Task SetTaxisSubtractAsync(int id, string parentsPath, int subtractNum)
        {
            await _repository.DecrementAsync(Attr.Taxis, Q
                .Where(Attr.Id, id)
                .OrWhere(Attr.ParentsPath, parentsPath)
                .OrWhereStarts(Attr.ParentsPath, $"{parentsPath},"), subtractNum);

            await _cache.RemoveAsync(_cacheKey);
        }

        private async Task UpdateIsLastNodeAsync(int parentId)
        {
            if (parentId <= 0) return;

            await _repository.UpdateAsync(Q
                .Set(Attr.IsLastNode, false)
                .Where(Attr.ParentId, parentId)
            );

            var topId = await _repository.GetAsync<int>(Q
                .Select(Attr.Id)
                .Where(Attr.ParentId, parentId)
                .OrderByDesc(Attr.Taxis));

            if (topId > 0)
            {
                await _repository.UpdateAsync(Q
                    .Set(Attr.IsLastNode, true)
                    .Where(Attr.Id, topId)
                );
            }
        }

        private async Task<int> GetMaxTaxisByParentPathAsync(string parentPath)
        {
            return await _repository.MaxAsync(Attr.Taxis, Q
                       .Where(Attr.ParentsPath, parentPath)
                       .OrWhereStarts(Attr.ParentsPath, $"{parentPath},")
                   ) ?? 0;
        }

        public async Task<int> InsertAsync(Department departmentInfo)
        {
            var parentDepartmentInfo = await _repository.GetAsync(departmentInfo.ParentId);

            departmentInfo.Id = await InsertAsync(parentDepartmentInfo, departmentInfo);

            await _cache.RemoveAsync(_cacheKey);

            return departmentInfo.Id;
        }

        public async Task<bool> UpdateAsync(Department departmentInfo)
        {
            var updated = await _repository.UpdateAsync(departmentInfo);
            if (updated)
            {
                await _cache.RemoveAsync(_cacheKey);
            }

            return updated;
        }

        public async Task UpdateTaxisAsync(int selectedId, bool isSubtract)
        {
            if (isSubtract)
            {
                await TaxisSubtractAsync(selectedId);
            }
            else
            {
                await TaxisAddAsync(selectedId);
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var departmentInfo = await _repository.GetAsync(id);
            if (departmentInfo == null) return false;

            var idList = new List<int>();
            if (departmentInfo.ChildrenCount > 0)
            {
                idList.AddRange(await GetIdListForDescendantAsync(id));
            }
            idList.Add(id);

            var deletedNum = await _repository.DeleteAsync(Q
                .WhereIn(Attr.Id, idList));

            if (deletedNum > 0)
            {
                await _repository.DecrementAsync(Attr.Taxis, Q
                    .Where(Attr.Taxis, ">", departmentInfo.Taxis), deletedNum);
            }

            await UpdateIsLastNodeAsync(departmentInfo.ParentId);
            await UpdateSubtractChildrenCountAsync(departmentInfo.ParentsPath, deletedNum);

            await _cache.RemoveAsync(_cacheKey);

            return true;
        }

        private async Task<IEnumerable<Department>> GetDepartmentInfoListAsync()
        {
            return await _repository.GetAllAsync(Q
                .OrderBy(Attr.Taxis));
        }

        public async Task<IEnumerable<int>> GetIdListByParentIdAsync(int parentId)
        {
            return await _repository.GetAllAsync<int>(Q
                .Select(Attr.Id)
                .Where(Attr.ParentId, parentId)
                .OrderBy(Attr.Taxis));
        }

        private async Task<IEnumerable<int>> GetIdListForDescendantAsync(int id)
        {
            return await _repository.GetAllAsync<int>(Q
                .Select(Attr.Id)
                .Where(Attr.ParentId, id)
                .OrWhereStarts(Attr.ParentsPath, $"{id},")
                .OrWhereContains(Attr.ParentsPath, $",{id},")
                .OrWhereEnds(Attr.ParentsPath, $",{id}"));
        }

        private async Task<List<KeyValuePair<int, Department>>> GetDepartmentInfoKeyValuePairToCacheAsync()
        {
            var list = new List<KeyValuePair<int, Department>>();

            var departmentInfoList = await GetDepartmentInfoListAsync();
            foreach (var departmentInfo in departmentInfoList)
            {
                var pair = new KeyValuePair<int, Department>(departmentInfo.Id, departmentInfo);
                list.Add(pair);
            }

            return list;
        }
    }
}
