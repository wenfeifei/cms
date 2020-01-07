﻿using System;
using System.IO;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SS.CMS.Core.Common;
using SS.CMS.Core.Repositories;
using SS.CMS.Core.Services;
using SS.CMS.Data;
using SS.CMS.Repositories;
using SS.CMS.Services;
using Xunit;

namespace SS.CMS.Utils.Tests
{
    public class IntegrationTestsFixture : IDisposable
    {
        public IConfiguration Configuration { get; }
        public IDistributedCache Cache { get; }
        public ISettingsManager SettingsManager { get; }
        public IDatabase Database { get; }
        public IUserManager UserManager { get; }
        public IPathManager PathManager { get; }
        public IPluginManager PluginManager { get; }
        public IUrlManager UrlManager { get; }
        public IFileManager FileManager { get; }
        public ICreateManager CreateManager { get; }
        public IAccessTokenRepository AccessTokenRepository { get; }
        public IAreaRepository AreaRepository { get; }
        public IChannelGroupRepository ChannelGroupRepository { get; }
        public IChannelRepository ChannelRepository { get; }
        public IConfigRepository ConfigRepository { get; }
        public IContentCheckRepository ContentCheckRepository { get; }
        public IContentGroupRepository ContentGroupRepository { get; }
        public IDatabaseRepository DatabaseRepository { get; }
        public IDbCacheRepository DbCacheRepository { get; }
        public IDepartmentRepository DepartmentRepository { get; }
        public IErrorLogRepository ErrorLogRepository { get; }
        public ILogRepository LogRepository { get; }
        public IPermissionRepository PermissionRepository { get; }
        public IPluginConfigRepository PluginConfigRepository { get; }
        public IPluginRepository PluginRepository { get; }
        public IRelatedFieldItemRepository RelatedFieldItemRepository { get; }
        public IRelatedFieldRepository RelatedFieldRepository { get; }
        public IRoleRepository RoleRepository { get; }
        public ISiteLogRepository SiteLogRepository { get; }
        public ISiteRepository SiteRepository { get; }
        public ISpecialRepository SpecialRepository { get; }
        public ITableStyleItemRepository TableStyleItemRepository { get; }
        public ITableStyleRepository TableStyleRepository { get; }
        public ITagRepository TagRepository { get; }
        public ITemplateLogRepository TemplateLogRepository { get; }
        public ITemplateRepository TemplateRepository { get; }
        public IUserGroupRepository UserGroupRepository { get; }
        public IUserLogRepository UserLogRepository { get; }
        public IUserMenuRepository UserMenuRepository { get; }
        public IUserRepository UserRepository { get; }
        public IUserRoleRepository UserRoleRepository { get; }

        public IntegrationTestsFixture()
        {
            var contentRootPath = Directory.GetCurrentDirectory();

            var config = new ConfigurationBuilder()
                .SetBasePath(contentRootPath)
                .AddJsonFile("ss.json")
                .Build();

            var settingsManager = new SettingsManager(config, contentRootPath, PathUtils.Combine(contentRootPath, "wwwroot"));

            var services = new ServiceCollection();
            services.AddDistributedMemoryCache();
            var provider = services.BuildServiceProvider();
            IDistributedCache cache = provider.GetService<IDistributedCache>();

            var database = new Database(settingsManager.DatabaseType, settingsManager.DatabaseConnectionString);

            var accessTokenRepository = new AccessTokenRepository(cache, settingsManager);
            var userRoleRepository = new UserRoleRepository(settingsManager);
            var areaRepository = new AreaRepository(cache, settingsManager);
            var channelGroupRepository = new ChannelGroupRepository(cache, settingsManager);
            var configRepository = new ConfigRepository(cache, settingsManager);
            var contentCheckRepository = new ContentCheckRepository(settingsManager);
            var contentGroupRepository = new ContentGroupRepository(cache, settingsManager);
            var dbCacheRepository = new DbCacheRepository(settingsManager);
            var departmentRepository = new DepartmentRepository(cache, settingsManager);
            var errorLogRepository = new ErrorLogRepository(settingsManager, configRepository);
            var logRepository = new LogRepository(settingsManager, configRepository, errorLogRepository);
            var pluginConfigRepository = new PluginConfigRepository(settingsManager);
            var pluginRepository = new PluginRepository(settingsManager);
            var relatedFieldItemRepository = new RelatedFieldItemRepository(settingsManager);
            var relatedFieldRepository = new RelatedFieldRepository(settingsManager);
            var roleRepository = new RoleRepository(settingsManager);
            var siteLogRepository = new SiteLogRepository(settingsManager, configRepository, errorLogRepository, logRepository);
            var siteRepository = new SiteRepository(cache, settingsManager);
            var specialRepository = new SpecialRepository(cache, settingsManager);

            var tableStyleItemRepository = new TableStyleItemRepository(settingsManager);
            var tagRepository = new TagRepository(cache, settingsManager);
            var templateLogRepository = new TemplateLogRepository(settingsManager);
            var userGroupRepository = new UserGroupRepository(cache, settingsManager, configRepository);
            var userLogRepository = new UserLogRepository(settingsManager, configRepository);
            var userMenuRepository = new UserMenuRepository(cache, settingsManager);
            var userRepository = new UserRepository(cache, settingsManager, configRepository, userRoleRepository);
            var permissionRepository = new PermissionRepository(settingsManager, roleRepository);

            var databaseRepository = new DatabaseRepository(cache, settingsManager, configRepository, errorLogRepository, userRepository);

            var channelRepository = new ChannelRepository(cache, settingsManager, databaseRepository, contentCheckRepository, userRepository, siteRepository, channelGroupRepository, tagRepository, errorLogRepository);
            var templateRepository = new TemplateRepository(cache, settingsManager, siteRepository, channelRepository, templateLogRepository);
            var tableStyleRepository = new TableStyleRepository(cache, settingsManager, databaseRepository, siteRepository, channelRepository, userRepository, tableStyleItemRepository, errorLogRepository);

            var pathManager = new PathManager(settingsManager, tableStyleRepository, siteRepository, channelRepository, templateRepository);
            var pluginManager = new PluginManager(cache, settingsManager, pathManager, databaseRepository, pluginRepository, siteRepository, channelRepository, tableStyleRepository, errorLogRepository);
            var urlManager = new UrlManager(settingsManager, pathManager, pluginManager, configRepository, siteRepository, channelRepository, specialRepository, templateRepository, errorLogRepository);
            var fileManager = new FileManager(settingsManager, urlManager, pathManager, pluginManager, siteRepository, channelRepository, templateRepository, tagRepository, errorLogRepository);
            var createManager = new CreateManager(config, cache, settingsManager, pluginManager, urlManager, pathManager, fileManager, siteRepository, channelRepository, specialRepository, userRepository, tableStyleRepository, templateRepository, tagRepository, errorLogRepository);

            Cache = cache;
            Configuration = config;
            SettingsManager = settingsManager;
            Database = database;
            PathManager = pathManager;
            UrlManager = urlManager;
            FileManager = fileManager;
            CreateManager = createManager;
            PluginManager = pluginManager;
            AccessTokenRepository = accessTokenRepository;
            UserRoleRepository = userRoleRepository;
            AreaRepository = areaRepository;
            ChannelGroupRepository = channelGroupRepository;
            ChannelRepository = channelRepository;
            ConfigRepository = configRepository;
            ContentCheckRepository = contentCheckRepository;
            ContentGroupRepository = contentGroupRepository;
            DatabaseRepository = databaseRepository;
            DbCacheRepository = dbCacheRepository;
            DepartmentRepository = departmentRepository;
            ErrorLogRepository = errorLogRepository;
            LogRepository = logRepository;
            PermissionRepository = permissionRepository;
            PluginConfigRepository = pluginConfigRepository;
            PluginRepository = pluginRepository;
            RelatedFieldItemRepository = relatedFieldItemRepository;
            RelatedFieldRepository = relatedFieldRepository;
            RoleRepository = roleRepository;
            SiteLogRepository = siteLogRepository;
            SiteRepository = siteRepository;
            SpecialRepository = specialRepository;
            TableStyleItemRepository = tableStyleItemRepository;
            TableStyleRepository = tableStyleRepository;
            TagRepository = tagRepository;
            TemplateLogRepository = templateLogRepository;
            TemplateRepository = templateRepository;
            UserGroupRepository = userGroupRepository;
            UserLogRepository = userLogRepository;
            UserMenuRepository = userMenuRepository;
            UserRepository = userRepository;

            var db = new Database(SettingsManager.DatabaseType, SettingsManager.DatabaseConnectionString);
            var tableNames = db.GetTableNamesAsync().GetAwaiter().GetResult();
            foreach (var tableName in tableNames)
            {
                db.DropTableAsync(tableName).GetAwaiter().GetResult();
            }

            var (_, repositories) = DatabaseUtils.GetAllRepositories(cache, settingsManager);

            databaseRepository.InstallDatabaseAsync("admin", "admin888", repositories).GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            var db = new Database(SettingsManager.DatabaseType, SettingsManager.DatabaseConnectionString);
            var tableNames = db.GetTableNamesAsync().GetAwaiter().GetResult();
            foreach (var tableName in tableNames)
            {
                db.DropTableAsync(tableName).GetAwaiter().GetResult();
            }
            // ... clean up test data from the database ...
        }
    }
}
