﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SS.CMS.Core.Models;
using SS.CMS.Core.Models.Attributes;
using SS.CMS.Enums;
using SS.CMS.Models;
using SS.CMS.Repositories;
using SS.CMS.Utils.Atom.Atom.AdditionalElements;
using SS.CMS.Utils.Atom.Atom.Core;

namespace SS.CMS.Core.Serialization.Components
{
    internal class ChannelIe
    {
        private readonly Site _siteInfo;
        private readonly ITemplateRepository _templateRepository;

        public ChannelIe(Site siteInfo)
        {
            _siteInfo = siteInfo;
        }

        //保存除内容表本身字段外的属性
        private const string ChannelTemplateName = "ChannelTemplateName";
        private const string ContentTemplateName = "ContentTemplateName";

        public async Task ImportChannelAsync(Channel channelInfo, ScopedElementCollection additionalElements, int parentId, IEnumerable<string> indexNames)
        {
            var indexNameList = new List<string>();
            if (indexNames != null)
            {
                indexNameList.AddRange(indexNames);
            }
            channelInfo.ChannelName = AtomUtility.GetDcElementContent(additionalElements, new List<string> { ChannelAttribute.ChannelName, "NodeName" });
            channelInfo.SiteId = _siteInfo.Id;
            var contentModelPluginId = AtomUtility.GetDcElementContent(additionalElements, ChannelAttribute.ContentModelPluginId);
            if (!string.IsNullOrEmpty(contentModelPluginId))
            {
                channelInfo.ContentModelPluginId = contentModelPluginId;
            }
            var contentRelatedPluginIds = AtomUtility.GetDcElementContent(additionalElements, ChannelAttribute.ContentRelatedPluginIds);
            if (!string.IsNullOrEmpty(contentRelatedPluginIds))
            {
                channelInfo.ContentRelatedPluginIds = contentRelatedPluginIds;
            }
            channelInfo.ParentId = parentId;
            var indexName = AtomUtility.GetDcElementContent(additionalElements, new List<string> { ChannelAttribute.IndexName, "NodeIndexName" });
            if (!string.IsNullOrEmpty(indexName) && indexNameList.IndexOf(indexName) == -1)
            {
                channelInfo.IndexName = indexName;
                indexNameList.Add(indexName);
            }
            channelInfo.GroupNameCollection = AtomUtility.GetDcElementContent(additionalElements, new List<string> { ChannelAttribute.GroupNameCollection, "NodeGroupNameCollection" });
            channelInfo.ImageUrl = AtomUtility.GetDcElementContent(additionalElements, ChannelAttribute.ImageUrl);
            channelInfo.Content = AtomUtility.Decrypt(AtomUtility.GetDcElementContent(additionalElements, ChannelAttribute.Content));
            channelInfo.FilePath = AtomUtility.GetDcElementContent(additionalElements, ChannelAttribute.FilePath);
            channelInfo.ChannelFilePathRule = AtomUtility.GetDcElementContent(additionalElements, ChannelAttribute.ChannelFilePathRule);
            channelInfo.ContentFilePathRule = AtomUtility.GetDcElementContent(additionalElements, ChannelAttribute.ContentFilePathRule);

            channelInfo.LinkUrl = AtomUtility.GetDcElementContent(additionalElements, ChannelAttribute.LinkUrl);
            channelInfo.LinkType = AtomUtility.GetDcElementContent(additionalElements, ChannelAttribute.LinkType);

            var channelTemplateName = AtomUtility.GetDcElementContent(additionalElements, ChannelTemplateName);
            if (!string.IsNullOrEmpty(channelTemplateName))
            {
                channelInfo.ChannelTemplateId = await _templateRepository.GetTemplateIdByTemplateNameAsync(_siteInfo.Id, TemplateType.ChannelTemplate, channelTemplateName);
            }
            var contentTemplateName = AtomUtility.GetDcElementContent(additionalElements, ContentTemplateName);
            if (!string.IsNullOrEmpty(contentTemplateName))
            {
                channelInfo.ContentTemplateId = await _templateRepository.GetTemplateIdByTemplateNameAsync(_siteInfo.Id, TemplateType.ContentTemplate, contentTemplateName);
            }

            channelInfo.Keywords = AtomUtility.GetDcElementContent(additionalElements, ChannelAttribute.Keywords);
            channelInfo.Description = AtomUtility.GetDcElementContent(additionalElements, ChannelAttribute.Description);

            channelInfo.ExtendValues = AtomUtility.GetDcElementContent(additionalElements, ChannelAttribute.ExtendValues);
        }

        public async Task<AtomFeed> ExportChannelAsync(Channel channelInfo)
        {
            var feed = AtomUtility.GetEmptyFeed();

            AtomUtility.AddDcElement(feed.AdditionalElements, new List<string> { ChannelAttribute.Id, "NodeId" }, channelInfo.Id.ToString());
            AtomUtility.AddDcElement(feed.AdditionalElements, new List<string> { ChannelAttribute.ChannelName, "NodeName" }, channelInfo.ChannelName);
            AtomUtility.AddDcElement(feed.AdditionalElements, new List<string> { ChannelAttribute.SiteId, "PublishmentSystemId" }, channelInfo.SiteId.ToString());
            AtomUtility.AddDcElement(feed.AdditionalElements, ChannelAttribute.ContentModelPluginId, channelInfo.ContentModelPluginId);
            AtomUtility.AddDcElement(feed.AdditionalElements, ChannelAttribute.ContentRelatedPluginIds, channelInfo.ContentRelatedPluginIds);
            AtomUtility.AddDcElement(feed.AdditionalElements, ChannelAttribute.ParentId, channelInfo.ParentId.ToString());
            AtomUtility.AddDcElement(feed.AdditionalElements, ChannelAttribute.ParentsPath, channelInfo.ParentsPath);
            AtomUtility.AddDcElement(feed.AdditionalElements, new List<string> { ChannelAttribute.IndexName, "NodeIndexName" }, channelInfo.IndexName);
            AtomUtility.AddDcElement(feed.AdditionalElements, new List<string> { ChannelAttribute.GroupNameCollection, "NodeGroupNameCollection" }, channelInfo.GroupNameCollection);
            AtomUtility.AddDcElement(feed.AdditionalElements, ChannelAttribute.Taxis, channelInfo.Taxis.ToString());
            AtomUtility.AddDcElement(feed.AdditionalElements, ChannelAttribute.ImageUrl, channelInfo.ImageUrl);
            AtomUtility.AddDcElement(feed.AdditionalElements, ChannelAttribute.Content, AtomUtility.Encrypt(channelInfo.Content));
            AtomUtility.AddDcElement(feed.AdditionalElements, ChannelAttribute.FilePath, channelInfo.FilePath);
            AtomUtility.AddDcElement(feed.AdditionalElements, ChannelAttribute.ChannelFilePathRule, channelInfo.ChannelFilePathRule);
            AtomUtility.AddDcElement(feed.AdditionalElements, ChannelAttribute.ContentFilePathRule, channelInfo.ContentFilePathRule);
            AtomUtility.AddDcElement(feed.AdditionalElements, ChannelAttribute.LinkUrl, channelInfo.LinkUrl);
            AtomUtility.AddDcElement(feed.AdditionalElements, ChannelAttribute.LinkType, channelInfo.LinkType);
            AtomUtility.AddDcElement(feed.AdditionalElements, ChannelAttribute.ChannelTemplateId, channelInfo.ChannelTemplateId.ToString());
            AtomUtility.AddDcElement(feed.AdditionalElements, ChannelAttribute.ContentTemplateId, channelInfo.ContentTemplateId.ToString());
            AtomUtility.AddDcElement(feed.AdditionalElements, ChannelAttribute.Keywords, channelInfo.Keywords);
            AtomUtility.AddDcElement(feed.AdditionalElements, ChannelAttribute.Description, channelInfo.Description);
            AtomUtility.AddDcElement(feed.AdditionalElements, ChannelAttribute.ExtendValues, channelInfo.ExtendValues);

            if (channelInfo.ChannelTemplateId != 0)
            {
                var channelTemplateName = await _templateRepository.GetTemplateNameAsync(channelInfo.ChannelTemplateId);
                AtomUtility.AddDcElement(feed.AdditionalElements, ChannelTemplateName, channelTemplateName);
            }

            if (channelInfo.ContentTemplateId != 0)
            {
                var contentTemplateName = await _templateRepository.GetTemplateNameAsync(channelInfo.ContentTemplateId);
                AtomUtility.AddDcElement(feed.AdditionalElements, ContentTemplateName, contentTemplateName);
            }

            return feed;
        }
    }
}
