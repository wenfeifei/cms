﻿using System;
using System.Collections.Generic;
using SS.CMS.Data;

namespace SS.CMS.Cli.Updater
{
    public class ConvertInfo
    {
        public bool IsAbandon { get; set; }

        public string NewTableName { get; set; }

        public IList<TableColumn> NewColumns { get; set; }

        public Dictionary<string, string> ConvertKeyDict { get; set; }

        public Dictionary<string, string> ConvertValueDict { get; set; }

        public Func<Dictionary<string, object>, Dictionary<string, object>> Process { get; set; }
    }
}
