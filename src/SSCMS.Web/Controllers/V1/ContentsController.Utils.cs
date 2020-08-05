﻿using System.Threading.Tasks;
using Datory;
using SqlKata;
using SSCMS.Enums;
using SSCMS.Utils;

namespace SSCMS.Web.Controllers.V1
{
    public partial class ContentsController
    {
        private async Task<Query> GetQueryAsync(int siteId, int? channelId, QueryRequest request)
        {
            var query = Q.Where(nameof(Models.Content.SiteId), siteId).Where(nameof(Models.Content.ChannelId), ">", 0);

            if (channelId.HasValue)
            {
                //query.Where(nameof(Abstractions.Content.ChannelId), channelId.Value);
                var channelIds = await _channelRepository.GetChannelIdsAsync(siteId, channelId.Value, ScopeType.All);

                query.WhereIn(nameof(Models.Content.ChannelId), channelIds);
            }

            if (request.Checked.HasValue)
            {
                query.Where(nameof(Models.Content.Checked), request.Checked.Value.ToString());
            }
            if (request.Top.HasValue)
            {
                query.Where(nameof(Models.Content.Top), request.Top.Value.ToString());
            }
            if (request.Recommend.HasValue)
            {
                query.Where(nameof(Models.Content.Recommend), request.Recommend.Value.ToString());
            }
            if (request.Color.HasValue)
            {
                query.Where(nameof(Models.Content.Color), request.Color.Value.ToString());
            }
            if (request.Hot.HasValue)
            {
                query.Where(nameof(Models.Content.Hot), request.Hot.Value.ToString());
            }

            if (request.GroupNames != null)
            {
                query.Where(q =>
                {
                    foreach (var groupName in request.GroupNames)
                    {
                        if (!string.IsNullOrEmpty(groupName))
                        {
                            q
                                .OrWhere(nameof(Models.Content.GroupNames), groupName)
                                .OrWhereLike(nameof(Models.Content.GroupNames), $"{groupName},%")
                                .OrWhereLike(nameof(Models.Content.GroupNames), $"%,{groupName},%")
                                .OrWhereLike(nameof(Models.Content.GroupNames), $"%,{groupName}");
                        }
                    }
                    return q;
                });
            }

            if (request.TagNames != null)
            {
                query.Where(q =>
                {
                    foreach (var tagName in request.TagNames)
                    {
                        if (!string.IsNullOrEmpty(tagName))
                        {
                            q
                                .OrWhere(nameof(Models.Content.TagNames), tagName)
                                .OrWhereLike(nameof(Models.Content.TagNames), $"{tagName},%")
                                .OrWhereLike(nameof(Models.Content.TagNames), $"%,{tagName},%")
                                .OrWhereLike(nameof(Models.Content.TagNames), $"%,{tagName}");
                        }
                    }
                    return q;
                });
            }

            if (request.Wheres != null)
            {
                foreach (var where in request.Wheres)
                {
                    if (string.IsNullOrEmpty(where.Operator)) where.Operator = OpEquals;
                    if (StringUtils.EqualsIgnoreCase(where.Operator, OpIn))
                    {
                        query.WhereIn(where.Column, ListUtils.GetStringList(where.Value));
                    }
                    else if (StringUtils.EqualsIgnoreCase(where.Operator, OpNotIn))
                    {
                        query.WhereNotIn(where.Column, ListUtils.GetStringList(where.Value));
                    }
                    else if (StringUtils.EqualsIgnoreCase(where.Operator, OpLike))
                    {
                        query.WhereLike(where.Column, where.Value);
                    }
                    else if (StringUtils.EqualsIgnoreCase(where.Operator, OpNotLike))
                    {
                        query.WhereNotLike(where.Column, where.Value);
                    }
                    else
                    {
                        query.Where(where.Column, where.Operator, where.Value);
                    }
                }
            }

            if (request.Orders != null)
            {
                foreach (var order in request.Orders)
                {
                    if (order.Desc)
                    {
                        query.OrderByDesc(order.Column);
                    }
                    else
                    {
                        query.OrderBy(order.Column);
                    }
                }
            }
            else
            {
                query.OrderByDesc(nameof(Models.Content.Top), 
                    nameof(Models.Content.ChannelId),
                    nameof(Models.Content.Taxis),
                    nameof(Models.Content.Id));
            }

            var page = request.Page > 0 ? request.Page : 1;
            var perPage = request.PerPage > 0 ? request.PerPage : 20;

            query.ForPage(page, perPage);

            return query;
        }
    }
}
