using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SS.CMS.Enums;
using SS.CMS.Models;
using SS.CMS.Services;
using SS.CMS.Utils;

namespace SS.CMS.Core.Services
{
    public partial class PathManager
    {
        private const string ContentRulesChannelId = "{@channelId}";
        private const string ContentRulesContentId = "{@contentId}";
        private const string ContentRulesYear = "{@year}";
        private const string ContentRulesMonth = "{@month}";
        private const string ContentRulesDay = "{@day}";
        private const string ContentRulesHour = "{@hour}";
        private const string ContentRulesMinute = "{@minute}";
        private const string ContentRulesSecond = "{@second}";
        private const string ContentRulesSequence = "{@sequence}";
        private const string ContentRulesParentRule = "{@parentRule}";
        private const string ContentRulesChannelName = "{@channelName}";
        private const string ContentRulesLowerChannelName = "{@lowerChannelName}";
        private const string ContentRulesChannelIndex = "{@channelIndex}";
        private const string ContentRulesLowerChannelIndex = "{@lowerChannelIndex}";

        public const string ContentRulesDefaultRule = "/contents/{@channelId}/{@contentId}.html";
        public const string ContentRulesDefaultDirectoryName = "/contents/";
        public const string ContentRulesDefaultRegexString = "/contents/(?<channelId>[^/]*)/(?<contentId>[^/]*)_?(?<pageIndex>[^_]*)";

        public async Task<IDictionary> ContentRulesGetDictionaryAsync(IPluginManager pluginManager, Site siteInfo, int channelId)
        {
            var dictionary = new ListDictionary
                {
                    {ContentRulesChannelId, "栏目ID"},
                    {ContentRulesContentId, "内容ID"},
                    {ContentRulesYear, "年份"},
                    {ContentRulesMonth, "月份"},
                    {ContentRulesDay, "日期"},
                    {ContentRulesHour, "小时"},
                    {ContentRulesMinute, "分钟"},
                    {ContentRulesSecond, "秒钟"},
                    {ContentRulesSequence, "顺序数"},
                    {ContentRulesParentRule, "父级命名规则"},
                    {ContentRulesChannelName, "栏目名称"},
                    {ContentRulesLowerChannelName, "栏目名称(小写)"},
                    {ContentRulesChannelIndex, "栏目索引"},
                    {ContentRulesLowerChannelIndex, "栏目索引(小写)"}
                };

            var channelInfo = await _channelRepository.GetChannelAsync(channelId);
            var styleInfoList = await _tableStyleRepository.GetContentStyleInfoListAsync(siteInfo, channelInfo);
            foreach (var styleInfo in styleInfoList)
            {
                if (styleInfo.Type == InputType.Text)
                {
                    dictionary.Add($@"{{@{StringUtils.LowerFirst(styleInfo.AttributeName)}}}", styleInfo.DisplayName);
                    dictionary.Add($@"{{@lower{styleInfo.AttributeName}}}", styleInfo.DisplayName + "(小写)");
                }
            }

            return dictionary;
        }

        public async Task<string> ContentRulesParseAsync(Site siteInfo, int channelId, int contentId)
        {
            var channelInfo = await _channelRepository.GetChannelAsync(channelId);
            var contentRepository = _channelRepository.GetContentRepository(siteInfo, channelInfo);

            var contentFilePathRule = await GetContentFilePathRuleAsync(siteInfo, channelId);
            var contentInfo = await contentRepository.GetContentInfoAsync(contentId);
            var filePath = await ContentRulesParseContentPathAsync(siteInfo, channelId, contentInfo, contentFilePathRule);
            return filePath;
        }

        public async Task<string> ContentRulesParseAsync(Site siteInfo, int channelId, Content contentInfo)
        {
            var contentFilePathRule = await GetContentFilePathRuleAsync(siteInfo, channelId);
            var filePath = await ContentRulesParseContentPathAsync(siteInfo, channelId, contentInfo, contentFilePathRule);
            return filePath;
        }

        private async Task<string> ContentRulesParseContentPathAsync(Site siteInfo, int channelId, Content contentInfo, string contentFilePathRule)
        {
            var filePath = contentFilePathRule.Trim();
            var regex = "(?<element>{@[^}]+})";
            var elements = RegexUtils.GetContents("element", regex, filePath);
            var addDate = contentInfo.AddDate;
            var contentId = contentInfo.Id;
            foreach (var element in elements)
            {
                var value = string.Empty;

                if (StringUtils.EqualsIgnoreCase(element, ContentRulesChannelId))
                {
                    value = channelId.ToString();
                }
                else if (StringUtils.EqualsIgnoreCase(element, ContentRulesContentId))
                {
                    value = contentId.ToString();
                }
                else if (StringUtils.EqualsIgnoreCase(element, ContentRulesSequence))
                {
                    var contentRepository = await _channelRepository.GetContentRepositoryAsync(siteInfo, channelId);
                    value = Convert.ToString(await contentRepository.GetSequenceAsync(channelId, contentId));
                }
                else if (StringUtils.EqualsIgnoreCase(element, ContentRulesParentRule))
                {
                    var nodeInfo = await _channelRepository.GetChannelAsync(channelId);
                    var parentInfo = await _channelRepository.GetChannelAsync(nodeInfo.ParentId);
                    if (parentInfo != null)
                    {
                        var parentRule = await GetContentFilePathRuleAsync(siteInfo, parentInfo.Id);
                        value = DirectoryUtils.GetDirectoryPath(await ContentRulesParseContentPathAsync(siteInfo, parentInfo.Id, contentInfo, parentRule)).Replace("\\", "/");
                    }
                }
                else if (StringUtils.EqualsIgnoreCase(element, ContentRulesChannelName))
                {
                    var nodeInfo = await _channelRepository.GetChannelAsync(channelId);
                    if (nodeInfo != null)
                    {
                        value = nodeInfo.ChannelName;
                    }
                }
                else if (StringUtils.EqualsIgnoreCase(element, ContentRulesLowerChannelName))
                {
                    var nodeInfo = await _channelRepository.GetChannelAsync(channelId);
                    if (nodeInfo != null)
                    {
                        value = nodeInfo.ChannelName.ToLower();
                    }
                }
                else if (StringUtils.EqualsIgnoreCase(element, ContentRulesChannelIndex))
                {
                    var nodeInfo = await _channelRepository.GetChannelAsync(channelId);
                    if (nodeInfo != null)
                    {
                        value = nodeInfo.IndexName;
                    }
                }
                else if (StringUtils.EqualsIgnoreCase(element, ContentRulesLowerChannelIndex))
                {
                    var nodeInfo = await _channelRepository.GetChannelAsync(channelId);
                    if (nodeInfo != null)
                    {
                        value = nodeInfo.IndexName.ToLower();
                    }
                }
                else if (StringUtils.EqualsIgnoreCase(element, ContentRulesYear) || StringUtils.EqualsIgnoreCase(element, ContentRulesMonth) || StringUtils.EqualsIgnoreCase(element, ContentRulesDay) || StringUtils.EqualsIgnoreCase(element, ContentRulesHour) || StringUtils.EqualsIgnoreCase(element, ContentRulesMinute) || StringUtils.EqualsIgnoreCase(element, ContentRulesSecond))
                {
                    if (StringUtils.EqualsIgnoreCase(element, ContentRulesYear))
                    {
                        if (addDate.HasValue)
                        {
                            value = addDate.Value.Year.ToString();
                        }
                    }
                    else if (StringUtils.EqualsIgnoreCase(element, ContentRulesMonth))
                    {
                        if (addDate.HasValue)
                        {
                            value = addDate.Value.Month.ToString("D2");
                        }

                        //value = addDate.ToString("MM");
                    }
                    else if (StringUtils.EqualsIgnoreCase(element, ContentRulesDay))
                    {
                        if (addDate.HasValue)
                        {
                            value = addDate.Value.Day.ToString("D2");
                        }

                        //value = addDate.ToString("dd");
                    }
                    else if (StringUtils.EqualsIgnoreCase(element, ContentRulesHour))
                    {
                        if (addDate.HasValue)
                        {
                            value = addDate.Value.Hour.ToString();
                        }
                    }
                    else if (StringUtils.EqualsIgnoreCase(element, ContentRulesMinute))
                    {
                        if (addDate.HasValue)
                        {
                            value = addDate.Value.Minute.ToString();
                        }
                    }
                    else if (StringUtils.EqualsIgnoreCase(element, ContentRulesSecond))
                    {
                        if (addDate.HasValue)
                        {
                            value = addDate.Value.Second.ToString();
                        }
                    }
                }
                else
                {
                    var attributeName = element.Replace("{@", string.Empty).Replace("}", string.Empty);

                    var isLower = false;
                    if (StringUtils.StartsWithIgnoreCase(attributeName, "lower"))
                    {
                        isLower = true;
                        attributeName = attributeName.Substring(5);
                    }

                    value = contentInfo.Get<string>(attributeName);
                    if (isLower)
                    {
                        value = value.ToLower();
                    }
                }

                value = StringUtils.HtmlDecode(value);

                filePath = filePath.Replace(element, value);
            }

            if (filePath.Contains("//"))
            {
                filePath = Regex.Replace(filePath, @"(/)\1{2,}", "/");
                filePath = filePath.Replace("//", "/");
            }

            if (filePath.Contains("("))
            {
                regex = @"(?<element>\([^\)]+\))";
                elements = RegexUtils.GetContents("element", regex, filePath);
                foreach (var element in elements)
                {
                    if (!element.Contains("|")) continue;

                    var value = element.Replace("(", string.Empty).Replace(")", string.Empty);
                    var value1 = value.Split('|')[0];
                    var value2 = value.Split('|')[1];
                    value = value1 + value2;

                    if (!string.IsNullOrEmpty(value1) && !string.IsNullOrEmpty(value1))
                    {
                        value = value1;
                    }

                    filePath = filePath.Replace(element, value);
                }
            }
            return filePath;
        }
    }
}