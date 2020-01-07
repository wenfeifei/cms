using System;
using SS.CMS.Data;

namespace SS.CMS.Models
{
    [Serializable]
    [DataTable("siteserver_RelatedFieldItem")]
    public class RelatedFieldItem : Entity
    {
        [DataColumn]
        public int RelatedFieldId { get; set; }

        [DataColumn]
        public string ItemName { get; set; }

        [DataColumn]
        public string ItemValue { get; set; }

        [DataColumn]
        public int ParentId { get; set; }

        [DataColumn]
        public int Taxis { get; set; }
    }
}
