﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using SSCMS.Enums;
using SSCMS.Repositories;
using SSCMS.Services;
using SSCMS.Utils;

namespace SSCMS.Web.Controllers.Admin.Cms.Channels
{
    [OpenApiIgnore]
    [Authorize(Roles = AuthTypes.Roles.Administrator)]
    [Route(Constants.ApiAdminPrefix)]
    public partial class ChannelsLayerCreateController : ControllerBase
    {
        private const string Route = "cms/channels/channelsLayerCreate";

        private readonly IAuthManager _authManager;
        private readonly ICreateManager _createManager;
        private readonly ISiteRepository _siteRepository;
        private readonly IChannelRepository _channelRepository;

        public ChannelsLayerCreateController(IAuthManager authManager, ICreateManager createManager, ISiteRepository siteRepository, IChannelRepository channelRepository)
        {
            _authManager = authManager;
            _createManager = createManager;
            _siteRepository = siteRepository;
            _channelRepository = channelRepository;
        }

        [HttpPost, Route(Route)]
        public async Task<ActionResult<List<int>>> Create([FromBody] CreateRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, AuthTypes.SitePermissions.Channels))
            {
                return Unauthorized();
            }

            var site = await _siteRepository.GetAsync(request.SiteId);
            if (site == null) return NotFound();

            var expendedChannelIds = new List<int>
            {
                request.SiteId
            };

            foreach (var channelId in request.ChannelIds)
            {
                var channel = await _channelRepository.GetAsync(channelId);
                if (!expendedChannelIds.Contains(channel.ParentId))
                {
                    expendedChannelIds.Add(channel.ParentId);
                }

                await _createManager.CreateChannelAsync(request.SiteId, channelId);
                if (request.IsCreateContents)
                {
                    await _createManager.CreateAllContentAsync(request.SiteId, channelId);
                }
                if (request.IsIncludeChildren)
                {
                    var channelIds = await _channelRepository.GetChannelIdsAsync(request.SiteId, channelId, ScopeType.Descendant);

                    foreach (var childChannelId in channelIds)
                    {
                        await _createManager.CreateChannelAsync(request.SiteId, childChannelId);
                        if (request.IsCreateContents)
                        {
                            await _createManager.CreateAllContentAsync(request.SiteId, channelId);
                        }
                    }
                }
            }

            return expendedChannelIds;
        }
    }
}
