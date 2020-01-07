﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SS.CMS.Core.Api.Sys.Stl;
using SS.CMS.Core.Common;
using SS.CMS.Core.StlParser.Models;
using SS.CMS.Enums;
using SS.CMS.Models;
using SS.CMS.Repositories;
using SS.CMS.Utils;

namespace SS.CMS.Core.StlParser.StlElement
{
    [StlElement(Title = "树状导航", Description = "通过 stl:tree 标签在模板中显示树状导航")]
    public class StlTree
    {
        private StlTree() { }
        public const string ElementName = "stl:tree";

        [StlAttribute(Title = "栏目索引")]
        private const string ChannelIndex = nameof(ChannelIndex);

        [StlAttribute(Title = "栏目名称")]
        private const string ChannelName = nameof(ChannelName);

        [StlAttribute(Title = "上级栏目的级别")]
        private const string UpLevel = nameof(UpLevel);

        [StlAttribute(Title = "从首页向下的栏目级别")]
        private const string TopLevel = nameof(TopLevel);

        [StlAttribute(Title = "指定显示的栏目组")]
        private const string GroupChannel = nameof(GroupChannel);

        [StlAttribute(Title = "指定不显示的栏目组")]
        private const string GroupChannelNot = nameof(GroupChannelNot);

        [StlAttribute(Title = "根节点显示名称")]
        private const string Title = nameof(Title);

        [StlAttribute(Title = "是否AJAX方式即时载入")]
        private const string IsLoading = nameof(IsLoading);

        [StlAttribute(Title = "是否显示栏目内容数")]
        private const string IsShowContentNum = nameof(IsShowContentNum);

        [StlAttribute(Title = "当前项格式化字符串")]
        private const string CurrentFormatString = nameof(CurrentFormatString);

        [StlAttribute(Title = "打开窗口目标")]
        private const string Target = nameof(Target);

        public static async Task<object> ParseAsync(ParseContext parseContext)
        {
            var channelIndex = string.Empty;
            var channelName = string.Empty;
            var upLevel = 0;
            var topLevel = -1;
            var groupChannel = string.Empty;
            var groupChannelNot = string.Empty;
            var title = string.Empty;
            var isLoading = false;
            var isShowContentNum = false;
            var currentFormatString = "<strong>{0}</strong>";

            foreach (var name in parseContext.Attributes.AllKeys)
            {
                var value = parseContext.Attributes[name];

                if (StringUtils.EqualsIgnoreCase(name, ChannelIndex))
                {
                    channelIndex = await parseContext.ReplaceStlEntitiesForAttributeValueAsync(value);
                }
                else if (StringUtils.EqualsIgnoreCase(name, ChannelName))
                {
                    channelName = await parseContext.ReplaceStlEntitiesForAttributeValueAsync(value);
                }
                else if (StringUtils.EqualsIgnoreCase(name, UpLevel))
                {
                    upLevel = TranslateUtils.ToInt(value);
                }
                else if (StringUtils.EqualsIgnoreCase(name, TopLevel))
                {
                    topLevel = TranslateUtils.ToInt(value);
                }
                else if (StringUtils.EqualsIgnoreCase(name, GroupChannel))
                {
                    groupChannel = await parseContext.ReplaceStlEntitiesForAttributeValueAsync(value);
                }
                else if (StringUtils.EqualsIgnoreCase(name, GroupChannelNot))
                {
                    groupChannelNot = await parseContext.ReplaceStlEntitiesForAttributeValueAsync(value);
                }
                else if (StringUtils.EqualsIgnoreCase(name, Title))
                {
                    title = await parseContext.ReplaceStlEntitiesForAttributeValueAsync(value);
                }
                else if (StringUtils.EqualsIgnoreCase(name, IsLoading))
                {
                    isLoading = TranslateUtils.ToBool(value, false);
                }
                else if (StringUtils.EqualsIgnoreCase(name, IsShowContentNum))
                {
                    isShowContentNum = TranslateUtils.ToBool(value, false);
                }
                else if (StringUtils.EqualsIgnoreCase(name, CurrentFormatString))
                {
                    currentFormatString = value;
                    if (!StringUtils.Contains(currentFormatString, "{0}"))
                    {
                        currentFormatString += "{0}";
                    }
                }
            }

            return isLoading ? await ParseImplAjaxAsync(parseContext, channelIndex, channelName, upLevel, topLevel, groupChannel, groupChannelNot, title, isShowContentNum, currentFormatString, parseContext.IsLocal) : await ParseImplNotAjaxAsync(parseContext, channelIndex, channelName, upLevel, topLevel, groupChannel, groupChannelNot, title, isShowContentNum, currentFormatString);
        }

        private static async Task<string> ParseImplNotAjaxAsync(ParseContext parseContext, string channelIndex, string channelName, int upLevel, int topLevel, string groupChannel, string groupChannelNot, string title, bool isShowContentNum, string currentFormatString)
        {
            var channelId = await parseContext.GetChannelIdByLevelAsync(parseContext.SiteId, parseContext.ChannelId, upLevel, topLevel);

            channelId = await parseContext.GetChannelIdByChannelIdOrChannelIndexOrChannelNameAsync(parseContext.SiteId, channelId, channelIndex, channelName);

            var channel = await parseContext.ChannelRepository.GetChannelAsync(channelId);

            var target = "";

            var htmlBuilder = new StringBuilder();

            htmlBuilder.Append(@"<table border=""0"" cellpadding=""0"" cellspacing=""0"" style=""width:100%;"">");

            //var theChannelIdList = DataProvider.ChannelDao.GetIdListByScopeType(channel.ChannelId, channel.ChildrenCount, EScopeType.All, groupChannel, groupChannelNot);
            var theChannelIdList = await parseContext.ChannelRepository.GetIdListAsync(channel, ScopeType.All, groupChannel, groupChannelNot, string.Empty);
            var isLastNodeArray = new bool[theChannelIdList.Count];
            var channelIdList = new List<int>();

            var currentChannelInfo = await parseContext.ChannelRepository.GetChannelAsync(parseContext.PageChannelId);
            if (currentChannelInfo != null)
            {
                channelIdList = TranslateUtils.StringCollectionToIntList(currentChannelInfo.ParentsPath);
                channelIdList.Add(currentChannelInfo.Id);
            }

            foreach (var theChannelId in theChannelIdList)
            {
                var theChannelInfo = await parseContext.ChannelRepository.GetChannelAsync(theChannelId);
                var nodeInfo = (Channel)theChannelInfo.Clone();
                if (theChannelId == parseContext.SiteId && !string.IsNullOrEmpty(title))
                {
                    nodeInfo.ChannelName = title;
                }
                var isDisplay = channelIdList.Contains(theChannelId);
                if (!isDisplay)
                {
                    isDisplay = nodeInfo.ParentId == channelId || channelIdList.Contains(nodeInfo.ParentId);
                }

                var selected = theChannelId == channelId;
                if (!selected && channelIdList.Contains(nodeInfo.Id))
                {
                    selected = true;
                }

                var childrenIds = await parseContext.ChannelRepository.GetChildrenIdsAsync(channel.SiteId, channel.Id);
                var hasChildren = childrenIds.Count() > 0;

                var linkUrl = await parseContext.UrlManager.GetChannelUrlAsync(parseContext.SiteInfo, theChannelInfo, parseContext.IsLocal);
                var level = parseContext.ChannelRepository.GetParentsCount(theChannelInfo) - parseContext.ChannelRepository.GetParentsCount(channel);
                var item = new StlTreeItemNotAjax(parseContext, isDisplay, selected, nodeInfo, hasChildren, linkUrl, target, isShowContentNum, isLastNodeArray, currentFormatString, channelId, level);

                htmlBuilder.Append(await item.GetTrHtmlAsync());
            }

            htmlBuilder.Append("</table>");

            if (!parseContext.BodyCodes.ContainsKey(PageInfo.Const.JsAgStlTreeNotAjax))
            {
                parseContext.BodyCodes.Add(PageInfo.Const.JsAgStlTreeNotAjax, StlTreeItemNotAjax.GetNodeTreeScript(parseContext));
            }

            return htmlBuilder.ToString();
        }

        private class StlTreeItemNotAjax
        {
            private readonly string _treeDirectoryUrl;
            private readonly string _iconFolderUrl;
            private readonly string _iconEmptyUrl;
            private readonly string _iconMinusUrl;
            private readonly string _iconPlusUrl;

            private readonly bool _isDisplay;
            private readonly bool _selected;
            private readonly PageInfo _pageInfo;
            private readonly Channel _nodeInfo;
            private readonly bool _hasChildren;
            private readonly string _linkUrl;
            private readonly string _target;
            private readonly bool _isShowContentNum;
            private readonly bool[] _isLastNodeArray;
            private readonly string _currentFormatString;
            private readonly int _topChannelId;
            private readonly int _level;
            private readonly ParseContext _parseContext;

            public StlTreeItemNotAjax(ParseContext parseContext, bool isDisplay, bool selected, Channel nodeInfo, bool hasChildren, string linkUrl, string target, bool isShowContentNum, bool[] isLastNodeArray, string currentFormatString, int topChannelId, int level)
            {
                _parseContext = parseContext;
                _isDisplay = isDisplay;
                _selected = selected;
                _nodeInfo = nodeInfo;
                _hasChildren = hasChildren;
                _linkUrl = linkUrl;
                _target = target;
                _isShowContentNum = isShowContentNum;
                _isLastNodeArray = isLastNodeArray;
                _currentFormatString = currentFormatString;
                _topChannelId = topChannelId;
                _level = level;

                _treeDirectoryUrl = SiteFilesAssets.GetUrl("tree");
                _iconFolderUrl = PageUtils.Combine(_treeDirectoryUrl, "folder.gif");
                _iconEmptyUrl = PageUtils.Combine(_treeDirectoryUrl, "empty.gif");
                _iconMinusUrl = PageUtils.Combine(_treeDirectoryUrl, "minus.png");
                _iconPlusUrl = PageUtils.Combine(_treeDirectoryUrl, "plus.png");
            }

            public async Task<string> GetTrHtmlAsync()
            {
                var displayHtml = (_isDisplay) ? Constants.ShowElementStyle : Constants.HideElementStyle;
                string trElementHtml = $@"
<tr style='{displayHtml}' treeItemLevel='{_level + 1}'>
	<td nowrap>
		{await GetItemHtmlAsync()}
	</td>
</tr>
";

                return trElementHtml;
            }

            private async Task<string> GetItemHtmlAsync()
            {
                var htmlBuilder = new StringBuilder();
                for (var i = 0; i < _level; i++)
                {
                    htmlBuilder.Append($"<img align=\"absmiddle\" src=\"{_iconEmptyUrl}\"/>");
                }

                if (_isDisplay)
                {
                    if (_hasChildren)
                    {
                        htmlBuilder.Append(
                            _selected
                                ? $"<img align=\"absmiddle\" style=\"cursor:pointer;\" onClick=\"stltree_displayChildren(this);\" isOpen=\"true\" src=\"{_iconMinusUrl}\"/>"
                                : $"<img align=\"absmiddle\" style=\"cursor:pointer;\" onClick=\"stltree_displayChildren(this);\" isOpen=\"false\" src=\"{_iconPlusUrl}\"/>");
                    }
                    else
                    {
                        htmlBuilder.Append($"<img align=\"absmiddle\" src=\"{_iconEmptyUrl}\"/>");
                    }
                }
                else
                {
                    htmlBuilder.Append(
                            _hasChildren
                                ? $"<img align=\"absmiddle\" style=\"cursor:pointer;\" onClick=\"stltree_displayChildren(this);\" isOpen=\"false\" src=\"{_iconPlusUrl}\"/>"
                                : $"<img align=\"absmiddle\" src=\"{_iconEmptyUrl}\"/>");
                }

                if (!string.IsNullOrEmpty(_iconFolderUrl))
                {
                    htmlBuilder.Append($"<img align=\"absmiddle\" src=\"{_iconFolderUrl}\"/>");
                }

                htmlBuilder.Append("&nbsp;");

                var nodeName = _nodeInfo.ChannelName;
                if ((_pageInfo.TemplateInfo.Type == TemplateType.ChannelTemplate || _pageInfo.TemplateInfo.Type == TemplateType.ContentTemplate) && _pageInfo.PageChannelId == _nodeInfo.Id)
                {
                    nodeName = string.Format(_currentFormatString, nodeName);
                }

                if (!string.IsNullOrEmpty(_linkUrl))
                {
                    var targetHtml = (string.IsNullOrEmpty(_target)) ? string.Empty : $"target='{_target}'";
                    var clickChangeHtml = "onclick='stltree_openFolderByA(this);'";

                    htmlBuilder.Append(
                        $"<a href='{_linkUrl}' {targetHtml} {clickChangeHtml} isTreeLink='true'>{nodeName}</a>");
                }
                else
                {
                    htmlBuilder.Append(nodeName);
                }

                if (_isShowContentNum)
                {
                    var contentRepository = _parseContext.ChannelRepository.GetContentRepository(_parseContext.SiteInfo, _nodeInfo);

                    var count = await contentRepository.GetCountAsync(_pageInfo.SiteInfo, _nodeInfo, true);
                    htmlBuilder.Append("&nbsp;");
                    htmlBuilder.Append($"<span style=\"font-size:8pt;font-family:arial\">({count})</span>");
                }

                return htmlBuilder.ToString();
            }

            public static string GetNodeTreeScript(ParseContext parseContext)
            {
                var script = @"
<script language=""JavaScript"">
function stltree_isNull(obj){
	if (typeof(obj) == 'undefined')
	  return true;
	  
	if (obj == undefined)
	  return true;
	  
	if (obj == null)
	  return true;
	 
	return false;
}

//取得Tree的级别，1为第一级
function stltree_getTreeLevel(e) {
	var length = 0;
	if (!stltree_isNull(e)){
		if (e.tagName == 'TR') {
			length = parseInt(e.getAttribute('treeItemLevel'));
		}
	}
	return length;
}

function stltree_closeAllFolder(){
	if (stltree_isNodeTree){
		var imgCol = document.getElementsByTagName('IMG');
		if (!stltree_isNull(imgCol)){
			for (x=0;x<imgCol.length;x++){
				if (!stltree_isNull(imgCol.item(x).getAttribute('src'))){
					if (imgCol.item(x).getAttribute('src') == '{iconOpenedFolderUrl}'){
						imgCol.item(x).setAttribute('src', '{iconFolderUrl}');
					}
				}
			}
		}
	}

	var aCol = document.getElementsByTagName('A');
	if (!stltree_isNull(aCol)){
		for (x=0;x<aCol.length;x++){
			if (aCol.item(x).getAttribute('isTreeLink') == 'true'){
				aCol.item(x).style.fontWeight = 'normal';
			}
		}
	}
}

function stltree_openFolderByA(element){
	stltree_closeAllFolder();
	if (stltree_isNull(element) || element.tagName != 'A') return;

	element.style.fontWeight = 'bold';

	if (stltree_isNodeTree){
		for (element = element.previousSibling;;){
			if (element != null && element.tagName == 'A'){
				element = element.firstChild;
			}
			if (element != null && element.tagName == 'IMG'){
				if (element.getAttribute('src') == '{iconFolderUrl}') break;
				break;
			}else{
				element = element.previousSibling;
			} 
		}
		if (!stltree_isNull(element)){
			element.setAttribute('src', '{iconOpenedFolderUrl}');
		}
	}
}

function stltree_getTrElement(element){
	if (stltree_isNull(element)) return;
	for (element = element.parentNode;;){
		if (element != null && element.tagName == 'TR'){
			break;
		}else{
			element = element.parentNode;
		} 
	}
	return element;
}

function stltree_getImgClickableElementByTr(element){
	if (stltree_isNull(element) || element.tagName != 'TR') return;
	var img = null;
	if (!stltree_isNull(element.childNodes)){
		var imgCol = element.getElementsByTagName('IMG');
		if (!stltree_isNull(imgCol)){
			for (x=0;x<imgCol.length;x++){
				if (!stltree_isNull(imgCol.item(x).getAttribute('isOpen'))){
					img = imgCol.item(x);
					break;
				}
			}
		}
	}
	return img;
}

//显示、隐藏下级目录
function stltree_displayChildren(element){
	if (stltree_isNull(element)) return;

	var tr = stltree_getTrElement(element);

	var img = stltree_getImgClickableElementByTr(tr);//需要变换的加减图标

	if (!stltree_isNull(img) && img.getAttribute('isOpen') != null){
		if (img.getAttribute('isOpen') == 'false'){
			img.setAttribute('isOpen', 'true');
            var iconMinusUrl = img.getAttribute('src').replace('plus','minus');
            img.setAttribute('src', iconMinusUrl);
		}else{
            img.setAttribute('isOpen', 'false');
            var iconPlusUrl = img.getAttribute('src').replace('minus','plus');
            img.setAttribute('src', iconPlusUrl);
		}
	}

	var level = stltree_getTreeLevel(tr);//点击项菜单的级别
	
	var collection = new Array();
	var index = 0;

	for ( var e = tr.nextSibling; !stltree_isNull(e) ; e = e.nextSibling) {
		if (!stltree_isNull(e) && !stltree_isNull(e.tagName) && e.tagName == 'TR'){
		    var currentLevel = stltree_getTreeLevel(e);
		    if (currentLevel <= level) break;
		    if(e.style.display == '') {
			    e.style.display = 'none';
		    }else{//展开
			    if (currentLevel != level + 1) continue;
			    e.style.display = '';
			    var imgClickable = stltree_getImgClickableElementByTr(e);
			    if (!stltree_isNull(imgClickable)){
				    if (!stltree_isNull(imgClickable.getAttribute('isOpen')) && imgClickable.getAttribute('isOpen') =='true'){
					    imgClickable.setAttribute('isOpen', 'false');
                        var iconPlusUrl = imgClickable.getAttribute('src').replace('minus','plus');
                        imgClickable.setAttribute('src', iconPlusUrl);
					    collection[index] = imgClickable;
					    index++;
				    }
			    }
		    }
        }
	}
	
	if (index > 0){
		for (i=0;i<=index;i++){
			stltree_displayChildren(collection[i]);
		}
	}
}
var stltree_isNodeTree = {isNodeTree};
</script>
";
                var treeDirectoryUrl = SiteFilesAssets.GetUrl("tree");
                var iconFolderUrl = PageUtils.Combine(treeDirectoryUrl, "folder.gif");
                var iconOpenedFolderUrl = PageUtils.Combine(treeDirectoryUrl, "openedfolder.gif");
                var iconMinusUrl = PageUtils.Combine(treeDirectoryUrl, "minus.png");
                var iconPlusUrl = PageUtils.Combine(treeDirectoryUrl, "plus.png");

                script = script.Replace("{iconFolderUrl}", iconFolderUrl);
                script = script.Replace("{iconMinusUrl}", iconMinusUrl);
                script = script.Replace("{iconOpenedFolderUrl}", iconOpenedFolderUrl);
                script = script.Replace("{iconPlusUrl}", iconPlusUrl);
                script = script.Replace("{isNodeTree}", "true");
                return script;
            }

        }

        private static async Task<string> ParseImplAjaxAsync(ParseContext parseContext, string channelIndex, string channelName, int upLevel, int topLevel, string groupChannel, string groupChannelNot, string title, bool isShowContentNum, string currentFormatString, bool isLocal)
        {
            parseContext.PageInfo.AddPageBodyCodeIfNotExists(parseContext.UrlManager, PageInfo.Const.Jquery);

            var channelId = await parseContext.GetChannelIdByLevelAsync(parseContext.SiteId, parseContext.ChannelId, upLevel, topLevel);

            channelId = await parseContext.GetChannelIdByChannelIdOrChannelIndexOrChannelNameAsync(parseContext.SiteId, channelId, channelIndex, channelName);

            var channel = await parseContext.ChannelRepository.GetChannelAsync(channelId);

            var target = "";

            var htmlBuilder = new StringBuilder();

            htmlBuilder.Append(@"<table border=""0"" cellpadding=""0"" cellspacing=""0"" style=""width:100%;"">");

            //var theChannelIdList = DataProvider.ChannelDao.GetIdListByScopeType(channel.ChannelId, channel.ChildrenCount, EScopeType.SelfAndChildren, groupChannel, groupChannelNot);
            var theChannelIdList = await parseContext.ChannelRepository.GetIdListAsync(channel, ScopeType.SelfAndChildren, groupChannel, groupChannelNot, string.Empty);

            foreach (var theChannelId in theChannelIdList)
            {
                var theChannelInfo = await parseContext.ChannelRepository.GetChannelAsync(theChannelId);
                var nodeInfo = (Channel)theChannelInfo.Clone();
                if (theChannelId == parseContext.SiteId && !string.IsNullOrEmpty(title))
                {
                    nodeInfo.ChannelName = title;
                }

                var rowHtml = await GetChannelRowHtmlAsync(parseContext, parseContext.SiteInfo, nodeInfo, target, isShowContentNum, currentFormatString, channelId, parseContext.ChannelRepository.GetParentsCount(channel), parseContext.PageChannelId, isLocal);

                htmlBuilder.Append(rowHtml);
            }

            htmlBuilder.Append("</table>");

            if (!parseContext.BodyCodes.ContainsKey(PageInfo.Const.JsAgStlTreeAjax))
            {
                parseContext.BodyCodes.Add(PageInfo.Const.JsAgStlTreeAjax, await StlTreeItemAjax.GetScriptAsync(parseContext, target, isShowContentNum, currentFormatString, channelId, parseContext.ChannelRepository.GetParentsCount(channel), parseContext.PageChannelId));
            }

            return htmlBuilder.ToString();
        }

        public static async Task<string> GetChannelRowHtmlAsync(ParseContext parseContext, Site siteInfo, Channel nodeInfo, string target, bool isShowContentNum, string currentFormatString, int topChannelId, int topParantsCount, int currentChannelId, bool isLocal)
        {
            var nodeTreeItem = new StlTreeItemAjax();
            await nodeTreeItem.LoadAsync(parseContext, siteInfo, nodeInfo, target, isShowContentNum, currentFormatString,
                topChannelId, topParantsCount, currentChannelId, isLocal);
            var title = await nodeTreeItem.GetItemHtmlAsync();

            string rowHtml = $@"
<tr treeItemLevel=""{parseContext.ChannelRepository.GetParentsCount(nodeInfo) + 1}"">
	<td nowrap>
		{title}
	</td>
</tr>
";

            return rowHtml;
        }

        private class StlTreeItemAjax
        {
            private string _iconFolderUrl;
            private string _iconEmptyUrl;
            private string _iconMinusUrl;
            private string _iconPlusUrl;

            private Site _siteInfo;
            private Channel _nodeInfo;
            private bool _hasChildren;
            private string _linkUrl;
            private string _target;
            private bool _isShowContentNum;
            private string _currentFormatString;
            private int _topChannelId;
            private int _level;
            private int _currentChannelId;
            private ParseContext _parseContext;

            public async Task LoadAsync(ParseContext parseContext, Site siteInfo, Channel nodeInfo, string target, bool isShowContentNum, string currentFormatString, int topChannelId, int topParentsCount, int currentChannelId, bool isLocal)
            {
                _parseContext = parseContext;
                _siteInfo = siteInfo;
                _nodeInfo = nodeInfo;
                var childrenIds = await parseContext.ChannelRepository.GetChildrenIdsAsync(_nodeInfo.SiteId, _nodeInfo.Id);
                _hasChildren = childrenIds.Count() > 0;
                _linkUrl = await parseContext.UrlManager.GetChannelUrlAsync(siteInfo, nodeInfo, isLocal);
                _target = target;
                _isShowContentNum = isShowContentNum;
                _currentFormatString = currentFormatString;
                _topChannelId = topChannelId;
                _level = parseContext.ChannelRepository.GetParentsCount(nodeInfo) - topParentsCount;
                _currentChannelId = currentChannelId;

                var treeDirectoryUrl = SiteFilesAssets.GetUrl("tree");
                _iconFolderUrl = PageUtils.Combine(treeDirectoryUrl, "folder.gif");
                _iconEmptyUrl = PageUtils.Combine(treeDirectoryUrl, "empty.gif");
                _iconMinusUrl = PageUtils.Combine(treeDirectoryUrl, "minus.png");
                _iconPlusUrl = PageUtils.Combine(treeDirectoryUrl, "plus.png");
            }

            public async Task<string> GetItemHtmlAsync()
            {
                var htmlBuilder = new StringBuilder();

                for (var i = 0; i < _level; i++)
                {
                    htmlBuilder.Append($"<img align=\"absmiddle\" src=\"{_iconEmptyUrl}\"/>");
                }

                if (_hasChildren)
                {
                    htmlBuilder.Append(
                        _topChannelId != _nodeInfo.Id
                            ? $"<img align=\"absmiddle\" style=\"cursor:pointer;\" onClick=\"stltree_displayChildren(this);\" isAjax=\"true\" isOpen=\"false\" id=\"{_nodeInfo.Id}\" src=\"{_iconPlusUrl}\"/>"
                            : $"<img align=\"absmiddle\" style=\"cursor:pointer;\" onClick=\"stltree_displayChildren(this);\" isAjax=\"false\" isOpen=\"true\" id=\"{_nodeInfo.Id}\" src=\"{_iconMinusUrl}\"/>");
                }
                else
                {
                    htmlBuilder.Append($"<img align=\"absmiddle\" src=\"{_iconEmptyUrl}\"/>");
                }

                if (!string.IsNullOrEmpty(_iconFolderUrl))
                {
                    htmlBuilder.Append($"<img align=\"absmiddle\" src=\"{_iconFolderUrl}\"/>");
                }

                htmlBuilder.Append("&nbsp;");

                var nodeName = _nodeInfo.ChannelName;
                if (_currentChannelId == _nodeInfo.Id)
                {
                    nodeName = string.Format(_currentFormatString, nodeName);
                }

                if (!string.IsNullOrEmpty(_linkUrl))
                {
                    var targetHtml = (string.IsNullOrEmpty(_target)) ? string.Empty : $"target='{_target}'";

                    htmlBuilder.Append($"<a href='{_linkUrl}' {targetHtml} isTreeLink='true'>{nodeName}</a>");
                }
                else
                {
                    htmlBuilder.Append(nodeName);
                }

                if (_isShowContentNum)
                {
                    var contentRepository = _parseContext.ChannelRepository.GetContentRepository(_parseContext.SiteInfo, _nodeInfo);

                    var count = await contentRepository.GetCountAsync(_siteInfo, _nodeInfo, true);
                    htmlBuilder.Append("&nbsp;");
                    htmlBuilder.Append($"<span style=\"font-size:8pt;font-family:arial\">({count})</span>");
                }

                return htmlBuilder.ToString();
            }

            public static async Task<string> GetScriptAsync(ParseContext parseContext, string target, bool isShowContentNum, string currentFormatString, int topChannelId, int topParentsCount, int currentChannelId)
            {
                var script = @"
<script language=""JavaScript"">
function stltree_isNull(obj){
	if (typeof(obj) == 'undefined')
	  return true;
	  
	if (obj == undefined)
	  return true;
	  
	if (obj == null)
	  return true;
	 
	return false;
}

function stltree_getTreeLevel(e) {
	var length = 0;
	if (!stltree_isNull(e)){
		if (e.tagName == 'TR') {
			length = parseInt(e.getAttribute('treeItemLevel'));
		}
	}
	return length;
}

function stltree_getTrElement(element){
	if (stltree_isNull(element)) return;
	for (element = element.parentNode;;){
		if (element != null && element.tagName == 'TR'){
			break;
		}else{
			element = element.parentNode;
		} 
	}
	return element;
}

function stltree_getImgClickableElementByTr(element){
	if (stltree_isNull(element) || element.tagName != 'TR') return;
	var img = null;
	if (!stltree_isNull(element.childNodes)){
		var imgCol = element.getElementsByTagName('IMG');
		if (!stltree_isNull(imgCol)){
			for (x=0;x<imgCol.length;x++){
				if (!stltree_isNull(imgCol.item(x).getAttribute('isOpen'))){
					img = imgCol.item(x);
					break;
				}
			}
		}
	}
	return img;
}

var weightedLink = null;

function fontWeightLink(element){
    if (weightedLink != null)
    {
        weightedLink.style.fontWeight = 'normal';
    }
    element.style.fontWeight = 'bold';
    weightedLink = element;
}

var completedChannelId = null;
function stltree_displayChildren(img){
	if (stltree_isNull(img)) return;

	var tr = stltree_getTrElement(img);

    var isToOpen = img.getAttribute('isOpen') == 'false';
    var isByAjax = img.getAttribute('isAjax') == 'true';
    var channelId = img.getAttribute('id');

	if (!stltree_isNull(img) && img.getAttribute('isOpen') != null){
		if (img.getAttribute('isOpen') == 'false'){
			img.setAttribute('isOpen', 'true');
            img.setAttribute('src', '{iconMinusUrl}');
		}else{
            img.setAttribute('isOpen', 'false');
            img.setAttribute('src', '{iconPlusUrl}');
		}
	}

    if (isToOpen && isByAjax)
    {
        var div = document.createElement('div');
        div.innerHTML = ""<img align='absmiddle' border='0' src='{iconLoadingUrl}' /> 加载中，请稍候..."";
        img.parentNode.appendChild(div);
        //Element.addClassName(div, 'loading');
        loadingChannels(tr, img, div, channelId);
    }
    else
    {
        var level = stltree_getTreeLevel(tr);
    	
	    var collection = new Array();
	    var index = 0;

	    for ( var e = tr.nextSibling; !stltree_isNull(e) ; e = e.nextSibling) {
		    if (!stltree_isNull(e) && !stltree_isNull(e.tagName) && e.tagName == 'TR'){
		        var currentLevel = stltree_getTreeLevel(e);
		        if (currentLevel <= level) break;
		        if(e.style.display == '') {
			        e.style.display = 'none';
		        }else{
			        if (currentLevel != level + 1) continue;
			        e.style.display = '';
			        var imgClickable = stltree_getImgClickableElementByTr(e);
			        if (!stltree_isNull(imgClickable)){
				        if (!stltree_isNull(imgClickable.getAttribute('isOpen')) && imgClickable.getAttribute('isOpen') =='true'){
					        imgClickable.setAttribute('isOpen', 'false');
                            imgClickable.setAttribute('src', '{iconPlusUrl}');
					        collection[index] = imgClickable;
					        index++;
				        }
			        }
		        }
            }
	    }
    	
	    if (index > 0){
		    for (i=0;i<=index;i++){
			    stltree_displayChildren(collection[i]);
		    }
	    }
    }
}
";
                var loadingUrl = ApiRouteActionsLoadingChannels.GetUrl();
                var formatString = parseContext.SettingsManager.Encrypt(currentFormatString);

                script += $@"
function loadingChannels(tr, img, div, channelId){{
    var url = '{loadingUrl}';
    var pars = 'siteID={parseContext.SiteId}&parentID=' + channelId + '&target={target}&isShowContentNum={isShowContentNum}&currentFormatString={formatString}&topChannelId={topChannelId}&topParentsCount={topParentsCount}&currentChannelId={currentChannelId}';

    //jQuery.post(url, pars, function(data, textStatus){{
        //$($.parseHTML(data)).insertAfter($(tr));
        //img.setAttribute('isAjax', 'false');
        //img.parentNode.removeChild(div);
        //completedChannelId = channelId;
    //}});
    $.ajax({{
                url: url,
                type: ""POST"",
                mimeType:""multipart/form-data"",
                contentType: false,
                processData: false,
                cache: false,
                xhrFields: {{   
                    withCredentials: true   
                }},
                data: pars,
                success: function(json, textStatus){{
                    $($.parseHTML(data)).insertAfter($(tr));
                    img.setAttribute('isAjax', 'false');
                    img.parentNode.removeChild(div);
                    completedChannelId = channelId;
                }}
    }});
}}

function loadingChannelsOnLoad(path){{
    if (path && path.length > 0){{
        var channelIds = path.split(',');
        var channelId = channelIds[0];
        var img = $(channelId);
        if (!img) return;
        if (img.getAttribute('isOpen') == 'false'){{
            stltree_displayChildren(img);
            new PeriodicalExecuter(function(pe){{
                if (completedChannelId && completedChannelId == channelId){{
                    if (path.indexOf(',') != -1){{
                        var thePath = path.substring(path.indexOf(',') + 1);
                        loadingChannelsOnLoad(thePath);
                    }}
                    pe.stop();
                }} 
            }}, 1);
        }}
    }}
}}
</script>
";

                script += await GetScriptOnLoadAsync(parseContext.ChannelRepository, parseContext.SiteId, topChannelId, parseContext.PageChannelId);

                var treeDirectoryUrl = SiteFilesAssets.GetUrl("tree");
                var iconFolderUrl = PageUtils.Combine(treeDirectoryUrl, "folder.gif");
                var iconOpenedFolderUrl = PageUtils.Combine(treeDirectoryUrl, "openedfolder.gif");
                var iconMinusUrl = PageUtils.Combine(treeDirectoryUrl, "minus.png");
                var iconPlusUrl = PageUtils.Combine(treeDirectoryUrl, "plus.png");

                script = script.Replace("{iconFolderUrl}", iconFolderUrl);
                script = script.Replace("{iconMinusUrl}", iconMinusUrl);
                script = script.Replace("{iconOpenedFolderUrl}", iconOpenedFolderUrl);
                script = script.Replace("{iconPlusUrl}", iconPlusUrl);
                script = script.Replace("{isNodeTree}", "true");

                script = script.Replace("{iconLoadingUrl}", SiteFilesAssets.GetUrl("loading.gif"));

                return script;
            }

            private static async Task<string> GetScriptOnLoadAsync(IChannelRepository channelRepository, int siteId, int topChannelId, int currentChannelId)
            {
                if (currentChannelId == 0 || currentChannelId == siteId || currentChannelId == topChannelId)
                    return string.Empty;
                var nodeInfo = await channelRepository.GetChannelAsync(currentChannelId);
                if (nodeInfo != null)
                {
                    string path;
                    if (nodeInfo.ParentId == siteId)
                    {
                        path = currentChannelId.ToString();
                    }
                    else
                    {
                        path = nodeInfo.ParentsPath.Substring(nodeInfo.ParentsPath.IndexOf(",", StringComparison.Ordinal) + 1) + "," + currentChannelId;
                    }
                    return $@"
<script language=""JavaScript"">
Event.observe(window,'load', function(){{
    loadingChannelsOnLoad('{path}');
}});
</script>
";
                }
                return string.Empty;
            }
        }
    }
}
