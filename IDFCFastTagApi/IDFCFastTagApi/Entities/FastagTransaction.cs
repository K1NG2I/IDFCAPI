using System;

namespace IDFCFastTagApi.Entities
{
    public class FastagTransaction
    {
        public virtual int Id { get; set; }
        public virtual PushBatch PushBatch { get; set; }

        public virtual string TxnDt { get; set; }
        public virtual string ProcessingDt { get; set; }
        public virtual string TxnNo { get; set; }
        public virtual string ClientTxnId { get; set; }
        public virtual string PlazaId { get; set; }
        public virtual string TxnType { get; set; }
        public virtual string TagId { get; set; }
        public virtual string CreditAmt { get; set; }
        public virtual string DebitAmt { get; set; }
        public virtual string Balance { get; set; }
        public virtual string TxnDetails { get; set; }
        public virtual string OrgTxnId { get; set; }
        public virtual string ReasonCodeDA { get; set; }
        public virtual string LaneDirection { get; set; }

        public virtual DateTime CreatedOn { get; set; }
    }
}
