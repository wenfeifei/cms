﻿using System.Collections.Generic;
using SSCMS.Dto;

namespace SSCMS.Web.Controllers.Home.Write
{
    public partial class ContentsLayerCheckController
    {
        public class GetRequest : ChannelRequest
        {
            public List<int> ContentIds { get; set; }
        }

        public class GetResult
        {
            public List<IDictionary<string, object>> Value { get; set; }
            public List<KeyValuePair<int, string>> CheckedLevels { get; set; }
            public int CheckedLevel { get; set; }
            public List<KeyValuePair<int, string>> AllChannels { get; set; }
        }

        public class SubmitRequest : ChannelRequest
        {
            public List<int> ContentIds { get; set; }
            public int CheckedLevel { get; set; }
            public bool IsTranslate { get; set; }
            public int TranslateChannelId { get; set; }
            public string Reasons { get; set; }
        }
    }
}
