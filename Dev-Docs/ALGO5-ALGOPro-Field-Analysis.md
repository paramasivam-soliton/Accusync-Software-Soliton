# ALGO 5 and ALGO Pro — field analysis against AccuSync

**Question being answered:** Haidee asked us to *"identify the additional fields to be added according to the ALGO 5 and ALGO Pro files."*

**What was examined**

| Source | Path | Contents |
|---|---|---|
| ALGO 5 export | `Dev-Docs/ALGO5_sample.xml` | 1 patient, 16 risk factors, 7 ABR tests |
| ALGO Pro export | `Dev-Docs/ALGOPro_sample.json` | 3 patients, 16 risk factors, 2 tests each |
| AccuSync patient model | `AccuSync.Core/Entities/PatientData.cs` | 68 properties |
| AccuSync patient screen | `AccuSync.WPF/Views/PatientsTests/Tabs/PatientDetailsTab.xaml` | 18 data fields |
| AccuSync additional-info screen | `AccuSync.WPF/Views/PatientsTests/Tabs/AdditionalInfoTab.xaml` | 40 data fields |
| AccuSync test model | `AccuSync.Core/Entities/TestRecord.cs` | 21 properties |
| AccuSync test screen | `AccuSync.WPF/Views/PatientsTests/TestResultsView.xaml` | 6 grid columns, 4 tabs |
| Schema design | `Databases/PatientDatabase.sql` | `Patients` 35 cols, `PatientContacts` 31, `TestRecords` 33, `ABRResults` 33 |

> These are two **different** file formats, not two versions of one. ALGO Pro is not a superset of
> ALGO 5 — it adds fields, and it also **drops** fields ALGO 5 carries.

---

## 0. Natus decisions — 18 Aug 2026

Haidee Kachniewicz answered the four questions we raised. Recorded here as the authority for the
scope of **Epic 2 F1 · story PM-39 · Add the ALGO device fields**.

| # | Question | Natus decision | Effect |
|---:|---|---|---|
| **1** | Does the baby need its own address and phone? | **Agree — skip them.** Use the mother/caregiver address | **8 fields dropped.** Patient scope 28 → **19** |
| **2** | Is one physician field enough? | **No — keep `PatientPhysician` and `MothersPhysician` separate.** *"They won't be the same person in the field"* | The mother gets her own field. **PM-39** |
| **3** | Rename to "Physician / Pediatrician"? | **Only the baby's field.** *"Since we're keeping them separate (per #2), let's not merge them under one label... the mother's stays its own field"* | Baby's label = **Physician / Pediatrician**; mother's keeps its own. **PM-39** |
| **4** | Medication — configurable list or multi-line? | **Multi-line text.** *"A configurable list means a whole config screen for a field that is not in the requirements, whereas multi-line is a small change that covers the notes use case"* | **PM-39** covers it; 0.5 days of the 8.0 instead of ~2.0 |

**Scope now committed:** 19 patient fields + the Medication change, built by **Epic 2 F1, story PM-39** —
one story, 8.0 coding days, **14.0 total days**, in slice 2A / milestone M1. Included in the plan (722.0 days).

**Still not scoped:** the 28 test-result fields and 24 device fields in §4 and §5. They belong to
Epic 2 F6 and Epic 3, and are **not** in the plan total. Questions A2c, A2d, A3, A6, A7, A8 and A9
below remain open.

---

## 1. Headline numbers

| | ALGO 5 XML | ALGO Pro JSON |
|---|---:|---:|
| Patient demographic fields | 43 | **54** (+11) |
| Risk factors | 16 | 16 |
| Test-result fields | 35 | **40** (+5) |
| Device/measurement fields per test | 47 | **46** (+5 new, −6 dropped) |

**Fields to add to AccuSync**

| Where | Count |
|---|---:|
| Patient level | **28** |
| Test-result level | **28** |
| Device / measurement level | **24** |
| **Total** | **80** |

None of these are on a screen today, and none are in `PatientData.cs` or `TestRecord.cs`.

---

## 2. Good news first — the risk factors match exactly

All 16 risk factor names in both sample files are character-for-character identical to the
16 already coded in AccuSync (`PdfParser.cs:505-521`, `Strings.Designer.cs`, and the Risk Factors tab):

Family History of Permanent Childhood Hearing Loss · Bacterial Meningitis · Craniofacial
Anomalies-Microtia/Atresia, Clefting · Hyperbilirubinemia with Exchange Transfusion · Low Birth
Weight · Ototoxic Medications · Perinatal or Postnatal Infection · Prolonged Ventilation ·
Asphyxia- Hypoxic Ischemic Encephalopathy (HIE) · Syndromes & Genetic Disorders of Hearing Loss ·
Aminoglycosides for >5 Days · In Utero Infections such as CMV & Zika · Caregiver Concern ·
**NICU >5 Days** · Exposure to Head Trauma or Chemotherapy · ECMO-Extracorporeal Membrane Oxygenation

**This settles an open question.** We had flagged that AccuLink uses 19 risk factors with a
**NICU >48 hours** threshold, while AccuSync uses 16 with **NICU >5 days**. The ALGO files confirm
AccuSync's list is the correct current one — it is the ALGO device list. AccuLink's 19 are the
legacy list. Nothing to change in AccuSync; the mismatch only matters when importing old
AccuLink data.

---

## 3. Patient fields — 28 to add

### 3.1 In both ALGO 5 and ALGO Pro, missing from AccuSync (18)

> **Rows 3–10 were dropped by Natus decision 1** — the baby uses the mother/caregiver address. **10 of these 18 are in scope.**

| # | ALGO field | Sample value | Why it is missing |
|---:|---|---|---|
| 1 | `AlsoKnownAs` | `Pat` | Exists on the list model `Patient.cs:AlsoKnownAs` but **not** in `PatientData.cs` and **not on any screen** |
| 2 | `Other` | `Other comments` | A second free-text box beside `Comment`. No equivalent |
| ~~3~~ | ~~~~ | `50 Commerce Dr` | AccuSync has `MotherAddress1` and `CaregiverAddress1` only — **the baby has no address of its own** |
| ~~4~~ | ~~~~ | `Suite 180` | Same |
| ~~5~~ | ~~~~ | `Schaumburg` | Same |
| ~~6~~ | ~~~~ | `Illinois` | Same |
| ~~7~~ | ~~~~ | `60173` | Same |
| ~~8~~ | ~~~~ | `United States` | Same |
| ~~9~~ | ~~~~ | `8475342150` | AccuSync has `MotherPhone` / `CaregiverPhone` only |
| ~~10~~ | ~~~~ | `8475342150` | Same |
| 11 | `ArchiveDate` | *(blank)* | No archive concept in AccuSync at all |
| 12 | `UserText4` | `User text 4` | Schema has `FreeText1-3` only — **2 short** |
| 13 | `UserText5` | `User text 5` | Same |
| 14 | `UserDate1` | `3-11-2026 5:53 PM` | **No date custom fields exist anywhere** |
| 15 | `UserDate2` | | Same |
| 16 | `UserDate3` | | Same |
| 17 | `UserDate4` | | Same |
| 18 | `UserDate5` | | Same |

### 3.2 ALGO Pro only, missing from AccuSync (10)

> **9 of these 10 are in scope.** `PatientPhysician` maps to the existing field.

| # | ALGO Pro field | Sample value | What it is |
|---:|---|---|---|
| 19 | `BirthOrder` | `1` | First, second, third of a multiple birth |
| 20 | `BirthType` | `Vag Single` | Delivery type |
| 21 | `BsPkuId` | `123123` | Newborn blood-spot / PKU screening ID — links hearing screening to the state metabolic screening program |
| 22 | `Insurance` | `Medicare` | Payer |
| 23 | `InsuranceType` | `Medicare` | Payer category |
| 24 | `MothersComments` | `Mother comment` | Free text specific to the mother |
| 25 | `MothersPhysician` | `Joseph` | Mother's doctor, distinct from the baby's |
| 26 | `ScreenLocation` | `WBN` | Well-Baby Nursery / NICU / etc. |
| 27 | `StageLevel` | `Inpatient` | Inpatient / Outpatient |
| ~~28~~ | ~~`PatientPhysician`~~ | `Joseph` | **Maps to the existing `Physician` column** (decision 2/3 — the baby keeps one field, relabelled **Physician / Pediatrician**). See §3.4 and question A2c |

`MothersEmail` is the only ALGO Pro addition AccuSync already has (`MotherEmail`).

### 3.3 AccuSync fields the ALGO files do not supply

These stay manual-entry — no import will ever fill them:

`GestationalAge` · `ScreeningConsent` · `ConsentState` · `TrackingConsent` · `NICU` · `Deceased` ·
`Title` (mother/caregiver) · **`SSN`** (mother/caregiver) · `MotherId` · `Language` · `Fax` ·
`AudiologyReferral` · `ReferralDate` · `ReferralTo` · `ReferralFrom` · `ReferralPhone` ·
`Audiologist` · `BirthWeight` · `TimeOfBirth`

> **Two of these connect to open questions.**
> **SSN** — neither ALGO file carries a Social Security Number. Combined with AccuLink having
> "Social No.", this confirms SSN is a manual field carried over from AccuLink, not device data.
> **Time of Birth** — not in either ALGO file either. It exists only on the AccuLink screen and on
> the `Patient.cs:BirthTime` list model.

### 3.4 ⚠ `Physician` means two different things — and the code already gets it wrong

**Natus has confirmed: AccuSync's `Physician` field is ALGO's `CurrentPediatrician`.**

That is a trap, because **ALGO also has a field literally called `Physician`**, and it is a
different person. In the ALGO Pro sample, four fields hold `Joseph` and one holds `Andrew`:

| ALGO field | Level | Sample value | Maps to |
|---|---|---|---|
| `CurrentPediatrician` | patient | `Joseph` | **AccuSync `Physician`** ✅ |
| `PatientPhysician` (Pro only) | patient | `Joseph` | ❌ nothing |
| `MothersPhysician` (Pro only) | patient | `Joseph` | ❌ nothing |
| `Pediatrician` | **test** | `Joseph` | ❌ nothing |
| `Physician` | **test** | **`Andrew`** | ❌ nothing |

Anyone mapping by name will wire ALGO's `Physician` into AccuSync's `Physician` and land the wrong
person in the record. **That is exactly what the code does today:**

- `AlgoProJsonParser.cs:135` correctly reads `p.Physician = Str(demo, "CurrentPediatrician")` …
- … then `AlgoProJsonParser.cs:150-151` **overwrites it** with the test-level `Physician`
  whenever that is non-empty. The pediatrician is read and silently thrown away.
- `Algo5XmlParser.cs:123` reads only the test-level `Physician` and ignores `CurrentPediatrician`
  entirely.

So both parsers currently put **Andrew** (the physician) into a box the screen labels
**Physician** but which Natus intends to hold **Joseph** (the pediatrician).

**Consequence for the plan:** the AccuSync screen label needs to say what it actually holds, and
four separate doctor fields need homes. Note the sample data uses the same value `Joseph` in four
places, so it cannot tell us whether `PatientPhysician`, `MothersPhysician` and `Pediatrician` are
genuinely distinct people in real use — that has to be confirmed (question A2).

---

## 4. Test-result fields — 28 to add

AccuSync's `TestRecord.cs` has 21 properties. ALGO's `TestResult` block has 35 (ALGO 5) / 40 (ALGO Pro).

### 4.1 Already covered

`Id` · `PatientId` · `TestType` · `LE_Result` / `RE_Result` → `TestResult` + `Ear` ·
`TestDateTime` → `TestDate` · `Duration` → `DurationMs` · `Examiner` · `TestFacility` ·
`Location` → `TestLocation` · `Comment` → Test Comments tab

### 4.2 Missing (28)

| Group | Fields | Count |
|---|---|---:|
| Clinical narrative | `Conclusion`, `Recommendation`, `Interpretation`, `TestInfo`, `Reason`, `Other` | 6 |
| People | `Pediatrician`, **`Physician`**, `ReferredBy`, `UserAccountId` | 4 |
| Place | `LocationType` | 1 |
| Lifecycle | `ArchiveDate`, `ExportDate` | 2 |
| Custom fields | `UserText1-5`, `UserDate1-5` | 10 |
| ALGO Pro only | `ReasonNotScreened` | 1 |
| ALGO Pro only — time zone | `TestDateTimeOffsetRegionId`, `TestDateTimeOffsetRegionName`, `TestDateTimeOffsetInMinutes`, `TestDateTimeOffsetInHours` | 4 |

**`Conclusion`, `Recommendation` and `Interpretation` are the significant ones.** These are the
clinician's written findings. AccuSync has nowhere to put them, and losing them on import would be
a clinical data loss, not a cosmetic gap.

**The four time-zone fields are also significant.** ALGO Pro records that a test happened at
`5:53 PM` **Eastern Standard Time, offset −300 minutes**. AccuSync stores a bare date/time string.
Import a test from a different time zone and it will display at the wrong time.

### 4.3 A structural mismatch — patient level vs test level

| Field | ALGO puts it on | AccuSync puts it on |
|---|---|---|
| `Medication` | **each test** | the **patient** (`PatientData.Medication`, Medical Information section) |
| `Physician` | **each test** | *(nowhere — the `Physician` box holds `CurrentPediatrician`, see §3.4)* |
| `Pediatrician` | **each test** | *(nowhere)* |

A patient with three screenings can have three different physicians and three different medications
in an ALGO file. AccuSync has one slot each. On import we would silently keep the last one and drop
the rest. **This needs a decision, not a mapping.**

---

## 5. Device and measurement fields — 24 to add

### 5.1 The ABR detail tables do not match

`Databases/PatientDatabase.sql` defines `ABRResults` with wave latencies (`WaveI_Latency_ms` …
`WaveVII_Latency_ms`), wave amplitudes, `EstimatedThreshold_dB`, `WaveformPointsJson` and
calibration blocks. That design came from **AccuLink's** binary blob.

ALGO 5 and ALGO Pro deliver a **completely different** set per ear:

`LikehoodRatio` · `TemplateShift` · `ABRSummaryData` (100 values) · `Sweeps` · `RejectedSweeps` ·
`BinoRan` (100 values) · `LikehoodRatioHistory` (30 values) · `LikehoodRatioNumberHistory` ·
`FullResult`

**There is zero overlap between the two lists.** The `ABRResults` table as designed cannot store an
ALGO test, and an ALGO test cannot fill a single one of its columns. Either a second detail table is
needed, or `ABRResults` has to be redesigned to hold both. This is a design decision that has to be
taken before Epic 2 F6 builds the test tables.

### 5.2 Device fields missing from `TestRecord.cs`

| Group | Fields | Count |
|---|---|---:|
| Per-ear ABR detail (§5.1) | 9 fields × 2 ears — modelled once | 9 |
| Ambient measurement | `AmbientNoise` | 1 |
| System info | `HWVersion`, `SWVersion`, `HwType`, `CompatibleHwType`, `ManufacturingDate` | 5 |
| Probe | `Version`, `CompatibleType`, `CalibrationData` | 3 |
| Test envelope | `RawDataId`, `TotalTestTime`, `ScreeningInterrupts` | 3 |
| ALGO Pro only — noise/impedance traces | `MyogenicNoiseValues`, `AmbientNoiseValues`, `ImpedanceVertexCommonValues`, `ImpedanceNapeCommonValues` | 4 |
| ALGO Pro only — battery | `SystemInfo.BatteryFWVersion` | 1 |

`ImpedanceVertexCommon` / `ImpedanceNapeCommon` map to AccuSync's `ImpedanceWhite` / `ImpedanceRed`
— **same reading, different name**. Confirm the pairing before wiring it up.

### 5.3 ALGO Pro drops the pre-amplifier

ALGO 5 carries a full `PreAmplifier` block — `Id`, `SerialNumber`, `Version`, `Type`,
`CompatibleType`, `ManufacturingDate`. **ALGO Pro has no pre-amplifier block at all.** Confirm this
is intended (different hardware) and not an omission from the export.

---

## 6. Three format problems that need fixing at source

These are defects in the sample files, not gaps in AccuSync.

### 6.1 ALGO Pro JSON repeats the same key — 15 of 16 risk factors are lost

```json
"RiskFactors" : {
  "RiskFactorName" : "Family History of Permanent Childhood Hearing Loss",
  "RiskFactorValue" : 1,
  "RiskFactorName" : "Bacterial Meningitis",
  "RiskFactorValue" : 1,
  ...
}
```

A JSON object cannot have two keys with the same name. Every standard parser — .NET, Java, Python,
JavaScript — keeps **only the last pair** and discards the rest. Parsed normally, this record has
**one** risk factor (`ECMO`), not sixteen.

This is not a parser we can work around cleanly; it is invalid data structure. **It should be an
array:**

```json
"RiskFactors" : [
  { "Name" : "Family History of Permanent Childhood Hearing Loss", "Value" : 1 },
  { "Name" : "Bacterial Meningitis", "Value" : 1 }
]
```

Since Haidee mentions the ALGO Pro format was recently updated, this may already be fixed — worth
confirming against the current format before we build against the sample.

### 6.2 ALGO Pro cannot tell an ABR test from a DPOAE test

ALGO 5 marks it: `<Plugin type="ABR">`. ALGO Pro has `"Plugin" : [ { ... } ]` with **no type
field**. The only hint is `TestResult.TestType`, which is a bare number — `7` in one sample record,
`1` in the others, with no key to what those mean.

GID-254900 requires importing ALGO Pro JSON, and AccuSync stores `TestType` as `'ABR' | 'TEOAE' |
'DPOAE'`. **We need the numeric code list**, or a type field added to the export.

### 6.3 The same value is encoded differently in the two formats

| Field | ALGO 5 | ALGO Pro |
|---|---|---|
| `Gender` | `1` | `Male` |
| `Method` | `LeftRightSimultaneous` | `1` |
| `Application` | `Amplitude35dBSPL` | `35 dB nHL` |
| `TestedEar` | `Left` / `Right` | `1` / `2` |
| `FullResult` | `Halted` | `11` / `4` |
| `TestType` | `1` | `7` / `1` |

Six fields where one format sends text and the other sends a number, in **opposite directions**. We
need the code tables for each. Guessing here produces silently wrong clinical records.

---

## 7. What this means for the plan

| Impact | Detail |
|---|---|
| **Epic 2 F1 (PM-01)** | 28 patient fields to add to `Patients` / `PatientContacts`, on top of the 6 already identified from AccuLink. The baby needs its own address and phone columns, which the current design does not have |
| **Epic 2 F6** | The `ABRResults` design (§5.1) cannot hold an ALGO test. Resolve before the table is built |
| **Epic 2 F8 / new** | Decide whether `Medication` / `Physician` / `Pediatrician` live on the patient or the test (§4.3) |
| **Epic 3 Import** | Blocked on the numeric code tables (§6.2, §6.3) and on the ALGO Pro risk-factor structure (§6.1) |
| **Epic 4 Export** | ALGO 5 XML export (GID-256285, F4) must write all 43 demographic + 35 test-result fields, not the subset AccuSync holds today |
| **Estimate** | Adding 80 fields across screens, models, tables and mapping is **not absorbed** by the current F1 and F6 estimates. Re-estimate once the decisions in §4.3, §5.1 and §6 are answered |

### 7.1 The patient fields are story PM-39 in Epic 2 F1

**One story, by request.** Epic 2 **F1 · PM-39 · Add the ALGO device fields** — 8.0 coding days,
**14.0 total days**, slice 2A, milestone M1. It is the largest story in the epic, because it covers
all 19 fields in one branch and one pull request. Every field touches three layers:

| Layer | What has to change |
|---|---|
| **Database** | New columns on `Patients`, `PatientContacts` and `TestRecords`, plus an EF Core migration |
| **Model** | New properties on `PatientData.cs` and `TestRecord.cs`, and on the DTOs and ViewModels above them |
| **Screen** | New controls, labels, validation and translated strings on the Patient Details, Additional Info and Test Results tabs |

Folding 80 fields into stories that are already sized for a defined field list would hide the work
and make the estimate wrong. Keeping them separate means each group can be scoped, estimated and
scheduled once Natus answers questions A1–A9 — and any group Natus drops simply does not get built.

**The 19 patient fields are in the plan** — Epic 2 is now 171.0 days and the plan is 722.0. **The 28
test-result fields and 24 device fields are not.** They belong to Epic 2 F6 and Epic 3 and cannot be
sized until questions A2d, A3 and A6 are answered.

---

## 8. Questions for Natus

| # | Question | Why it blocks us |
|---:|---|---|
| **A1** | Are all 28 patient fields and 27 test fields in scope, or only a subset? | DOC-076814 names **none** of them. GID-254883 lists four field *groups* and zero field names |
| **A2** | **Four doctor fields, one box.** You confirmed AccuSync `Physician` = ALGO `CurrentPediatrician`. That leaves ALGO's own `Physician` (test level, a different person), `Pediatrician` (test level), `PatientPhysician` and `MothersPhysician` with no home. Which do you need, and are they per patient or per test? | Both parsers currently overwrite the pediatrician with the physician (§3.4). `Medication` has the same patient-vs-test problem |
| **A3** | Numeric code tables for `Gender`, `Method`, `TestedEar`, `FullResult`, `TestType`, `LocationType` | Import cannot be written without them |
| **A4** | Is the ALGO Pro `RiskFactors` duplicate-key structure being fixed in the updated format? | Otherwise 15 of 16 risk factors are lost on every import |
| **A5** | How is an ABR test distinguished from a DPOAE test in ALGO Pro JSON? | No type field exists in the sample |
| **A6** | Should the `ABRResults` detail table hold ALGO's measurements, AccuLink's, or both? | The two are entirely disjoint |
| **A7** | Is the missing `PreAmplifier` in ALGO Pro intended? | ALGO 5 has six pre-amp fields; ALGO Pro has none |
| **A8** | Confirm `ImpedanceVertexCommon`/`ImpedanceNapeCommon` = `ImpedanceWhite`/`ImpedanceRed` | Electrode naming differs between the device and AccuSync |
| **A9** | Are `Insurance`, `InsuranceType` and `BsPkuId` needed in AccuSync? | These are billing / state-program identifiers, outside hearing screening |

---

*Prepared from `ALGO5_sample.xml` and `ALGOPro_sample.json` as provided on the ImportTest share,
checked against the AccuSync codebase at commit `cf608be`.*
