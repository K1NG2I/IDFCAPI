using System;

namespace IDFCFastTagApi.Entities
{
    public class ApiLog
    {
        public virtual int Id { get; set; }
        public virtual string ReqId { get; set; }
        public virtual string RawRequestXml { get; set; }
        public virtual string RawResponseXml { get; set; }
        public virtual string Status { get; set; }
        public virtual string ErrorMessage { get; set; }
        public virtual DateTime CreatedOn { get; set; }
    }
}
