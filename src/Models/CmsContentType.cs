namespace AspNetCore.Umbraco.Identity.Models
{
    public class CmsContentType
    {
        public int Pk { get; set; }
        public int NodeId { get; set; }
        public string Alias { get; set; }
        public string Icon { get; set; }
        public string Thumbnail { get; set; }
        public string Description { get; set; }
        public int? MasterContentType { get; set; }
    }
}
