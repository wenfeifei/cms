using System;
using SS.CMS.Data;

namespace SS.CMS.Models
{
    [Serializable]
    [DataTable("siteserver_ContentGroup")]
    public class ContentGroup : Entity
    {
        [DataColumn]
        public string GroupName { get; set; }

        [DataColumn]
        public int SiteId { get; set; }

        [DataColumn]
        public int Taxis { get; set; }

        [DataColumn(Length = 2000)]
        public string Description { get; set; }
    }
}
