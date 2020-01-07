﻿using System;
using System.Threading.Tasks;
using SS.CMS.Models;
using SS.CMS.Utils;

namespace SS.CMS.Core.Repositories
{
    public partial class SiteLogRepository
    {
        public async Task AddSiteLogAsync(int siteId, int channelId, int contentId, string ipAddress, int userId, string action, string summary)
        {
            // if (!ConfigManager.Instance.IsLogSite) return;

            if (siteId <= 0)
            {
                await _logRepository.AddAdminLogAsync(ipAddress, userId, action, summary);
            }
            else
            {
                try
                {
                    await DeleteIfThresholdAsync();

                    if (!string.IsNullOrEmpty(action))
                    {
                        action = StringUtils.MaxLengthText(action, 250);
                    }
                    if (!string.IsNullOrEmpty(summary))
                    {
                        summary = StringUtils.MaxLengthText(summary, 250);
                    }
                    if (channelId < 0)
                    {
                        channelId = -channelId;
                    }

                    var siteLogInfo = new SiteLog
                    {
                        SiteId = siteId,
                        ChannelId = channelId,
                        ContentId = contentId,
                        UserId = userId,
                        IpAddress = ipAddress,
                        Action = action,
                        Summary = summary
                    };
                    await InsertAsync(siteLogInfo);
                }
                catch (Exception ex)
                {
                    await _errorLogRepository.AddErrorLogAsync(ex);
                }
            }
        }
    }
}
