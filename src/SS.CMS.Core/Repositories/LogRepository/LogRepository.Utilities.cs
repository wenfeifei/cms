﻿using System;
using System.Threading.Tasks;
using SS.CMS.Models;
using SS.CMS.Utils;

namespace SS.CMS.Core.Repositories
{
    public partial class LogRepository
    {
        public async Task AddAdminLogAsync(string ipAddress, int userId, string action, string summary = "")
        {
            // if (!ConfigManager.Instance.IsLogAdmin) return;

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

                var logInfo = new Log
                {
                    UserId = userId,
                    IpAddress = ipAddress,
                    Action = action,
                    Summary = summary
                };

                await InsertAsync(logInfo);
            }
            catch (Exception ex)
            {
                await _errorLogRepository.AddErrorLogAsync(ex);
            }
        }
    }
}
