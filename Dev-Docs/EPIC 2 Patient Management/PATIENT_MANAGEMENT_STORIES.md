# ASWD-2 Patient Management — Features and User Stories

**For the JIRA board.** Epic → Feature → User Story. Every feature has a description and its own acceptance criteria; every story underneath has its own.

| | |
|---|---|
| Epic | **ASWD-2 Patient Management** |
| Features | **9** |
| User stories | **37** |
| Estimate | **96.1 developer-days** (85 days of coding, plus design, tests, review and rework) |
| Requirements covered | **44** |
| Milestones | **M1** (slices 2A, 2B) and **M2** (slices 2C, 2D) |
| Plan | [AccuSync-Delivery-Roadmap.md](../AccuSync-Delivery-Roadmap.md) |
| All feature criteria | [AccuSync-Feature-Acceptance-Criteria.md](../AccuSync-Feature-Acceptance-Criteria.md) |
| All epic criteria | [AccuSync-Epic-Acceptance-Criteria.md](../AccuSync-Epic-Acceptance-Criteria.md) |

---

## 1. How to read this

Three levels, each answering a different question.

| Level | Question | What you get here |
|---|---|---|
| **Feature** | Does this capability work? | A description, why it matters, and feature acceptance criteria |
| **Story** | Is this piece of work done? | The story, its acceptance criteria, and its requirement IDs |
| **Epic** | Is the whole area finished? | In the [epic criteria file](../AccuSync-Epic-Acceptance-Criteria.md) |

**A feature is done when all its stories are done *and* its own acceptance criteria hold.** The feature criteria are deliberately not just the sum of the story criteria — they check the capability end to end, which is where things usually fall over.

**Build in the order given.** Features are listed in build order, and stories within them. The order is not arbitrary; §3 explains where the dependencies are.

Estimates are **coding days only**. Add roughly 85% for design, unit tests, self-review, PR rework and merge.

---

## 2. Where this epic stands today

**Nothing in the patient area is saved.** The screens exist and look finished — `PatientsView.xaml.cs` alone is 1,350 lines — but the data lives in memory and disappears when the window closes.

| | |
|---|---|
| Patient tables in the database | **none** |
| Patient repository or service | **none** |
| Configuration screens that save anything | **none** — `FieldSetupConfigView.xaml.cs` says `// TODO: HandleSave has no actual persistence` |
| What "Save" does today | Shows a "Patient saved" message and writes nothing |

So this epic is not "wire up the existing screens". It is building everything underneath them, and rewriting most of the screen code so it can be tested.

---

## 3. The four slices

96.1 days is about 26 weeks. Too long to go without something to show, so the epic is delivered in four slices.

| Slice | Features | Stories | Coding | Total | Milestone | You can demonstrate |
|---|---|---|---|---|---|---|
| **2A · Patient records exist** | F1, F2 | PM-01–PM-09, **PM-40, PM-39** | 35.5 | **30.7** | M1 | Create, view, edit and delete a patient, **with every ALGO device field on the screen**. **It is still there after a restart.** Add/Edit/Delete and Save/Revert/Undo buttons work |
| **2B · The list is usable** | F3, F4, F5 | PM-10–PM-21 | 23 | **26.6** | M1 | The patient list with its six columns, search, the test list with its seven columns, validation, and permissions enforced |
| **2C · Configuration** | F6 | PM-22–PM-27 | 11 | **12.9** | M2 | An administrator can set the patient ID format, list sorting, confirmation prompts and custom field labels |
| **2D · Risk factors and comments** | F7, F8, F9 | PM-28–PM-38 | 15.5 | **25.9** | M2 | Risk factors and comments are lists an administrator manages, with translations, and they can be put on a patient. The patient report prints |
| | **9** | **37** | **85** | **96.1** | | |

### Why the order is what it is

**Tables sit with the feature that needs them** (decision D5). F1 builds the patient tables, F6 the test tables, F14 the field setup tables, F10 the risk factor table, F12 the comments table.

**The shared button framework (F2) is built here.** Add/Edit/Delete and Save/Revert/Undo are needed on twelve screens. This is the first screen that needs them, so they are built once here and every later epic adopts them.

**Inside F5: PM-16 → PM-18 → PM-20.** The field setup tables hold which fields are mandatory; the validation service reads them and is built inside PM-18, the workflow that first shows its results; the mandatory-field check calls that service. Get that order wrong and validation is built twice.

> **This ordering is a correction.** F14 and F19 were originally planned for slice 2C, one slice *after* the stories that call them. Writing these stories exposed it. They now sit in 2B. Slice sizes changed; the epic total did not.

**F10–F13 before F3 and F4.** You cannot pick from a list that does not exist.

---

## 4. Two things to settle before coding starts

### 4.1 Risk factors are designed twice

**Settle this before the report work in Epic 18 and F3.**

| In the code today | In the requirements |
|---|---|
| A **fixed 16-question form** on the patient screen. Set questions, Yes/No/Unknown, grouped Perinatal / Postnatal / Other, with a "12 of 16 answered" counter. 520 lines, and it works | A **list the administrator creates and edits** (GID-254950, GID-254951), with translations per language (GID-254954) |

They are incompatible. Add a risk factor under the requirements and **it will not appear on the patient screen**, because that screen does not read a list.

**Our recommendation:** build the configurable list, and keep the 16 existing questions as the default list that ships. They are standard newborn-screening clinical content and worth keeping.

### 4.2 Two bugs to fix inside this epic

Both sit inside GID-254883. They are defects in work this epic covers, not extra scope.

| | Bug | Why it matters | Fix in |
|---|---|---|---|
| **DF1** | 11 of the 12 clinical dropdowns bind `SelectedValue` without `SelectedValuePath` | The stored value is the list item object, not its text, so **patient demographic data does not survive a round trip** | PM-04 |
| **DF2** | Name capitalisation rewrites on every keystroke: O'BRIEN → O'brien, McDONALD → Mcdonald, and "Van Dyke" cannot be typed | Silently corrupting a family's surname in a medical record is worse than not helping | PM-04 |

---

## 5. Features and user stories


## Slice 2A — Patient records exist

*20.0 coding days · 37.5 total · milestone M1*

**At the end of this slice you can demonstrate:** Create, view, edit and delete a patient. **It is still there after a restart.** Add/Edit/Delete and Save/Revert/Undo buttons work.

---

### Feature F1 · Create, view, edit and delete a patient record

*29 coding days · 7 stories · slice 2A · milestone M1 · **requirements:** GID-254874, GID-254875, GID-254876, GID-254883, GID-254885, GID-254886, GID-254891, GID-255042, GID-255044*

**What it is.** The heart of the epic. A patient record that is genuinely stored: created, opened, changed and removed, with the mother's and caregiver's contact details alongside it. Everything else in Patient Management sits on top of this.

**Why it matters.** Today none of this is saved. `PatientsView.xaml.cs` is 1,350 lines and the data disappears when the window closes. This feature builds the tables, the repository, the service, and the DTO, converter and ViewModel that the other nine features reuse — all of it inside the workflows that need it, not as a layer of its own.

**Feature acceptance criteria**

1. A patient created in the application is still present after the application is closed and reopened.
2. The `Patients` and `PatientContacts` tables are created by a code-first migration, and a brand-new database is built correctly on first run.
3. Deleting a patient hides them from the list, from search and from test results, but the row is still in the database. **Any user can delete** — it is not restricted to administrators.
4. Every one of the 12 clinical dropdowns stores its value and shows it again correctly when the record is reopened (defect DF1).
5. Surnames are stored exactly as typed. O'Brien, McDonald, Van Dyke and hyphenated names all survive (defect DF2).
6. A screen never reports a successful save unless something was written.
7. The agreed patient field list is recorded before any table is built (open question Q16).
8. **Every field AccuLink holds exists in AccuSync**, or is listed as a recorded exclusion with a reason (PM-40). The comparison is complete before the first migration is written.
9. **All 19 ALGO device fields are on a screen and saved** (PM-39), and importing an ALGO 5 or ALGO Pro file no longer silently discards patient data.
9. **No patient demographic data reaches a log file.** A test logs from a fully populated patient record and asserts that none of its values — names, dates of birth, patient and hospital ID, addresses, phone numbers, email, both SSN fields, mother and caregiver details, free-text and custom fields — appear in the log output. **This is where GID-255034 is proven**, because this is the first point a patient record exists to test with. Logging itself is built earlier, in ASWD-26.
10. **The `Physician` import defect is fixed** — the field holds the pediatrician, not the test-level physician, and a regression test proves it.

**User stories**

| Story | Title | Days |
|---|---|---|
| PM-01 | Create the patient tables and repository | 3.0 |
| PM-03 | Add a new patient | 4.0 |
| PM-04 | View and edit a patient, and fix the two data bugs | 3.0 |
| PM-05 | Delete a patient (soft delete) | 1.5 |
| PM-06 | Save the patient's contact details | 1.5 |
| PM-40 | Add the AccuLink fields AccuSync is missing | 8.0 |
| PM-39 | Add the ALGO device fields | 8.0 |

#### PM-01 · Create the patient tables and repository

*3.0 days · requirements: GID-255042, GID-255044*

**Story.** As a developer, I want the `Patients` and `PatientContacts` tables with a repository and a service, so that patient information can be saved and read back.

**Fields — `Patients`** (the screening record)

`PatientId` · `PatientRecordNumber` · `HospitalId` · `SiteId` · `AssignedUserId` · `NicuStatus` · `GestationalAge` · `Discharged` · `Deceased` · `Medication` · `RaceReferenceId` · `ConsentState` · `ScreeningConsent` · `TrackingConsent` · `PatientRiskFactors` · `PredefinedComments` · `ReferralFrom` · `ReferralTo` · `ReferralPhone` · **`AudiologyReferral`** ⚠ · **`ReferralDate`** ⚠ · **`Physician`** ⚠ (holds ALGO's `CurrentPediatrician` — see A2) · **`Audiologist`** ⚠ · `FreeText1` · `FreeText2` · `FreeText3` · `FreeField1Value` · `FreeField2Value` · `FreeField3Value` · `FreeField4Value` · `IsDeleted` · `IsExported` · `ExportedAt` · `SourceId` · `ImportBatchId` · `SourceCreatedAt` · `SourceModifiedAt` · `CreatedAt` · `ModifiedAt`

**Fields — `PatientContacts`** (one row per person — baby, mother, caregiver, told apart by `ContactType`)

`ContactId` · `PatientId` · `ContactType` · `Title` · `Forename1` · `Forename2` · `Surname` · `SocialSecurityNumber` · `IdNumber` · `DateOfBirth` · `CalculatedDateOfBirth` · **`TimeOfBirth`** ⚠ · `Gender` · `Height` · `Weight` · `BirthLocation` · `LanguageCode` · `NationalityCode` · `Address1` · **`Address2`** ⚠ · `Zip` · `City` · `State` · `Country` · `Phone` · `CellPhone` · `Fax` · `Email` · `SourceId` · `SourceCreatedAt` · `SourceModifiedAt` · `CreatedAt` · `ModifiedAt`

⚠ **Six fields are not in the current schema design and must be added.** Five come from AccuLink (`TimeOfBirth`, `AudiologyReferral`, `ReferralDate`, `Physician`, `Audiologist`); one is an AccuSync addition (`Address2`).

> **⚠ A further 28 fields come from the ALGO device files.** `ALGO5_sample.xml` and
> `ALGOPro_sample.json` carry 43 and 54 patient fields respectively. 28 of them have no home in
> AccuSync — including the **baby's own address and phone** (today only the mother and caregiver
> have one), five `UserDate` custom fields, two more `UserText` fields, `CurrentPediatrician`,
> `BirthOrder`, `BirthType`, `BsPkuId`, `Insurance`, `ScreenLocation` and `StageLevel`.
> Full list and evidence: [ALGO5-ALGOPro-Field-Analysis.md](../ALGO5-ALGOPro-Field-Analysis.md).
> **This story is not sized for them.** They will be added as **separate user stories**, not folded
> into PM-01 — each field group needs a database migration, model and DTO changes, and new controls
> on the patient screens. Confirm scope with Natus (question A1) first; anything Natus drops is not
> built.

**Acceptance criteria**

1. A `Patients` table and a `PatientContacts` table exist in the **patient database**, created by a code-first migration, with all the fields listed above **including the six marked ⚠**, plus whichever of the 28 ALGO fields Natus confirms are in scope.
2. `PatientContacts` holds one row per person, with `ContactType` distinguishing the baby, the mother and the caregiver. A patient can have a baby row and no mother or caregiver row.
3. The tables go in the **patient database**, per Natus 22 Aug 2026: *"Patient database contain all patient information and test data — patient details, risks, comments, test data/details. Settings database contain all configurations."* The allocation is recorded in the ASWD-77 design document. Note the split: the **risk factor and comment lists** are configuration and live in the settings database, while the **risks and comments assigned to a patient** are patient data and live here.
4. Every patient record has an `IsDeleted` flag, defaulting to not deleted, so that soft delete works later.
5. A patient repository can create, read, update and soft-delete a patient, and can read a list.
6. A patient service sits above the repository and is the only thing the screens talk to.
7. Every read filters out soft-deleted records unless it is explicitly asked not to, and deleting a patient hides their contact rows too.
8. `SocialSecurityNumber` is never written to a log line, from the first day the column exists.
9. Unit tests cover create, read, update, soft-delete, the "deleted records are hidden" rule, and a patient with only a baby row.

> **`RaceReferenceId` is confirmed and stays.** Natus, 22 Aug 2026: *"It is in AccuLink — there is a Race dropdown in patient details and it's also in the AccuLink export file (`<RaceReferenceId xsi:nil="true" />`)."* Our earlier recommendation to drop it was wrong — the AccuLink screenshots we were given did not cover it.
>
> **And a scoping rule that reaches past this story.** Natus also said: *"I didn't add all fields to match AccuLink. Use AccuLink as reference for fields to display and add the additional from ALGO 5 and ALGO Pro."* So **AccuLink is the baseline field set**, the schema design is known to be short of it, and ALGO 5 / ALGO Pro add to it. The six fields already flagged ⚠ above are the ones we have found so far; **there may be more, and a field-by-field pass against AccuLink is needed before the migration is written.**

#### PM-03 · Add a new patient

*4.0 days · requirements: GID-254883, GID-254874*

**Story.** As a screener, I want to add a new patient record, so that I can record a baby's details before screening them.

**Acceptance criteria**

1. Pressing **Add** opens a blank patient form.
2. The form collects patient details, caregiver details, medical details and consent information.
3. Pressing **Save** writes the record to the database.
4. **The record is still there after closing and reopening the application.**
5. If saving fails, the user is told, and the entered data stays on screen so nothing is lost.
6. A success message only appears if the record was actually saved. *(Today the screen says "Patient saved" and writes nothing.)*
7. **This is the first patient workflow built, so it also creates the shared shapes the rest of the feature reuses:** a patient DTO in `AccuSync.Presentation` with the agreed fields, a converter both ways between entity and DTO, and a patient ViewModel built on the Epic 0 base classes.
8. **No database entity is ever stored on a ViewModel property or bound to a screen.** The converter takes an entity in and gives a DTO back; the entity is not kept.
9. `AccuSync.Presentation` still has no reference to WPF.
10. Unit tests cover a successful save, a failed save, saving with only the mandatory fields filled, and the conversions both ways including empty and missing values.

> **PM-04, PM-05 and PM-06 reuse what this story builds.** The DTO, the converter and the ViewModel are written once, here. That is why viewing, editing and deleting cost less than adding, and why there is no separate story for the presentation layer — a layer is not a workflow.

#### PM-04 · View and edit a patient, and fix the two data bugs

*3.0 days · requirements: GID-254891, GID-254885, GID-254875*

**Story.** As a screener, I want to open a patient from the list and see and change their details, so that I can correct mistakes and add information as it arrives.

**Acceptance criteria**

1. Selecting a patient in the list opens their record with every saved value shown.
2. Pressing **Edit** makes the fields editable.
3. Changes are saved to the database and survive a restart.
4. **DF1 fixed:** all 12 dropdowns (gender, birth location, nationality, screening consent, consent state, NICU, tracking consent, mother language, mother country, caregiver language, caregiver country, gestational age) save the chosen value and show it again correctly when the record is reopened.
5. **DF2 fixed:** names are no longer rewritten as you type. O'Brien, McDonald, Van Dyke and hyphenated surnames can all be typed and are stored exactly as entered.
6. Unit tests cover a full round trip: save every field, reopen, and confirm every value matches — including all 12 dropdowns and the four surname formats above.

#### PM-05 · Delete a patient (soft delete)

*1.5 days · requirements: GID-254886, GID-254876*

**Story.** As a user, I want to delete a patient record, so that records created in error do not stay in the system.

**Acceptance criteria**

1. **Any user with access to the patient screen can delete a patient.** Delete is not restricted to administrators — see the note below.
2. Deleting asks for confirmation first, naming the patient.
3. Deleting sets the record to deleted. **The row is not removed from the database** (decision D3).
4. Deleted patients disappear from the patient list and from search results.
5. Test results belonging to a deleted patient are also hidden.
6. The record can still be found in the database, so audit history stays intact when audit logging is added later.
7. Unit tests cover: a delete by an ordinary user and by an administrator both succeed, deleted patients are hidden everywhere, and the row still exists.

> **Why delete is open to every user.** GID-254886 says the software *"shall allow administrative users to delete a patient record"*. That grants a permission; it does not forbid anyone else. Where the SRS means to forbid, it says **prevent** — as in GID-255021, GID-254977 and GID-254983. Separately, GID-254876 allows *"the user"* to delete a record on every screen listed in GID-254877, and Patient is one of them. **Customer decision: patient delete behaves the same for users and administrators.**

#### PM-06 · Save the patient's contact details

*1.5 days · requirements: GID-254883*

**Story.** As a screener, I want the mother's and caregiver's contact details saved with the patient, so that we can reach the family if the baby needs follow-up.

**Acceptance criteria**

1. Mother and caregiver details are saved in `PatientContacts` and linked to the patient.
2. All agreed contact fields save and reload correctly.
3. A patient can be saved with no caregiver details.
4. Deleting a patient hides their contacts too.
5. Unit tests cover a patient with both contacts, with only a mother, and with neither.

> **Note:** failing to reach families is the main reason babies are lost to follow-up after a referral, so these fields matter clinically. The exact field list depends on **Q16** — the code today includes Social Security Numbers, which no requirement asks for and which carry privacy weight.

#### PM-40 · Add the AccuLink fields AccuSync is missing

*8.0 days · requirements: GID-254883 · **run this before PM-01 writes the migration***

**Story.** As a screener moving from AccuLink, I want every field AccuLink holds to exist in AccuSync, so that nothing I record today is lost when the product changes.

**Why this story exists.** Natus set the field scope on 22 August 2026: *"I didn't add all fields to match AccuLink. Use AccuLink as reference for fields to display and add the additional from ALGO 5 and ALGO Pro."* So AccuLink is the baseline, and the schema design is **known** to be short of it.

**The gaps come from two places, and both are AccuLink.**

**1. AccuLink's own patient field set.** The lead is aligning this with the Natus team directly. **That comparison, and the sign-off on any field Natus does not want, are not development work and are not costed here.** Six are confirmed so far: `TimeOfBirth`, `AudiologyReferral`, `ReferralDate`, `Physician`, `Audiologist`, `Address2`. `RaceReferenceId` is the warning about how these were found — we recommended dropping it because it was on no screen we had seen, and it turned out to be AccuLink's Race dropdown.

**2. AccuLink's data-exchange plugins.** Installing the HiTrack and Australia plugins surfaced seven more fields, plus one existing field that has to change. These are **fully enumerated and decided** — nothing is waiting on the alignment:

| Group | Field | List behaviour |
|---|---|---|
| Patient | `BirthOrder` | Fixed list: Single, Multiple A … Multiple H |
| Patient | `Nursery` | **Empty until an import fills it** |
| Patient | `Race` | **Empty until an import fills it** |
| Patient | `IndigenousStatus` | **Fixed list, held as an enum in code**: Aboriginal · Torres Strait Islander · Aboriginal and Torres Strait Islander · Neither Aboriginal nor Torres Strait Islander · Not Stated |
| Mother | `MothersRace` | **The same shared list as the patient's `Race`** |
| Mother | `Ethnicity` | Fixed list: Hispanic, Not Hispanic, Unknown |
| Mother | `Education` | Fixed list: High School, Below High School, College, Some Degree, Unknown |
| Patient | `BirthLocation` ⚠ *changes* | Existing field. Becomes a combo box over the facility list, and accepts a typed value |

**What Natus decided, 26 August 2026** (Haidee Kachniewicz, answering all three questions):

1. **Combo boxes, not plain dropdowns**, for Nursery, Race, Birth Location and Indigenous Status — **"Yes."**
   - ⚠ **Amended by the lead on 27 Aug 2026: `IndigenousStatus` is an enum, not a combo box.** Its five values are fixed in code, so there is no list to seed and **a user cannot type a sixth value**. It joins Birth Order, Ethnicity and Education as a plain dropdown. **This narrows what Natus approved and should be told to them** — the combo box was offered so a screener could record a value the list did not have. For Nursery, Race and Birth Location, where the list arrives by import, the combo box stands.
2. **A typed value is saved on that patient only.** It never joins the shared list — **"Save it on the patient only."**
3. **Race is one shared list**, used by both the baby and the mother — **"One shared list."**

**Acceptance criteria**

1. Every field — the seven above and every field the alignment hands over — exists as an entity property with its EF Core configuration.
2. **All of them are in PM-01's migration**, not a later one. That is why this story runs first.
3. Each field has its DTO property and its mapping both ways in the converter.
4. Each field has its control, label and binding on the right screen and in the right section.
5. The four fixed lists — `BirthOrder`, `Ethnicity`, `Education` and `IndigenousStatus` — are **enums in code**, hold exactly the values above, and are not editable by a user or by an import.
6. `IndigenousStatus` is an **enum in code** holding exactly those five values. There is no lookup table, nothing to seed, and no way for a user or an import to add a sixth value. If Australia changes the list, that is a code change and a migration.
7. `Nursery` and `Race` are **editable combo boxes**. On a fresh install their lists are empty, and **the screener can still type a value and save it** — in AccuLink these are empty dropdowns and the screener is stuck until someone runs an import.
8. **A typed value is stored on that patient only and never joins the shared list.** A typo cannot become a permanent list entry for the whole site.
9. Administrators manage the shared lists. Screeners do not.
10. **`Race` and `MothersRace` read from one shared list.** Two lists would drift apart.
11. `BirthLocation` becomes a combo box over the facility list from Sites and Facilities, and still accepts a typed location that is not a facility.
12. ⚠ **`BirthOrder` is reconciled with PM-39's ALGO `BirthOrder`** — one field, one value list, not two fields with the same name. HiTrack expects Single and Multiple A–H; if ALGO sends something different, the mapping is written down.
13. Where an AccuLink field is the same thing as an ALGO or HiTrack field under another name, it is built once, not twice. **`RaceReferenceId` and HiTrack's `Race` are the first such pair.**
14. Unit tests cover save and reload for every field added, a typed value that is not in the list, and that a typed value does not appear for the next patient.

> **The shared lists are filled by the import, which is [ASWD-32 F6](../AccuSync-Delivery-Roadmap.md).** AccuLink's HiTrack **pick-list importer** brings in exactly these lists — hospitals, physicians, audiologists, screeners, nursery types and race types. **Natus supplied sample HiTrack pick-list files on 26 Aug 2026.** This story builds the fields and the combo-box behaviour; ASWD-32 F6 fills the lists. Neither blocks the other, which is the whole point of the combo box.

> **What this story is priced at, and what changes it.** **30 development hours** — 10 core, 8 ViewModel and DTO, 12 screen — set by the lead, the same basis as PM-39. **22 fields plus one changed field**, so about **1.4 hours a field**: 15 from the AccuLink field set (six confirmed, nine an allowance until the alignment lands) and seven from the data-exchange plugins. Plus three list sources to create and seed, the editable-combo-box behaviour built once and reused by four fields, and the `BirthOrder` reconciliation. **Merged from two stories** — one story means one design and verification pass rather than two. **Each field the alignment adds or drops moves this by about an hour and a half.** The 8.0 days above no longer drives the figure; it stays as the record of the work.

---

#### PM-39 · Add the ALGO device fields

*8.0 days · requirements: none — no requirement in DOC-076814 names any of these fields*

**Story.** As a screener, I want every field the ALGO 5 and ALGO Pro devices send to have a place in AccuSync, so that importing a screening record does not silently throw patient data away.

**Scope confirmed by Natus** (Haidee Kachniewicz, 18 Aug 2026): **19 new fields**, plus one change to a field that already exists. The baby's own address, city, state, postal code, country, telephone and mobile were **dropped** — the mother's or caregiver's address is used instead. Evidence and the full field-by-field analysis: [ALGO5-ALGOPro-Field-Analysis.md](../ALGO5-ALGOPro-Field-Analysis.md).

**Fields to add (19)**

| Group | Fields | Count | Screen |
|---|---|---:|---|
| Custom text and dates | `UserText4` · `UserText5` · `UserDate1` · `UserDate2` · `UserDate3` · `UserDate4` · `UserDate5` | 7 | new **Custom Fields** section, Additional Info tab |
| Names and notes | `AlsoKnownAs` · `Other` · `MothersComments` | 3 | Patient Details (nickname, notes) and Mother Information |
| Birth details | `BirthOrder` · `BirthType` ⚠ | 2 | Patient Details, beside Gestational Age |
| Screening context | `ScreenLocation` ⚠ · `StageLevel` ⚠ | 2 | Patient Details, near NICU and Discharged |
| Insurance and program | `Insurance` · `InsuranceType` · `BsPkuId` | 3 | new **Program and Billing** section, Additional Info tab. All three **confirmed wanted** by Natus, 22 Aug 2026 |
| Mother's doctor | `MothersPhysician` | 1 | Mother Information, with its own label |
| Archive | `ArchiveDate` | 1 | Patient Details, read-only, hidden when empty |

**Three dropdowns, and their values are now known** — Natus, 22 Aug 2026:

| Field | Values |
|---|---|
| `BirthType` | Vag Single · Vag Multiple · Vag Triple · Ceasection Single · Ceasection Multiple · Ceasection Triple |
| `ScreenLocation` | WBN · NICU · OP |
| `StageLevel` | Inpatient · Outpatient · Readmission |

> **Why this story carries 30 development hours and not 8.** The one-day cap on a sub-task exists so no single line of the breakdown hides a week of work. Field-bulk stories are the exception: **19 fields is the same small job 19 times**, so the volume is real even though each piece is small. **The hours are set by the lead — 10 core, 8 ViewModel and DTO, 12 screen** — which works out at roughly **1.1 hours a field** once the dropdowns and the two new screen sections are allowed for. Neither the cap nor the reuse discount is applied, for the same reason. **The 8.0 days above no longer drives this figure**; it stays as the record of the work, the way Epic 0’s day figures do.

**`InsuranceType` is not a dropdown** — Natus confirmed it is **single-line text**. That leaves three coded dropdowns in this story, not four.

**Field to change (1).** `Medication` becomes **multi-line text**. Natus chose this over a configurable list, because a list would need a whole configuration screen for a field no requirement mentions.

**Database.** `Patients` gains 15 columns — the 7 custom fields, `Other`, `BirthOrder`, `BirthType`, `ScreenLocation`, `StageLevel`, `Insurance`, `InsuranceType`, `BsPkuId`, `ArchiveDate`. `PatientContacts` gains `AlsoKnownAs` (baby row) and `MothersComments` and `MothersPhysician` (mother row). `Medication` needs no schema change — it is already `TEXT`. All by one code-first migration.

**Screen.** Two new sections on the Additional Info tab (**Custom Fields**, **Program and Billing**), six fields added to existing sections, the **Physician** label renamed **Physician / Pediatrician**, and the Medication box changed from single-line to multi-line.

**Acceptance criteria**

1. All 19 fields exist in the database, on `PatientData.cs`, on the DTO and ViewModel above it, **and** on a screen. No field is added in one layer and missing from another.
2. Every one of the 19 round-trips: type a value, save, close the application, reopen — the value is still there.
3. A brand-new database is built correctly on first run with all 19 fields present.
4. Every new field has a **translated label**. No hardcoded English string reaches a screen.
5. Every new field can be left empty. None is mandatory by default.
6. Every new field is registered in the field setup tables, so PM-22 can later make it mandatory or hide it.
7. The **three** new dropdowns — `BirthType`, `ScreenLocation`, `StageLevel` — store a **code** and display a translated label, using the values Natus gave on 22 Aug 2026. Same pattern as the other 12 clinical dropdowns, so defect DF1 does not recur on any of them.
7a. **`InsuranceType` is a single-line text box, not a dropdown** (Natus, 22 Aug 2026).
8. `BirthOrder` accepts whole numbers from 1 upward and rejects anything else with a message.
9. The five custom date fields use the same date picker and **Today** button as Date of Birth. An invalid date is rejected with a message naming the field.
10. A custom field with no configured label is **hidden**, not shown blank. The labels are the ones F20 renames, so both features read the same configuration.
11. `Other`, `MothersComments` and `Medication` are multi-line, accept at least 500 characters, keep their line breaks after save and reload, and use the same character counter as the existing Comments box.
12. An existing single-line `Medication` value still displays correctly after the change.
13. `AlsoKnownAs` is searchable in the same way First Name and Last Name are (PM-11).
14. The Medical Information label reads **Physician / Pediatrician** in every supported language, and that field stores ALGO's `CurrentPediatrician` — **not** ALGO's test-level `Physician`.
15. **Mother's Physician** is a separate field with its own label in Mother Information, saved to the mother's contact row, never merged with the baby's field.
16. **The `Physician` import defect is fixed.** A regression test runs `ALGOPro_sample.json` and asserts the field holds `Joseph`, not `Andrew`. Both parsers are covered by a test using a file where the two names differ.
17. `ArchiveDate` is read-only, shown only when it has a value, and never editable by hand. Archived patients still appear in the patient list — **archiving is not deleting** (PM-05).
18. `BsPkuId` is treated as an identifier that can identify a person, so it is stripped from de-identified export alongside SSN.
19. No new field is written to a log line.
20. Importing `ALGO5_sample.xml` and `ALGOPro_sample.json` fills every field those files carry, and a test asserts it.
21. Unit tests cover save and reload for all 19 fields.

> **This story fixes a live defect.** `AlgoProJsonParser.cs:135` reads `CurrentPediatrician` into `Physician`, then `AlgoProJsonParser.cs:150-151` **overwrites it** with the test-level `Physician` whenever that has a value. `Algo5XmlParser.cs:123` never reads `CurrentPediatrician` at all. Both parsers show the wrong doctor today, and the ambiguous label is why it went unnoticed.

> **⚠ At 8.0 days this is the largest story in the epic** — more than twice PM-01. It is one story by request, so it will be one branch and one pull request covering a migration, three model layers and two screens. **If the PR turns out to be too large to review properly, the natural split is by the seven groups in the field table above.** Recorded so the choice is visible, not to reopen it.

> **✅ A3 and A9 were answered on 22 Aug 2026.** All three dropdown value lists are given above, `InsuranceType` is text rather than a dropdown, and `Insurance` / `InsuranceType` / `BsPkuId` are all wanted — so nothing comes out of this story. **Two questions remain:**
> **A2c** — ALGO Pro sends both `CurrentPediatrician` and `PatientPhysician`, both `Joseph` in the sample. AccuSync has one field for the baby's doctor and the confirmed mapping is `CurrentPediatrician`. If a real file has them differing, the import must flag it, not silently pick one.
> **A2d** — ALGO sends `Medication` on each **test**; AccuSync holds one value on the **patient**. Not fixed by this story.

> **Not in this story.** The **28 test-result fields** and **24 device fields** the ALGO files also carry. Those belong to F6 and Epic 3 and are not yet scoped or estimated.

---

---

### Feature F2 · Shared Add/Edit/Delete and Save/Revert/Undo framework

*6.5 coding days · 3 stories · slice 2A · milestone M1 · **requirements:** GID-254873, GID-254874, GID-254875, GID-254876, GID-254877, GID-254878, GID-254879, GID-254880, GID-254881, GID-254882, GID-255009*

**What it is.** One set of Add, Edit, Delete, Save, Revert and Undo behaviours, built once here and reused by every other screen in the product.

**Why it matters.** The requirements name twelve screens that must have these controls (GID-254877, GID-254882). Today six screens each carry their own copy of undo logic. This is the first screen that needs them, so they get built properly here rather than a thirteenth time later.

**Feature acceptance criteria**

1. Add, Edit, Delete, Save, Revert and Undo all work on the patient screen and are driven by shared code, not code local to that screen.
2. Undo can be pressed repeatedly, stepping back one change at a time to the last saved state.
3. The buttons are greyed out when they do not apply, rather than shown and then refused.
4. Closing a window or navigating away with unsaved changes offers save, discard or cancel, and cancel keeps the work.
5. The commands are wired to the existing ribbon toolbar, which already renders and routes correctly.
6. Another epic can adopt this framework with a single small story and delete its own copy.

**User stories**

| Story | Title | Days |
|---|---|---|
| PM-07 | Build the shared Add, Edit and Delete buttons | 2.0 |
| PM-08 | Build the shared Save, Revert and Undo | 3.0 |
| PM-09 | Warn before losing unsaved changes | 1.5 |

#### PM-07 · Build the shared Add, Edit and Delete buttons

*2.0 days · requirements: GID-254873, GID-254874, GID-254875, GID-254876, GID-254877*

**Story.** As a developer, I want one shared set of Add, Edit and Delete commands, so that all 12 screens behave the same way and the code is written once.

**Acceptance criteria**

1. Shared Add, Edit and Delete commands live in `AccuSync.Presentation` and any screen can use them.
2. **Add** opens a blank form. **Edit** opens the selected record for editing. **Delete** asks for confirmation before removing anything.
3. Buttons are greyed out when they do not apply — for example Edit and Delete with nothing selected.
4. The commands are wired to the existing ribbon toolbar buttons, which already render and route correctly.
5. The patient screen uses the shared commands, not its own.
6. Unit tests cover each command, the enabled/disabled rules, and cancelling a delete.

#### PM-08 · Build the shared Save, Revert and Undo

*3.0 days · requirements: GID-254878, GID-254879, GID-254880, GID-254881, GID-254882*

**Story.** As a user, I want Save, Revert and Undo on data entry screens, so that I can correct mistakes without losing my work.

**Acceptance criteria**

1. **Save** writes all changes to the database.
2. **Revert** puts everything back to the last saved state.
3. **Undo** reverses only the most recent change.
4. Undo can be pressed repeatedly, stepping back one change at a time to the last saved state.
5. Save, Revert and Undo are greyed out when there are no unsaved changes.
6. The screen shows clearly when there are unsaved changes.
7. This replaces the per-screen copies that exist today, rather than adding a thirteenth.
8. Unit tests cover: change then save, change then revert, several changes then repeated undo, and undo with nothing to undo.

#### PM-09 · Warn before losing unsaved changes

*1.5 days · requirements: GID-255009*

**Story.** As a user, I want a warning before I close a window or move away with unsaved changes, so that I do not lose work by accident.

**Acceptance criteria**

1. Closing a window with unsaved changes asks whether to save, discard, or cancel.
2. Moving to another screen with unsaved changes asks the same.
3. **Cancel** leaves the user where they were with their changes intact.
4. **Save** saves and then continues. **Discard** throws the changes away and continues.
5. No warning appears when there is nothing unsaved.
6. Unit tests cover all three answers, and the no-changes case.

---


## Slice 2B — The list is usable

*23.5 coding days · 44.0 total · milestone M1*

**At the end of this slice you can demonstrate:** The patient list with its six columns, search, the test list with its seven columns, validation, and permissions enforced.

---

---

### Feature F3 · Show and search the patient list

*5 coding days · 2 stories · slice 2B · milestone M1 · **requirements:** GID-254884, GID-254887, GID-254890*

**What it is.** The screen a screener spends most of their day on: every patient in the system, and a way to find one quickly.

**Why it matters.** The list is the entry point to every other patient action. It must load real saved data and stay usable on a busy ward.

**Feature acceptance criteria**

1. The list shows real saved patients from the database. No sample data written into the code remains.
2. All six columns required by GID-254890 are present: Patient ID / Hospital ID, Last Name, First Name, Date of Birth, Risk, Comment.
3. A patient can be found by patient ID, first name, last name, date of birth, or a test date range.
4. Searching is not case sensitive and partial names match.
5. Deleted patients never appear in the list or in search results.
6. The list stays responsive with 10,000 patients.

**User stories**

| Story | Title | Days |
|---|---|---|
| PM-10 | Show the patient list | 2.5 |
| PM-11 | Search the patient list | 2.5 |

#### PM-10 · Show the patient list

*2.5 days · requirements: GID-254884, GID-254890*

**Story.** As a screener, I want a list of all patients, so that I can find the baby I need to work on.

**Acceptance criteria**

1. The list shows every saved patient who is not deleted.
2. Each row shows all six required columns: **Patient ID / Hospital ID, Last Name, First Name, Date of Birth, Risk, Comment**.
3. The list loads from the database, not from a hardcoded list.
4. An empty database shows a clear "no patients yet" message, not a blank screen.
5. The list stays responsive with 10,000 patients.
6. Selecting a row opens that patient (PM-04).
7. Unit tests cover an empty list, one patient, many patients, and that deleted patients are hidden.

#### PM-11 · Search the patient list

*2.5 days · requirements: GID-254887*

**Story.** As a screener, I want to search for a patient, so that I can find one baby quickly on a busy ward.

**Acceptance criteria**

1. The user can search by **patient ID, first name, last name, date of birth, or a test date range**.
2. The list narrows to matching patients as the search is typed.
3. Searches are not case sensitive, and partial names match.
4. Clearing the search shows the full list again.
5. No matches shows a clear "nothing found" message.
6. Deleted patients never appear in results.
7. Unit tests cover each of the five search types, a partial match, no match, and clearing the search.

---

---

### Feature F4 · Show, delete and reassign test results, with permissions enforced

*11.5 coding days · 5 stories · slice 2B · milestone M1 · **requirements:** GID-254885, GID-254886, GID-254895, GID-254896, GID-254897, GID-255042, GID-255044*

**What it is.** The screening results recorded against a patient, and the two administrative corrections that go with them: removing a test entered in error, and moving a test to the right baby. Limiting what a user can do to a patient, based on the permissions their profile grants. Note that **patient delete is deliberately not limited** — see PM-05.

**Why it matters.** This feature also builds the `TestSessions` and `TestRecords` tables, which the OAE and ABR result views (ASWD-5, ASWD-6), Import, Export and Report Generation all depend on. Nine requirements across this epic say “administrative users”, and GID-254885 says a patient may be edited “unless locked by permissions”. This feature is where that is enforced.

**Combined from 2 earlier features** on 19 Aug 2026, because each was only story-sized on its own.

**Feature acceptance criteria**

1. A patient's tests are listed newest first, with all seven columns required by GID-254897.
2. The tables are created by migration and a test record is linked to exactly one patient.
3. An administrator can delete a single test; other tests and the patient record are untouched.
4. An administrator can move a test to a different patient, and it disappears from the old one.
5. A test cannot be moved to a deleted patient.
6. Deleting a patient hides their tests.
7. A patient with no tests shows a clear message rather than an empty panel.
8. A user without edit permission can open a patient but cannot change anything.
9. Only administrators can delete a test or reassign a test. **Patient delete is open to every user**, by customer decision.
10. Actions a user cannot perform are hidden or greyed out, not shown and then refused.
11. Permission is checked in the service layer as well as on screen, so hiding a button is not the only protection.
12. When ASWD-8 lands, the temporary role check is replaced by real profile permissions with no change to these behaviours.

**User stories**

| Story | Title | Days |
|---|---|---|
| PM-12 | Create the test result tables | 2.5 |
| PM-13 | Show the patient's test list | 2.5 |
| PM-14 | Delete one test entry | 2.0 |
| PM-15 | Move a test to the correct patient | 2.5 |
| PM-21 | Enforce patient permissions | 2.0 |

#### PM-12 · Create the test result tables

*2.5 days · requirements: GID-255042, GID-255044*

**Story.** As a developer, I want `TestSessions` and `TestRecords` tables, so that screening results can be stored against a patient.

**Acceptance criteria**

1. `TestSessions` and `TestRecords` tables exist in the patient database, created by a migration.
2. Every test record links to a patient.
3. A test record stores test type, which ear, the result, the date and time, how long it took, and who ran it.
4. Deleting a patient hides their tests.
5. A repository can read all tests for a patient, add a test, and delete one test.
6. Unit tests cover reading, adding and deleting, and that a deleted patient's tests are hidden.

#### PM-13 · Show the patient's test list

*2.5 days · requirements: GID-254897*

**Story.** As a screener, I want to see all tests recorded for a patient, so that I can review what screening has been done.

**Acceptance criteria**

1. Opening a patient shows their tests, newest first.
2. Each test row shows all seven required columns: **Test Type, Left Ear Result, Right Ear Result, Date/Time of Test, Test Configuration, Duration, Examiner**.
3. "Test Configuration" shows the name of the protocol used.
4. A patient with no tests shows a clear message.
5. Unit tests cover no tests, one test, and many tests.

> **Depends on ASWD-13 and ASWD-14** for protocol names. Those epics run later (weeks 86–90). Until then, show the stored protocol identifier and swap in the name when the protocol epics land.

#### PM-14 · Delete one test entry

*2.0 days · requirements: GID-254895*

**Story.** As an administrator, I want to delete a single test from a patient's list, so that a test recorded in error can be removed.

**Acceptance criteria**

1. Only an administrator sees the delete option on a test row.
2. Deleting asks for confirmation, naming the test and its date.
3. The test disappears from the patient's list.
4. The patient record and their other tests are untouched.
5. Unit tests cover: an administrator can delete, a non-administrator cannot, and other tests are unaffected.

#### PM-15 · Move a test to the correct patient

*2.5 days · requirements: GID-254896*

**Story.** As an administrator, I want to move a test result to a different patient, so that a test recorded against the wrong baby can be corrected.

**Acceptance criteria**

1. Only an administrator can reassign a test.
2. The administrator picks the correct patient from a searchable list.
3. Confirmation shows both the old and the new patient before the change is made.
4. After the move, the test appears under the new patient and is gone from the old one.
5. The test data itself is unchanged — only which patient it belongs to.
6. A test cannot be moved to a deleted patient.
7. Unit tests cover a successful move, cancelling, and the deleted-patient rule.

---

#### PM-21 · Enforce patient permissions

*2.0 days · requirements: GID-254885, GID-254886*

**Story.** As an administrator, I want patient actions limited by the user's permissions, so that people can only do what their role allows.

**Acceptance criteria**

1. A user without edit permission can open a patient but not change anything — GID-254885's "unless locked by permissions".
2. Deleting a test (PM-14) and reassigning a test (PM-15) are limited to administrators. **Deleting a patient (PM-05) is not** — it is open to every user, by customer decision.
3. Actions the user cannot perform are hidden or greyed out, not shown and then refused.
4. Permission is checked in the service layer as well as on screen, so hiding a button is not the only protection.
5. Unit tests cover each permission for an allowed user and a blocked user, including the service-layer check.

> **Depends on ASWD-8 Profile Management** (weeks 66–70) for real database-driven permissions. Until then, use the existing role check and swap it for profile permissions when ASWD-8 lands. This is a known temporary step, not an oversight.

---


---

### Feature F5 · Store the field setup, validate input and enforce mandatory fields

*6.5 coding days · 4 stories · slice 2B · milestone M1 · **requirements:** GID-254892, GID-254893, GID-254960, GID-254966, GID-255012*

**What it is.** The two tables that hold an administrator's choices about patient data entry: which fields exist, which are mandatory, what they are called, and the application-wide settings. One service that decides whether a patient record may be saved, and tells the user exactly what is wrong when it may not. Showing a screener which fields they must fill in, and stopping the save until they have.

**Why it matters.** Small, but it gates four later features. The validation service reads from it, and so do the mandatory-field marking, the list sorting and the confirmation prompts. The service itself is built in PM-18, inside the workflow that shows what is wrong — not as a story of its own — and the save framework and the mandatory-field check both call it. Building PM-16 and PM-18 before either of those is what stops validation being written twice. Which fields are mandatory comes from configuration, not from code, so the same screen behaves differently in different hospitals.

**Combined from 3 earlier features** on 19 Aug 2026, because each was only story-sized on its own.

**Feature acceptance criteria**

1. `FieldSetup` and `SystemSettings` exist in the settings database, created by migration.
2. `FieldSetup` holds, per patient field, whether it is active, whether it is mandatory, and its display label.
3. Sensible defaults are seeded on a new installation, and only where no value exists, so an administrator's changes survive an upgrade.
4. A repository can read and update both, covered by tests.
5. Saving is blocked while any configured rule fails.
6. Every failure is reported at once, not just the first one.
7. The failing field is marked on screen and the message says what is wrong and what is needed.
8. The mark clears as soon as the field is corrected.
9. The rules come from `FieldSetup` and `SystemSettings`, not from code.
10. Messages come from the resource file so they can be translated later.
11. Both the save framework (F2) and the mandatory-field check (F7) call this service rather than doing their own checks.
12. Mandatory fields are clearly marked, and the marking does not rely on colour alone.
13. Which fields are mandatory comes from `FieldSetup`. Changing the configuration changes the marking with no code change.
14. Save is blocked while any mandatory field is empty, and the empty ones are marked.
15. Save becomes available as soon as the last one is filled.
16. This uses the validation service built in PM-18 rather than its own separate check.

**User stories**

| Story | Title | Days |
|---|---|---|
| PM-16 | Create the field setup tables | 1.5 |
| PM-18 | Validate a patient record and show what is wrong | 2.5 |
| PM-19 | Show which fields are mandatory | 1.5 |
| PM-20 | Only allow save when mandatory fields are filled | 1.0 |

#### PM-16 · Create the field setup tables

*1.5 days · requirements: enabler for GID-254960 to GID-254966*

**Story.** As a developer, I want `FieldSetup` and `SystemSettings` tables, so that the administrator's choices about patient data entry can be stored.

**Acceptance criteria**

1. `FieldSetup` and `SystemSettings` tables exist in the **settings database**, created by a migration.
2. `FieldSetup` holds, for each patient field, whether it is active, whether it is mandatory, and its display label.
3. `SystemSettings` holds the single-row application settings.
4. Sensible defaults are seeded on a new installation, and are only written where no value exists — so an administrator's changes are never overwritten by an upgrade.
5. A repository can read and update both.
6. Unit tests cover reading, updating and the seed-only-if-absent rule.

---

#### PM-18 · Validate a patient record and show what is wrong

*2.5 days · requirements: GID-254966, GID-255012*

**Story.** As a user, I want the software to stop me saving invalid data and tell me exactly which field is wrong and what to do about it, so that patient records are trustworthy and I can fix mistakes without guessing.

**Acceptance criteria**

1. A failing field is visibly marked on screen.
2. The message says what is wrong and what is needed — for example "Date of birth cannot be in the future".
3. Several failing fields are all marked at once.
4. The mark clears as soon as the field is corrected.
5. Messages come from the resource file, not hardcoded text, so they can be translated later.
6. A validation service checks a patient record against the rules held in `FieldSetup` and `SystemSettings` — mandatory fields filled, values the right type and within range — and returns **every** failure at once, not just the first.
7. Saving is blocked while any rule fails.
8. That service is shared: the save framework (PM-08) and mandatory-field checking (PM-20) call it rather than doing their own checks.
9. Unit tests cover a valid record, one failure, several failures at once, an empty record, and clearing the marking on correction.

> **The validation service is built here, inside the workflow that first needs it**, rather than as a story of its own. PM-19 and PM-20 call it. An engine with no screen behind it is not something anyone can demonstrate.

---

#### PM-19 · Show which fields are mandatory

*1.5 days · requirements: GID-254892*

**Story.** As a screener, I want to see which fields I must fill in, so that I know what is needed before I try to save.

**Acceptance criteria**

1. Mandatory fields are clearly marked on the patient form.
2. Which fields are mandatory comes from `FieldSetup`, not from the code.
3. Changing the configuration changes the marks without a code change.
4. The marking is visible without relying on colour alone.
5. Unit tests cover: all fields mandatory, none mandatory, and a mixture.

#### PM-20 · Only allow save when mandatory fields are filled

*1.0 days · requirements: GID-254893*

**Story.** As a screener, I want the software to stop me saving until the mandatory fields are filled, so that incomplete records do not enter the system.

**Acceptance criteria**

1. Save is blocked while any mandatory field is empty.
2. The empty mandatory fields are marked using PM-18.
3. Save becomes available as soon as they are all filled.
4. This uses the validation service built in PM-18, not its own separate check.
5. Unit tests cover: all filled, one missing, several missing, and filling the last one.

---

---

## Slice 2C — Configuration

*11.0 coding days · 20.5 total · milestone M2*

**At the end of this slice you can demonstrate:** An administrator can set the patient ID format, list sorting, confirmation prompts and custom field labels.

---

### Feature F6 · Configure patient fields, ID format, list sorting and prompts

*11 coding days · 6 stories · slice 2C · milestone M2 · **requirements:** GID-254960, GID-254961, GID-254962, GID-254963, GID-254964, GID-255466*

**What it is.** The administrator screen for choosing which patient fields appear on the form and which of them must be filled in. Letting an administrator define the shape a patient ID must take, so IDs match the hospital's own numbering. Setting the order the patient list opens in. Turning the save, delete and data-change confirmations on or off. Giving the spare “Available Field” slots names that mean something locally.

**Why it matters.** This is what makes the patient form fit a particular unit. It writes to the tables built in F14 and is read by F7 and F19. Without this, any text is accepted as a patient ID, and identifiers drift between wards. Small but visible: it decides what a screener sees first every time they open the list. Confirmation prompts protect against mistakes but slow staff down. GID-254964 makes that a choice rather than a fixed behaviour. Lets a unit record something the standard form does not cover, without a code change.

**Combined from 5 earlier features** on 19 Aug 2026, because each was only story-sized on its own.

**Feature acceptance criteria**

1. Every patient field is listed with an Active and a Mandatory tick box.
2. Turning a field off hides it from the patient form; turning it on as mandatory makes F7 mark it.
3. A field cannot be both mandatory and inactive; the screen prevents that combination.
4. Fields the system needs in order to work cannot be switched off.
5. Changes are saved and survive a restart.
6. An administrator can define the required patient ID format and see it explained in plain words with an example.
7. A patient ID that does not match is refused, with a message showing the expected format.
8. An empty rule means any patient ID is accepted.
9. Changing the rule never alters or invalidates patients already saved.
10. The rule is saved and survives a restart.
11. An administrator can choose the default sort field and direction, and the list opens in that order.
12. A user can still re-sort their own view without changing the default.
13. The setting is saved and survives a restart.
14. Confirm on save, confirm on delete and warn on data change can each be turned on or off.
15. Turning one off stops that prompt appearing.
16. The shared framework in F2 reads these settings rather than always prompting.
17. **Delete confirmation cannot be turned off for patient records** — GID-254876 requires it — and the screen explains why.
18. Settings are saved and survive a restart.
19. A spare field can be given a new name, and that name appears on the patient form and in the field configuration list.
20. Clearing the name restores the default label.
21. Renaming never affects data already stored in that field.
22. Names are saved and survive a restart.
23. The scope — four spare fields or all 62 — is confirmed before work starts (open question Q19).

**User stories**

| Story | Title | Days |
|---|---|---|
| PM-22 | Choose which fields are mandatory and which are shown | 3.0 |
| PM-23 | Set the patient ID format rule | 2.5 |
| PM-24 | Set how the patient list is sorted | 1.5 |
| PM-25 | Turn confirmation prompts on and off | 2.0 |
| PM-26 | Rename the custom fields | 2.0 |
| PM-27 | Check the configuration works end to end | 0.0 |

#### PM-22 · Choose which fields are mandatory and which are shown

*3.0 days · requirements: GID-254961, GID-254962*

**Story.** As an administrator, I want to choose which patient fields appear and which are mandatory, so that the form matches how our unit works.

**Acceptance criteria**

1. A configuration screen lists every patient field with an **Active** and a **Mandatory** tick box.
2. Turning a field off hides it from the patient form.
3. Turning a field on as mandatory makes it required, and PM-19 marks it.
4. Changes are saved to `FieldSetup` and survive a restart.
5. A field cannot be mandatory and inactive at the same time — the screen prevents it.
6. Fields the system needs to work cannot be switched off.
7. Unit tests cover: turning a field off hides it, mandatory drives the marking, the contradiction is blocked, and settings survive a restart.

---

#### PM-23 · Set the patient ID format rule

*2.5 days · requirements: GID-254960*

**Story.** As an administrator, I want to set the format a patient ID must follow, so that IDs match our hospital's numbering.

**Acceptance criteria**

1. An administrator can define the required patient ID format.
2. The screen explains the format in plain words and shows an example.
3. Entering a patient ID that does not match is refused, with a message showing the expected format.
4. An empty rule means any patient ID is accepted.
5. The rule is saved and survives a restart.
6. Changing the rule does not alter or invalidate patients already saved.
7. Unit tests cover a matching ID, a non-matching ID, no rule set, and that existing patients are untouched.

---

#### PM-24 · Set how the patient list is sorted

*1.5 days · requirements: GID-254963*

**Story.** As an administrator, I want to set the default order of the patient list, so that it opens the way our staff expect.

**Acceptance criteria**

1. An administrator can choose the default sort field and direction.
2. The patient list opens in that order.
3. The setting is saved and survives a restart.
4. A user can still re-sort their own view without changing the default.
5. Unit tests cover each sort option and that the default is applied on open.

---

#### PM-25 · Turn confirmation prompts on and off

*2.0 days · requirements: GID-254964*

**Story.** As an administrator, I want to control which confirmation prompts appear, so that staff are not slowed down by prompts we do not need.

**Acceptance criteria**

1. Three settings can each be turned on or off: **confirm on save**, **confirm on delete**, **warn on data change**.
2. Turning one off stops that prompt appearing.
3. The shared framework (PM-07, PM-08) reads these settings rather than always prompting.
4. Settings are saved and survive a restart.
5. **Delete confirmation cannot be turned off for patient records** — GID-254876 requires it. The screen explains why.
6. Unit tests cover each setting on and off, and the delete-confirmation exception.

---

#### PM-26 · Rename the custom fields

*2.0 days · requirements: GID-255466*

**Story.** As an administrator, I want to rename the spare "Available Field" labels, so that we can record something our unit needs without a code change.

**Acceptance criteria**

1. The spare fields labelled "Available Field" can each be given a new name.
2. The new name appears on the patient form and in the field configuration list.
3. Names are saved and survive a restart.
4. Clearing a name puts the default label back.
5. Renaming does not affect data already stored in that field.
6. Unit tests cover renaming, clearing, that data is preserved, and that the name appears everywhere it should.

> **Open question Q19:** the code today allows renaming **all 62** patient fields, including Patient ID and Date of Birth. GID-255466 reads as the four spare fields only. Renaming a mandatory clinical identifier is a much bigger deal than renaming a spare slot. **Confirm which is intended before starting.**

### Slice 2C check · PM-27 · Check the configuration works end to end

*0.0 days — the work is covered by the features above; it is listed separately because it spans all of them · requirements: GID-254966*

**Story.** As an administrator, I want my configuration choices to take effect immediately on the patient form, so that I can see the result of what I changed.

**Acceptance criteria**

1. Changing any setting in PM-22 to PM-26 and reopening the patient form shows the effect.
2. Two administrators changing settings at once do not overwrite each other silently.
3. Settings are read fresh when the patient form opens, not cached from application start.

> **⚠ Settle §4.1 before starting.** The 16 hardcoded questions on the patient screen and the configurable list the requirements ask for cannot both exist. These stories assume the configurable list wins, with the 16 questions kept as the default content.

---


## Slice 2D — Risk factors and comments

*18.5 coding days · 34.0 total · milestone M2*

**At the end of this slice you can demonstrate:** Risk factors and comments are lists an administrator manages, with translations, and they can be put on a patient. The patient report prints.

---

---

### Feature F7 · Set a patient's risk factors and comments

*4.5 coding days · 2 stories · slice 2D · milestone M2 · **requirements:** GID-254888, GID-254889*

**What it is.** Answering the risk factor questions for a baby: Yes, No or Unknown for each one. Putting a comment on a baby's record: either one picked from the managed list, or free text for this patient only.

**Why it matters.** This is the clinical point of the risk factor machinery. It is also the test that proves §4.1 was resolved properly — a risk factor added by an administrator has to appear here without a code change. Both kinds matter. The managed list keeps wording consistent; the free-text option covers the case the list did not anticipate.

**Combined from 2 earlier features** on 19 Aug 2026, because each was only story-sized on its own.

**Feature acceptance criteria**

1. The patient screen shows the risk factors from the administrator's list, not a fixed set written into the code.
2. Each risk factor can be answered Yes, No or Unknown, and the answers are saved against the patient.
3. **Adding a new risk factor in configuration makes it appear on the patient screen with no code change.**
4. An unanswered risk factor is clearly different from one answered “Unknown”.
5. The screen shows how many are answered, for example “12 of 16 answered”.
6. Names appear in the user's language where a translation exists.
7. A screener can pick one or more comments from the administrator's list.
8. A screener can also type a comment that applies to this patient only.
9. Both kinds are saved against the patient and survive a restart.
10. Comments appear in the Comment column of the patient list.
11. Removing a comment from a patient does not remove it from the master list.
12. Predefined comments appear in the user's language where a translation exists.

**User stories**

| Story | Title | Days |
|---|---|---|
| PM-36 | Set risk factors on a patient | 2.5 |
| PM-37 | Put comments on a patient | 2.0 |

#### PM-36 · Set risk factors on a patient

*2.5 days · requirements: GID-254888*

**Story.** As a screener, I want to answer the risk factor questions for a baby, so that their hearing-loss risk is recorded.

**Acceptance criteria**

1. The patient screen shows **the risk factors from the administrator's list** (PM-29), not a fixed set written into the code.
2. Each risk factor can be answered **Yes**, **No** or **Unknown**.
3. Answers are saved against the patient and survive a restart.
4. Adding a new risk factor in the configuration makes it appear on the patient screen without a code change. *(This is the test that proves §4.1 was resolved properly.)*
5. Names appear in the user's language where a translation exists.
6. Unanswered risk factors are clearly different from ones answered "Unknown".
7. The screen shows how many are answered, for example "12 of 16 answered".
8. Unit tests cover: all three answers save and reload, a newly added risk factor appears, unanswered differs from Unknown, and the counter is right.

---

#### PM-37 · Put comments on a patient

*2.0 days · requirements: GID-254889*

**Story.** As a screener, I want to add a comment to a patient, so that I can record something that matters about this baby.

**Acceptance criteria**

1. The screener can pick one or more comments from the administrator's list (PM-33).
2. The screener can also type a comment for this patient only.
3. Both kinds are saved against the patient and survive a restart.
4. Predefined comments appear in the user's language where a translation exists.
5. Comments appear in the patient list's Comment column (PM-10).
6. A comment can be removed from a patient without deleting it from the master list.
7. Unit tests cover: picking a predefined comment, typing a patient-specific one, both together, removing one, and that the master list is untouched.

---

---

### Feature F8 · Manage and translate the risk factor list

*5.5 coding days · 4 stories · slice 2D · milestone M2 · **requirements:** GID-254950, GID-254951, GID-254952, GID-254953, GID-254954*

**What it is.** Turning risk factors from a fixed list written into the code into a list an administrator manages. The translated name and description for each risk factor, in each supported language.

**Why it matters.** **This is the feature that resolves the design conflict in §4.1.** Today the patient screen has 16 questions hardcoded into it. The requirements want a list. One of the two has to go, and this feature is where that is decided in code. Risk factors are shown to staff during screening. If the application runs in French, the risk factors have to as well.

**Combined from 2 earlier features** on 19 Aug 2026, because each was only story-sized on its own.

**Feature acceptance criteria**

1. An administrator can create, view, edit and delete risk factors.
2. A risk factor that is already used on a patient cannot be edited or deleted, and the refusal says how many patients use it.
3. Each risk factor has a stable identifier, so renaming one does not break patient records that point at it.
4. The 16 existing clinical questions are seeded as the default list on a new installation.
5. Seeding only writes where nothing exists, so an administrator's edits survive an upgrade.
6. The “is it in use?” check reads the patient database from the settings side. With two databases this cannot be a foreign key and must be a service-layer check — recorded in the design document.
7. An administrator can enter a translated name and description per supported language for any risk factor.
8. A missing translation falls back to the default language rather than showing blank.
9. The screen shows which languages are still missing a translation.
10. Deleting a risk factor deletes its translations.
11. Translations are saved and survive a restart.

**User stories**

| Story | Title | Days |
|---|---|---|
| PM-28 | Create the risk factor table | 1.0 |
| PM-29 | Manage the risk factor list | 2.5 |
| PM-30 | Create the risk factor translation table | 0.5 |
| PM-31 | Enter risk factor translations | 1.5 |

#### PM-28 · Create the risk factor table

*1.0 days · requirements: enabler for GID-254950 to GID-254953*

**Story.** As a developer, I want a `RiskFactors` table, so that risk factors become a list an administrator can manage.

**Acceptance criteria**

1. A `RiskFactors` table exists in the settings database, created by a migration.
2. Each risk factor has a stable identifier that never changes, a name, a description and an active flag.
3. The stable identifier is what patient records point at, so renaming a risk factor does not break existing records.
4. **The 16 existing clinical questions are seeded as the default list** on a new installation.
5. Seeding only writes where nothing exists, so an administrator's edits survive an upgrade.
6. Unit tests cover reading, adding, updating, and that seeding does not overwrite.

#### PM-29 · Manage the risk factor list

*2.5 days · requirements: GID-254950, GID-254951, GID-254952, GID-254953*

**Story.** As an administrator, I want to create, view, change and remove risk factors, so that the list matches our screening programme.

**Acceptance criteria**

1. An administrator can see the list of risk factors.
2. An administrator can add a new one with a name and description.
3. An administrator can change one **that is not already used on any patient**.
4. An administrator can delete one **that is not already used on any patient**.
5. Trying to change or delete one that is in use is refused, with a message saying how many patients use it.
6. The check counts patients across the patient database.
7. Unit tests cover: add, change unused, delete unused, blocked change on used, blocked delete on used, and the count in the message.

> **Note:** with two databases (decision D1), this "is it in use?" check reads the patient database from the settings side. It cannot be a database foreign key and must be a service-layer check. Record that in the design document.

---

#### PM-30 · Create the risk factor translation table

*0.5 days · requirements: enabler for GID-254954*

**Story.** As a developer, I want a `RiskFactorTranslations` table, so that risk factor names can be shown in each supported language.

**Acceptance criteria**

1. The table exists, linked to `RiskFactors`, with one row per language.
2. Each row holds a translated name and description.
3. Deleting a risk factor deletes its translations.
4. Unit tests cover adding, reading and cascade deletion.

#### PM-31 · Enter risk factor translations

*1.5 days · requirements: GID-254954*

**Story.** As an administrator, I want to type the translated name and description for each risk factor, so that staff see them in their own language.

**Acceptance criteria**

1. For a selected risk factor, the administrator can enter a name and description for each supported language.
2. Translations are saved and survive a restart.
3. A missing translation falls back to the default language rather than showing blank.
4. The screen shows which languages are still missing a translation.
5. Unit tests cover entering, saving, the fallback, and the missing-language indicator.

> **Sized by open question Q1.** How many languages ship decides how much entry work this is. Translating the text itself is not a development task.

---

---

### Feature F9 · Manage and translate the comment list

*5.5 coding days · 4 stories · slice 2D · milestone M2 · **requirements:** GID-254955, GID-254956, GID-254957, GID-254958, GID-254959*

**What it is.** The list of predefined comments an administrator maintains, so staff pick from agreed wording. The translated text for each predefined comment, in each supported language.

**Why it matters.** Free-text comments cannot be reported on. A managed list can. Same reason as F11: a comment shown to staff has to be in the language they are working in.

**Combined from 2 earlier features** on 19 Aug 2026, because each was only story-sized on its own.

**Feature acceptance criteria**

1. An administrator can create, view, edit and delete predefined comments.
2. A comment already used on a patient cannot be deleted, and the refusal says how many patients use it.
3. Editing the text of a comment in use is allowed, and patients already carrying it show the updated text.
4. The 14 existing comments are seeded as the default list, written only where nothing exists.
5. Each comment has a stable identifier that patient records point at.
6. An administrator can enter translated text per supported language for any comment.
7. A missing translation falls back to the default language.
8. The screen shows which languages are still missing.
9. Deleting a comment deletes its translations.
10. Translations are saved and survive a restart.

**User stories**

| Story | Title | Days |
|---|---|---|
| PM-32 | Create the comments table | 1.0 |
| PM-33 | Manage the comment list | 2.5 |
| PM-34 | Create the comment translation table | 0.5 |
| PM-35 | Enter comment translations | 1.5 |

#### PM-32 · Create the comments table

*1.0 days · requirements: enabler for GID-254955 to GID-254958*

**Story.** As a developer, I want a `PredefinedComments` table, so that comments become a list an administrator can manage.

**Acceptance criteria**

1. The table exists in the settings database, created by a migration.
2. Each comment has a stable identifier, its text and an active flag.
3. The 14 existing comments are seeded as the default list.
4. Seeding only writes where nothing exists.
5. Unit tests cover reading, adding, updating and the seed rule.

#### PM-33 · Manage the comment list

*2.5 days · requirements: GID-254955, GID-254956, GID-254957, GID-254958*

**Story.** As an administrator, I want to create, view, change and remove predefined comments, so that staff can pick from wording we have agreed.

**Acceptance criteria**

1. An administrator can see, add and change predefined comments.
2. An administrator can delete a comment **that is not used on any patient**.
3. Deleting one that is in use is refused, with a message saying how many patients use it.
4. Changing the text of a comment already in use is allowed, and existing patients show the updated text.
5. Unit tests cover add, change, delete unused, blocked delete on used, and the change-in-use behaviour.

---

#### PM-34 · Create the comment translation table

*0.5 days · requirements: enabler for GID-254959*

**Story.** As a developer, I want a `PredefinedCommentTranslations` table, so that comments can be shown in each supported language.

**Acceptance criteria**

1. The table exists, linked to `PredefinedComments`, one row per language.
2. Deleting a comment deletes its translations.
3. Unit tests cover adding, reading and cascade deletion.

#### PM-35 · Enter comment translations

*1.5 days · requirements: GID-254959*

**Story.** As an administrator, I want to type the translated text for each comment, so that staff see them in their own language.

**Acceptance criteria**

1. For a selected comment, the administrator can enter text for each supported language.
2. Translations are saved and survive a restart.
3. A missing translation falls back to the default language.
4. The screen shows which languages are still missing.
5. Unit tests cover entering, saving, the fallback and the indicator.

---

---

## 6. Feature summary

| Feature | Name | Stories | Coding | Slice | Requirements |
|---|---|---|---|---|---|
| **F1** | Create, view, edit and delete a patient record | 7 | 29 | 2A | GID-254874, GID-254875, GID-254876, GID-254883, GID-254885, GID-254886, GID-254891, GID-255042, GID-255044 |
| **F2** | Shared Add/Edit/Delete and Save/Revert/Undo framework | 3 | 6.5 | 2A | GID-254873, GID-254874, GID-254875, GID-254876, GID-254877, GID-254878, GID-254879, GID-254880, GID-254881, GID-254882, GID-255009 |
| **F3** | Show and search the patient list | 2 | 5 | 2B | GID-254884, GID-254887, GID-254890 |
| **F4** | Show, delete and reassign test results, with permissions enforced | 5 | 11.5 | 2B | GID-254885, GID-254886, GID-254895, GID-254896, GID-254897, GID-255042, GID-255044 |
| **F5** | Store the field setup, validate input and enforce mandatory fields | 4 | 6.5 | 2B | GID-254892, GID-254893, GID-254960, GID-254966, GID-255012 |
| **F6** | Configure patient fields, ID format, list sorting and prompts | 6 | 11 | 2C | GID-254960, GID-254961, GID-254962, GID-254963, GID-254964, GID-255466 |
| **F7** | Set a patient's risk factors and comments | 2 | 4.5 | 2D | GID-254888, GID-254889 |
| **F8** | Manage and translate the risk factor list | 4 | 5.5 | 2D | GID-254950, GID-254951, GID-254952, GID-254953, GID-254954 |
| **F9** | Manage and translate the comment list | 4 | 5.5 | 2D | GID-254955, GID-254956, GID-254957, GID-254958, GID-254959 |
| | **9 features** | **37** | **85** | | **44 requirements** |

## 7. Story summary

| Story | Title | Feature | Slice | Days | Requirements |
|---|---|---|---|---|---|
| PM-01 | Create the patient tables and repository | F1 | 2A | 3.0 | GID-255042, GID-255044 |
| PM-03 | Add a new patient | F1 | 2A | 4.0 | GID-254883, GID-254874 |
| PM-04 | View and edit a patient, and fix the two data bugs | F1 | 2A | 3.0 | GID-254891, GID-254885, GID-254875 |
| PM-05 | Delete a patient (soft delete) | F1 | 2A | 1.5 | GID-254886, GID-254876 |
| PM-06 | Save the patient's contact details | F1 | 2A | 1.5 | GID-254883 |
| PM-40 | Add the AccuLink fields AccuSync is missing | F1 | 2A | 8.0 | GID-254883 |
| PM-39 | Add the ALGO device fields | F1 | 2A | 8.0 | none |
| PM-07 | Build the shared Add, Edit and Delete buttons | F2 | 2A | 2.0 | GID-254873, GID-254874, GID-254875, GID-254876, GID-254877 |
| PM-08 | Build the shared Save, Revert and Undo | F2 | 2A | 3.0 | GID-254878, GID-254879, GID-254880, GID-254881, GID-254882 |
| PM-09 | Warn before losing unsaved changes | F2 | 2A | 1.5 | GID-255009 |
| PM-10 | Show the patient list | F3 | 2B | 2.5 | GID-254884, GID-254890 |
| PM-11 | Search the patient list | F3 | 2B | 2.5 | GID-254887 |
| PM-12 | Create the test result tables | F4 | 2B | 2.5 | GID-255042, GID-255044 |
| PM-13 | Show the patient's test list | F4 | 2B | 2.5 | GID-254897 |
| PM-14 | Delete one test entry | F4 | 2B | 2.0 | GID-254895 |
| PM-15 | Move a test to the correct patient | F4 | 2B | 2.5 | GID-254896 |
| PM-16 | Create the field setup tables | F5 | 2B | 1.5 | enabler for GID-254960 to GID-254966 |
| PM-18 | Validate a patient record and show what is wrong | F5 | 2B | 2.5 | GID-254966, GID-255012 |
| PM-19 | Show which fields are mandatory | F5 | 2B | 1.5 | GID-254892 |
| PM-20 | Only allow save when mandatory fields are filled | F5 | 2B | 1.0 | GID-254893 |
| PM-21 | Enforce patient permissions | F4 | 2B | 2.0 | GID-254885, GID-254886 |
| PM-22 | Choose which fields are mandatory and which are shown | F6 | 2C | 3.0 | GID-254961, GID-254962 |
| PM-23 | Set the patient ID format rule | F6 | 2C | 2.5 | GID-254960 |
| PM-24 | Set how the patient list is sorted | F6 | 2C | 1.5 | GID-254963 |
| PM-25 | Turn confirmation prompts on and off | F6 | 2C | 2.0 | GID-254964 |
| PM-26 | Rename the custom fields | F6 | 2C | 2.0 | GID-255466 |
| PM-27 | Check the configuration works end to end | F6 | 2C | 0.0 | GID-254966 |
| PM-28 | Create the risk factor table | F8 | 2D | 1.0 | enabler for GID-254950 to GID-254953 |
| PM-29 | Manage the risk factor list | F8 | 2D | 2.5 | GID-254950, GID-254951, GID-254952, GID-254953 |
| PM-30 | Create the risk factor translation table | F8 | 2D | 0.5 | enabler for GID-254954 |
| PM-31 | Enter risk factor translations | F8 | 2D | 1.5 | GID-254954 |
| PM-32 | Create the comments table | F9 | 2D | 1.0 | enabler for GID-254955 to GID-254958 |
| PM-33 | Manage the comment list | F9 | 2D | 2.5 | GID-254955, GID-254956, GID-254957, GID-254958 |
| PM-34 | Create the comment translation table | F9 | 2D | 0.5 | enabler for GID-254959 |
| PM-35 | Enter comment translations | F9 | 2D | 1.5 | GID-254959 |
| PM-36 | Set risk factors on a patient | F7 | 2D | 2.5 | GID-254888 |
| PM-37 | Put comments on a patient | F7 | 2D | 2.0 | GID-254889 |

| | **37 stories** | **10 features** | | **85** | **44 requirements** |

---

## 8. Things to settle

| # | What | Affects | Needed by |
|---|---|---|---|
| **§4.1** | Risk factors: configurable list, or the 16 hardcoded questions? One has to be rebuilt | F10, F3 | Before slice 2D |
| ~~Q16~~ | ~~The patient field list~~ — **answered.** AccuLink has 52 fields, AccuSync adds 7, total **59**. See the [field comparison](../Patient-Field-Comparison-AccuLink-vs-AccuSync.md). Six fields are missing from the schema design and are added in PM-01 | F1 | ✅ closed |
| **Q9** | What happens when you delete something in use? Settled for patients (soft delete), risk factors and comments (block). Not for the rest | F10, F12 | Before slice 2D |
| ~~Q5~~ | ~~Is the patient test report one feature or two?~~ — **answered 27 Aug 2026: one workflow, and it moved to Epic 18 F2** | — | ✅ closed |
| **Q19** | Rename four spare fields, or all 62? The code does all 62, including Patient ID | F20 | Before slice 2C |
| **Q1** | How many languages? Sizes the translation entry work | F11, F13 | Before slice 2D |
| ~~O1~~ | ~~Which table goes in which database?~~ — **answered 22 Aug 2026.** **Patient database:** all patient information and test data — patient details, risks and comments on a patient, test data and detail. **Settings database:** all configuration — field setup, risk factor list, comment list, protocols, sites, devices, users, profiles | F1, F6, F14, F10, F12 | ✅ closed |
| **Q-adm** | **Does “allow administrative users to X” forbid everyone else?** Patient delete is settled — open to all users. The same wording covers **62 requirements**, including test delete (GID-254895) and test reassignment (GID-254896), which stay admin-only until this is answered | F6, F8 | Before slice 2B |
| ~~A1~~ | ~~Are the 28 extra ALGO patient fields in scope?~~ — **answered 18 Aug 2026.** Natus agreed to **skip the baby's own address and phone (8 fields)** and use the mother/caregiver address instead. The remaining **19 fields are in scope** and are built by **PM-39** | F1 | ✅ closed |
| ~~A2a~~ | ~~One physician field or two?~~ — **answered 18 Aug 2026.** The baby's and the mother's physician stay **separate fields**; they will not be the same person. Built by **PM-39** | F1 | ✅ closed |
| ~~A2b~~ | ~~Merge them under one label?~~ — **answered 18 Aug 2026.** **No.** The baby's field is labelled **Physician / Pediatrician**; the mother's keeps its own label. Built by **PM-39** | F1 | ✅ closed |
| ~~A4~~ | ~~Medication as a configurable list or multi-line text?~~ — **answered 18 Aug 2026.** **Multi-line text.** A configurable list would need a whole config screen for a field no requirement mentions. Built by **PM-39** | F1 | ✅ closed |
| ~~A10~~ | ~~AccuLink is the baseline field set, and our schema is short of it~~ — **now owned by story PM-40**, which runs before PM-01. What remains for Natus is confirming the exclusions PM-40 proposes. Natus: *"I didn't add all fields to match AccuLink. Use AccuLink as reference for fields to display."* We have found six missing so far. A field-by-field pass against AccuLink is needed before the migration is written | F1 | **Before PM-01** |
| **A2c** | **Which patient-level doctor field wins?** ALGO Pro sends both `CurrentPediatrician` and `PatientPhysician`, and both hold `Joseph` in the sample. The confirmed mapping is `CurrentPediatrician`. If a real file has them differing, the import must flag it rather than silently pick one | F1, Epic 3 | Before PM-39 |
| **A2d** | **Is `Medication` per patient or per test?** ALGO sends it on each test; AccuSync holds one value on the patient. A baby with three tests can have three different values. **Not solved by PM-39** | F1, F6 | Before Epic 3 |
| ~~A3~~ | ~~Code lists for the new dropdowns~~ — **answered 22 Aug 2026.** `BirthType`: Vag Single / Vag Multiple / Vag Triple / Ceasection Single / Ceasection Multiple / Ceasection Triple. `ScreenLocation`: WBN / NICU / OP. `StageLevel`: Inpatient / Outpatient / Readmission. **`InsuranceType` is single-line text, not a dropdown** | F1 | ✅ closed |
| ~~A9~~ | ~~Are `Insurance`, `InsuranceType` and `BsPkuId` wanted?~~ — **answered 22 Aug 2026: yes, all three.** Nothing comes out of PM-39 | F1 | ✅ closed |
| **A6** | **What goes in the ABR detail table?** The `ABRResults` design holds AccuLink wave latencies; ALGO delivers likelihood ratios and sweep counts. **Zero overlap** — the table cannot hold an ALGO test | F6 | **Before PM-10** |

---

## 9. What this epic depends on, and what depends on it

**Before it can start**

| Needs | From | Why |
|---|---|---|
| A build that works, and CI | ASWD-77 | The solution does not build from a fresh copy today |
| Shared base classes and navigation | ASWD-77 | Every ViewModel here uses them |
| Two databases wired up | ASWD-77 | F1 adds tables to them |

**Waiting on this epic**

| Who | Needs | From |
|---|---|---|
| Every later screen epic | The shared Add/Edit/Delete and Save/Revert/Undo framework | **F2** |
| ASWD-5, ASWD-6 test result views | The test tables | **F4** |
| ASWD-3 Import | Somewhere to save imported patients | **F1, F6** |
| ASWD-4 Export | Patient and test data to export | **F1, F6** |
| ASWD-18 Report Generation | Patient and test data to report on | **F1, F6** |
| ASWD-25 Audit Trail | Patient events to record | **F1** |

**Landing later, and needing a second visit**

| What | Feature | When |
|---|---|---|
| Real profile permissions replace the temporary role check | F8 | ASWD-8, weeks 66–70 |
| Protocol names replace protocol identifiers in the test list | F6 | ASWD-13/14, weeks 86–90 |
| Language switching makes the translations visible | F11, F13 | ASWD-19, weeks 114–120 |
