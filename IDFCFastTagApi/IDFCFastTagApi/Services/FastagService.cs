using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using IDFCFastTagApi.DTOs;
using IDFCFastTagApi.Entities;
using IDFCFastTagApi.Infrastructure;
using NHibernate;
using RSuite.Infrastructure.Core.Base;
using RSuite.Domain.Transport.Master.Fleet;
using RSuite.Domain.Transport.Transaction.Fasttag;
using RSuite.Domain.Transport.Master.Domestic;
using System.Collections;
using RSuite.Infrastructure.Specification.Transport.Transaction.Fasttag;
using RSuite.Infrastructure.Specification.Common.Login;
using RSuite.Infrastructure.Core.Context;
using RSuite.UserInterface.Web.Mvc.AppCode.Common.Factory;
using System.Web;

namespace IDFCFastTagApi.Services
{
    public class FastagService : IFastagService
    {

        IQueryExecutor _queryExecutor;
        IFastTagLogRepository _fastTagLogRepository;


        public FastagService(IQueryExecutor queryExecutor, IFastTagLogRepository fastTagLogRepository)
        {
            _queryExecutor =queryExecutor;
            _fastTagLogRepository = fastTagLogRepository;
        }

        public string ProcessPush(string rawXml)
        {

            var enableDbWrites = IsDbWriteEnabled();

            PushRequestDto request;
            try
            {
                request = DeserializeRequest(rawXml);

                var transactions = request.GetAllTransactions();

                foreach (var txn in transactions)
                {
                    var fastTagLog = MapToFastTagLog(txn);
              
                    _fastTagLogRepository.SaveFastTagLog(0, 0, Convert.ToString(DateTime.Now), fastTagLog);
                }

            }
            catch (Exception ex)
            {
                var responseXml = BuildNormalResponseXml();
                if (enableDbWrites)
                {
                    var apiLogId = SaveInitialLog(rawXml);
                    UpdateLog(apiLogId, null, responseXml, "FAILED", ex.Message);
                }
                return responseXml;
            }

            var responseIsChargeback = HasChargeback(request);
            var responseXmlFinal = responseIsChargeback ? BuildChargebackResponseXml() : BuildNormalResponseXml();
            var reqId = request.Head != null ? request.Head.ReqId : null;

            if (enableDbWrites)
            {
                var apiLogId = SaveInitialLog(rawXml);
                try
                {
                    PersistRequest(request);
                    UpdateLog(apiLogId, reqId, responseXmlFinal, "SUCCESS", null);
                }
                catch (Exception ex)
                {
                    UpdateLog(apiLogId, reqId, responseXmlFinal, "FAILED", ex.Message);
                }
            }

            return responseXmlFinal;
        }

   

        private FastTagLog MapToFastTagLog(PushTxnDto txn)
        {
            Vehicle vehicle = GetVehicleInformation(txn.TagId);
            return new FastTagLog
            {
                ProcessingDateTime = Convert.ToDateTime(txn.ProcessingDt),
                TransactionDateTime = Convert.ToDateTime(txn.TxnDt),
                FastTagTransactionId = txn.TxnNo,
                HexTagId = txn.TagId,
                VehicleId = vehicle.VehicleId,
                VehicleNo = vehicle.VehicleNo != null ? vehicle.VehicleNo : "",
                PlazaCode = txn.PlazaId,
                TransactionReferenceNo = txn.TxnNo,
                TransactionStatus = txn.TxnType,
                LaneCode = txn.LaneDirection,
                AdditionalKey = txn.OrgTxnId,
                AdditionalInfo1 = txn.ReasonCodeDA,
                TransactionAmount = Convert.ToDecimal(txn.TxnType == "D"
                    ? txn.DebitAmt
                    : txn.CreditAmt),
                PlazaName = ExtractPlazaName(txn.TxnDetails)
            };
        }

        private string ExtractPlazaName(string txnDetails)
        {
            if (string.IsNullOrWhiteSpace(txnDetails))
                return null;

            txnDetails = txnDetails.Trim().TrimEnd('<');

            var lastDashIndex = txnDetails.LastIndexOf('-');

            if (lastDashIndex == -1)
                return null;

            return txnDetails.Substring(lastDashIndex + 1).Trim();
        }


        private static bool IsDbWriteEnabled()
        {
            var setting = ConfigurationManager.AppSettings["EnableDbWrites"];
            bool enabled;
            if (bool.TryParse(setting, out enabled))
            {
                return enabled;
            }
            return true;
        }

        private Vehicle GetVehicleInformation(string tagId)
        {
          

            string query = @"select veh from Vehicle veh where vehicleid = (select dca.VehicleId from DigiCardAllocation dca where DigiCardId= (select dc.DigiCardId from DigiCard dc where DigiCardNo = '34161FA820328EE837C03D80')) ";

            Vehicle vehicle = _queryExecutor.ExecuteDynamicQuery<Vehicle>(query, 0, 0).FirstOrDefault() ?? new Vehicle();


            return vehicle;
        }

        private static int SaveInitialLog(string rawXml)
        {
            using (var session = NHibernateHelper.OpenSession())
            using (var tx = session.BeginTransaction())
            {
                var log = new ApiLog
                {
                    RawRequestXml = rawXml,
                    Status = "RECEIVED",
                    CreatedOn = DateTime.UtcNow
                };

                session.Save(log);
                tx.Commit();
                return log.Id;
            }
        }

        private static void UpdateLog(int logId, string reqId, string responseXml, string status, string error)
        {
            using (var session = NHibernateHelper.OpenSession())
            using (var tx = session.BeginTransaction())
            {
                var log = session.Get<ApiLog>(logId);
                if (log != null)
                {
                    if (!string.IsNullOrWhiteSpace(reqId))
                    {
                        log.ReqId = reqId;
                    }
                    log.RawResponseXml = responseXml;
                    log.Status = status;
                    log.ErrorMessage = error;
                    session.Update(log);
                }
                tx.Commit();
            }
        }

        private static PushRequestDto DeserializeRequest(string rawXml)
        {
            var serializer = new XmlSerializer(typeof(PushRequestDto));
            using (var reader = new StringReader(rawXml))
            {
                return (PushRequestDto)serializer.Deserialize(reader);
            }
        }

        private static bool HasChargeback(PushRequestDto request)
        {
            var txns = request.GetAllTransactions();
            return txns.Any(t => string.Equals(t.TxnType, "C", StringComparison.OrdinalIgnoreCase));
        }

        private static void PersistRequest(PushRequestDto request)
        {
            using (var session = NHibernateHelper.OpenSession())
            using (var tx = session.BeginTransaction())
            {
                var batch = new PushBatch
                {
                    ReqId = request.Head != null ? request.Head.ReqId : null,
                    EntityId = request.Head != null ? request.Head.EntityId : null,
                    FromDate = request.Head != null ? request.Head.FromDate : null,
                    ToDate = request.Head != null ? request.Head.ToDate : null,
                    TxnCount = request.Head != null ? request.Head.TxnCount : 0,
                    Token = request.Head != null ? request.Head.Token : null,
                    ResCode = request.Head != null ? request.Head.ResCode : null,
                    ResMsg = request.Head != null ? request.Head.ResMsg : null,
                    CreatedOn = DateTime.UtcNow
                };

                session.Save(batch);

                var transactions = request.GetAllTransactions();
                foreach (var t in transactions)
                {
                    if (string.IsNullOrWhiteSpace(t.TxnNo))
                    {
                        continue;
                    }

                    var exists = session.QueryOver<FastagTransaction>()
                        .Where(x => x.TxnNo == t.TxnNo)
                        .RowCount() > 0;

                    if (exists)
                    {
                        continue;
                    }

                    var entity = new FastagTransaction
                    {
                        PushBatch = batch,
                        TxnDt = t.TxnDt,
                        ProcessingDt = t.ProcessingDt,
                        TxnNo = t.TxnNo,
                        ClientTxnId = t.ClientTxnId,
                        PlazaId = t.PlazaId,
                        TxnType = t.TxnType,
                        TagId = t.TagId,
                        CreditAmt = t.CreditAmt,
                        DebitAmt = t.DebitAmt,
                        Balance = t.Balance,
                        TxnDetails = t.TxnDetails,
                        OrgTxnId = t.OrgTxnId,
                        ReasonCodeDA = t.ReasonCodeDA,
                        LaneDirection = t.LaneDirection,
                        CreatedOn = DateTime.UtcNow
                    };

                    session.Save(entity);
                }

                tx.Commit();
            }
        }

        private static string BuildNormalResponseXml()
        {
            var dto = new PushResponseDto
            {
                Code = "1",
                Status = "Success",
                ResponseDateTime = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff"),
                Message = "Transaction Recorded Successfully..!!"
            };

            return Serialize(dto, omitXmlDeclaration: true);
        }

        private static string BuildChargebackResponseXml()
        {
            var dto = new PaymentResponseDto
            {
                Code = "1",
                Message = "success"
            };

            return Serialize(dto, omitXmlDeclaration: false);
        }

        private static string Serialize<T>(T dto, bool omitXmlDeclaration)
        {
            var serializer = new XmlSerializer(typeof(T));
            var settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = omitXmlDeclaration,
                Encoding = new UTF8Encoding(false),
                Indent = false
            };
            var ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty);

            using (var sw = new Utf8StringWriter())
            using (var xw = XmlWriter.Create(sw, settings))
            {
                serializer.Serialize(xw, dto, ns);
                return sw.ToString();
            }
        }

        private sealed class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding => new UTF8Encoding(false);
        }
    }
}
