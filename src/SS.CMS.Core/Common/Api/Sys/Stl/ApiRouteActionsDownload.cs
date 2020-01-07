﻿using System.Collections.Specialized;
using SS.CMS.Services;
using SS.CMS.Utils;

namespace SS.CMS.Core.Api.Sys.Stl
{
    public class ApiRouteActionsDownload
    {
        public const string Route = "sys/stl/actions/download";

        public static string GetUrl(int siteId, int channelId, int contentId)
        {
            return PageUtils.AddQueryString(PageUtils.Combine(Constants.ApiPrefix, Route), new NameValueCollection
            {
                {"siteId", siteId.ToString()},
                {"channelId", channelId.ToString()},
                {"contentId", contentId.ToString()}
            });
        }

        public static string GetUrl(ISettingsManager settingsManager, int siteId, int channelId, int contentId, string fileUrl)
        {
            return PageUtils.AddQueryString(PageUtils.Combine(Constants.ApiPrefix, Route), new NameValueCollection
            {
                {"siteId", siteId.ToString()},
                {"channelId", channelId.ToString()},
                {"contentId", contentId.ToString()},
                {"fileUrl", settingsManager.Encrypt(fileUrl)}
            });
        }

        public static string GetUrl(ISettingsManager settingsManager, int siteId, string fileUrl)
        {
            return PageUtils.AddQueryString(PageUtils.Combine(Constants.ApiPrefix, Route), new NameValueCollection
            {
                {"siteId", siteId.ToString()},
                {"fileUrl", settingsManager.Encrypt(fileUrl)}
            });
        }

        public static string GetUrl(ISettingsManager settingsManager, string filePath)
        {
            return PageUtils.AddQueryString(PageUtils.Combine(Constants.ApiPrefix, Route), new NameValueCollection
            {
                {"filePath", settingsManager.Encrypt(filePath)}
            });
        }
    }
}