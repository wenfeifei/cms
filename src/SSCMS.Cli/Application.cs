﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Mono.Options;
using Quartz;
using Quartz.Impl;
using SSCMS.Cli.Core;
using SSCMS.Services;
using SSCMS.Utils;

namespace SSCMS.Cli
{
    public class Application
    {
        private bool _isHelp;
        private string _repeat;
        private readonly OptionSet _options;
        public static string CommandName { get; private set; }
        public static string[] CommandArgs { get; private set; }

        public static string[] CommandExtras { get; private set; }

        private readonly ISettingsManager _settingsManager;

        public Application(ISettingsManager settingsManager)
        {
            _settingsManager = settingsManager;

            _options = new OptionSet {
                { "r|repeat=", "schedule CRON expression",
                    v => _repeat = v },
                { "h|help",  "命令说明",
                    v => _isHelp = v != null }
            };
        }

        public async Task RunAsync(string[] args)
        {
            if (!CliUtils.ParseArgs(_options, args)) return;

            var jobServiceCommandNames = CliUtils.GetJobServiceCommandNames();
            var isJobService = false;

            var commandName = string.Empty;
            var commandArgs = new List<string>();
            var commandExtras = new List<string>();
            if (args.Length >= 1)
            {
                var isCommand = true;
                foreach (var arg in args)
                {
                    if (isCommand && !StringUtils.StartsWith(arg, "-"))
                    {
                        if (isJobService)
                        {
                            commandExtras.Add(StringUtils.Trim(arg));
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(commandName))
                            {
                                commandName += " " + StringUtils.Trim(arg);
                            }
                            else
                            {
                                commandName = StringUtils.Trim(arg);
                            }

                            isJobService = StringUtils.ContainsIgnoreCase(jobServiceCommandNames, commandName);
                        }
                    }
                    else
                    {
                        isCommand = false;
                        commandArgs.Add(StringUtils.Trim(arg));
                    }
                }
            }
            CommandName = commandName;
            CommandArgs = commandArgs.ToArray();
            CommandExtras = commandExtras.ToArray();

            //Console.WriteLine("欢迎使用 SiteServer Cli 命令行工具");
            //Console.WriteLine();

            // PluginManager.LoadPlugins(CliUtils.PhysicalApplicationPath);
            // var pluginJobs = PluginJobManager.GetJobs();
            // if (pluginJobs != null && pluginJobs.Count > 0)
            // {
            //     foreach (var command in pluginJobs.Keys)
            //     {
            //         if (!Jobs.ContainsKey(command))
            //         {
            //             Jobs.Add(command, pluginJobs[command]);
            //         }
            //     }
            // }

            if (!isJobService)
            {
                await RunHelpAsync(CommandName);
            }
            else if (!string.IsNullOrEmpty(_repeat))
            {
                await RunRepeatAsync();
            }
            else
            {
                await RunExecuteAsync(CommandName, CommandArgs, CommandExtras, null);
            }
        }

        private async Task RunHelpAsync(string commandName)
        {
            if (_isHelp || string.IsNullOrEmpty(commandName))
            {
                await WriteUtils.PrintInfoAsync(_settingsManager);
                await Console.Out.WriteLineAsync();

                var services = CliUtils.GetJobServices();
                foreach (var service in services)
                {
                    await WriteUtils.PrintRowLine();
                    await WriteUtils.PrintRow(service.CommandName);
                    await WriteUtils.PrintRowLine();

                    service.PrintUsage();
                }
            }
            else
            {
                Console.WriteLine($"'{commandName}' is not a sscms-cli command. See 'sscms-cli --help'");
            }
        }

        private async Task RunRepeatAsync()
        {
            try
            {
                var factory = new StdSchedulerFactory(new NameValueCollection
                {
                    { "quartz.serializer.type", "binary" }
                });
                var scheduler = await factory.GetScheduler();

                await scheduler.Start();

                var job = JobBuilder.Create<SchedulerJob>()
                    .WithIdentity("job1", "group1")
                    .Build();

                var trigger = TriggerBuilder.Create()
                    .WithIdentity("trigger1", "group1")
                    .StartNow()
                    .WithCronSchedule(_repeat)
                    .WithPriority(1)
                    .Build();

                await scheduler.ScheduleJob(job, trigger);
                await Task.Delay(-1);
                await scheduler.Shutdown();
            }
            catch (Exception ex)
            {
                await WriteUtils.PrintErrorAsync(ex.Message);
            }
        }

        public async Task RunExecuteAsync(string commandName, string[] commandArgs, string[] commandExtras, IJobExecutionContext jobContext)
        {
            try
            {
                var service = CliUtils.GetJobService(commandName);
                if (service != null)
                {
                    var context = new JobContext(commandName, commandArgs, commandExtras, jobContext);
                    await service.ExecuteAsync(context);
                }
            }
            catch (Exception ex)
            {
                await WriteUtils.PrintErrorAsync(ex.Message);

                //var errorLogFilePath = CliUtils.CreateErrorLogFile("siteserver", _settingsManager);

                //await CliUtils.AppendErrorLogsAsync(errorLogFilePath, new List<TextLogInfo>
                //{
                //    new TextLogInfo
                //    {
                //        DateTime = DateTime.Now,
                //        Detail = "Console Error",
                //        Exception = ex
                //    }
                //});
            }
        }
    }
}