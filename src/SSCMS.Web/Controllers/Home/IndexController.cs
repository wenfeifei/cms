﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using SSCMS.Configuration;
using SSCMS.Extensions;
using SSCMS.Repositories;
using SSCMS.Services;

namespace SSCMS.Web.Controllers.Home
{
    [OpenApiIgnore]
    [Authorize(Roles = Types.Roles.User)]
    [Route(Constants.ApiHomePrefix)]
    public partial class IndexController : ControllerBase
    {
        private const string Route = "index";

        private readonly IAuthManager _authManager;
        private readonly IConfigRepository _configRepository;
        private readonly IUserMenuRepository _userMenuRepository;

        public IndexController(IAuthManager authManager, IConfigRepository configRepository, IUserMenuRepository userMenuRepository)
        {
            _authManager = authManager;
            _configRepository = configRepository;
            _userMenuRepository = userMenuRepository;
        }

        [HttpGet, Route(Route)]
        public async Task<ActionResult<GetResult>> Get()
        {
            var config = await _configRepository.GetAsync();
            if (config.IsHomeClosed) return this.Error("对不起，用户中心已被禁用！");

            var menus = new List<Menu>();
            var user = await _authManager.GetUserAsync();
            var userMenus = await _userMenuRepository.GetUserMenusAsync();

            foreach (var menuInfo1 in userMenus)
            {
                var groupIds = menuInfo1.GroupIds ?? new List<int>();
                if (menuInfo1.Disabled || menuInfo1.ParentId != 0 ||
                    groupIds.Contains(user.GroupId)) continue;
                var children = new List<Menu>();
                foreach (var menuInfo2 in userMenus)
                {
                    var groupIds2 = menuInfo2.GroupIds ?? new List<int>();
                    if (menuInfo2.Disabled || menuInfo2.ParentId != menuInfo1.Id ||
                        groupIds2.Contains(user.GroupId)) continue;

                    children.Add(new Menu
                    {
                        Id = menuInfo2.Id.ToString(),
                        Text = menuInfo2.Text,
                        IconClass = menuInfo2.IconClass,
                        Link = menuInfo2.Link,
                        Target = menuInfo2.Target
                    });
                }

                menus.Add(new Menu
                {
                    Id = menuInfo1.Id.ToString(),
                    Text = menuInfo1.Text,
                    IconClass = menuInfo1.IconClass,
                    Link = menuInfo1.Link,
                    Target = menuInfo1.Target,
                    Children = children
                });
            }

            return new GetResult
            {
                User = user,
                HomeTitle = config.HomeTitle,
                HomeLogoUrl = config.HomeLogoUrl,
                Menus = menus
            };
        }
    }
}
