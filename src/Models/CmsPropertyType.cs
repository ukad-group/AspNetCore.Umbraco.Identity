namespace AspNetCore.Umbraco.Identity.Models
{
    public class CmsPropertyType
    {
        public int Id { get; set; }
        public int DataTypeId { get; set; }
        public int ContentTypeId { get; set; }
        public int? TabId { get; set; }
        public string Alias { get; set; }
        public string Name { get; set; }
        public string HelpText { get; set; }
        public int SortOrder { get; set; }
        public bool Mandatory { get; set; }
        public string ValidationRegExp { get; set; }
        public string Description { get; set; }
    }
}
