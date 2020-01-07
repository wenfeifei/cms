﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SS.CMS.Core.Common.Office;
using SS.CMS.Core.Common.Enums;
using SS.CMS.Core.Serialization.Components;
using SS.CMS.Models;
using SS.CMS.Repositories;
using SS.CMS.Services;
using SS.CMS.Utils;
using SS.CMS.Utils.Atom.Atom.Core;

namespace SS.CMS.Core.Serialization
{
    public class ImportObject
    {
        private Site _siteInfo;
        private string _sitePath;
        private int _userId;
        private ISettingsManager _settingsManager;
        private IPluginManager _pluginManager;
        private ICreateManager _createManager;
        private IPathManager _pathManager;
        private IFileManager _fileManager;
        private IDbCacheRepository _dbCacheRepository;
        private ISiteRepository _siteRepository;
        private IChannelRepository _channelRepository;
        private IChannelGroupRepository _channelGroupRepository;
        private IContentGroupRepository _contentGroupRepository;
        private ISpecialRepository _specialRepository;
        private ITableStyleRepository _tableStyleRepository;
        private ITemplateRepository _templateRepository;

        public async Task LoadAsync(int siteId, int userId)
        {
            _siteInfo = await _siteRepository.GetSiteAsync(siteId);
            _sitePath = PathUtils.Combine(_settingsManager.WebRootPath, _siteInfo.SiteDir);
            _userId = userId;
        }

        //获取保存辅助表名称对应集合数据库缓存键
        private string GetTableNameNameValueCollectionDbCacheKey()
        {
            return "SiteServer.CMS.Core.ImportObject.TableNameNameValueCollection_" + _siteInfo.Id;
        }

        public async Task<NameValueCollection> GetTableNameCacheAsync()
        {
            NameValueCollection nameValueCollection = null;
            var cacheValue = await _dbCacheRepository.GetValueAsync(GetTableNameNameValueCollectionDbCacheKey());
            if (!string.IsNullOrEmpty(cacheValue))
            {
                nameValueCollection = TranslateUtils.ToNameValueCollection(cacheValue);
            }
            return nameValueCollection;
        }

        public async Task SaveTableNameCacheAsync(NameValueCollection nameValueCollection)
        {
            if (nameValueCollection != null && nameValueCollection.Count > 0)
            {
                var cacheKey = GetTableNameNameValueCollectionDbCacheKey();
                var cacheValue = TranslateUtils.NameValueCollectionToString(nameValueCollection);
                await _dbCacheRepository.RemoveAndInsertAsync(cacheKey, cacheValue);
            }
        }

        public async Task RemoveDbCacheAsync()
        {
            var cacheKey = GetTableNameNameValueCollectionDbCacheKey();
            await _dbCacheRepository.GetValueAndRemoveAsync(cacheKey);
        }

        public async Task ImportFilesAsync(string siteTemplatePath, bool isOverride)
        {
            //if (this.FSO.IsRoot)
            //{
            //    string[] filePaths = DirectoryUtils.GetFilePaths(siteTemplatePath);
            //    foreach (string filePath in filePaths)
            //    {
            //        string fileName = PathUtils.GetFileName(filePath);
            //        if (!PathUtility.IsSystemFile(fileName))
            //        {
            //            string destFilePath = PathUtils.Combine(FSO.SitePath, fileName);
            //            FileUtils.MoveFile(filePath, destFilePath, isOverride);
            //        }
            //    }

            //    ArrayList siteDirArrayList = DataProvider.SiteDAO.GetLowerSiteDirArrayListThatNotIsRoot();

            //    string[] directoryPaths = DirectoryUtils.GetDirectoryPaths(siteTemplatePath);
            //    foreach (string subDirectoryPath in directoryPaths)
            //    {
            //        string directoryName = PathUtils.GetDirectoryName(subDirectoryPath);
            //        if (!PathUtility.IsSystemDirectory(directoryName) && !siteDirArrayList.Contains(directoryName.ToLower()))
            //        {
            //            string destDirectoryPath = PathUtils.Combine(FSO.SitePath, directoryName);
            //            DirectoryUtils.MoveDirectory(subDirectoryPath, destDirectoryPath, isOverride);
            //        }
            //    }
            //}
            //else
            //{
            //    DirectoryUtils.MoveDirectory(siteTemplatePath, FSO.SitePath, isOverride);
            //}
            //string siteTemplateMetadataPath = PathUtils.Combine(FSO.SitePath, _fileManager.SiteTemplates.SiteTemplateMetadata);
            //DirectoryUtils.DeleteDirectoryIfExists(siteTemplateMetadataPath);
            await _fileManager.ImportSiteFilesAsync(_siteInfo, siteTemplatePath, isOverride);
        }

        public async Task ImportSiteContentAsync(string siteContentDirectoryPath, string filePath, bool isImportContents)
        {
            var siteIe = new SiteIe(_siteInfo, siteContentDirectoryPath);
            await siteIe.ImportChannelsAndContentsAsync(filePath, isImportContents, false, 0, _userId);
        }


        /// <summary>
        /// 从指定的地址导入网站模板至站点中
        /// </summary>
        public async Task ImportTemplatesAsync(string filePath, bool overwrite, int userId)
        {
            var templateIe = new TemplateIe(_siteInfo.Id, filePath);
            await templateIe.ImportTemplatesAsync(overwrite, userId);
        }

        public async Task ImportRelatedFieldByZipFileAsync(string zipFilePath, bool overwrite)
        {
            var directoryPath = _pathManager.GetTemporaryFilesPath("RelatedField");
            DirectoryUtils.DeleteDirectoryIfExists(directoryPath);
            DirectoryUtils.CreateDirectoryIfNotExists(directoryPath);

            ZipUtils.ExtractZip(zipFilePath, directoryPath);

            var relatedFieldIe = new RelatedFieldIe(_siteInfo.Id, directoryPath);
            await relatedFieldIe.ImportRelatedFieldAsync(overwrite);
        }

        public async Task ImportTableStylesAsync(string tableDirectoryPath)
        {
            if (DirectoryUtils.IsDirectoryExists(tableDirectoryPath))
            {
                var tableStyleIe = new TableStyleIe(tableDirectoryPath, _userId);
                await tableStyleIe.ImportTableStylesAsync(_siteInfo.Id);
            }
        }

        public static async Task ImportTableStyleByZipFileAsync(IPathManager pathManager, ITableStyleRepository tableStyleRepository, string tableName, int channelId, string zipFilePath)
        {
            var styleDirectoryPath = pathManager.GetTemporaryFilesPath("TableStyle");
            DirectoryUtils.DeleteDirectoryIfExists(styleDirectoryPath);
            DirectoryUtils.CreateDirectoryIfNotExists(styleDirectoryPath);

            ZipUtils.ExtractZip(zipFilePath, styleDirectoryPath);

            await TableStyleIe.SingleImportTableStyleAsync(tableStyleRepository, tableName, styleDirectoryPath, channelId);
        }

        public async Task ImportConfigurationAsync(string configurationFilePath)
        {
            var configIe = new ConfigurationIe(_siteInfo.Id, configurationFilePath);
            await configIe.ImportAsync();
        }


        public async Task ImportChannelsAndContentsByZipFileAsync(int parentId, string zipFilePath, bool isOverride)
        {
            var siteContentDirectoryPath = _pathManager.GetTemporaryFilesPath(EBackupTypeUtils.GetValue(EBackupType.ChannelsAndContents));
            DirectoryUtils.DeleteDirectoryIfExists(siteContentDirectoryPath);
            DirectoryUtils.CreateDirectoryIfNotExists(siteContentDirectoryPath);

            ZipUtils.ExtractZip(zipFilePath, siteContentDirectoryPath);

            await ImportChannelsAndContentsFromZipAsync(parentId, siteContentDirectoryPath, isOverride);

            var uploadFolderPath = PathUtils.Combine(siteContentDirectoryPath, BackupUtility.UploadFolderName);
            var uploadFilePath = PathUtils.Combine(uploadFolderPath, BackupUtility.UploadFileName);
            if (!FileUtils.IsFileExists(uploadFilePath))
            {
                return;
            }
            var feed = AtomFeed.Load(FileUtils.GetFileStreamReadOnly(uploadFilePath));
            if (feed != null)
            {
                AtomEntry entry = feed.Entries[0];
                string imageUploadDirectoryPath = AtomUtility.GetDcElementContent(entry.AdditionalElements, "ImageUploadDirectoryName");
                if (imageUploadDirectoryPath != null)
                {
                    DirectoryUtils.MoveDirectory(PathUtils.Combine(siteContentDirectoryPath, imageUploadDirectoryPath), PathUtils.Combine(_sitePath, _siteInfo.ImageUploadDirectoryName), isOverride);
                }
                string videoUploadDirectoryPath = AtomUtility.GetDcElementContent(entry.AdditionalElements, "VideoUploadDirectoryName");
                if (videoUploadDirectoryPath != null)
                {
                    DirectoryUtils.MoveDirectory(PathUtils.Combine(siteContentDirectoryPath, videoUploadDirectoryPath), PathUtils.Combine(_sitePath, _siteInfo.VideoUploadDirectoryName), isOverride);
                }
                string fileUploadDirectoryPath = AtomUtility.GetDcElementContent(entry.AdditionalElements, "FileUploadDirectoryName");
                if (fileUploadDirectoryPath != null)
                {
                    DirectoryUtils.MoveDirectory(PathUtils.Combine(siteContentDirectoryPath, fileUploadDirectoryPath), PathUtils.Combine(_sitePath, _siteInfo.FileUploadDirectoryName), isOverride);
                }
            }
        }

        public async Task ImportChannelsAndContentsFromZipAsync(int parentId, string siteContentDirectoryPath, bool isOverride)
        {
            var filePathList = GetSiteContentFilePathList(siteContentDirectoryPath);

            var siteIe = new SiteIe(_siteInfo, siteContentDirectoryPath);

            Hashtable levelHashtable = null;
            foreach (var filePath in filePathList)
            {
                var firstIndex = filePath.LastIndexOf(PathUtils.SeparatorChar) + 1;
                var lastIndex = filePath.LastIndexOf(".", StringComparison.Ordinal);
                var orderString = filePath.Substring(firstIndex, lastIndex - firstIndex);

                var level = StringUtils.GetCount("_", orderString);

                if (levelHashtable == null)
                {
                    levelHashtable = new Hashtable
                    {
                        [level] = parentId
                    };
                }

                var insertChannelId = await siteIe.ImportChannelsAndContentsAsync(filePath, true, isOverride, (int)levelHashtable[level], _userId);
                levelHashtable[level + 1] = insertChannelId;
            }
        }

        public async Task ImportChannelsAndContentsAsync(int parentId, string siteContentDirectoryPath, bool isOverride)
        {
            var filePathList = GetSiteContentFilePathList(siteContentDirectoryPath);

            var siteIe = new SiteIe(_siteInfo, siteContentDirectoryPath);

            var parentOrderString = "none";
            //int parentID = 0;
            foreach (var filePath in filePathList)
            {
                var firstIndex = filePath.LastIndexOf(PathUtils.SeparatorChar) + 1;
                var lastIndex = filePath.LastIndexOf(".", StringComparison.Ordinal);
                var orderString = filePath.Substring(firstIndex, lastIndex - firstIndex);

                if (StringUtils.StartsWithIgnoreCase(orderString, parentOrderString))
                {
                    parentId = await siteIe.ImportChannelsAndContentsAsync(filePath, true, isOverride, parentId, _userId);
                    parentOrderString = orderString;
                }
                else
                {
                    await siteIe.ImportChannelsAndContentsAsync(filePath, true, isOverride, parentId, _userId);
                }
            }
        }

        public async Task ImportContentsByZipFileAsync(Channel nodeInfo, string zipFilePath, bool isOverride, int importStart, int importCount, bool isChecked, int checkedLevel)
        {
            var siteContentDirectoryPath = _pathManager.GetTemporaryFilesPath("contents");
            DirectoryUtils.DeleteDirectoryIfExists(siteContentDirectoryPath);
            DirectoryUtils.CreateDirectoryIfNotExists(siteContentDirectoryPath);

            ZipUtils.ExtractZip(zipFilePath, siteContentDirectoryPath);

            var contentRepository = _channelRepository.GetContentRepository(_siteInfo, nodeInfo);

            var taxis = await contentRepository.GetMaxTaxisAsync(nodeInfo.Id, false);

            await ImportContentsAsync(nodeInfo, siteContentDirectoryPath, isOverride, taxis, importStart, importCount, isChecked, checkedLevel);
        }

        public async Task ImportContentsByZipFileAsync(Channel nodeInfo, string zipFilePath, bool isOverride, bool isChecked, int checkedLevel, int userId, int sourceId)
        {
            var siteContentDirectoryPath = _pathManager.GetTemporaryFilesPath("contents");
            DirectoryUtils.DeleteDirectoryIfExists(siteContentDirectoryPath);
            DirectoryUtils.CreateDirectoryIfNotExists(siteContentDirectoryPath);

            ZipUtils.ExtractZip(zipFilePath, siteContentDirectoryPath);

            var contentRepository = _channelRepository.GetContentRepository(_siteInfo, nodeInfo);

            var taxis = await contentRepository.GetMaxTaxisAsync(nodeInfo.Id, false);

            await ImportContentsAsync(nodeInfo, siteContentDirectoryPath, isOverride, taxis, isChecked, checkedLevel, userId, sourceId);
        }

        public async Task ImportContentsByCsvFileAsync(int channelId, string csvFilePath, bool isOverride, int importStart, int importCount, bool isChecked, int checkedLevel)
        {
            var channelInfo = await _channelRepository.GetChannelAsync(channelId);
            var contentRepository = _channelRepository.GetContentRepository(_siteInfo, channelInfo);

            var contentInfoList = await ExcelObject.GetContentsByCsvFileAsync(_pluginManager, _tableStyleRepository, csvFilePath, _siteInfo, channelInfo);
            contentInfoList.Reverse();

            if (importStart > 1 || importCount > 0)
            {
                var theList = new List<Content>();

                if (importStart == 0)
                {
                    importStart = 1;
                }
                if (importCount == 0)
                {
                    importCount = contentInfoList.Count;
                }

                var firstIndex = contentInfoList.Count - importStart - importCount + 1;
                if (firstIndex <= 0)
                {
                    firstIndex = 0;
                }

                var addCount = 0;
                for (var i = 0; i < contentInfoList.Count; i++)
                {
                    if (addCount >= importCount) break;
                    if (i >= firstIndex)
                    {
                        theList.Add(contentInfoList[i]);
                        addCount++;
                    }
                }

                contentInfoList = theList;
            }

            foreach (var contentInfo in contentInfoList)
            {
                contentInfo.IsChecked = isChecked;
                contentInfo.CheckedLevel = checkedLevel;
                if (isOverride)
                {
                    var existsIds = await contentRepository.GetIdListBySameTitleAsync(contentInfo.ChannelId, contentInfo.Title);
                    if (existsIds.Count() > 0)
                    {
                        foreach (var id in existsIds)
                        {
                            contentInfo.Id = id;
                            await contentRepository.UpdateAsync(_siteInfo, channelInfo, contentInfo);
                        }
                    }
                    else
                    {
                        contentInfo.Id = await contentRepository.InsertAsync(_siteInfo, channelInfo, contentInfo);
                    }
                }
                else
                {
                    contentInfo.Id = await contentRepository.InsertAsync(_siteInfo, channelInfo, contentInfo);
                }
                //this.FSO.AddContentToWaitingCreate(contentInfo.ChannelId, contentID);
            }
        }

        public async Task ImportContentsByCsvFileAsync(Channel channelInfo, string csvFilePath, bool isOverride, bool isChecked, int checkedLevel, int userId, int sourceId)
        {
            var contentRepository = _channelRepository.GetContentRepository(_siteInfo, channelInfo);
            var contentInfoList = await ExcelObject.GetContentsByCsvFileAsync(_pluginManager, _tableStyleRepository, csvFilePath, _siteInfo, channelInfo);
            contentInfoList.Reverse();

            foreach (var contentInfo in contentInfoList)
            {
                contentInfo.IsChecked = isChecked;
                contentInfo.CheckedLevel = checkedLevel;
                contentInfo.AddDate = DateTime.Now;
                contentInfo.UserId = userId;
                contentInfo.SourceId = sourceId;

                if (isOverride)
                {
                    var existsIds = await contentRepository.GetIdListBySameTitleAsync(contentInfo.ChannelId, contentInfo.Title);
                    if (existsIds.Count() > 0)
                    {
                        foreach (var id in existsIds)
                        {
                            contentInfo.Id = id;
                            await contentRepository.UpdateAsync(_siteInfo, channelInfo, contentInfo);
                        }
                    }
                    else
                    {
                        contentInfo.Id = await contentRepository.InsertAsync(_siteInfo, channelInfo, contentInfo);
                    }
                }
                else
                {
                    contentInfo.Id = await contentRepository.InsertAsync(_siteInfo, channelInfo, contentInfo);
                }
            }
        }

        public async Task ImportContentsByTxtZipFileAsync(int channelId, string zipFilePath, bool isOverride, int importStart, int importCount, bool isChecked, int checkedLevel)
        {
            var directoryPath = _pathManager.GetTemporaryFilesPath("contents");
            DirectoryUtils.DeleteDirectoryIfExists(directoryPath);
            DirectoryUtils.CreateDirectoryIfNotExists(directoryPath);

            ZipUtils.ExtractZip(zipFilePath, directoryPath);

            var channelInfo = await _channelRepository.GetChannelAsync(channelId);
            var contentRepository = _channelRepository.GetContentRepository(_siteInfo, channelInfo);

            var contentInfoList = TxtObject.GetContentListByTxtFile(directoryPath, _siteInfo, channelInfo);

            if (importStart > 1 || importCount > 0)
            {
                var theList = new List<Content>();

                if (importStart == 0)
                {
                    importStart = 1;
                }
                if (importCount == 0)
                {
                    importCount = contentInfoList.Count;
                }

                var firstIndex = contentInfoList.Count - importStart - importCount + 1;
                if (firstIndex <= 0)
                {
                    firstIndex = 0;
                }

                var addCount = 0;
                for (var i = 0; i < contentInfoList.Count; i++)
                {
                    if (addCount >= importCount) break;
                    if (i >= firstIndex)
                    {
                        theList.Add(contentInfoList[i]);
                        addCount++;
                    }
                }

                contentInfoList = theList;
            }

            foreach (var contentInfo in contentInfoList)
            {
                contentInfo.IsChecked = isChecked;
                contentInfo.CheckedLevel = checkedLevel;

                //int contentID = DataProvider.ContentRepository.Insert(tableName, this.FSO.SiteInfo, contentInfo);

                if (isOverride)
                {
                    var existsIDs = await contentRepository.GetIdListBySameTitleAsync(contentInfo.ChannelId, contentInfo.Title);
                    if (existsIDs.Count() > 0)
                    {
                        foreach (int id in existsIDs)
                        {
                            contentInfo.Id = id;
                            await contentRepository.UpdateAsync(_siteInfo, channelInfo, contentInfo);
                        }
                    }
                    else
                    {
                        contentInfo.Id = await contentRepository.InsertAsync(_siteInfo, channelInfo, contentInfo);
                    }
                }
                else
                {
                    contentInfo.Id = await contentRepository.InsertAsync(_siteInfo, channelInfo, contentInfo);
                }

                //this.FSO.AddContentToWaitingCreate(contentInfo.ChannelId, contentID);
            }
        }

        public async Task ImportContentsByTxtFileAsync(Channel channelInfo, string txtFilePath, bool isOverride, bool isChecked, int checkedLevel, int userId, int sourceId)
        {
            var contentRepository = _channelRepository.GetContentRepository(_siteInfo, channelInfo);
            var contentInfo = new Content
            {
                SiteId = channelInfo.SiteId,
                ChannelId = channelInfo.Id,
                Title = PathUtils.GetFileNameWithoutExtension(txtFilePath),
                Body = StringUtils.ReplaceNewlineToBr(FileUtils.ReadText(txtFilePath, Encoding.UTF8)),
                IsChecked = isChecked,
                CheckedLevel = checkedLevel,
                AddDate = DateTime.Now,
                UserId = userId,
                SourceId = sourceId
            };

            if (isOverride)
            {
                var existsIDs = await contentRepository.GetIdListBySameTitleAsync(contentInfo.ChannelId, contentInfo.Title);
                if (existsIDs.Count() > 0)
                {
                    foreach (var id in existsIDs)
                    {
                        contentInfo.Id = id;
                        await contentRepository.UpdateAsync(_siteInfo, channelInfo, contentInfo);
                    }
                }
                else
                {
                    contentInfo.Id = await contentRepository.InsertAsync(_siteInfo, channelInfo, contentInfo);
                }
            }
            else
            {
                contentInfo.Id = await contentRepository.InsertAsync(_siteInfo, channelInfo, contentInfo);
            }
        }

        public async Task ImportContentsAsync(Channel nodeInfo, string siteContentDirectoryPath, bool isOverride, int taxis, int importStart, int importCount, bool isChecked, int checkedLevel)
        {
            var filePath = PathUtils.Combine(siteContentDirectoryPath, "contents.xml");

            var contentIe = new ContentIe(_siteInfo, siteContentDirectoryPath);

            await contentIe.ImportContentsAsync(filePath, isOverride, nodeInfo, taxis, importStart, importCount, isChecked, checkedLevel, _userId);

            FileUtils.DeleteFileIfExists(filePath);

            DirectoryUtils.MoveDirectory(siteContentDirectoryPath, _sitePath, isOverride);
        }

        public async Task ImportContentsAsync(Channel nodeInfo, string siteContentDirectoryPath, bool isOverride, int taxis, bool isChecked, int checkedLevel, int userId, int sourceId)
        {
            var filePath = PathUtils.Combine(siteContentDirectoryPath, "contents.xml");

            var contentIe = new ContentIe(_siteInfo, siteContentDirectoryPath);

            await contentIe.ImportContentsAsync(filePath, isOverride, nodeInfo, taxis, isChecked, checkedLevel, userId, sourceId);

            FileUtils.DeleteFileIfExists(filePath);

            DirectoryUtils.MoveDirectory(siteContentDirectoryPath, _sitePath, isOverride);
        }

        //public void ImportInputContentsByCsvFile(InputInfo inputInfo, string excelFilePath, int importStart, int importCount, bool isChecked)
        //{
        //    var contentInfoList = ExcelObject.GetInputContentsByCsvFile(excelFilePath, _siteInfo, inputInfo);
        //    contentInfoList.Reverse();

        //    if (importStart > 1 || importCount > 0)
        //    {
        //        var theList = new List<InputContentInfo>();

        //        if (importStart == 0)
        //        {
        //            importStart = 1;
        //        }
        //        if (importCount == 0)
        //        {
        //            importCount = contentInfoList.Count;
        //        }

        //        var firstIndex = contentInfoList.Count - importStart - importCount + 1;
        //        if (firstIndex <= 0)
        //        {
        //            firstIndex = 0;
        //        }

        //        var addCount = 0;
        //        for (var i = 0; i < contentInfoList.Count; i++)
        //        {
        //            if (addCount >= importCount) break;
        //            if (i >= firstIndex)
        //            {
        //                theList.Add(contentInfoList[i]);
        //                addCount++;
        //            }
        //        }

        //        contentInfoList = theList;
        //    }

        //    foreach (var contentInfo in contentInfoList)
        //    {
        //        contentInfo.IsChecked = isChecked;
        //        DataProvider.InputContentRepository.Insert(contentInfo);
        //    }
        //}

        public static IList<string> GetSiteContentFilePathList(string siteContentDirectoryPath)
        {
            var filePaths = DirectoryUtils.GetFilePaths(siteContentDirectoryPath);
            var filePathSortedList = new SortedList<string, string>();
            foreach (var filePath in filePaths)
            {
                var keyBuilder = new StringBuilder();
                var fileName = PathUtils.GetFileName(filePath).ToLower().Replace(".xml", "");
                var nums = fileName.Split('_');
                foreach (var numStr in nums)
                {
                    var count = 7 - numStr.Length;
                    if (count > 0)
                    {
                        for (var i = 0; i < count; i++)
                        {
                            keyBuilder.Append("0");
                        }
                    }
                    keyBuilder.Append(numStr);
                    keyBuilder.Append("_");
                }
                if (keyBuilder.Length > 0) keyBuilder.Remove(keyBuilder.Length - 1, 1);
                filePathSortedList.Add(keyBuilder.ToString(), filePath);
            }
            return filePathSortedList.Values;
        }

    }
}
