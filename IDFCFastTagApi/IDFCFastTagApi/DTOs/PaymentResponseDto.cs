using System.Xml.Serialization;

namespace IDFCFastTagApi.DTOs
{
    [XmlRoot("payment")]
    public class PaymentResponseDto
    {
        [XmlElement("code")]
        public string Code { get; set; }

        [XmlElement("message")]
        public string Message { get; set; }
    }
}
