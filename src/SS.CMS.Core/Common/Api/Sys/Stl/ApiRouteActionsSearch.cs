using System.Collections.Generic;
using SS.CMS.Core.StlParser.StlElement;
using SS.CMS.Services;
using SS.CMS.Utils;

namespace SS.CMS.Core.Api.Sys.Stl
{
    public static class ApiRouteActionsSearch
    {
        public const string Route = "sys/stl/actions/search";

        public static string GetUrl()
        {
            return PageUtils.Combine(Constants.ApiPrefix, Route);
        }

        public static string GetParameters(ISettingsManager settingsManager, bool isAllSites, string siteName, string siteDir, string siteIds, string channelIndex, string channelName, string channelIds, string type, string word, string dateAttribute, string dateFrom, string dateTo, string since, int pageNum, bool isHighlight, int siteId, string ajaxDivId, string template)
        {
            return $@"
{{
    {StlSearch.IsAllSites.ToLower()}: {isAllSites.ToString().ToLower()},
    {StlSearch.SiteName.ToLower()}: '{siteName}',
    {StlSearch.SiteDir.ToLower()}: '{siteDir}',
    {StlSearch.SiteIds.ToLower()}: '{siteIds}',
    {StlSearch.ChannelIndex.ToLower()}: '{channelIndex}',
    {StlSearch.ChannelName.ToLower()}: '{channelName}',
    {StlSearch.ChannelIds.ToLower()}: '{channelIds}',
    {StlSearch.Type.ToLower()}: '{type}',
    {StlSearch.Word.ToLower()}: '{word}',
    {StlSearch.DateAttribute.ToLower()}: '{dateAttribute}',
    {StlSearch.DateFrom.ToLower()}: '{dateFrom}',
    {StlSearch.DateTo.ToLower()}: '{dateTo}',
    {StlSearch.Since.ToLower()}: '{since}',
    {StlSearch.PageNum.ToLower()}: {pageNum},
    {StlSearch.IsHighlight.ToLower()}: {isHighlight.ToString().ToLower()},
    siteid: '{siteId}',
    ajaxdivid: '{ajaxDivId}',
    template: '{settingsManager.Encrypt(template)}',
}}";
        }

        public static List<string> ExlcudeAttributeNames => new List<string>
        {
            StlSearch.IsAllSites.ToLower(),
            StlSearch.SiteName.ToLower(),
            StlSearch.SiteDir.ToLower(),
            StlSearch.SiteIds.ToLower(),
            StlSearch.ChannelIndex.ToLower(),
            StlSearch.ChannelName.ToLower(),
            StlSearch.ChannelIds.ToLower(),
            StlSearch.Type.ToLower(),
            StlSearch.Word.ToLower(),
            StlSearch.DateAttribute.ToLower(),
            StlSearch.DateFrom.ToLower(),
            StlSearch.DateTo.ToLower(),
            StlSearch.Since.ToLower(),
            StlSearch.PageNum.ToLower(),
            StlSearch.IsHighlight.ToLower(),
            "siteid",
            "ajaxdivid",
            "template",
        };
    }
}