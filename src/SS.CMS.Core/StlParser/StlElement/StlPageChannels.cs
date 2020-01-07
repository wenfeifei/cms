﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SS.CMS.Core.StlParser.Models;
using SS.CMS.Core.StlParser.Utility;
using SS.CMS.Enums;
using SS.CMS.Models;
using SS.CMS.Utils;

namespace SS.CMS.Core.StlParser.StlElement
{
    [StlElement(Title = "翻页栏目列表", Description = "通过 stl:pageChannels 标签在模板中显示翻页栏目列表")]
    public class StlPageChannels : StlChannels
    {
        public new const string ElementName = "stl:pageChannels";

        [StlAttribute(Title = "每页显示的栏目数目")]
        private const string PageNum = nameof(PageNum);

        private string _stlPageChannelsElement;
        private ParseContext _parseContext;
        private ListInfo _listInfo;
        private IList<KeyValuePair<int, Channel>> _channelList;


        public async Task LoadAsync(string stlPageChannelsElement, ParseContext parseContext)
        {
            _stlPageChannelsElement = stlPageChannelsElement;
            _stlPageChannelsElement = stlPageChannelsElement;
            var stlElementInfo = StlParserUtility.ParseStlElement(stlPageChannelsElement);

            _parseContext = parseContext.Clone(stlPageChannelsElement, stlElementInfo.InnerHtml, stlElementInfo.Attributes);
            _parseContext.ContextType = EContextType.Channel;
            _listInfo = await ListInfo.GetListInfoAsync(_parseContext);

            var channelId = await parseContext.GetChannelIdByLevelAsync(_parseContext.SiteId, _parseContext.ChannelId, _listInfo.UpLevel, _listInfo.TopLevel);

            channelId = await parseContext.GetChannelIdByChannelIdOrChannelIndexOrChannelNameAsync(_parseContext.SiteId, channelId, _listInfo.ChannelIndex, _listInfo.ChannelName);

            var isTotal = TranslateUtils.ToBool(_listInfo.Others.Get(IsTotal));

            if (TranslateUtils.ToBool(_listInfo.Others.Get(IsAllChildren)))
            {
                _listInfo.Scope = ScopeType.Descendant;
            }

            var taxisType = parseContext.GetChannelTaxisType(_listInfo.Order, TaxisType.OrderByTaxis);

            _channelList = (await _parseContext.ChannelRepository.GetContainerChannelListAsync(_parseContext.SiteId, channelId, _listInfo.GroupChannel, _listInfo.GroupChannelNot, _listInfo.IsImage, _listInfo.StartNum, _listInfo.TotalNum, taxisType, _listInfo.Scope, isTotal)).ToList();
        }

        public int GetPageCount(out int totalNum)
        {
            var pageCount = 1;
            totalNum = 0;//数据库中实际的内容数目
            if (_channelList == null || _channelList.Count == 0) return pageCount;

            totalNum = _channelList.Count;
            if (_listInfo.PageNum != 0 && _listInfo.PageNum < totalNum)//需要翻页
            {
                pageCount = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(totalNum) / Convert.ToDouble(_listInfo.PageNum)));//需要生成的总页数
            }
            return pageCount;
        }

        public async Task<string> ParseAsync(int currentPageIndex, int pageCount)
        {
            var parsedContent = string.Empty;

            _parseContext.PageItemIndex = currentPageIndex * _listInfo.PageNum;

            try
            {
                if (_channelList != null && _channelList.Count > 0)
                {
                    IList<KeyValuePair<int, Channel>> pageChannelList;

                    if (pageCount > 1)
                    {
                        pageChannelList = _channelList.Skip(_parseContext.PageItemIndex).Take(_listInfo.PageNum).ToList();
                    }
                    else
                    {
                        pageChannelList = _channelList;
                    }

                    parsedContent = await StlChannels.ParseElementAsync(_parseContext, _listInfo, pageChannelList);
                }
            }
            catch (Exception ex)
            {
                parsedContent = await _parseContext.GetErrorMessageAsync(ElementName, _stlPageChannelsElement, ex);
            }

            //还原翻页为0，使得其他列表能够正确解析ItemIndex
            _parseContext.PageItemIndex = 0;

            return parsedContent;
        }
    }

}
