using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace AspNetCore.Umbraco.Identity.Models
{
    [XmlRoot(ElementName = "node")]
    public class XmlNode
    {
        [XmlAttribute("id")]
        public int Id { get; set; }

        [XmlAttribute("version")]
        public Guid Version { get; set; }

        [XmlAttribute("parentID")]
        public int ParentId { get; set; }

        [XmlAttribute("level")]
        public int Level { get; set; }

        [XmlAttribute("writerID")]
        public int WriterId { get; set; }

        [XmlAttribute("nodeType")]
        public int NodeType { get; set; }

        [XmlAttribute("template")]
        public int Template { get; set; }

        [XmlAttribute("sortOrder")]
        public int SortOrder { get; set; }

        [XmlAttribute("createDate")]
        public DateTime CreateDate { get; set; }

        [XmlAttribute("updateDate")]
        public DateTime UpdateDate { get; set; }

        [XmlAttribute("nodeName")]
        public string NodeName { get; set; }

        [XmlAttribute("urlName")]
        public string UrlName { get; set; }

        [XmlAttribute("writerName")]
        public string WriterName { get; set; }

        [XmlAttribute("nodeTypeAlias")]
        public string NodeTypeAlias { get; set; }

        [XmlAttribute("path")]
        public string Path { get; set; }

        [XmlAttribute("loginName")]
        public string LoginName { get; set; }

        [XmlAttribute("email")]
        public string Email { get; set; }

        [XmlElement("lastName")]
        public string LastName { get; set; }

        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("lastPasswordReset")]
        public DateTime LastPasswordReset { get; set; }
    }
}
