﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SS.CMS.Data;
using SS.CMS.Models;
using SS.CMS.Repositories;

namespace SS.CMS.Cli.Updater.Tables
{
    public partial class TableAdministrator
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("passwordFormat")]
        public string PasswordFormat { get; set; }

        [JsonProperty("passwordSalt")]
        public string PasswordSalt { get; set; }

        [JsonProperty("creationDate")]
        public DateTime CreationDate { get; set; }

        [JsonProperty("lastActivityDate")]
        public DateTime LastActivityDate { get; set; }

        [JsonProperty("lastModuleID")]
        public string LastModuleId { get; set; }

        [JsonProperty("countOfLogin")]
        public int CountOfLogin { get; set; }

        [JsonProperty("creatorUserName")]
        public string CreatorUserName { get; set; }

        [JsonProperty("isChecked")]
        public string IsChecked { get; set; }

        [JsonProperty("isLockedOut")]
        public string IsLockedOut { get; set; }

        [JsonProperty("publishmentSystemID")]
        public int PublishmentSystemId { get; set; }

        [JsonProperty("departmentID")]
        public int DepartmentId { get; set; }

        [JsonProperty("areaID")]
        public int AreaId { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("question")]
        public string Question { get; set; }

        [JsonProperty("answer")]
        public string Answer { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("mobile")]
        public string Mobile { get; set; }

        [JsonProperty("theme")]
        public string Theme { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }
    }

    public partial class TableAdministrator
    {
        public static readonly List<string> OldTableNames = new List<string>
        {
            "bairong_Administrator",
            "siteserver_Administrator"
        };

        public static ConvertInfo GetConverter(IUserRepository userRepository)
        {
            return new ConvertInfo
            {
                NewTableName = userRepository.TableName,
                NewColumns = userRepository.TableColumns,
                ConvertKeyDict = ConvertKeyDict,
                ConvertValueDict = ConvertValueDict
            };
        }

        private static readonly Dictionary<string, string> ConvertKeyDict =
            new Dictionary<string, string>
            {
                {nameof(User.SiteId), nameof(PublishmentSystemId)}
            };

        private static readonly Dictionary<string, string> ConvertValueDict = null;
    }
}
