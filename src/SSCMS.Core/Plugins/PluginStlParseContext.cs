﻿using System.Collections.Generic;
using System.Collections.Specialized;
using SSCMS.Enums;
using SSCMS.Parse;

namespace SSCMS.Core.Plugins
{
    public class PluginStlParseContext : IStlParseContext
    {
        public void Load(string stlOuterHtml, string stlInnerHtml, NameValueCollection stlAttributes, ParsePage pageInfo, ParseContext contextInfo)
        {
            SiteId = contextInfo.Site.Id;
            ChannelId = contextInfo.ChannelId;
            ContentId = contextInfo.ContentId;
            TemplateType = pageInfo.Template.TemplateType;
            TemplateId = pageInfo.Template.Id;

            HeadCodes = pageInfo.HeadCodes;
            BodyCodes = pageInfo.BodyCodes;
            FootCodes = pageInfo.FootCodes;
            StlOuterHtml = stlOuterHtml;
            StlInnerHtml = stlInnerHtml;
            StlAttributes = stlAttributes;
            
            IsStlElement = !contextInfo.IsStlEntity;
            PluginItems = pageInfo.PluginItems;
        }

        public Dictionary<string, object> PluginItems { get; private set; }

        public void Set<T>(string key, T objectValue)
        {
            if (PluginItems != null && !string.IsNullOrEmpty(key))
            {
                PluginItems[key] = objectValue;
            }
        }

        public T Get<T>(string key)
        {
            object objectValue;
            if (PluginItems.TryGetValue(key, out objectValue))
            {
                if (objectValue is T)
                {
                    return (T) objectValue;
                }
            }

            return default(T);
        }

        public int SiteId  { get; private set; }

        public int ChannelId { get; private set; }

        public int ContentId { get; private set; }

        public TemplateType TemplateType { get; private set; }

        public string FilePath { get; private set; }

        public int TemplateId { get; private set; }

        public SortedDictionary<string, string> HeadCodes { get; private set; }

        public SortedDictionary<string, string> BodyCodes { get; private set; }

        public SortedDictionary<string, string> FootCodes { get; private set; }

        public string StlOuterHtml { get; private set; }

        public string StlInnerHtml { get; private set; }

        public NameValueCollection StlAttributes { get; private set; }

        public bool IsStlElement { get; private set; }
    }
}
