using System;
using System.Collections.Generic;

namespace IDFCFastTagApi.Entities
{
    public class PushBatch
    {
        public virtual int Id { get; set; }
        public virtual string ReqId { get; set; }
        public virtual string EntityId { get; set; }
        public virtual string FromDate { get; set; }
        public virtual string ToDate { get; set; }
        public virtual int TxnCount { get; set; }
        public virtual string Token { get; set; }
        public virtual string ResCode { get; set; }
        public virtual string ResMsg { get; set; }
        public virtual DateTime CreatedOn { get; set; }

        public virtual IList<FastagTransaction> Transactions { get; set; }
    }
}
