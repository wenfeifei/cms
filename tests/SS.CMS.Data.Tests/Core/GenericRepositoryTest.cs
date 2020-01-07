﻿using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using SqlKata;
using SS.CMS.Data.Tests.Mocks;
using SS.CMS.Data.Utils;
using SS.CMS.Utils.Tests;
using Xunit;
using Xunit.Abstractions;

namespace SS.CMS.Data.Tests.Core
{
    public class GenericRepositoryTest : IClassFixture<UnitTestsFixture>, IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Repository<TestTableInfo> _repository;

        public GenericRepositoryTest(UnitTestsFixture fixture, ITestOutputHelper output)
        {
            _output = output;

            var db = new Database(fixture.SettingsManager.DatabaseType, fixture.SettingsManager.DatabaseConnectionString);
            _repository = new Repository<TestTableInfo>(db);

            if (!TestEnv.IsTestMachine) return;

            var isExists = db.IsTableExistsAsync(_repository.TableName).GetAwaiter().GetResult();
            if (isExists)
            {
                db.DropTableAsync(_repository.TableName).GetAwaiter().GetResult();
            }

            db.CreateTableAsync(_repository.TableName, _repository.TableColumns).GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            if (!TestEnv.IsTestMachine) return;

            //_fixture.Db.DropTable(_repository.TableName);
        }

        [SkippableFact]
        public void Start()
        {
            Skip.IfNot(TestEnv.IsTestMachine);

            var tableName = _repository.TableName;
            var tableColumns = _repository.TableColumns;

            Assert.Equal("TestTable", tableName);
            Assert.NotNull(tableColumns);
            Assert.Equal(12, tableColumns.Count);

            var varChar100Column = tableColumns.FirstOrDefault(x => x.AttributeName == nameof(TestTableInfo.TypeVarChar100));
            Assert.NotNull(varChar100Column);
            Assert.Equal(DataType.VarChar, varChar100Column.DataType);
            Assert.Equal(100, varChar100Column.DataLength);

            var varCharDefaultColumn = tableColumns.FirstOrDefault(x => x.AttributeName == nameof(TestTableInfo.TypeVarCharDefault));
            Assert.NotNull(varCharDefaultColumn);
            Assert.Equal(DataType.VarChar, varCharDefaultColumn.DataType);
            Assert.Equal(DbUtils.VarCharDefaultLength, varCharDefaultColumn.DataLength);

            var boolColumn = tableColumns.FirstOrDefault(x => x.AttributeName == nameof(TestTableInfo.TypeBool));
            Assert.NotNull(boolColumn);
            Assert.Equal(DataType.Boolean, boolColumn.DataType);

            var contentColumn = tableColumns.FirstOrDefault(x => x.AttributeName == nameof(TestTableInfo.Content));
            Assert.NotNull(contentColumn);
            Assert.Equal(DataType.Text, contentColumn.DataType);

            var isLockedOutColumn = tableColumns.FirstOrDefault(x => x.AttributeName == "IsLockedOut");
            Assert.NotNull(isLockedOutColumn);

            var lockedColumn = tableColumns.FirstOrDefault(x => x.AttributeName == nameof(TestTableInfo.Locked));
            Assert.Null(lockedColumn);
        }

        [SkippableFact]
        public async Task InsertTest()
        {
            Skip.IfNot(TestEnv.IsTestMachine);

            var id = await _repository.InsertAsync(null);
            Assert.Equal(0, id);

            id = await _repository.InsertAsync(null);
            Assert.Equal(0, id);

            var dataInfo = new TestTableInfo();
            await _repository.InsertAsync(dataInfo);
            Assert.Equal(1, dataInfo.Id);
            Assert.True(Utilities.IsGuid(dataInfo.Guid));
            Assert.True(dataInfo.LastModifiedDate.HasValue);
            Assert.Null(dataInfo.TypeVarChar100);
            Assert.Null(dataInfo.TypeVarCharDefault);
            Assert.Null(dataInfo.Content);
            Assert.Equal(0, dataInfo.Num);
            Assert.Equal(0, dataInfo.Currency);
            Assert.False(dataInfo.Date.HasValue);
            Assert.True(dataInfo.Date == null);
            Assert.False(dataInfo.Locked);

            dataInfo = new TestTableInfo();
            await _repository.InsertAsync(dataInfo);
            Assert.Equal(2, dataInfo.Id);
            Assert.True(Utilities.IsGuid(dataInfo.Guid));

            dataInfo = new TestTableInfo
            {
                Guid = "wrong guid",
                TypeVarChar100 = "string",
                Num = -100,
                Date = DateTime.Now,
                Locked = true
            };
            await _repository.InsertAsync(dataInfo);
            Assert.Equal(3, dataInfo.Id);
            Assert.True(Utilities.IsGuid(dataInfo.Guid));
            Assert.True(dataInfo.LastModifiedDate.HasValue);
            Assert.Equal("string", dataInfo.TypeVarChar100);
            Assert.Null(dataInfo.TypeVarCharDefault);
            Assert.Null(dataInfo.Content);
            Assert.Equal(-100, dataInfo.Num);
            Assert.Equal(0, dataInfo.Currency);
            Assert.True(dataInfo.Date.HasValue);
            Assert.True(dataInfo.Locked);

            _output.WriteLine(dataInfo.Guid);
            _output.WriteLine(dataInfo.LastModifiedDate.ToString());

            dataInfo = await _repository.GetAsync(1);
            Assert.NotNull(dataInfo);
            Assert.Equal(1, dataInfo.Id);
            Assert.True(Utilities.IsGuid(dataInfo.Guid));
            Assert.True(dataInfo.LastModifiedDate.HasValue);
            Assert.Null(dataInfo.TypeVarChar100);
            Assert.Null(dataInfo.TypeVarCharDefault);
            Assert.Null(dataInfo.Content);
            Assert.Equal(0, dataInfo.Num);
            Assert.Equal(0, dataInfo.Currency);
            Assert.False(dataInfo.Date.HasValue);
            Assert.True(dataInfo.Date == null);

            _output.WriteLine(dataInfo.Guid);
            _output.WriteLine(dataInfo.LastModifiedDate.ToString());

            dataInfo = await _repository.GetAsync(3);
            Assert.NotNull(dataInfo);
            Assert.Equal(3, dataInfo.Id);
            Assert.True(Utilities.IsGuid(dataInfo.Guid));
            Assert.True(dataInfo.LastModifiedDate.HasValue);
            Assert.Equal("string", dataInfo.TypeVarChar100);
            Assert.Null(dataInfo.TypeVarCharDefault);
            Assert.Null(dataInfo.Content);
            Assert.Equal(-100, dataInfo.Num);
            Assert.Equal(0, dataInfo.Currency);
            Assert.True(dataInfo.Date.HasValue);

            dataInfo = await _repository.GetAsync(new Query().Where(Attr.TypeVarChar100, "string"));
            Assert.NotNull(dataInfo);
            Assert.Equal(3, dataInfo.Id);
            Assert.True(Utilities.IsGuid(dataInfo.Guid));
            Assert.True(dataInfo.LastModifiedDate.HasValue);
            Assert.Equal("string", dataInfo.TypeVarChar100);
            Assert.Null(dataInfo.TypeVarCharDefault);
            Assert.Null(dataInfo.Content);
            Assert.Equal(-100, dataInfo.Num);
            Assert.Equal(0, dataInfo.Currency);
            Assert.True(dataInfo.Date.HasValue);

            dataInfo = await _repository.GetAsync(dataInfo.Guid);
            Assert.NotNull(dataInfo);
            Assert.Equal(3, dataInfo.Id);
            Assert.True(Utilities.IsGuid(dataInfo.Guid));
            Assert.True(dataInfo.LastModifiedDate.HasValue);
            Assert.Equal("string", dataInfo.TypeVarChar100);
            Assert.Null(dataInfo.TypeVarCharDefault);
            Assert.Null(dataInfo.Content);
            Assert.Equal(-100, dataInfo.Num);
            Assert.Equal(0, dataInfo.Currency);
            Assert.True(dataInfo.Date.HasValue);

            dataInfo = await _repository.GetAsync(dataInfo.Guid);
            Assert.NotNull(dataInfo);
            Assert.Equal(3, dataInfo.Id);
            Assert.True(Utilities.IsGuid(dataInfo.Guid));
            Assert.True(dataInfo.LastModifiedDate.HasValue);
            Assert.Equal("string", dataInfo.TypeVarChar100);
            Assert.Null(dataInfo.TypeVarCharDefault);
            Assert.Null(dataInfo.Content);
            Assert.Equal(-100, dataInfo.Num);
            Assert.Equal(0, dataInfo.Currency);
            Assert.True(dataInfo.Date.HasValue);

            dataInfo = await _repository.GetAsync(new Query().Where(Attr.TypeVarChar100, "not exists"));
            Assert.Null(dataInfo);

            var exists = await _repository.ExistsAsync(new Query()
                .Where(nameof(TestTableInfo.TypeVarChar100), "string"));
            Assert.True(exists);

            exists = await _repository.ExistsAsync(new Query().Where(nameof(TestTableInfo.TypeVarChar100), "not exists"));
            Assert.False(exists);

            exists = await _repository.ExistsAsync(new Query());
            Assert.True(exists);

            await _repository.DeleteAsync();
        }

        [SkippableFact]
        public async Task TestCount()
        {
            Skip.IfNot(TestEnv.IsTestMachine);

            await _repository.InsertAsync(new TestTableInfo
            {
                TypeVarChar100 = "string",
                Num = -100,
                Date = DateTime.Now,
                Locked = true
            });

            await _repository.InsertAsync(new TestTableInfo
            {
                TypeVarChar100 = "test",
                Num = -100,
                Date = DateTime.Now,
                Locked = true
            });

            var count = await _repository.CountAsync();
            Assert.Equal(2, count);

            count = await _repository.CountAsync(new Query().Where("TypeVarChar100", "test"));
            Assert.Equal(1, count);

            await _repository.DeleteAsync();
        }

        [SkippableFact]
        public async Task TestGetValue()
        {
            Skip.IfNot(TestEnv.IsTestMachine);

            await _repository.InsertAsync(new TestTableInfo
            {
                Guid = "wrong guid",
                TypeVarChar100 = "string"
            });

            var guid = await _repository.GetAsync<string>(new Query()
                .Select(nameof(Entity.Guid)).Where("TypeVarChar100", "string"));
            Assert.True(Utilities.IsGuid(guid));

            var date = await _repository.GetAsync<DateTime?>(new Query()
                .Select(nameof(TestTableInfo.Date)).Where("Guid", guid));
            Assert.False(date.HasValue);

            var lastModifiedDate = await _repository.GetAsync<DateTime?>(new Query()
                .Select(nameof(TestTableInfo.LastModifiedDate))
                .Where("Guid", guid));
            Assert.True(lastModifiedDate.HasValue);
            _output.WriteLine(lastModifiedDate.Value.ToString(CultureInfo.InvariantCulture));

            await _repository.DeleteAsync();
        }

        [SkippableFact]
        public async Task TestGetValues()
        {
            Skip.IfNot(TestEnv.IsTestMachine);

            await _repository.InsertAsync(new TestTableInfo
            {
                TypeVarChar100 = "string"
            });

            var guidList = await _repository.GetAllAsync<string>(new Query()
                .Select(nameof(TestTableInfo.Guid))
                .Where("TypeVarChar100", "string"));

            Assert.NotNull(guidList);
            Assert.True(Utilities.IsGuid(guidList.First()));

            var dateList = await _repository.GetAllAsync<DateTime?>(new Query()
                .Select(nameof(TestTableInfo.Date))
                .Where("Guid", guidList.First()));
            Assert.False(dateList.First().HasValue);

            var lastModifiedDateList = await _repository.GetAllAsync<DateTime?>(new Query()
                .Select(nameof(TestTableInfo.LastModifiedDate)));
            lastModifiedDateList.ToList().ForEach(x => Assert.True(x.HasValue));

            await _repository.DeleteAsync();
        }

        [SkippableFact]
        public async Task TestGetAll()
        {
            Skip.IfNot(TestEnv.IsTestMachine);

            await _repository.InsertAsync(new TestTableInfo
            {
                TypeVarChar100 = "str1"
            });
            await _repository.InsertAsync(new TestTableInfo
            {
                TypeVarChar100 = "str2"
            });

            var allList = await _repository.GetAllAsync();
            Assert.Equal(2, allList.ToList().Count);

            var list = await _repository.GetAllAsync(new Query()
                .Where("Guid", allList.First().Guid));

            Assert.Single(list);
        }

        [SkippableFact]
        public async Task TestUpdate()
        {
            Skip.IfNot(TestEnv.IsTestMachine);

            await _repository.InsertAsync(new TestTableInfo
            {
                TypeVarChar100 = "str1"
            });
            await _repository.InsertAsync(new TestTableInfo
            {
                TypeVarChar100 = "str2"
            });

            var dataInfo = await _repository.GetAsync(Q.Where("TypeVarChar100", "str2"));
            Assert.True(dataInfo.LastModifiedDate.HasValue);
            var lastModified = dataInfo.LastModifiedDate.Value.Ticks;

            dataInfo.Content = "new content";
            dataInfo.LastModifiedDate = DateTime.Now.AddDays(-1);

            var updated = await _repository.UpdateAsync(dataInfo);
            Assert.True(updated);

            Assert.True(Utilities.IsGuid(dataInfo.Guid));
            Assert.True(dataInfo.LastModifiedDate.HasValue);
            Assert.NotEmpty(dataInfo.TypeVarChar100);
            Assert.Null(dataInfo.TypeVarCharDefault);
            Assert.Equal("new content", dataInfo.Content);
            Assert.Equal(0, dataInfo.Num);
            Assert.Equal(0, dataInfo.Currency);
            Assert.False(dataInfo.Date.HasValue);
            Assert.True(dataInfo.Date == null);

            var lastModified2 = dataInfo.LastModifiedDate.Value.Ticks;
            _output.WriteLine(lastModified.ToString());
            _output.WriteLine(lastModified2.ToString());
            Assert.True(lastModified2 > lastModified);

            updated = await _repository.UpdateAsync((TestTableInfo)null);
            Assert.False(updated);

            await _repository.DeleteAsync();
        }

        [SkippableFact]
        public async Task TestUpdateWithParameters()
        {
            Skip.IfNot(TestEnv.IsTestMachine);

            await _repository.InsertAsync(new TestTableInfo
            {
                TypeVarChar100 = "str1"
            });
            await _repository.InsertAsync(new TestTableInfo
            {
                TypeVarChar100 = "str2"
            });

            var lastModified = await _repository.GetAsync<DateTime?>(new Query()
                .Select(nameof(Entity.LastModifiedDate)).Where("Id", 1));
            Assert.True(lastModified.HasValue);

            var updated = await _repository.UpdateAsync(new Query()
                .Set("Content", "new content2")
                .Set("LastModifiedDate", DateTime.Now.AddDays(-1))
                .Where(nameof(Attr.Id), 1));
            Assert.True(updated == 1);

            var dataInfo = await _repository.GetAsync(1);
            Assert.True(dataInfo.LastModifiedDate.HasValue);
            var lastModified2 = dataInfo.LastModifiedDate.Value.Ticks;

            Assert.Equal(1, dataInfo.Id);
            Assert.True(Utilities.IsGuid(dataInfo.Guid));
            Assert.True(dataInfo.LastModifiedDate.HasValue);
            Assert.NotNull(dataInfo.TypeVarChar100);
            Assert.Null(dataInfo.TypeVarCharDefault);
            Assert.Equal("new content2", dataInfo.Content);
            Assert.Equal(0, dataInfo.Num);
            Assert.Equal(0, dataInfo.Currency);
            Assert.False(dataInfo.Date.HasValue);
            Assert.True(dataInfo.Date == null);

            Assert.True(lastModified2 > lastModified.Value.Ticks);

            updated = await _repository.UpdateAsync(new Query());
            Assert.True(updated == 2);

            await _repository.DeleteAsync();
        }

        [SkippableFact]
        public async Task TestUpdateAll()
        {
            Skip.IfNot(TestEnv.IsTestMachine);

            await _repository.InsertAsync(new TestTableInfo
            {
                TypeVarChar100 = "str1"
            });
            await _repository.InsertAsync(new TestTableInfo
            {
                TypeVarChar100 = "str2"
            });

            var lastModified = await _repository.GetAsync<DateTime?>(new Query()
                .Select(nameof(Entity.LastModifiedDate))
                .Where("TypeVarChar100", "str1"));
            Assert.True(lastModified.HasValue);

            var updatedCount = await _repository.UpdateAsync(new Query()
                .Set("Content", "new content2")
                .Set("LastModifiedDate", DateTime.Now.AddDays(-1))
                .Where("TypeVarChar100", "str1"));

            Assert.True(updatedCount == 1);

            updatedCount = await _repository.UpdateAsync(new Query()
                .Set("Content", "new content3")
                .Where("Content", "new content2"));

            Assert.True(updatedCount == 1);

            var dataInfo = await _repository.GetAsync(Q.Where("TypeVarChar100", "str1"));
            Assert.True(dataInfo.LastModifiedDate.HasValue);
            var lastModified2 = dataInfo.LastModifiedDate.Value.Ticks;

            Assert.True(lastModified2 > lastModified.Value.Ticks);

            await _repository.DeleteAsync();
        }

        [SkippableFact]
        public async Task TestIncrementAll()
        {
            Skip.IfNot(TestEnv.IsTestMachine);

            await _repository.InsertAsync(new TestTableInfo
            {
                TypeVarChar100 = "str1"
            });

            var dataInfo = await _repository.GetAsync(Q.Where("TypeVarChar100", "str1"));
            Assert.Equal(0, dataInfo.Num);

            var affected = await _repository.IncrementAsync(Attr.Num, Q.Where("TypeVarChar100", "str1"));
            Assert.True(affected == 1);

            dataInfo = await _repository.GetAsync(Q.Where("TypeVarChar100", "str1"));
            Assert.Equal(1, dataInfo.Num);

            affected = await _repository.DecrementAsync(Attr.Num, Q.Where(Attr.Id, 1));
            Assert.True(affected == 1);

            dataInfo = await _repository.GetAsync(Q.Where("TypeVarChar100", "str1"));
            Assert.Equal(0, dataInfo.Num);

            await _repository.DeleteAsync();
        }

        [SkippableFact]
        public async Task TestDelete()
        {
            Skip.IfNot(TestEnv.IsTestMachine);

            await _repository.InsertAsync(new TestTableInfo
            {
                TypeVarChar100 = "str"
            });

            var deleted = await _repository.DeleteAsync(Q.Where("TypeVarChar100", "str"));
            Assert.Equal(1, deleted);

            Assert.False(await _repository.DeleteAsync(1));

            await _repository.DeleteAsync();
        }
    }
}
