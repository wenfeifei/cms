﻿using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using SS.CMS.Core.Models.Attributes;
using SS.CMS.Models;
using SS.CMS.Repositories;
using SS.CMS.Services;
using SS.CMS.Utils;
using SS.CMS.Utils.Atom.Atom.Core;
using SS.CMS.Utils.Atom.Atom.Core.Collections;

namespace SS.CMS.Core.Serialization.Components
{
    internal class ContentIe
    {
        private readonly Site _site;
        private readonly string _siteContentDirectoryPath;
        private readonly IPathManager _pathManager;
        private readonly IUrlManager _urlManager;
        private readonly IChannelRepository _channelRepository;
        private readonly ITagRepository _tagRepository;

        public ContentIe(Site site, string siteContentDirectoryPath)
        {
            _siteContentDirectoryPath = siteContentDirectoryPath;
            _site = site;
        }

        public async Task ImportContentsAsync(string filePath, bool isOverride, Channel nodeInfo, int taxis, int importStart, int importCount, bool isChecked, int checkedLevel, int userId)
        {
            if (!FileUtils.IsFileExists(filePath)) return;
            var feed = AtomFeed.Load(FileUtils.GetFileStreamReadOnly(filePath));

            await ImportContentsAsync(feed.Entries, nodeInfo, taxis, importStart, importCount, false, isChecked, checkedLevel, isOverride, userId);
        }

        public async Task ImportContentsAsync(string filePath, bool isOverride, Channel nodeInfo, int taxis, bool isChecked, int checkedLevel, int userId, int sourceId)
        {
            if (!FileUtils.IsFileExists(filePath)) return;
            var feed = AtomFeed.Load(FileUtils.GetFileStreamReadOnly(filePath));

            await ImportContentsAsync(feed.Entries, nodeInfo, taxis, false, isChecked, checkedLevel, isOverride, userId, sourceId);
        }

        public async Task ImportContentsAsync(AtomEntryCollection entries, Channel channel, int taxis, bool isOverride, int userId)
        {
            await ImportContentsAsync(entries, channel, taxis, 0, 0, true, true, 0, isOverride, userId);
        }

        // 内部消化掉错误
        private async Task ImportContentsAsync(AtomEntryCollection entries, Channel channel, int taxis, int importStart, int importCount, bool isCheckedBySettings, bool isChecked, int checkedLevel, bool isOverride, int userId)
        {
            if (importStart > 1 || importCount > 0)
            {
                var theEntries = new AtomEntryCollection();

                if (importStart == 0)
                {
                    importStart = 1;
                }
                if (importCount == 0)
                {
                    importCount = entries.Count;
                }

                var firstIndex = entries.Count - importStart - importCount + 1;
                if (firstIndex <= 0)
                {
                    firstIndex = 0;
                }

                var addCount = 0;
                for (var i = 0; i < entries.Count; i++)
                {
                    if (addCount >= importCount) break;
                    if (i >= firstIndex)
                    {
                        theEntries.Add(entries[i]);
                        addCount++;
                    }
                }

                entries = theEntries;
            }

            var contentRepository = _channelRepository.GetContentRepository(_site, channel);

            foreach (AtomEntry entry in entries)
            {
                try
                {
                    taxis++;
                    var groupNameCollection = AtomUtility.GetDcElementContent(entry.AdditionalElements, new List<string> { ContentAttribute.GroupNameCollection, "ContentGroupNameCollection" });
                    if (isCheckedBySettings)
                    {
                        isChecked = TranslateUtils.ToBool(AtomUtility.GetDcElementContent(entry.AdditionalElements, ContentAttribute.IsChecked));
                        checkedLevel = TranslateUtils.ToInt(AtomUtility.GetDcElementContent(entry.AdditionalElements, ContentAttribute.CheckedLevel));
                    }
                    var hits = TranslateUtils.ToInt(AtomUtility.GetDcElementContent(entry.AdditionalElements, ContentAttribute.Hits));
                    var hitsByDay = TranslateUtils.ToInt(AtomUtility.GetDcElementContent(entry.AdditionalElements, ContentAttribute.HitsByDay));
                    var hitsByWeek = TranslateUtils.ToInt(AtomUtility.GetDcElementContent(entry.AdditionalElements, ContentAttribute.HitsByWeek));
                    var hitsByMonth = TranslateUtils.ToInt(AtomUtility.GetDcElementContent(entry.AdditionalElements, ContentAttribute.HitsByMonth));
                    var lastHitsDate = AtomUtility.GetDcElementContent(entry.AdditionalElements, ContentAttribute.LastHitsDate);
                    var downloads = TranslateUtils.ToInt(AtomUtility.GetDcElementContent(entry.AdditionalElements, ContentAttribute.Downloads));
                    var title = AtomUtility.GetDcElementContent(entry.AdditionalElements, ContentAttribute.Title);
                    var isTop = TranslateUtils.ToBool(AtomUtility.GetDcElementContent(entry.AdditionalElements, ContentAttribute.IsTop));
                    var isRecommend = TranslateUtils.ToBool(AtomUtility.GetDcElementContent(entry.AdditionalElements, ContentAttribute.IsRecommend));
                    var isHot = TranslateUtils.ToBool(AtomUtility.GetDcElementContent(entry.AdditionalElements, ContentAttribute.IsHot));
                    var isColor = TranslateUtils.ToBool(AtomUtility.GetDcElementContent(entry.AdditionalElements, ContentAttribute.IsColor));
                    var linkUrl = AtomUtility.Decrypt(AtomUtility.GetDcElementContent(entry.AdditionalElements, ContentAttribute.LinkUrl));
                    var addDate = AtomUtility.GetDcElementContent(entry.AdditionalElements, ContentAttribute.AddDate);

                    var topTaxis = 0;
                    if (isTop)
                    {
                        topTaxis = taxis - 1;
                        taxis = await contentRepository.GetMaxTaxisAsync(channel.Id, true) + 1;
                    }
                    var tags = AtomUtility.Decrypt(AtomUtility.GetDcElementContent(entry.AdditionalElements, ContentAttribute.Tags));

                    var dict = new Dictionary<string, object>
                    {
                        {ContentAttribute.SiteId, _site.Id},
                        {ContentAttribute.ChannelId, channel.Id},
                        {ContentAttribute.UserId, userId},
                        {ContentAttribute.LastModifiedUserId, userId},
                        {ContentAttribute.AddDate, TranslateUtils.ToDateTime(addDate)}
                    };
                    var contentInfo = new Content(dict);

                    contentInfo.GroupNameCollection = groupNameCollection;
                    contentInfo.Tags = tags;
                    contentInfo.IsChecked = isChecked;
                    contentInfo.CheckedLevel = checkedLevel;
                    contentInfo.Hits = hits;
                    contentInfo.HitsByDay = hitsByDay;
                    contentInfo.HitsByWeek = hitsByWeek;
                    contentInfo.HitsByMonth = hitsByMonth;
                    contentInfo.LastHitsDate = TranslateUtils.ToDateTime(lastHitsDate);
                    contentInfo.Downloads = downloads;
                    contentInfo.Title = AtomUtility.Decrypt(title);
                    contentInfo.IsTop = isTop;
                    contentInfo.IsRecommend = isRecommend;
                    contentInfo.IsHot = isHot;
                    contentInfo.IsColor = isColor;
                    contentInfo.LinkUrl = linkUrl;

                    var attributes = AtomUtility.GetDcElementNameValueCollection(entry.AdditionalElements);
                    foreach (string attributeName in attributes.Keys)
                    {
                        if (!contentInfo.ContainsKey(attributeName.ToLower()))
                        {
                            contentInfo.Set(attributeName, AtomUtility.Decrypt(attributes[attributeName]));
                        }
                    }

                    var isInsert = false;
                    if (isOverride)
                    {
                        var existsIDs = await contentRepository.GetIdListBySameTitleAsync(contentInfo.ChannelId, contentInfo.Title);
                        if (existsIDs.Count() > 0)
                        {
                            foreach (int id in existsIDs)
                            {
                                contentInfo.Id = id;
                                await contentRepository.UpdateAsync(_site, channel, contentInfo);
                            }
                        }
                        else
                        {
                            isInsert = true;
                        }
                    }
                    else
                    {
                        isInsert = true;
                    }

                    if (isInsert)
                    {
                        var contentId = await contentRepository.InsertWithTaxisAsync(_site, channel, contentInfo, taxis);

                        await _tagRepository.UpdateTagsAsync(string.Empty, tags, _site.Id, contentId);
                    }

                    if (isTop)
                    {
                        taxis = topTaxis;
                    }
                }
                catch
                {
                    // ignored
                }
            }
        }

        private async Task ImportContentsAsync(AtomEntryCollection entries, Channel channel, int taxis, bool isCheckedBySettings, bool isChecked, int checkedLevel, bool isOverride, int userId, int sourceId)
        {
            var contentRepository = _channelRepository.GetContentRepository(_site, channel);

            foreach (AtomEntry entry in entries)
            {
                try
                {
                    taxis++;
                    var groupNameCollection = AtomUtility.GetDcElementContent(entry.AdditionalElements, new List<string> { ContentAttribute.GroupNameCollection, "ContentGroupNameCollection" });
                    if (isCheckedBySettings)
                    {
                        isChecked = TranslateUtils.ToBool(AtomUtility.GetDcElementContent(entry.AdditionalElements, ContentAttribute.IsChecked));
                        checkedLevel = TranslateUtils.ToInt(AtomUtility.GetDcElementContent(entry.AdditionalElements, ContentAttribute.CheckedLevel));
                    }
                    var hits = TranslateUtils.ToInt(AtomUtility.GetDcElementContent(entry.AdditionalElements, ContentAttribute.Hits));
                    var hitsByDay = TranslateUtils.ToInt(AtomUtility.GetDcElementContent(entry.AdditionalElements, ContentAttribute.HitsByDay));
                    var hitsByWeek = TranslateUtils.ToInt(AtomUtility.GetDcElementContent(entry.AdditionalElements, ContentAttribute.HitsByWeek));
                    var hitsByMonth = TranslateUtils.ToInt(AtomUtility.GetDcElementContent(entry.AdditionalElements, ContentAttribute.HitsByMonth));
                    var lastHitsDate = AtomUtility.GetDcElementContent(entry.AdditionalElements, ContentAttribute.LastHitsDate);
                    var downloads = TranslateUtils.ToInt(AtomUtility.GetDcElementContent(entry.AdditionalElements, ContentAttribute.Downloads));
                    var title = AtomUtility.GetDcElementContent(entry.AdditionalElements, ContentAttribute.Title);
                    var isTop = TranslateUtils.ToBool(AtomUtility.GetDcElementContent(entry.AdditionalElements, ContentAttribute.IsTop));
                    var isRecommend = TranslateUtils.ToBool(AtomUtility.GetDcElementContent(entry.AdditionalElements, ContentAttribute.IsRecommend));
                    var isHot = TranslateUtils.ToBool(AtomUtility.GetDcElementContent(entry.AdditionalElements, ContentAttribute.IsHot));
                    var isColor = TranslateUtils.ToBool(AtomUtility.GetDcElementContent(entry.AdditionalElements, ContentAttribute.IsColor));
                    var linkUrl = AtomUtility.Decrypt(AtomUtility.GetDcElementContent(entry.AdditionalElements, ContentAttribute.LinkUrl));
                    var addDate = AtomUtility.GetDcElementContent(entry.AdditionalElements, ContentAttribute.AddDate);

                    var topTaxis = 0;
                    if (isTop)
                    {
                        topTaxis = taxis - 1;
                        taxis = await contentRepository.GetMaxTaxisAsync(channel.Id, true) + 1;
                    }
                    var tags = AtomUtility.Decrypt(AtomUtility.GetDcElementContent(entry.AdditionalElements, ContentAttribute.Tags));

                    var dict = new Dictionary<string, object>
                    {
                        {ContentAttribute.SiteId, _site.Id},
                        {ContentAttribute.ChannelId, channel.Id},
                        {ContentAttribute.UserId, userId},
                        {ContentAttribute.LastModifiedUserId, userId},
                        {ContentAttribute.SourceId, sourceId},
                        {ContentAttribute.AddDate, TranslateUtils.ToDateTime(addDate)}
                    };
                    var contentInfo = new Content(dict);

                    contentInfo.GroupNameCollection = groupNameCollection;
                    contentInfo.Tags = tags;
                    contentInfo.IsChecked = isChecked;
                    contentInfo.CheckedLevel = checkedLevel;
                    contentInfo.Hits = hits;
                    contentInfo.HitsByDay = hitsByDay;
                    contentInfo.HitsByWeek = hitsByWeek;
                    contentInfo.HitsByMonth = hitsByMonth;
                    contentInfo.LastHitsDate = TranslateUtils.ToDateTime(lastHitsDate);
                    contentInfo.Downloads = downloads;
                    contentInfo.Title = AtomUtility.Decrypt(title);
                    contentInfo.IsTop = isTop;
                    contentInfo.IsRecommend = isRecommend;
                    contentInfo.IsHot = isHot;
                    contentInfo.IsColor = isColor;
                    contentInfo.LinkUrl = linkUrl;

                    var attributes = AtomUtility.GetDcElementNameValueCollection(entry.AdditionalElements);
                    foreach (string attributeName in attributes.Keys)
                    {
                        if (!contentInfo.ContainsKey(attributeName.ToLower()))
                        {
                            contentInfo.Set(attributeName, AtomUtility.Decrypt(attributes[attributeName]));
                        }
                    }

                    var isInsert = false;
                    if (isOverride)
                    {
                        var existsIDs = await contentRepository.GetIdListBySameTitleAsync(contentInfo.ChannelId, contentInfo.Title);
                        if (existsIDs.Count() > 0)
                        {
                            foreach (int id in existsIDs)
                            {
                                contentInfo.Id = id;
                                await contentRepository.UpdateAsync(_site, channel, contentInfo);
                            }
                        }
                        else
                        {
                            isInsert = true;
                        }
                    }
                    else
                    {
                        isInsert = true;
                    }

                    if (isInsert)
                    {
                        var contentId = await contentRepository.InsertWithTaxisAsync(_site, channel, contentInfo, taxis);

                        await _tagRepository.UpdateTagsAsync(string.Empty, tags, _site.Id, contentId);
                    }

                    if (isTop)
                    {
                        taxis = topTaxis;
                    }
                }
                catch
                {
                    // ignored
                }
            }
        }

        public async Task<bool> ExportContentsAsync(Site site, int channelId, IEnumerable<int> contentIdList, bool isPeriods, string dateFrom, string dateTo, bool? checkedState)
        {
            var filePath = _siteContentDirectoryPath + PathUtils.SeparatorChar + "contents.xml";
            var channel = await _channelRepository.GetChannelAsync(channelId);
            var contentRepository = _channelRepository.GetContentRepository(_site, channel);

            var feed = AtomUtility.GetEmptyFeed();

            if (contentIdList == null || contentIdList.Count() == 0)
            {
                contentIdList = await contentRepository.GetContentIdListAsync(channelId, isPeriods, dateFrom, dateTo, checkedState);
            }
            if (contentIdList.Count() == 0) return false;

            var collection = new NameValueCollection();

            foreach (var contentId in contentIdList)
            {
                var contentInfo = await contentRepository.GetContentInfoAsync(contentId);
                try
                {
                    _urlManager.PutImagePaths(site, contentInfo, collection);
                }
                catch
                {
                    // ignored
                }
                var entry = ExportContentInfo(contentInfo);
                feed.Entries.Add(entry);
            }
            feed.Save(filePath);

            foreach (string imageUrl in collection.Keys)
            {
                var sourceFilePath = collection[imageUrl];
                var destFilePath = _pathManager.MapPath(_siteContentDirectoryPath, imageUrl);
                DirectoryUtils.CreateDirectoryIfNotExists(destFilePath);
                FileUtils.MoveFile(sourceFilePath, destFilePath, true);
            }

            return true;
        }

        public bool ExportContents(Site site, List<Content> contentInfoList)
        {
            var filePath = _siteContentDirectoryPath + PathUtils.SeparatorChar + "contents.xml";
            var feed = AtomUtility.GetEmptyFeed();

            var collection = new NameValueCollection();

            foreach (var contentInfo in contentInfoList)
            {
                try
                {
                    _urlManager.PutImagePaths(site, contentInfo, collection);
                }
                catch
                {
                    // ignored
                }
                var entry = ExportContentInfo(contentInfo);
                feed.Entries.Add(entry);
            }
            feed.Save(filePath);

            foreach (string imageUrl in collection.Keys)
            {
                var sourceFilePath = collection[imageUrl];
                var destFilePath = _pathManager.MapPath(_siteContentDirectoryPath, imageUrl);
                DirectoryUtils.CreateDirectoryIfNotExists(destFilePath);
                FileUtils.MoveFile(sourceFilePath, destFilePath, true);
            }

            return true;
        }

        public AtomEntry ExportContentInfo(Content contentInfo)
        {
            var entry = AtomUtility.GetEmptyEntry();

            AtomUtility.AddDcElement(entry.AdditionalElements, ContentAttribute.Id, contentInfo.Id.ToString());
            AtomUtility.AddDcElement(entry.AdditionalElements, new List<string> { ContentAttribute.ChannelId, "NodeId" }, contentInfo.ChannelId.ToString());
            AtomUtility.AddDcElement(entry.AdditionalElements, new List<string> { ContentAttribute.SiteId, "PublishmentSystemId" }, contentInfo.SiteId.ToString());
            AtomUtility.AddDcElement(entry.AdditionalElements, ContentAttribute.Taxis, contentInfo.Taxis.ToString());
            AtomUtility.AddDcElement(entry.AdditionalElements, new List<string> { ContentAttribute.GroupNameCollection, "ContentGroupNameCollection" }, contentInfo.GroupNameCollection);
            AtomUtility.AddDcElement(entry.AdditionalElements, ContentAttribute.Tags, AtomUtility.Encrypt(contentInfo.Tags));
            AtomUtility.AddDcElement(entry.AdditionalElements, ContentAttribute.SourceId, contentInfo.SourceId.ToString());
            AtomUtility.AddDcElement(entry.AdditionalElements, ContentAttribute.ReferenceId, contentInfo.ReferenceId.ToString());
            AtomUtility.AddDcElement(entry.AdditionalElements, ContentAttribute.IsChecked, contentInfo.IsChecked.ToString());
            AtomUtility.AddDcElement(entry.AdditionalElements, ContentAttribute.CheckedLevel, contentInfo.CheckedLevel.ToString());
            AtomUtility.AddDcElement(entry.AdditionalElements, ContentAttribute.Hits, contentInfo.Hits.ToString());
            AtomUtility.AddDcElement(entry.AdditionalElements, ContentAttribute.HitsByDay, contentInfo.HitsByDay.ToString());
            AtomUtility.AddDcElement(entry.AdditionalElements, ContentAttribute.HitsByWeek, contentInfo.HitsByWeek.ToString());
            AtomUtility.AddDcElement(entry.AdditionalElements, ContentAttribute.HitsByMonth, contentInfo.HitsByMonth.ToString());
            if (contentInfo.LastHitsDate.HasValue)
            {
                AtomUtility.AddDcElement(entry.AdditionalElements, ContentAttribute.LastHitsDate,
                    contentInfo.LastHitsDate.Value.ToString(CultureInfo.InvariantCulture));
            }

            AtomUtility.AddDcElement(entry.AdditionalElements, ContentAttribute.Downloads, contentInfo.Downloads.ToString());
            AtomUtility.AddDcElement(entry.AdditionalElements, ContentAttribute.Title, AtomUtility.Encrypt(contentInfo.Title));
            AtomUtility.AddDcElement(entry.AdditionalElements, ContentAttribute.IsTop, contentInfo.IsTop.ToString());
            AtomUtility.AddDcElement(entry.AdditionalElements, ContentAttribute.IsRecommend, contentInfo.IsRecommend.ToString());
            AtomUtility.AddDcElement(entry.AdditionalElements, ContentAttribute.IsHot, contentInfo.IsHot.ToString());
            AtomUtility.AddDcElement(entry.AdditionalElements, ContentAttribute.IsColor, contentInfo.IsColor.ToString());
            AtomUtility.AddDcElement(entry.AdditionalElements, ContentAttribute.LinkUrl, AtomUtility.Encrypt(contentInfo.LinkUrl));
            if (contentInfo.AddDate.HasValue)
            {
                AtomUtility.AddDcElement(entry.AdditionalElements, ContentAttribute.AddDate,
                    contentInfo.AddDate.Value.ToString(CultureInfo.InvariantCulture));
            }

            foreach (var attributeName in contentInfo.ToDictionary().Keys)
            {
                if (!StringUtils.ContainsIgnoreCase(ContentAttribute.AllAttributes.Value, attributeName))
                {
                    AtomUtility.AddDcElement(entry.AdditionalElements, attributeName, AtomUtility.Encrypt(contentInfo.Get<string>(attributeName)));
                }
            }

            return entry;
        }
    }
}
