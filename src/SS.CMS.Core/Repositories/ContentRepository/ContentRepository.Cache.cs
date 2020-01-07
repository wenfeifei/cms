﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SS.CMS.Core.Common;
using SS.CMS.Core.Models.Attributes;
using SS.CMS.Core.Plugin;
using SS.CMS.Enums;
using SS.CMS.Models;
using SS.CMS.Utils;

namespace SS.CMS.Core.Repositories
{
    public partial class ContentRepository
    {
        public async Task RemoveCacheBySiteIdAsync(string tableName, int siteId)
        {
            foreach (var channelId in await _channelRepository.GetIdListAsync(siteId))
            {
                ListRemove(channelId);
            }
            CountClear(tableName);
        }

        public void RemoveCache(string tableName, int channelId)
        {
            ListRemove(channelId);
            CountClear(tableName);
        }

        public void RemoveCountCache(string tableName)
        {
            CountClear(tableName);
        }

        private async Task InsertCacheAsync(Site siteInfo, Channel channelInfo, Content contentInfo)
        {
            if (contentInfo.SourceId == SourceManager.Preview) return;

            ListAdd(channelInfo, contentInfo);

            var tableName = _channelRepository.GetTableName(siteInfo, channelInfo);
            await CountAddAsync(tableName, contentInfo);
        }

        private async Task UpdateCacheAsync(Site siteInfo, Channel channelInfo, Content contentInfoToUpdate)
        {
            var contentInfo = await GetContentInfoAsync(contentInfoToUpdate.Id);
            if (contentInfo != null)
            {
                if (ListIsChanged(channelInfo, contentInfo, contentInfoToUpdate))
                {
                    ListRemove(channelInfo.Id);
                }

                if (CountIsChanged(contentInfo, contentInfoToUpdate))
                {
                    var tableName = _channelRepository.GetTableName(siteInfo, channelInfo);
                    await CountRemoveAsync(tableName, contentInfo);
                    await CountAddAsync(tableName, contentInfoToUpdate);
                }
            }
        }

        public async Task<Content> CalculateAsync(int sequence, Content contentInfo, List<ContentColumn> columns, Dictionary<string, Dictionary<string, Func<IContentContext, string>>> pluginColumns)
        {
            if (contentInfo == null) return null;

            var retVal = new Content(contentInfo.ToDictionary());

            foreach (var column in columns)
            {
                if (!column.IsCalculate) continue;

                if (StringUtils.EqualsIgnoreCase(column.AttributeName, ContentAttribute.Sequence))
                {
                    retVal.Set(ContentAttribute.Sequence, sequence);
                }
                else if (StringUtils.EqualsIgnoreCase(column.AttributeName, ContentAttribute.SourceId))
                {
                    retVal.Set(ContentAttribute.SourceId, await _channelRepository.GetSourceNameAsync(contentInfo.SourceId));
                }
                else if (StringUtils.EqualsIgnoreCase(column.AttributeName, ContentAttribute.UserId))
                {
                    var value = string.Empty;
                    if (contentInfo.UserId > 0)
                    {
                        var userInfo = await _userRepository.GetByUserIdAsync(contentInfo.UserId);
                        if (userInfo != null)
                        {
                            value = string.IsNullOrEmpty(userInfo.DisplayName) ? userInfo.UserName : userInfo.DisplayName;
                        }
                    }
                    retVal.Set(ContentAttribute.UserId, value);
                }
                else if (StringUtils.EqualsIgnoreCase(column.AttributeName, ContentAttribute.LastModifiedUserId))
                {
                    var value = string.Empty;
                    if (contentInfo.LastModifiedUserId > 0)
                    {
                        var userInfo = await _userRepository.GetByUserIdAsync(contentInfo.LastModifiedUserId);
                        if (userInfo != null)
                        {
                            value = string.IsNullOrEmpty(userInfo.DisplayName) ? userInfo.UserName : userInfo.DisplayName;
                        }
                    }
                    retVal.Set(ContentAttribute.LastModifiedUserId, value);
                }
            }

            if (pluginColumns != null)
            {
                foreach (var pluginId in pluginColumns.Keys)
                {
                    var contentColumns = pluginColumns[pluginId];
                    if (contentColumns == null || contentColumns.Count == 0) continue;

                    foreach (var columnName in contentColumns.Keys)
                    {
                        var attributeName = $"{pluginId}:{columnName}";
                        if (columns.All(x => x.AttributeName != attributeName)) continue;

                        try
                        {
                            var func = contentColumns[columnName];
                            var value = func(new ContentContext
                            {
                                SiteId = contentInfo.SiteId,
                                ChannelId = contentInfo.ChannelId,
                                ContentId = contentInfo.Id
                            });

                            retVal.Set(attributeName, value);
                        }
                        catch (Exception ex)
                        {
                            await _errorLogRepository.AddErrorLogAsync(pluginId, ex);
                        }
                    }
                }
            }

            return retVal;
        }

        public bool IsCreatable(Channel channelInfo, Content contentInfo)
        {
            if (channelInfo == null || contentInfo == null) return false;

            //引用链接，不需要生成内容页；引用内容，需要生成内容页；
            if (contentInfo.ReferenceId > 0 &&
                TranslateContentType.Parse(contentInfo.TranslateContentType) !=
                TranslateContentType.ReferenceContent)
            {
                return false;
            }

            return channelInfo.IsContentCreatable && string.IsNullOrEmpty(contentInfo.LinkUrl) && contentInfo.IsChecked && contentInfo.SourceId != SourceManager.Preview && contentInfo.ChannelId > 0;
        }
    }
}