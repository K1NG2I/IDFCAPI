using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace IDFCFastTagApi.DTOs
{
    [XmlRoot("txnXML")]
    public class PushRequestDto
    {
        [XmlElement("head")]
        public PushHeadDto Head { get; set; }

        [XmlArray("txns")]
        [XmlArrayItem("txn")]
        public List<PushTxnDto> Txns { get; set; }

        [XmlElement("txn")]
        public PushTxnDto SingleTxn { get; set; }

        public List<PushTxnDto> GetAllTransactions()
        {
            var list = new List<PushTxnDto>();
            if (Txns != null && Txns.Count > 0)
            {
                list.AddRange(Txns);
            }
            if (SingleTxn != null)
            {
                list.Add(SingleTxn);
            }
            return list;
        }
    }

    public class PushHeadDto
    {
        [XmlElement("token")]
        public string Token { get; set; }

        [XmlElement("resCode")]
        public string ResCode { get; set; }

        [XmlElement("resMsg")]
        public string ResMsg { get; set; }

        [XmlElement("reqId")]
        public string ReqId { get; set; }

        [XmlElement("entityId")]
        public string EntityId { get; set; }

        [XmlElement("fromDate")]
        public string FromDate { get; set; }

        [XmlElement("toDate")]
        public string ToDate { get; set; }

        [XmlElement("txnCount")]
        public int TxnCount { get; set; }
    }

    public class PushTxnDto
    {
        [XmlElement("txnDt")]
        public string TxnDt { get; set; }

        [XmlElement("processingDt")]
        public string ProcessingDt { get; set; }

        [XmlElement("txnNo")]
        public string TxnNo { get; set; }

        [XmlElement("clientTxnID")]
        public string ClientTxnId { get; set; }

        [XmlElement("plazaId")]
        public string PlazaId { get; set; }

        [XmlElement("txnType")]
        public string TxnType { get; set; }

        [XmlElement("tagId")]
        public string TagId { get; set; }

        [XmlElement("creditAmt")]
        public string CreditAmt { get; set; }

        [XmlElement("debitAmt")]
        public string DebitAmt { get; set; }

        [XmlElement("balance")]
        public string Balance { get; set; }

        [XmlElement("txnDetails")]
        public string TxnDetails { get; set; }

        [XmlElement("org_txn_id")]
        public string OrgTxnId { get; set; }

        [XmlElement("reasonCodeDA")]
        public string ReasonCodeDA { get; set; }

        [XmlElement("laneDirection")]
        public string LaneDirection { get; set; }
    }
}
