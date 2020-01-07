﻿using System.Text;
using System.Threading.Tasks;
using SS.CMS.Core.Api.Sys.Stl;
using SS.CMS.Core.Common;
using SS.CMS.Core.Models.Attributes;
using SS.CMS.Core.StlParser.Models;
using SS.CMS.Core.StlParser.Utility;
using SS.CMS.Utils;

namespace SS.CMS.Core.StlParser.StlElement
{
    [StlElement(Title = "搜索", Description = "通过 stl:search 标签在模板中显示搜索结果")]
    public class StlSearch
    {
        private StlSearch() { }
        public const string ElementName = "stl:search";
        public const string ElementName2 = "stl:searchOutput";

        [StlAttribute(Title = "是否对全部站点进行搜索")]
        public const string IsAllSites = nameof(IsAllSites);

        [StlAttribute(Title = "站点名称")]
        public const string SiteName = nameof(SiteName);

        [StlAttribute(Title = "站点文件夹")]
        public const string SiteDir = nameof(SiteDir);

        [StlAttribute(Title = "站点Id列表")]
        public const string SiteIds = nameof(SiteIds);

        [StlAttribute(Title = "栏目索引")]
        public const string ChannelIndex = nameof(ChannelIndex);

        [StlAttribute(Title = "栏目名称")]
        public const string ChannelName = nameof(ChannelName);

        [StlAttribute(Title = "栏目Id列表")]
        public const string ChannelIds = nameof(ChannelIds);

        [StlAttribute(Title = "搜索类型")]
        public const string Type = nameof(Type);

        [StlAttribute(Title = "搜索关键词")]
        public const string Word = nameof(Word);

        [StlAttribute(Title = "搜索时间字段")]
        public const string DateAttribute = nameof(DateAttribute);

        [StlAttribute(Title = "搜索开始时间")]
        public const string DateFrom = nameof(DateFrom);

        [StlAttribute(Title = "搜索结束时间")]
        public const string DateTo = nameof(DateTo);

        [StlAttribute(Title = "搜索时间段")]
        public const string Since = nameof(Since);

        [StlAttribute(Title = "每页显示的内容数目")]
        public const string PageNum = nameof(PageNum);

        [StlAttribute(Title = "是否关键字高亮")]
        public const string IsHighlight = nameof(IsHighlight);

        public static async Task<object> ParseAsync(ParseContext parseContext)
        {
            var isAllSites = false;
            var siteName = string.Empty;
            var siteDir = string.Empty;
            var siteIds = string.Empty;
            var channelIndex = string.Empty;
            var channelName = string.Empty;
            var channelIds = string.Empty;
            var type = ContentAttribute.Title;
            var word = string.Empty;
            var dateAttribute = ContentAttribute.AddDate;
            var dateFrom = string.Empty;
            var dateTo = string.Empty;
            var since = string.Empty;
            var pageNum = 0;
            var isHighlight = false;

            foreach (var name in parseContext.Attributes.AllKeys)
            {
                var value = parseContext.Attributes[name];

                if (StringUtils.EqualsIgnoreCase(name, IsAllSites))
                {
                    isAllSites = TranslateUtils.ToBool(value);
                }
                else if (StringUtils.EqualsIgnoreCase(name, SiteName))
                {
                    siteName = value;
                }
                else if (StringUtils.EqualsIgnoreCase(name, SiteDir))
                {
                    siteDir = value;
                }
                else if (StringUtils.EqualsIgnoreCase(name, SiteIds))
                {
                    siteIds = value;
                }
                else if (StringUtils.EqualsIgnoreCase(name, ChannelIndex))
                {
                    channelIndex = value;
                }
                else if (StringUtils.EqualsIgnoreCase(name, ChannelName))
                {
                    channelName = value;
                }
                else if (StringUtils.EqualsIgnoreCase(name, ChannelIds))
                {
                    channelIds = value;
                }
                else if (StringUtils.EqualsIgnoreCase(name, Type))
                {
                    type = value;
                }
                else if (StringUtils.EqualsIgnoreCase(name, Word))
                {
                    word = value;
                }
                else if (StringUtils.EqualsIgnoreCase(name, DateAttribute))
                {
                    dateAttribute = value;
                }
                else if (StringUtils.EqualsIgnoreCase(name, DateFrom))
                {
                    dateFrom = value;
                }
                else if (StringUtils.EqualsIgnoreCase(name, DateTo))
                {
                    dateTo = value;
                }
                else if (StringUtils.EqualsIgnoreCase(name, Since))
                {
                    since = value;
                }
                else if (StringUtils.EqualsIgnoreCase(name, PageNum))
                {
                    pageNum = TranslateUtils.ToInt(value);
                }
                else if (StringUtils.EqualsIgnoreCase(name, IsHighlight))
                {
                    isHighlight = TranslateUtils.ToBool(value);
                }
            }

            string loading;
            string yes;
            string no;
            StlParserUtility.GetLoadingYesNo(parseContext.InnerHtml, out loading, out yes, out no);

            if (string.IsNullOrEmpty(loading))
            {
                loading = await parseContext.TemplateRepository.GetContentByFilePathAsync(SiteFilesAssets.Search.LoadingTemplatePath);
            }
            if (string.IsNullOrEmpty(yes))
            {
                yes = await parseContext.TemplateRepository.GetContentByFilePathAsync(SiteFilesAssets.Search.YesTemplatePath);
            }
            if (string.IsNullOrEmpty(no))
            {
                no = await parseContext.TemplateRepository.GetContentByFilePathAsync(SiteFilesAssets.Search.NoTemplatePath);
            }

            parseContext.PageInfo.AddPageBodyCodeIfNotExists(parseContext.UrlManager, PageInfo.Const.Jquery);

            var ajaxDivId = StlParserUtility.GetAjaxDivId(parseContext.UniqueId);
            var apiUrl = ApiRouteActionsSearch.GetUrl();
            var apiParameters = ApiRouteActionsSearch.GetParameters(parseContext.SettingsManager, isAllSites, siteName, siteDir, siteIds, channelIndex, channelName, channelIds, type, word, dateAttribute, dateFrom, dateTo, since, pageNum, isHighlight, parseContext.SiteId, ajaxDivId, yes);

            var builder = new StringBuilder();
            builder.Append($@"
<div id=""{ajaxDivId}"">
    <div class=""stl_loading"">{loading}</div>
    <div class=""stl_yes"" style=""display:none""></div>
    <div class=""stl_no"" style=""display:none"">{no}</div>
</div>
");

            builder.Append($@"
<script type=""text/javascript"" language=""javascript"">
jQuery(document).ready(function(){{
    var url = '{apiUrl}';
    var parameters = {apiParameters};

    var queryString = document.location.search;
    if (queryString && queryString.length > 1) {{
        queryString = queryString.substring(1);
        var arr = queryString.split('&');
        for(var i=0; i < arr.length; i++) {{
            var item = arr[i];
            var arr2 = item.split('=');
            if (arr2 && arr2.length == 2) {{
                var key = (arr2[0] || '').toLowerCase();
                if (key) {{
                    parameters[key] = decodeURIComponent(arr2[1]);
                }}
            }}
        }}
        if (!parameters['page']) {{
            parameters['page'] = 1;
        }}

        jQuery.support.cors = true;
        jQuery.ajax({{
            url: url,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(parameters),
            dataType: 'json',
            success: function(res) {{
                jQuery(""#{ajaxDivId} .stl_loading"").hide();
                jQuery(""#{ajaxDivId} .stl_yes"").show();
                jQuery(""#{ajaxDivId} .stl_no"").hide();
                jQuery(""#{ajaxDivId} .stl_yes"").html(res);
            }},
            error: function(e) {{
                jQuery(""#{ajaxDivId} .stl_loading"").hide();
                jQuery(""#{ajaxDivId} .stl_yes"").hide();
                jQuery(""#{ajaxDivId} .stl_no"").show();
            }}
        }});
    }} else {{
        jQuery(""#{ajaxDivId} .stl_loading"").hide();
        jQuery(""#{ajaxDivId} .stl_yes"").hide();
        jQuery(""#{ajaxDivId} .stl_no"").hide();
    }}
}});

function stlRedirect{ajaxDivId}(page)
{{
    var queryString = document.location.search;
    if (queryString && queryString.length > 1) {{
        queryString = queryString.substring(1);
        var parameters = '';
        var arr = queryString.split('&');
        for(var i=0; i < arr.length; i++) {{
            var item = arr[i];
            var arr2 = item.split('=');
            if (arr2 && arr2.length == 2) {{
                if (arr2[0] !== 'page') {{
                    parameters += item + '&';
                }}
            }}
        }}
        parameters += 'page=' + page;
        location.href = location.protocol + '//' + location.host + location.pathname + location.hash + '?' + parameters;
    }}
}}

function stlJump{ajaxDivId}(selObj)
{{
    stlRedirect{ajaxDivId}(selObj.options[selObj.selectedIndex].value);
}}
</script>
");

            return builder.ToString();
        }
    }
}
