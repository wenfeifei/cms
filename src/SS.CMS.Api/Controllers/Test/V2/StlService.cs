﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SS.CMS.Core.Cache;
using SS.CMS.Core.Models;
using SS.CMS.Core.Repositories;
using SS.CMS.Core.StlParser.Models;
using SS.CMS.Core.StlParser.Parsers;
using SS.CMS.Plugin;
using SS.CMS.Utils;

namespace SS.CMS.Core.Services.V2
{
    public class StlService : ServiceBase
    {
        private const string Route = "stl/{elementName}";

        private readonly IAccessTokenRepository _accessTokenRepository;

        public StlService(IAccessTokenRepository accessTokenRepository)
        {
            _accessTokenRepository = accessTokenRepository;
        }

        public async Task<ResponseResult<object>> Get(IRequest request, IResponse response)
        {
            var elementName = TranslateUtils.Get<string>(request.RouteValues, "elementName");

            var stlRequest = new StlRequest();
            await stlRequest.RunAsync(request, _accessTokenRepository);

            if (!stlRequest.IsApiAuthorized)
            {
                return Unauthorized();
            }

            var siteInfo = stlRequest.SiteInfo;

            if (siteInfo == null)
            {
                return NotFound();
            }

            elementName = $"stl:{elementName.ToLower()}";

            object value = null;

            if (StlElementParser.ElementsToParseDic.ContainsKey(elementName))
            {
                Func<PageInfo, ContextInfo, object> func;
                if (StlElementParser.ElementsToParseDic.TryGetValue(elementName, out func))
                {
                    var obj = func(stlRequest.PageInfo, stlRequest.ContextInfo);

                    if (obj is string)
                    {
                        value = (string)obj;
                    }
                    else
                    {
                        value = obj;
                    }
                }
            }

            return Ok(new
            {
                Value = value
            });
        }

        private class StlRequest
        {
            private IRequest Request { get; set; }

            public bool IsApiAuthorized { get; private set; }

            public SiteInfo SiteInfo { get; private set; }

            public PageInfo PageInfo { get; private set; }

            public ContextInfo ContextInfo { get; private set; }

            public async Task RunAsync(IRequest request, IAccessTokenRepository accessTokenRepository)
            {
                Request = request;
                IsApiAuthorized = !string.IsNullOrEmpty(request.ApiToken) && await accessTokenRepository.IsScopeAsync(Request.ApiToken, AccessTokenManager.ScopeStl);

                if (!IsApiAuthorized) return;

                var siteId = Request.GetQueryInt("siteId");
                var siteDir = Request.GetQueryString("siteDir");

                var channelId = Request.GetQueryInt("channelId");
                var contentId = Request.GetQueryInt("contentId");

                if (siteId > 0)
                {
                    SiteInfo = SiteManager.GetSiteInfo(siteId);
                }
                else if (!string.IsNullOrEmpty(siteDir))
                {
                    SiteInfo = SiteManager.GetSiteInfoByDirectory(siteDir);
                }
                else
                {
                    SiteInfo = SiteManager.GetSiteInfoByIsRoot();
                    if (SiteInfo == null)
                    {
                        var siteInfoList = SiteManager.GetSiteInfoList();
                        if (siteInfoList != null && siteInfoList.Count > 0)
                        {
                            SiteInfo = siteInfoList[0];
                        }
                    }
                }

                if (SiteInfo == null) return;

                if (channelId == 0)
                {
                    channelId = SiteInfo.Id;
                }

                var templateInfo = new TemplateInfo
                {
                    SiteId = SiteInfo.Id,
                    TemplateName = string.Empty,
                    Type = TemplateType.IndexPageTemplate,
                    RelatedFileName = string.Empty,
                    CreatedFileFullName = string.Empty,
                    CreatedFileExtName = string.Empty,
                    Default = true
                };

                PageInfo = new PageInfo(channelId, contentId, SiteInfo, templateInfo, new Dictionary<string, object>())
                {
                    UniqueId = 1000,
                    UserInfo = Request.UserInfo
                };

                var attributes = TranslateUtils.NewIgnoreCaseNameValueCollection();
 
                foreach (var key in request.QueryKeys)
                {
                    attributes[key] = request.GetQueryString(key);
                }

                ContextInfo = new ContextInfo(PageInfo)
                {
                    IsStlEntity = true,
                    Attributes = attributes,
                    InnerHtml = string.Empty
                };
            }
        }
    }
}
