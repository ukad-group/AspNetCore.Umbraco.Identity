using System;

namespace AspNetCore.Umbraco.Identity.Models
{
    public class CmsContentVersion
    {
        public int Id { get; set; }
        public int ContentId { get; set; }
        public Guid VersionId { get; set; }
        public DateTime VersionDate { get; set; }
    }
}
