# AccuLink Data Exchange — what exists, and what AccuSync can reuse

**Source read:** `C:\Drive\natus\AccuLink\src\Component.DataExchange` and `Component.Core\Core\DataExchange`, 25 August 2026.
**Why:** AccuSync must support the same exchange formats. Before estimating that work, we read what AccuLink already does, because two of the plan's open blockers turned out to be answered by it.

---

## 1. The headline

**Two questions the plan has been carrying as blockers are answered by this source code.**

| Blocker | Where it was | Answer found |
|---|---|---|
| *"The SRS gives the folder and file name for HiTrack, OZ and CSV in exact detail, but never says what goes inside the file."* | Epic 4 F3, three 🔴 items, all waiting on **Q3** | `HiTrackTest.cs` carries **114 columns** with identifier, max length and date format. `Oz7Test.cs` carries the OZ binary record the same way. **The layouts are fully specified in code.** |
| *"3-day timeboxed discovery of the web service contract"* | Epic 20 F1, waiting on **Q11** | `Web References/EspWebService/northgate.wsdl`, plus `eSPSyncAllRequest.xsd`, `eSPSyncAllResponse.xsd` and `TestResult.xsd`. **The contract is on disk.** The discovery is a read, not an investigation. |

**S4H is eSP.** AccuLink's eSP connector calls `northgate.esp.sedq.bserv.ProxySEDQ` with two SOAP operations, `uploadData` and `downloadSyncData`. That is the SMaRT4Hearing sync AccuSync's Epic 20 describes.

---

## 2. What is actually there

| Component | Files | Lines | Import | Export |
|---|---:|---:|---|---|
| **Component.Core / DataExchange** | 80 | 9,899 | shared | shared |
| **HiTrack** | 33 | 8,283 | pick lists only | patients and tests |
| **Oz7** | 20 | 2,704 | **none** | patients and tests, **binary** |
| **Xml** (AccuLink XML) | 58 | 8,399 | yes | yes |
| **eSP** (= S4H) | 58 | 7,005 | configuration | test results, by web service |

### 2.1 The shared framework — the most valuable piece

`Component.Core\Core\DataExchange` is a format-independent engine, and it is what makes the per-format components small:

- **`Description/`** — `DataExchangeColumnAttribute`, `DataExchangeRecordAttribute`, `RecordDescription`, `TextColumnDescription`, `NumericColumnDescription`. A record is declared by attributes on a plain class; the engine reads them.
- **`Stream/`** — `FlatFileStreamWriter`, `BinaryFileStreamWriter`, `GenericFlatFileStreamReader`, `XmlStreamReader`, `XmlStreamWriter`, `FlatFileRecordEnumerator`.
- **`Formatter/`, `Map/`, `Set/`, `Tokens/`, `PersistenceLayer/`** — value formatting, field mapping, selection sets, and the transmission bookkeeping.

A format is then declared, not hand-written:

```csharp
[DataExchangeColumn(Identifier = "CDOB", Format = "{0:yyyy}{0:MM}{0:dd}", ImportFormat = "yyyyMMdd", MaxLength = 8)]
public DateTime DateOfBirth { get; set; }
```

Note `ImportFormat` alongside `Format` — **the same declaration serves both directions.** That is the single strongest argument for pairing each format's import and export in one feature: in this design they are one piece of work, not two.

### 2.2 HiTrack

- **Export** — `HiTrackPatientDataExporter`, 1,286 lines. Real business rules, not just field copying: result codes for deceased and discharged patients, risk-factor and comment aggregation, ethnicity and education code maps (`Ethnicity.Hispanic → "eeeebbbb-fa1c-…"`).
- **The record** — `HiTrackTest.cs`, **114 declared columns**: `CMID`, `CLAST`, `CFIRST`, `SEX`, `CDOB`, `CTOB`, `MULTI_CODE`, `RACE`, and so on.
- **Import** — `HiTrackPickListImporter`, 2,189 lines, and it imports **reference data, not patients**: hospitals, physicians, audiologists, screeners, nursery types, race types.

### 2.3 Oz7

- **Export only.** There is no importer anywhere in the component.
- The file is **binary**, written through `BinaryFileStreamWriter<Oz7Test>`, with columns declared as `MaxLength = 30, NeedsTermination = true`.
- The exporter is 436 lines — the smallest of the four.

### 2.4 Xml — the AccuLink format

- Both directions: `XmlDataImporter` (1,671 lines) and `XmlPatientDataExporter` (1,533 lines).
- **35 typed classes** (1,658 lines) generated from `AccuLinkXiMpLe.xsd`, rooted at `<AccuLink.XiMpLe>` and covering patients, tests, sites, facilities, locations, risk factors, predefined comments, transducers, users and consent.

### 2.5 eSP — which is S4H

- **Contract on disk**: `northgate.wsdl`, `eSPSyncAllRequest.xsd`, `eSPSyncAllResponse.xsd`, `TestResult.xsd`.
- **Two operations**: `uploadData` (results out) and `downloadSyncData` (configuration in).
- **Down the wire**: `UserDetails`, `DeviceDetails`, `ScreeningDevices`, `FacilityDetails`, `SiteFacilityData`, `RiskFactors`, `ScreenerType` — precisely the four lists Epic 20 F3 and F4 describe.
- **Up the wire**: `EspTestResults` with its own typed tree.
- `EspManager` is 2,297 lines and holds the sync rules.

---

## 3. Three corrections to what we assumed

**1. OZ has no import.** AccuLink exports OZ and never reads it. If AccuSync needs OZ import, it is new work with no reference implementation and no stated format owner. **This needs a decision from Natus.**

**2. HiTrack import is not patient import.** It brings in pick lists — hospitals, physicians, audiologists, screeners, nursery and race types. Anyone reading "HiTrack import and export" as a round trip of patient records would size it wrongly.

**3. eSP import is not file import.** It is a configuration download from the web service. It belongs with S4H, which is why folding S4H into this epic works rather than being a filing convenience.

---

## 4. What ports, and what does not

| Piece | Ports? | Why |
|---|---|---|
| `DataExchangeColumnAttribute` and the record descriptions | **Yes, cleanly** | Attributes on POCOs. No framework dependency |
| Flat-file, binary and XML stream readers and writers | **Yes, mostly** | `System.IO` and `System.Xml`, both present in .NET 10 |
| `HiTrackTest`, `Oz7Test`, the 35 XML datatypes | **Yes — these are the specifications** | Declarative classes. Porting them *is* acquiring the format spec |
| Exporter and importer business rules | **Yes, as reference** | Translate the logic; the persistence calls underneath are AccuSync's |
| **eSP SOAP client** | **No** | `SoapHttpClientProtocol` is .NET Framework only. .NET 10 needs a generated WCF client or SOAP over `HttpClient`. **The contract ports; the client does not** |
| Anything under `*.WindowsForms` | **No** | WinForms editors, assistants and modules. AccuSync is WPF/MVVM |
| `PathMedical.*` persistence and site/facility layers | **No** | AccuLink's own data layer |

**The honest summary:** the formats and the rules port; the plumbing at both ends does not. That still removes the largest risk in this epic, which was never the code — it was not knowing what the files contain.

---

## 5. What this means for the estimate

- The **3-day S4H discovery disappears.** Reading a WSDL and four XSDs is not a timeboxed investigation.
- The **three 🔴 Q3 items on HiTrack, OZ and CSV lose their blocker** for HiTrack and OZ. CSV is still open — no AccuLink CSV component exists, and GID-256283/256284 still say "fixed width" for something called CSV.
- Porting the shared record and stream layer is **real work that pays for itself three times over**, because HiTrack, OZ and the XML formats all sit on it.
- Pairing import and export per format is not just a delivery preference — in this design one declaration drives both, so splitting them would mean touching the same class twice.

---

## 6. Still open

| | Question | Who |
|---|---|---|
| **X1** | **Does AccuSync need OZ import?** AccuLink has none, and no format owner is named. If yes, we need a sample file and a spec | Natus |
| **X2** | **Is HiTrack import meant to be pick lists, as in AccuLink, or patient records?** They are very different jobs | Natus |
| **X3** | **CSV: what goes in it, and is it really fixed width?** GID-256283 and GID-256284 say "fixed width" for a format called CSV. Still Q3 | Natus |
| **X4** | **Is the SEDQ endpoint in `northgate.wsdl` still the live contract**, or has the service moved on since AccuLink shipped? | Natus |
| **X5** | **Are we licensed to port this code**, or must we re-implement from the specifications it contains? | Natus / Soliton legal |

X5 matters before anyone copies a file. Reading AccuLink to learn the formats is safe; lifting source into a new product is a question someone else has to answer.
