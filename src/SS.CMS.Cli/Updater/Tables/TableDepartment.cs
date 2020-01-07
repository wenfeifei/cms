﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SS.CMS.Models;
using SS.CMS.Repositories;

namespace SS.CMS.Cli.Updater.Tables
{
    public partial class TableDepartment
    {
        [JsonProperty("departmentID")]
        public long DepartmentId { get; set; }

        [JsonProperty("departmentName")]
        public string DepartmentName { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("parentID")]
        public long ParentId { get; set; }

        [JsonProperty("parentsPath")]
        public string ParentsPath { get; set; }

        [JsonProperty("parentsCount")]
        public long ParentsCount { get; set; }

        [JsonProperty("childrenCount")]
        public long ChildrenCount { get; set; }

        [JsonProperty("isLastNode")]
        public string IsLastNode { get; set; }

        [JsonProperty("taxis")]
        public long Taxis { get; set; }

        [JsonProperty("addDate")]
        public DateTimeOffset AddDate { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("countOfAdmin")]
        public long CountOfAdmin { get; set; }

        [JsonProperty("countOfUser")]
        public long CountOfUser { get; set; }
    }

    public partial class TableDepartment
    {
        public const string OldTableName = "bairong_Department";

        public static ConvertInfo GetConverter(IDepartmentRepository departmentRepository)
        {
            return new ConvertInfo
            {
                NewTableName = departmentRepository.TableName,
                NewColumns = departmentRepository.TableColumns,
                ConvertKeyDict = ConvertKeyDict,
                ConvertValueDict = ConvertValueDict
            };
        }

        private static readonly Dictionary<string, string> ConvertKeyDict =
            new Dictionary<string, string>
            {
                {nameof(Department.Id), nameof(DepartmentId)}
            };

        private static readonly Dictionary<string, string> ConvertValueDict = null;
    }
}
