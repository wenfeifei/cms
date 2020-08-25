﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using SSCMS.Configuration;
using SSCMS.Dto;
using SSCMS.Core.Utils;
using SSCMS.Enums;
using SSCMS.Extensions;
using SSCMS.Repositories;
using SSCMS.Services;
using SSCMS.Utils;

namespace SSCMS.Web.Controllers.Admin.Cms.Settings
{
    [OpenApiIgnore]
    [Authorize(Roles = Types.Roles.Administrator)]
    [Route(Constants.ApiAdminPrefix)]
    public partial class SettingsCrossSiteTransChannelsController : ControllerBase
    {
        private const string Route = "cms/settings/settingsCrossSiteTransChannels";
        private const string RouteOptions = "cms/settings/settingsCrossSiteTransChannels/actions/options";

        private readonly IAuthManager _authManager;
        private readonly IDatabaseManager _databaseManager;
        private readonly ISiteRepository _siteRepository;
        private readonly IChannelRepository _channelRepository;
        private readonly IContentRepository _contentRepository;

        public SettingsCrossSiteTransChannelsController(IAuthManager authManager, IDatabaseManager databaseManager, ISiteRepository siteRepository, IChannelRepository channelRepository, IContentRepository contentRepository)
        {
            _authManager = authManager;
            _databaseManager = databaseManager;
            _siteRepository = siteRepository;
            _channelRepository = channelRepository;
            _contentRepository = contentRepository;
        }

        [HttpGet, Route(Route)]
        public async Task<ActionResult<GetResult>> List([FromQuery] SiteRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId,
                    Types.SitePermissions.SettingsCrossSiteTransChannels))
            {
                return Unauthorized();
            }

            var site = await _siteRepository.GetAsync(request.SiteId);
            if (site == null) return this.Error("无法确定内容对应的站点");

            var crossSiteTransManager = new CrossSiteTransManager(_databaseManager);

            var channel = await _channelRepository.GetAsync(request.SiteId);
            var cascade = await _channelRepository.GetCascadeAsync(site, channel, async summary =>
            {
                var count = await _contentRepository.GetCountAsync(site, summary);
                var entity = await _channelRepository.GetAsync(summary.Id);
                var contribute = await crossSiteTransManager.GetDescriptionAsync(request.SiteId, entity);

                return new
                {
                    entity.IndexName,
                    Count = count,
                    Contribute = contribute,
                    entity.TransType,
                    entity.TransSiteId,
                };
            });

            IEnumerable<Select<string>> transTypes;
            if (site.ParentId == 0)
            {
                transTypes = ListUtils.GetEnums<TransType>()
                    .Where(x => x != TransType.ParentSite && x != TransType.AllParentSite)
                    .Select(transType => new Select<string>(transType));
            }
            else
            {
                transTypes = ListUtils.GetEnums<TransType>()
                    .Select(transType => new Select<string>(transType));
            }
            var transDoneTypes = ListUtils.GetEnums<TranslateContentType>().Select(transType => new Select<string>(transType));

            return new GetResult
            {
                Channels = cascade,
                TransTypes = transTypes,
                TransDoneTypes = transDoneTypes
            };
        }

        [HttpPost, Route(RouteOptions)]
        public async Task<ActionResult<GetOptionsResult>> GetOptions([FromBody]GetOptionsRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId,
                    Types.SitePermissions.SettingsCrossSiteTransChannels))
            {
                return Unauthorized();
            }

            var site = await _siteRepository.GetAsync(request.SiteId);
            if (site == null) return this.Error("无法确定内容对应的站点");

            var channel = await _channelRepository.GetAsync(request.ChannelId);

            var result = new GetOptionsResult
            {
                ChannelName = channel.ChannelName,
                TransType = request.TransType,
                IsTransSiteId = false,
                TransSiteId = request.TransSiteId,
                IsTransChannelIds = false,
                TransChannelIds = ListUtils.GetIntList(channel.TransChannelIds),
                IsTransChannelNames = false,
                TransChannelNames = channel.TransChannelNames,
                IsTransIsAutomatic = false,
                TransIsAutomatic = channel.TransIsAutomatic,
                TransDoneType = channel.TransDoneType,
                TransSites = new List<Select<int>>(),
                TransChannels = new Cascade<int>()
            };

            if (request.TransType == TransType.None)
            {
                result.IsTransSiteId = false;
                result.IsTransChannelIds = false;
                result.IsTransChannelNames = false;
                result.IsTransIsAutomatic = false;
            }
            else if (request.TransType == TransType.SelfSite)
            {
                result.IsTransSiteId = false;
                result.TransSiteId = request.SiteId;
                result.IsTransChannelIds = true;
                result.IsTransChannelNames = false;
                result.IsTransIsAutomatic = true;
            }
            else if (request.TransType == TransType.SpecifiedSite)
            {
                result.IsTransSiteId = true;
                if (result.TransSiteId == 0)
                {
                    result.TransSiteId = request.SiteId;
                }
                
                var siteIdList = await _siteRepository.GetSiteIdsAsync();
                foreach (var siteId in siteIdList)
                {
                    var info = await _siteRepository.GetAsync(siteId);
                    var show = false;
                    if (request.TransType == TransType.SpecifiedSite)
                    {
                        show = true;
                    }
                    else if (request.TransType == TransType.SelfSite)
                    {
                        if (siteId == request.SiteId)
                        {
                            show = true;
                        }
                    }
                    else if (request.TransType == TransType.ParentSite)
                    {
                        if (info.Id == site.ParentId || (site.ParentId == 0 && info.Root))
                        {
                            show = true;
                        }
                    }
                    if (!show) continue;

                    result.TransSites.Add(new Select<int>(info.Id, info.SiteName));
                }
                result.IsTransChannelIds = true;
                result.IsTransChannelNames = false;
                result.IsTransIsAutomatic = true;
            }
            else if (request.TransType == TransType.ParentSite)
            {
                result.IsTransSiteId = false;
                result.IsTransChannelIds = true;
                result.IsTransChannelNames = false;
                result.IsTransIsAutomatic = true;
            }
            else if (request.TransType == TransType.AllParentSite || request.TransType == TransType.AllSite)
            {
                result.IsTransSiteId = false;
                result.IsTransChannelIds = false;
                result.IsTransChannelNames = true;
                result.IsTransIsAutomatic = true;
            }

            if (result.IsTransChannelIds && result.TransSiteId > 0)
            {
                var transChannels = await _channelRepository.GetAsync(result.TransSiteId);
                var transSite = await _siteRepository.GetAsync(result.TransSiteId);
                var cascade = await _channelRepository.GetCascadeAsync(transSite, transChannels, async summary =>
                {
                    var count = await _contentRepository.GetCountAsync(site, summary);

                    return new
                    {
                        summary.IndexName,
                        Count = count
                    };
                });
                result.TransChannels = cascade;
            }

            return result;
        }

        [HttpPut, Route(Route)]
        public async Task<ActionResult<BoolResult>> Submit([FromBody] SubmitRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId,
                    Types.SitePermissions.SettingsCrossSiteTransChannels))
            {
                return Unauthorized();
            }

            var site = await _siteRepository.GetAsync(request.SiteId);
            if (site == null) return this.Error("无法确定内容对应的站点");

            var channel = await _channelRepository.GetAsync(request.ChannelId);

            channel.TransType = request.TransType;
            channel.TransSiteId = request.TransType == TransType.SpecifiedSite ? request.TransSiteId : 0;
            channel.TransChannelIds = ListUtils.ToString(request.TransChannelIds);
            channel.TransChannelNames = request.TransChannelNames;
            channel.TransIsAutomatic = request.TransIsAutomatic;
            channel.TransDoneType = request.TransDoneType;

            await _channelRepository.UpdateAsync(channel);

            await _authManager.AddSiteLogAsync(request.SiteId, "修改跨站转发设置");

            return new BoolResult
            {
                Value = true
            };
        }
    }
}