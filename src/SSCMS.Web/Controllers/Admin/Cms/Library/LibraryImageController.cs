﻿using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using SSCMS.Core.Extensions;
using SSCMS.Dto;
using SSCMS.Enums;
using SSCMS.Models;
using SSCMS.Repositories;
using SSCMS.Services;
using SSCMS.Utils;

namespace SSCMS.Web.Controllers.Admin.Cms.Library
{
    [OpenApiIgnore]
    [Authorize(Roles = AuthTypes.Roles.Administrator)]
    [Route(Constants.ApiAdminPrefix)]
    public partial class LibraryImageController : ControllerBase
    {
        private const string Route = "cms/library/libraryImage";
        private const string RouteId = "cms/library/libraryImage/{id}";
        private const string RouteList = "cms/library/libraryImage/list";
        private const string RouteGroups = "cms/library/libraryImage/groups";
        private const string RouteGroupId = "cms/library/libraryImage/groups/{id}";

        private readonly ISettingsManager _settingsManager;
        private readonly IAuthManager _authManager;
        private readonly IPathManager _pathManager;
        private readonly ILibraryGroupRepository _libraryGroupRepository;
        private readonly ILibraryImageRepository _libraryImageRepository;

        public LibraryImageController(ISettingsManager settingsManager, IAuthManager authManager, IPathManager pathManager, ILibraryGroupRepository libraryGroupRepository, ILibraryImageRepository libraryImageRepository)
        {
            _settingsManager = settingsManager;
            _authManager = authManager;
            _pathManager = pathManager;
            _libraryGroupRepository = libraryGroupRepository;
            _libraryImageRepository = libraryImageRepository;
        }

        [HttpPost, Route(RouteList)]
        public async Task<ActionResult<QueryResult>> List([FromBody]QueryRequest req)
        {
            if (!await _authManager.HasSitePermissionsAsync(req.SiteId,
                    AuthTypes.SitePermissions.LibraryImage))
            {
                return Unauthorized();
            }

            var groups = await _libraryGroupRepository.GetAllAsync(LibraryType.Image);
            groups.Insert(0, new LibraryGroup
            {
                Id = 0,
                GroupName = "全部图片"
            });
            var count = await _libraryImageRepository.GetCountAsync(req.GroupId, req.Keyword);
            var items = await _libraryImageRepository.GetAllAsync(req.GroupId, req.Keyword, req.Page, req.PerPage);

            return new QueryResult
            {
                Groups = groups,
                Count = count,
                Items = items
            };
        }

        [HttpPost, Route(Route)]
        public async Task<ActionResult<LibraryImage>> Create([FromQuery]CreateRequest request, [FromForm] IFormFile file)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId,
                    AuthTypes.SitePermissions.LibraryImage))
            {
                return Unauthorized();
            }

            var library = new LibraryImage
            {
                GroupId = request.GroupId
            };

            if (file == null)
            {
                return this.Error("请选择有效的文件上传");
            }

            var fileName = Path.GetFileName(file.FileName);

            if (!PathUtils.IsExtension(PathUtils.GetExtension(fileName), ".jpg", ".jpeg", ".bmp", ".gif", ".png", ".svg", ".webp"))
            {
                return this.Error("文件只能是图片格式，请选择有效的文件上传!");
            }

            var libraryFileName = PathUtils.GetLibraryFileName(fileName);
            var virtualDirectoryPath = PathUtils.GetLibraryVirtualDirectoryPath(UploadType.Image);
            
            var directoryPath = PathUtils.Combine(_settingsManager.WebRootPath, virtualDirectoryPath);
            var filePath = PathUtils.Combine(directoryPath, libraryFileName);

            await _pathManager.UploadAsync(file, filePath);

            library.Title = fileName;
            library.Url = PageUtils.Combine(virtualDirectoryPath, libraryFileName);

            library.Id = await _libraryImageRepository.InsertAsync(library);

            return library;
        }

        [HttpPut, Route(RouteId)]
        public async Task<ActionResult<LibraryImage>> Update([FromBody] UpdateRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId,
                    AuthTypes.SitePermissions.LibraryImage))
            {
                return Unauthorized();
            }

            var lib = await _libraryImageRepository.GetAsync(request.Id);
            lib.Title = request.Title;
            lib.GroupId = request.GroupId;
            await _libraryImageRepository.UpdateAsync(lib);

            return lib;
        }

        [HttpDelete, Route(RouteId)]
        public async Task<ActionResult<BoolResult>> Delete([FromBody]DeleteRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId,
                    AuthTypes.SitePermissions.LibraryImage))
            {
                return Unauthorized();
            }

            await _libraryImageRepository.DeleteAsync(request.Id);

            return new BoolResult
            {
                Value = true
            };
        }

        [HttpPost, Route(RouteGroups)]
        public async Task<ActionResult<LibraryGroup>> CreateGroup([FromBody] GroupRequest group)
        {
            if (!await _authManager.HasSitePermissionsAsync(group.SiteId,
                    AuthTypes.SitePermissions.LibraryImage))
            {
                return Unauthorized();
            }

            var libraryGroup = new LibraryGroup
            {
                LibraryType = LibraryType.Image,
                GroupName = group.Name
            };
            libraryGroup.Id = await _libraryGroupRepository.InsertAsync(libraryGroup);

            return libraryGroup;
        }

        [HttpPut, Route(RouteGroupId)]
        public async Task<ActionResult<LibraryGroup>> RenameGroup([FromQuery]int id, [FromBody] GroupRequest group)
        {
            if (!await _authManager.HasSitePermissionsAsync(group.SiteId,
                    AuthTypes.SitePermissions.LibraryImage))
            {
                return Unauthorized();
            }

            var libraryGroup = await _libraryGroupRepository.GetAsync(id);
            libraryGroup.GroupName = group.Name;
            await _libraryGroupRepository.UpdateAsync(libraryGroup);

            return libraryGroup;
        }

        [HttpDelete, Route(RouteGroupId)]
        public async Task<ActionResult<BoolResult>> DeleteGroup([FromBody]DeleteRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId,
                    AuthTypes.SitePermissions.LibraryImage))
            {
                return Unauthorized();
            }

            await _libraryGroupRepository.DeleteAsync(LibraryType.Image, request.Id);

            return new BoolResult
            {
                Value = true
            };
        }
    }
}
