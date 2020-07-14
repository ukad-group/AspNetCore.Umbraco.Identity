using System;
using System.Xml.Serialization;

namespace AspNetCore.Umbraco.Identity.Models
{
    public class UmbracoNode
    {
        public int Id { get; set; }
        public bool Trashed { get; set; }
        public int ParentId { get; set; }
        public int NodeUser { get; set; }
        public short Level { get; set; }
        public string Path { get; set; }
        public int SortOrder { get; set; }
        public Guid UniqueId { get; set; }
        public string Text { get; set; }
        public Guid NodeObjectType { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
