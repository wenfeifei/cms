﻿using System;
using System.Threading.Tasks;
using Quartz;

namespace SS.CMS.Cli.Core
{
    internal class SchedulerJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            await Application.RunExecuteAsync(Application.CommandName, Application.CommandArgs, context);

            if (context.NextFireTimeUtc != null)
            {
                await Console.Out.WriteLineAsync();
                await CliUtils.PrintRowLineAsync();
                await CliUtils.PrintRowAsync("Fire Time", "Next Fire Time");
                await CliUtils.PrintRowLineAsync();
                await CliUtils.PrintRowAsync($"{context.FireTimeUtc.ToLocalTime():yyyy-MM-dd HH:mm:ss}", $"{context.NextFireTimeUtc.Value.ToLocalTime():yyyy-MM-dd HH:mm:ss}");
                await CliUtils.PrintRowLineAsync();
                await Console.Out.WriteLineAsync();
            }
        }
    }
}
