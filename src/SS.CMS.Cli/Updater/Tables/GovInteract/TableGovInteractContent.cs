﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SS.CMS.Data;
using SS.CMS.Models;
using SS.CMS.Utils;

namespace SS.CMS.Cli.Updater.Tables.GovInteract
{
    public partial class TableGovInteractContent
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("nodeID")]
        public long NodeId { get; set; }

        [JsonProperty("publishmentSystemID")]
        public long PublishmentSystemId { get; set; }

        [JsonProperty("addUserName")]
        public string AddUserName { get; set; }

        [JsonProperty("lastEditUserName")]
        public string LastEditUserName { get; set; }

        [JsonProperty("lastEditDate")]
        public DateTimeOffset LastEditDate { get; set; }

        [JsonProperty("taxis")]
        public long Taxis { get; set; }

        [JsonProperty("contentGroupNameCollection")]
        public string ContentGroupNameCollection { get; set; }

        [JsonProperty("tags")]
        public string Tags { get; set; }

        [JsonProperty("sourceID")]
        public long SourceId { get; set; }

        [JsonProperty("referenceID")]
        public long ReferenceId { get; set; }

        [JsonProperty("isChecked")]
        public string IsChecked { get; set; }

        [JsonProperty("checkedLevel")]
        public long CheckedLevel { get; set; }

        [JsonProperty("comments")]
        public long Comments { get; set; }

        [JsonProperty("hits")]
        public long Hits { get; set; }

        [JsonProperty("hitsByDay")]
        public long HitsByDay { get; set; }

        [JsonProperty("hitsByWeek")]
        public long HitsByWeek { get; set; }

        [JsonProperty("hitsByMonth")]
        public long HitsByMonth { get; set; }

        [JsonProperty("lastHitsDate")]
        public DateTimeOffset LastHitsDate { get; set; }

        [JsonProperty("settingsXML")]
        public string SettingsXml { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("subTitle")]
        public string SubTitle { get; set; }

        [JsonProperty("imageUrl")]
        public string ImageUrl { get; set; }

        [JsonProperty("videoUrl")]
        public string VideoUrl { get; set; }

        [JsonProperty("fileUrl")]
        public string FileUrl { get; set; }

        [JsonProperty("linkUrl")]
        public string LinkUrl { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("isRecommend")]
        public string IsRecommend { get; set; }

        [JsonProperty("isHot")]
        public string IsHot { get; set; }

        [JsonProperty("isColor")]
        public string IsColor { get; set; }

        [JsonProperty("isTop")]
        public string IsTop { get; set; }

        [JsonProperty("addDate")]
        public DateTimeOffset AddDate { get; set; }
    }

    public partial class TableGovInteractContent
    {
        public const string NewTableName = "ss_govinteract_content";

        private static List<TableColumn> NewColumns => new List<TableColumn>
        {
            new TableColumn
            {
                AttributeName = "RealName",
                DataType = DataType.VarChar,
                DataLength = 255
            },
            new TableColumn
            {
                AttributeName = "Organization",
                DataType = DataType.VarChar,
                DataLength = 255
            },
            new TableColumn
            {
                AttributeName = "CardType",
                DataType = DataType.VarChar,
                DataLength = 255
            },
            new TableColumn
            {
                AttributeName = "CardNo",
                DataType = DataType.VarChar,
                DataLength = 255
            },
            new TableColumn
            {
                AttributeName = "Phone",
                DataType = DataType.VarChar,
                DataLength = 50
            },
            new TableColumn
            {
                AttributeName = "PostCode",
                DataType = DataType.VarChar,
                DataLength = 50
            },
            new TableColumn
            {
                AttributeName = "Address",
                DataType = DataType.VarChar,
                DataLength = 255
            },
            new TableColumn
            {
                AttributeName = "Email",
                DataType = DataType.VarChar,
                DataLength = 255
            },
            new TableColumn
            {
                AttributeName = "Fax",
                DataType = DataType.VarChar,
                DataLength = 50
            },
            new TableColumn
            {
                AttributeName = "TypeId",
                DataType = DataType.Integer
            },
            new TableColumn
            {
                AttributeName = "IsPublic",
                DataType = DataType.VarChar,
                DataLength = 18
            },
            new TableColumn
            {
                AttributeName = "Content",
                DataType = DataType.Text
            },
            new TableColumn
            {
                AttributeName = "FileUrl",
                DataType = DataType.VarChar,
                DataLength = 255
            },
            new TableColumn
            {
                AttributeName = "DepartmentId",
                DataType = DataType.Integer
            },
            new TableColumn
            {
                AttributeName = "DepartmentName",
                DataType = DataType.VarChar,
                DataLength = 255
            },
            new TableColumn
            {
                AttributeName = "QueryCode",
                DataType = DataType.VarChar,
                DataLength = 255
            },
            new TableColumn
            {
                AttributeName = "State",
                DataType = DataType.VarChar,
                DataLength = 50
            },
            new TableColumn
            {
                AttributeName = "IpAddress",
                DataType = DataType.VarChar,
                DataLength = 50
            },
            new TableColumn
            {
                AttributeName = "ReplyContent",
                DataType = DataType.Text
            },
            new TableColumn
            {
                AttributeName = "ReplyFileUrl",
                DataType = DataType.VarChar,
                DataLength = 255
            },
            new TableColumn
            {
                AttributeName = "ReplyDepartmentName",
                DataType = DataType.VarChar,
                DataLength = 50
            },
            new TableColumn
            {
                AttributeName = "ReplyUserName",
                DataType = DataType.VarChar,
                DataLength = 50
            },
            new TableColumn
            {
                AttributeName = "ReplyAddDate",
                DataType = DataType.DateTime
            }
        };

        private static List<TableColumn> GetNewColumns(IList<TableColumn> oldColumns)
        {
            var columns = new List<TableColumn>();
            var tableColumns = (new Database(null, null)).GetTableColumns<Content>();

            columns.AddRange(tableColumns);
            columns.AddRange(NewColumns);

            foreach (var tableColumnInfo in oldColumns)
            {
                if (StringUtils.EqualsIgnoreCase(tableColumnInfo.AttributeName, nameof(NodeId)))
                {
                    tableColumnInfo.AttributeName = nameof(Models.Content.ChannelId);
                }
                else if (StringUtils.EqualsIgnoreCase(tableColumnInfo.AttributeName, nameof(PublishmentSystemId)))
                {
                    tableColumnInfo.AttributeName = nameof(Models.Content.SiteId);
                }
                else if (StringUtils.EqualsIgnoreCase(tableColumnInfo.AttributeName, nameof(ContentGroupNameCollection)))
                {
                    tableColumnInfo.AttributeName = nameof(Models.Content.GroupNameCollection);
                }

                if (!columns.Exists(c => StringUtils.EqualsIgnoreCase(c.AttributeName, tableColumnInfo.AttributeName)))
                {
                    columns.Add(tableColumnInfo);
                }
            }

            return columns;
        }

        public static ConvertInfo GetConverter(IList<TableColumn> oldColumns)
        {
            return new ConvertInfo
            {
                NewTableName = NewTableName,
                NewColumns = GetNewColumns(oldColumns),
                ConvertKeyDict = ConvertKeyDict,
                ConvertValueDict = ConvertValueDict
            };
        }

        private static readonly Dictionary<string, string> ConvertKeyDict =
            new Dictionary<string, string>
            {
                {nameof(Models.Content.ChannelId), nameof(NodeId)},
                {nameof(Models.Content.SiteId), nameof(PublishmentSystemId)},
                {nameof(Models.Content.GroupNameCollection), nameof(ContentGroupNameCollection)}
            };

        private static readonly Dictionary<string, string> ConvertValueDict = null;
    }
}