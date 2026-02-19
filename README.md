# IDFCFastTagApi

ASP.NET Web API 2 (.NET Framework 4.7) implementation for IDFC FASTag Push transaction notifications using NHibernate 3 and XML serialization.

## Project Structure

- Solution: `IDFCFastTagApi/IDFCFastTagApi.sln`
- Web project: `IDFCFastTagApi/IDFCFastTagApi/IDFCFastTagApi.csproj`
- Controller: `IDFCFastTagApi/IDFCFastTagApi/Controllers/FastagController.cs`
- Service: `IDFCFastTagApi/IDFCFastTagApi/Services/FastagService.cs`
- DTOs: `IDFCFastTagApi/IDFCFastTagApi/DTOs/`
- Entities: `IDFCFastTagApi/IDFCFastTagApi/Entities/`
- NHibernate mappings: `IDFCFastTagApi/IDFCFastTagApi/Mappings/`
- NHibernate config: `IDFCFastTagApi/IDFCFastTagApi/App_Data/hibernate.cfg.xml`
- Web config: `IDFCFastTagApi/IDFCFastTagApi/Web.config`

## Endpoint

- `POST /api/fastag/push`
- Content-Type: `application/xml`
- Response: `application/xml`

## XML Request Formats

Normal push (multiple transactions):

```xml
<txnXML>
  <head>
    <token>...</token>
    <resCode>700</resCode>
    <resMsg>Success</resMsg>
    <reqId>...</reqId>
    <entityId>...</entityId>
    <fromDate>01/05/2020 16:07:00</fromDate>
    <toDate>01/05/2020 16:16:00</toDate>
    <txnCount>1</txnCount>
  </head>
  <txns>
    <txn>
      <txnDt>01/05/2020 16:04:53</txnDt>
      <processingDt>01/05/2020 16:07:48</processingDt>
      <txnNo>0010002505011605330635</txnNo>
      <clientTxnID>N/A</clientTxnID>
      <plazaId>154002</plazaId>
      <txnType>D</txnType>
      <tagId>34161FA820328EE829483EE0</tagId>
      <creditAmt>0</creditAmt>
      <debitAmt>415</debitAmt>
      <balance>-316</balance>
      <txnDetails>Issuer Debit Transaction for toll fare - 154002 - Ghagghar Toll Plaza</txnDetails>
      <org_txn_id>0010002505011605330635</org_txn_id>
      <reasonCodeDA>NA</reasonCodeDA>
      <laneDirection>N</laneDirection>
    </txn>
  </txns>
</txnXML>
```

Chargeback push (single transaction):

```xml
<txnXML>
  <head>
    <token>...</token>
    <resCode>700</resCode>
    <resMsg>Success</resMsg>
    <reqId>...</reqId>
    <entityId>...</entityId>
    <fromDate>01/05/2020 16:07:00</fromDate>
    <toDate>01/05/2020 16:16:00</toDate>
    <txnCount>1</txnCount>
  </head>
  <txn>
    <processingDt>05/05/2025 18:50:40</processingDt>
    <txnNo>952505051849283513</txnNo>
    <clientTxnID>N/A</clientTxnID>
    <plazaId>536003</plazaId>
    <txnType>C</txnType>
    <tagId>34161FA820328EE81C9A5240</tagId>
    <creditAmt>105</creditAmt>
    <debitAmt>0</debitAmt>
    <balance>473</balance>
    <txnDetails>Chargeback Acceptance - 536003 - Gaman Bariyanamuvada</txnDetails>
    <org_txn_id>17421470868680</org_txn_id>
    <reasonCodeDA>470-CHARGEBACK ACCEPTANCE</reasonCodeDA>
    <laneDirection>NA</laneDirection>
  </txn>
</txnXML>
```

## XML Responses

Normal response:

```xml
<response>
  <code>1</code>
  <status>Success</status>
  <responseDateTime>15-05-2025 17:12:04.919</responseDateTime>
  <message>Transaction Recorded Successfully..!!</message>
</response>
```

Chargeback response:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<payment>
  <code>1</code>
  <message>success</message>
</payment>
```

## Behavior Summary

- Raw XML request is stored in `ApiLog` with status `RECEIVED`.
- XML is deserialized via `System.Xml.Serialization`.
- `PushBatch` and `FastagTransaction` are persisted within a single NHibernate transaction.
- Duplicate `txnNo` values are ignored.
- If any transaction has `txnType = "C"`, a chargeback response is returned.
- On exception: DB transaction is rolled back, log is marked `FAILED`, and a success XML response is still returned to avoid IDFC retry loops.

## Configuration

- Update DB connection in `IDFCFastTagApi/IDFCFastTagApi/Web.config` under `connectionStrings`.
- NHibernate configuration is in `IDFCFastTagApi/IDFCFastTagApi/App_Data/hibernate.cfg.xml`.

## NuGet Packages

Restore packages on Windows/Visual Studio:

- `Microsoft.AspNet.WebApi` 5.2.7
- `NHibernate` 3.3.3.4000

## Build Notes

- Designed for IIS hosting on Windows with HTTPS.
- Project targets .NET Framework 4.7.
