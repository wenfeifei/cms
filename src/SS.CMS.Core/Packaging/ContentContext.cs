﻿namespace SS.CMS.Core.Plugin
{
    public class ContentContext: IContentContext
    {
        public int SiteId  { get; set; }

        public int ChannelId  { get; set; }

        public int ContentId { get; set; }
    }
}
