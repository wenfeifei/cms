﻿using System.Collections.Generic;
using System.Threading.Tasks;
using SS.CMS.Core.StlParser.Models;
using SS.CMS.Core.StlParser.Utility;
using SS.CMS.Utils;

namespace SS.CMS.Core.StlParser.StlEntity
{
    [StlElement(Title = "导航实体", Description = "通过 {navigation.}  实体在模板中显示导航链接")]
    public class StlNavigationEntities
    {
        private StlNavigationEntities()
        {
        }

        public const string EntityName = "navigation";

        public static string PreviousChannel = "PreviousChannel";
        public static string NextChannel = "NextChannel";
        public static string PreviousContent = "PreviousContent";
        public static string NextContent = "NextContent";

        public static SortedList<string, string> AttributeList => new SortedList<string, string>
        {
            {PreviousChannel, "上一栏目链接"},
            {NextChannel, "下一栏目链接"},
            {PreviousContent, "上一内容链接"},
            {NextContent, "下一内容链接"}
        };

        internal static async Task<string> ParseAsync(string stlEntity, ParseContext parseContext)
        {
            var parsedContent = string.Empty;
            try
            {
                var entityName = StlParserUtility.GetNameFromEntity(stlEntity);
                var attributeName = entityName.Substring(12, entityName.Length - 13);

                var nodeInfo = await parseContext.ChannelRepository.GetChannelAsync(parseContext.ChannelId);

                if (StringUtils.EqualsIgnoreCase(PreviousChannel, attributeName) || StringUtils.EqualsIgnoreCase(NextChannel, attributeName))
                {
                    var taxis = nodeInfo.Taxis;
                    var isNextChannel = !StringUtils.EqualsIgnoreCase(attributeName, PreviousChannel);
                    //var siblingChannelId = DataProvider.ChannelDao.GetIdByParentIdAndTaxis(nodeInfo.ParentId, taxis, isNextChannel);
                    var siblingChannelId = await parseContext.ChannelRepository.GetIdByParentIdAndTaxisAsync(nodeInfo.ParentId, taxis, isNextChannel);
                    if (siblingChannelId != 0)
                    {
                        var siblingNodeInfo = await parseContext.ChannelRepository.GetChannelAsync(siblingChannelId);
                        parsedContent = await parseContext.UrlManager.GetChannelUrlAsync(parseContext.SiteInfo, siblingNodeInfo, parseContext.IsLocal);
                    }
                }
                else if (StringUtils.EqualsIgnoreCase(PreviousContent, attributeName) || StringUtils.EqualsIgnoreCase(NextContent, attributeName))
                {
                    if (parseContext.ContentId != 0)
                    {
                        var channelInfo = await parseContext.GetChannelAsync();
                        var contentRepository = parseContext.ChannelRepository.GetContentRepository(parseContext.SiteInfo, channelInfo);

                        var contentInfo = await parseContext.GetContentInfoAsync();
                        var taxis = contentInfo.Taxis;
                        var isNextContent = !StringUtils.EqualsIgnoreCase(attributeName, PreviousContent);
                        var siblingContentId = await contentRepository.GetContentIdAsync(channelInfo.Id, taxis, isNextContent);
                        if (siblingContentId != 0)
                        {
                            contentInfo = await contentRepository.GetContentInfoAsync(siblingContentId);
                            parsedContent = await parseContext.UrlManager.GetContentUrlAsync(parseContext.SiteInfo, contentInfo, parseContext.IsLocal);
                        }
                    }
                }
            }
            catch
            {
                // ignored
            }

            if (string.IsNullOrEmpty(parsedContent))
            {
                parsedContent = PageUtils.UnClickableUrl;
            }

            return parsedContent;
        }
    }
}
