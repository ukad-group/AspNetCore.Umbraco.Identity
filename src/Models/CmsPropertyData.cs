using System;

namespace AspNetCore.Umbraco.Identity.Models
{
    public class CmsPropertyData
    {
        public int Id { get; set; }
        public int ContentNodeId { get; set; }
        public Guid VersionId { get; set; }
        public int PropertyTypeId { get; set; }
        public int? DataInt { get; set; }
        public DateTime? DataDate { get; set; }
        public string DataNvarchar { get; set; }
        public string DataNtext { get; set; }
    }
}
