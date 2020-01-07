﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SS.CMS.Core.Models;
using SS.CMS.Models;
using SS.CMS.Utils;

namespace SS.CMS.Core.Repositories
{
    public partial class TagRepository
    {
        private async Task AddTagsAsync(List<string> tags, int siteId, int contentId)
        {
            if (tags == null || tags.Count == 0) return;

            foreach (var tagName in tags)
            {
                var tagInfo = await GetTagInfoAsync(siteId, tagName);
                if (tagInfo != null)
                {
                    var contentIdList = TranslateUtils.StringCollectionToIntList(tagInfo.ContentIdCollection);
                    if (!contentIdList.Contains(contentId))
                    {
                        contentIdList.Add(contentId);
                        tagInfo.ContentIdCollection = TranslateUtils.ObjectCollectionToString(contentIdList);
                        tagInfo.UseNum = contentIdList.Count;
                        await UpdateAsync(tagInfo);
                    }
                }
                else
                {
                    tagInfo = new Tag
                    {
                        SiteId = siteId,
                        ContentIdCollection = contentId.ToString(),
                        Value = tagName,
                        UseNum = contentId > 0 ? 1 : 0
                    };
                    await InsertAsync(tagInfo);
                }
            }
        }

        private async Task RemoveTagsAsync(IEnumerable<string> tags, int siteId, int contentId)
        {
            if (tags == null || tags.Count() == 0) return;

            foreach (var tagName in tags)
            {
                var tagInfo = await GetTagInfoAsync(siteId, tagName);
                if (tagInfo == null) continue;

                var contentIdList = TranslateUtils.StringCollectionToIntList(tagInfo.ContentIdCollection);
                contentIdList.Remove(contentId);
                tagInfo.ContentIdCollection = TranslateUtils.ObjectCollectionToString(contentIdList);
                tagInfo.UseNum = contentIdList.Count;

                if (tagInfo.UseNum == 0)
                {
                    await DeleteTagAsync(tagName, siteId);
                }
                else
                {
                    await UpdateAsync(tagInfo);
                }
            }
        }

        public async Task UpdateTagsAsync(string tagsPrevious, string tagsNow, int siteId, int contentId)
        {
            if (tagsPrevious == tagsNow) return;

            var previousTags = ParseTagsString(tagsPrevious);
            var nowTags = ParseTagsString(tagsNow);

            var tagsToRemove = new List<string>();
            var tagsToAdd = new List<string>();

            foreach (var tag in previousTags)
            {
                if (!nowTags.Contains(tag))
                {
                    tagsToRemove.Add(tag);
                }
            }
            foreach (var tag in nowTags)
            {
                if (!previousTags.Contains(tag))
                {
                    tagsToAdd.Add(tag);
                }
            }

            await RemoveTagsAsync(tagsToRemove, siteId, contentId);
            await AddTagsAsync(tagsToAdd, siteId, contentId);
        }

        public async Task RemoveTagsAsync(int siteId, IEnumerable<int> contentIdList)
        {
            foreach (var contentId in contentIdList)
            {
                await RemoveTagsAsync(siteId, contentId);
            }
        }

        public async Task RemoveTagsAsync(int siteId, int contentId)
        {
            var tagInfoList = await GetTagInfoListAsync(siteId, contentId);
            if (tagInfoList == null) return;

            foreach (var tagInfo in tagInfoList)
            {
                var contentIdList = TranslateUtils.StringCollectionToIntList(tagInfo.ContentIdCollection);
                contentIdList.Remove(contentId);
                tagInfo.ContentIdCollection = TranslateUtils.ObjectCollectionToString(contentIdList);
                tagInfo.UseNum = contentIdList.Count;
                await UpdateAsync(tagInfo);
            }
        }

        public string GetTagsString(StringCollection tags)
        {
            if (tags == null || tags.Count == 0) return string.Empty;

            var tagsBuilder = new StringBuilder();
            foreach (var tag in tags)
            {
                tagsBuilder.Append(tag.Trim().IndexOf(",", StringComparison.Ordinal) != -1 ? $"\"{tag}\"" : tag);
                tagsBuilder.Append(" ");
            }
            --tagsBuilder.Length;
            return tagsBuilder.ToString();
        }

        public List<string> ParseTagsString(string tagsString)
        {
            var stringCollection = new List<string>();

            if (string.IsNullOrEmpty(tagsString)) return stringCollection;

            var regex = new Regex("\"([^\"]*)\"", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            var mc = regex.Matches(tagsString);
            for (var i = 0; i < mc.Count; i++)
            {
                if (!string.IsNullOrEmpty(mc[i].Value))
                {
                    var tag = mc[i].Value.Replace("\"", string.Empty);
                    if (!stringCollection.Contains(tag))
                    {
                        stringCollection.Add(tag);
                    }

                    var startIndex = tagsString.IndexOf(mc[i].Value, StringComparison.Ordinal);
                    if (startIndex != -1)
                    {
                        tagsString = tagsString.Substring(0, startIndex) + tagsString.Substring(startIndex + mc[i].Value.Length);
                    }
                }
            }

            regex = new Regex("([^,;\\s]+)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            mc = regex.Matches(tagsString);
            for (var i = 0; i < mc.Count; i++)
            {
                if (!string.IsNullOrEmpty(mc[i].Value))
                {
                    var tag = mc[i].Value.Replace("\"", string.Empty);
                    if (!stringCollection.Contains(tag))
                    {
                        stringCollection.Add(tag);
                    }
                }
            }

            return stringCollection;
        }

        public IList<Tag> GetTagInfoList(IEnumerable<Tag> tagInfoList)
        {
            return GetTagInfoList(tagInfoList, 0, 0);
        }

        public IList<Tag> GetTagInfoList(IEnumerable<Tag> tagInfoList, int totalNum, int tagLevel)
        {
            var list = new List<Tag>();
            var sortedlist = new SortedList();
            if (tagInfoList != null)
            {
                foreach (var tagInfo in tagInfoList)
                {
                    list.Add(tagInfo);

                    var tagNames = (List<string>)sortedlist[tagInfo.UseNum];
                    if (tagNames == null || tagNames.Count == 0)
                    {
                        tagNames = new List<string>();
                    }
                    tagNames.Add(tagInfo.Value);
                    sortedlist[tagInfo.UseNum] = tagNames;
                }
            }

            var count1 = 1;
            var count2 = 2;
            var count3 = 3;
            if (sortedlist.Keys.Count > 3)
            {
                count1 = (int)Math.Ceiling(0.3 * sortedlist.Keys.Count);
                if (count1 < 1)
                {
                    count1 = 1;
                }
                count2 = (int)Math.Ceiling(0.7 * sortedlist.Keys.Count);
                if (count2 == sortedlist.Keys.Count)
                {
                    count2--;
                }
                if (count2 <= count1)
                {
                    count2++;
                }
                count3 = count2 + 1;
            }

            var currentCount = 0;
            foreach (int count in sortedlist.Keys)
            {
                currentCount++;

                var level = 1;

                if (currentCount <= count1)
                {
                    level = 1;
                }
                else if (currentCount > count1 && currentCount <= count2)
                {
                    level = 2;
                }
                else if (currentCount > count2 && currentCount <= count3)
                {
                    level = 3;
                }
                else if (currentCount > count3)
                {
                    level = 4;
                }

                var tagNames = (List<string>)sortedlist[count];
                foreach (var tagInfo in list)
                {
                    if (tagNames.Contains(tagInfo.Value))
                    {
                        tagInfo.Level = level;
                    }
                }
            }

            if (tagLevel > 1)
            {
                var levelList = new List<Tag>();
                foreach (var tagInfo in list)
                {
                    if (tagInfo.Level >= tagLevel)
                    {
                        levelList.Add(tagInfo);
                    }
                    if (totalNum > 0 && levelList.Count > totalNum)
                    {
                        break;
                    }
                }
                list = levelList;
            }

            return list;
        }
    }
}
