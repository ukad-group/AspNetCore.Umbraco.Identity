namespace AspNetCore.Umbraco.Identity.Models
{
    public class CmsMember
    {
        public int NodeId { get; set; }
        public string Email { get; set; }
        public string LoginName { get; set; }
        public string Password { get; set; }
    }
}
