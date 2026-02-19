using System;
using System.Xml.Serialization;

namespace IDFCFastTagApi.DTOs
{
    [XmlRoot("response")]
    public class PushResponseDto
    {
        [XmlElement("code")]
        public string Code { get; set; }

        [XmlElement("status")]
        public string Status { get; set; }

        [XmlElement("responseDateTime")]
        public string ResponseDateTime { get; set; }

        [XmlElement("message")]
        public string Message { get; set; }
    }
}
