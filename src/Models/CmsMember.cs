using System.Collections.Generic;

namespace AspNetCore.Umbraco.Identity.Models
{
    public class CmsMember
    {
        public CmsMember()
        {
            CmsPropertyData = new HashSet<CmsPropertyData>();
        }
        public int NodeId { get; set; }
        public string Email { get; set; }
        public string LoginName { get; set; }
        public string Password { get; set; }
        public ICollection<CmsPropertyData> CmsPropertyData { get; set; }
    }
}
