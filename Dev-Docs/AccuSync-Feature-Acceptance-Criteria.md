# AccuSync — Feature Acceptance Criteria

**What has to be true before a feature can be called done.**

| | |
|---|---|
| Features | **67** across 25 epics |
| Requirements | **196** |
| Total | **536.1 developer-days** |
| Plan | [AccuSync-Delivery-Roadmap.md](AccuSync-Delivery-Roadmap.md) |
| Epic-level criteria | [AccuSync-Epic-Acceptance-Criteria.md](AccuSync-Epic-Acceptance-Criteria.md) |
| Story-level criteria | [Patient Management stories](EPIC%202%20Patient%20Management/PATIENT_MANAGEMENT_STORIES.md) |

---

## How to use this

One entry per feature, grouped by epic, in build order. Each entry gives:

- **What it is** — the work, in plain words
- **Done when** — the criteria. Requirement-derived criteria quote the requirement text, so there is no gap between what was asked for and what gets checked
- **Always** — four checks that apply to every feature

A feature marked **builds tables** creates database tables. Tables are built inside the feature that needs them (decision D5), not in one big data-model epic.

Features with no requirement are **enablers** — they make other features possible. They still have acceptance criteria.

---

## The four checks that apply to every feature

1. Unit tests cover the behaviour above, including the failure paths, and new code reaches the 80% target.
2. All existing tests still pass.
3. The pull request is reviewed, comments are resolved, and it is merged to `main` with CI green.
4. The requirement IDs this feature delivers are recorded against it.

---

## Epic 0 · ASWD-77 — Revamp Code Base

*1 feature · 9.1 days · milestone M1 · one developer: 2026-09-04*

### ASWD-77 F1 · Fix the clean-copy build, merge the branches and set up CI

**What it is** — Fix the clean-copy build failure (0.5); Regenerate the migrations so a fresh install works (1.0); Merge all 12 branches into `main` (1.5); Set up CI: build + test + coverage (2.0); Base classes: `ObservableObject`, `ViewModelBase`, `ValidatableViewModelBase`, `AsyncRelayCommand`, `Result<T>` (2.0); Navigation, dialog and file-picker services (2.0); Move the existing windows onto the new navigation (1.5); Generic Host startup and move DI out of `App.xaml.cs` (1.0); Release build settings and version numbers, and remove the dev-mode login bypass from Release (1.5); Investigate how to encrypt a SQLite file (1.5); Write down the design info held only in the two `.sql` files, before they are deleted (2.0); Repository and unit-of-work pattern, shared base (2.0); Two DbContexts wired up: Patient and Settings (3.0); Move the parsing interfaces into `Core` and point `DataParser` at `Core` instead of `Application`

**Done when**

1. The capability described above works and is demonstrable.
2. The tables are created by a code-first migration, not by a hand-written script. A brand-new database is created correctly on first run.
3. Which database each table goes in is written into the epic design document and added to the running allocation table in the ASWD-77 design document (open item O1).
4. A repository can read, add, change and remove records, and is covered by tests.
5. The investigation ends with a **written finding** that says what was learned and whether the estimate still holds. If it does not, the estimate is corrected before the next story starts.
6. The four checks above are satisfied. ---
7. The four checks above are satisfied.

## Epic 1 · ASWD-1 — Login

*1 feature · 8.9 days · milestone M1 · one developer: 2026-10-23*

### ASWD-1 F1 · Re-verify login after the branch merge and record traceability

*1 coding days*

**What it is** — Re-verify the six requirements after the branch merge, record requirement traceability, confirm the 103 tests still pass

**Done when**

1. **GID-255015 User Login** — The software shall require users to enter username and password credentials to login to the system.
2. **GID-255016 User Logout** — The software shall provide an option to logout of the system.
3. **GID-255017 User Account Lockout** — The software shall lock out the user from logging into the system after a minimum of 5 failed sequential login attempts with improper credentials.
4. **GID-255018 Credential Encryption** — The software shall securely hash all login passwords and encrypt all usernames stored in the system. ⚠ **safety-related (`U*`)**
5. **GID-255019 Role-Based Access Control** — The software shall support role-based access control with minimum two roles: Admin and Screener.
6. **GID-255021 Deactivated User Login Prevention** — The software shall prevent deactivated users from logging in.
7. The four checks above are satisfied. ---
8. The four checks above are satisfied.

## Epic 26 · ASWD-26 — Logging: the basics

*1 feature · 3.8 days · milestone M1 · one developer: 2026-10-30*

### ASWD-26 F1 · Logging that works, and logging in the code already written

*4.0 coding days*

**What it is** — The logging mechanism: an interface in `Core`, a file writer, four severity levels, the agreed entry format, a hard rule that no patient demographic data reaches the file, and logging added to the Epic 0 and Epic 1 code already written. Every format and location below was set by Natus on 22 Aug 2026. **Rotation, retention and file protection are not here** — they moved to **ASWD-31** on 25 Aug 2026, because they produce nothing a stakeholder can see.

**Done when**

1. **GID-255031 Log Message Generation** — The software shall provide a mechanism to generate log messages during the operation of the software.
2. **GID-255032 Log Message Types** — The software shall provide a logging mechanism that supports informational, warning, and error message types. **Built with four levels — Debug, Info, Warning, Error** — Debug being an agreed addition beyond the requirement.
3. **GID-255033 Log Message Storage** — The software shall store generated log messages on the system for troubleshooting purposes. Written to **`%ProgramData%\Natus\AccuSync\Logs`**, path overridable in the config file.
4. **GID-255034 Log Data Privacy** — The software shall **not** include or retain any patient-identifiable information in the log data. **Natus ruling: no patient demographic data at all.** Enforced here by one rule the logger owns — exception logging never dumps  — plus developer discipline in every later epic. **Proven in Epic 2 F1**, the first point a patient record exists to test against.
5. **GID-255038 Log Entry Fields** — The software shall associate all log entries with timestamp, description and status of the operation. **Met by** the timestamp, the level in `[{level}]`, and the outcome stated in the message.
6. Every entry follows the agreed format exactly: **`{date:yyyy-MM-dd HH:mm:ss} [{level}] {class-name}.{method}() {message}`**, with class and method captured automatically.
7. A log file is produced with **no debugger attached** and without administrator rights.
8. Messages are **English only** and are not added to the translated resource set.
9. No `Debug.WriteLine` or `Console.WriteLine` remains as the only record of anything that matters.
10. The four checks above are satisfied.

**Not done if**

- Any patient demographic value can be found in a log file produced by a real run.
- The entry format differs from the agreed string in any way.
- Log messages appear in the translation resource files.

## Epic 2 · ASWD-2 — Patient Management

*9 features · 96.1 days · milestone M1 · one developer: 2027-05-21*

### ASWD-2 F1 · Create, view, edit and delete a patient record

*21.5 coding days*

**What it is** — Create the `Patients` and `PatientContacts` tables, repository and service; Presentation layer: ViewModels, DTOs, converters; Add a new patient; View a patient; Edit a patient; Soft-delete a patient; **add the 19 ALGO device fields** plus the Physician / Pediatrician relabel and multi-line Medication (PM-39)

**Done when**

1. **GID-254883 Create Patient Record** — The software shall allow users to create a patient record containing patient demographics, caregiver data, medical data, and consent information.
2. **GID-254885 Edit Patient Record** — The software shall allow users to select a patient from the list view and edit the patient record, unless locked by permissions.
3. **GID-254886 Delete Patient Record** — The software shall allow administrative users to delete a patient record.
4. **All 19 ALGO device fields are in the database, on the model and on a screen, and every one round-trips a restart** (PM-39). Importing an ALGO 5 or ALGO Pro file no longer silently discards patient data. *No requirement covers these fields — scope confirmed by Natus 18 Aug 2026.*
5. **The two databases are split as Natus set on 22 Aug 2026** — patient information, test data, and the risks and comments assigned to a patient go in the **patient** database; field setup, the risk factor and comment **lists**, protocols, sites, devices, users and profiles go in the **settings** database.
6. **`RaceReferenceId` is built** — it is AccuLink's Race dropdown and appears in the AccuLink export file. Confirmed by Natus 22 Aug 2026.
7. **The three new coded dropdowns use the values Natus supplied** — `BirthType` (6 values), `ScreenLocation` (WBN / NICU / OP), `StageLevel` (Inpatient / Outpatient / Readmission). **`InsuranceType` is single-line text, not a dropdown.**
8. **`Insurance`, `InsuranceType` and `BsPkuId` are all built** — confirmed wanted.
5. **The `Physician` import defect is fixed** — a regression test asserts the field holds the pediatrician (`Joseph`), not the test-level physician (`Andrew`).
6. **GID-254891 View Patient Information** — The software shall allow users select a patient from the list view and view patient information.
7. The tables are created by a code-first migration, not by a hand-written script. A brand-new database is created correctly on first run.
8. Which database each table goes in is written into the epic design document and added to the running allocation table in the ASWD-77 design document (open item O1).
9. A repository can read, add, change and remove records, and is covered by tests.
10. **Data entered is written to the database and is still there after the application is closed and reopened.** This is the check that most screens fail today.
11. A success message appears only when something was actually saved.
12. The four checks above are satisfied.

### ASWD-2 F2 · Shared Add/Edit/Delete and Save/Revert/Undo framework

*6.5 coding days*

**What it is** — Shared Add/Edit/Delete commands and the delete-confirmation prompt; Shared change-tracking and undo stack for Save/Revert/Undo; Warn before closing or navigating away with unsaved changes

**Done when**

1. **GID-254873 Data Management Options** — The software shall provide data management options Add, Edit, and Delete for creating, modifying, and removing database records.
2. **GID-254874 Add Function** — The software shall allow the user to create a new record by selecting Add, which shall open a blank form for data entry.
3. **GID-254875 Edit Function** — The software shall allow the user to modify a selected record by selecting Edit, which shall open the record in an editable form.
4. **GID-254876 Delete Function** — The software shall allow the user to delete a selected record by selecting Delete, and shall require user confirmation before removal from the database.
5. **GID-254877 Data Management Screen Availability** — The software shall provide data management options (Add, Edit, Delete) on the following screens: Patient, User, Profile, Device, Site, Facility, Location, ABR Protocols, DPOAE Protocols, Risk Factors, Comments.
6. **GID-254878 Unsaved Data Management Options** — The software shall provide unsaved data management options Save, Revert, and Undo for managing uncommitted changes.
7. **GID-254879 Save Function** — The software shall allow the user to save all modified data by selecting Save, which shall persist changes to the database.
8. **GID-254880 Revert Function** — The software shall allow the user to discard all unsaved changes by selecting Revert, restoring the data to its last saved state.
9. **GID-254881 Undo Function** — The software shall allow the user to reverse the most recent unsaved change by selecting Undo.
10. **GID-254882 Unsaved Data Management Screen Availability** — The software shall provide unsaved data management options (Save, Revert, Undo) on the following screens: Patient, User, Profile, Device, Site, Facility, Location, ABR Protocols, DPOAE Protocols, Risk Factors, Comments, Patient Field Configuration.
11. **GID-255009 Unsaved Data Warning** — The software shall warn users before closing windows or navigating away when unsaved data would be lost.
12. **Data entered is written to the database and is still there after the application is closed and reopened.** This is the check that most screens fail today.
13. A success message appears only when something was actually saved.
14. The four checks above are satisfied.

### ASWD-2 F3 · Show and search the patient list

*5 coding days*

**What it is** — Show the patient list with the 6 required columns; Search by patient ID, first name, last name, date of birth, or test date range

**Done when**

1. **GID-254884 View Patient List** — The software shall allow users to view a list of patient records saved in the system.
2. **GID-254887 Patient Search** — The software shall provide search functionality on the patient list view, allowing users to search by patient ID, first name, last name, date of birth, or date of test range, and display the filtered results.
3. **GID-254890 Patient List Display Fields** — The software shall display the following information for each patient in the patient list view: · Patient ID / Hospital ID · Last Name · First Name · Date of Birth · Risk · Comment
4. The screen reads from the database. **No sample data written into the code remains.**
5. The four checks above are satisfied.

### ASWD-2 F4 · Show, delete and reassign test results, with permissions enforced

*11.5 coding days*

**What it is** — Create the `TestSessions` and `TestRecords` tables; Show the test list with the 7 required columns; Delete one test entry, admin only; Move a test result to the correct patient; Enforce "unless locked by permissions" on edit; limit test delete and reassignment to administrators. Patient delete is open to every user

**Done when**

1. **GID-254895 Delete Patient Test Entry** — The software shall allow administrative users to delete individual test entries from a patient's test result list.
2. **GID-254896 Test Result Reassignment** — The software shall allow administrative users to reassign test results to the correct patient in the system.
3. **GID-254897 Patient Test List Display Fields** — The software shall display the following information for each test of a selected patient: · Test Type · Left Ear Result · Right Ear Result · Date/Time of Test · Test Configuration · Duration · Examiner
4. The tables are created by a code-first migration, not by a hand-written script. A brand-new database is created correctly on first run.
5. Which database each table goes in is written into the epic design document and added to the running allocation table in the ASWD-77 design document (open item O1).
6. A repository can read, add, change and remove records, and is covered by tests.
7. **Data entered is written to the database and is still there after the application is closed and reopened.** This is the check that most screens fail today.
8. A success message appears only when something was actually saved.
9. The screen reads from the database. **No sample data written into the code remains.**
10. The capability described above works and is demonstrable.
11. The four checks above are satisfied.

### ASWD-2 F5 · Store the field setup, validate input and enforce mandatory fields

*7 coding days*

**What it is** — Create the `FieldSetup` and `SystemSettings` tables; Show which fields are mandatory; Only allow save when mandatory fields are filled; Validate input and block saving bad data; Show which field is wrong and what to fix

**Done when**

1. The tables are created by a code-first migration, not by a hand-written script. A brand-new database is created correctly on first run.
2. Which database each table goes in is written into the epic design document and added to the running allocation table in the ASWD-77 design document (open item O1).
3. A repository can read, add, change and remove records, and is covered by tests.
4. **Data entered is written to the database and is still there after the application is closed and reopened.** This is the check that most screens fail today.
5. A success message appears only when something was actually saved.
6. **GID-254892 Mandatory Field Indication** — The software shall indicate which fields are mandatory during patient data entry.
7. **GID-254893 Patient Save Validation** — The software shall allow the user to save patient information when all mandatory fields are populated.
8. **GID-254966 Data Validation** — The software shall validate user inputs and prevent saving of invalid or incomplete data according to configured validation rules.
9. **GID-255012 Validation Error Messages** — The software shall display error messages identifying which fields contain invalid data and what corrections are needed.
10. The four checks above are satisfied.

### ASWD-2 F6 · Configure patient fields, ID format, list sorting and prompts

*11 coding days*

**What it is** — Choose which patient fields are mandatory and which are shown; Configure the patient ID format check; Configure how the patient list is sorted; Turn on or off: confirm on save, confirm on delete, warn on change; Rename the "Available Field" labels

**Done when**

1. **GID-254961 Patient Mandatory Field Configuration** — The software shall allow administrative users to configure which patient record fields are mandatory during data entry.
2. **GID-254962 Patient Active Field Configuration** — The software shall allow administrative users to configure which patient record fields are active during data entry.
3. **GID-254960 Patient ID Format Validation Configuration** — The software shall allow administrative users to configure patient ID rule format validation.
4. **GID-254963 Patient List Sort Configuration** — The software shall allow administrative users to define how patient lists are sorted.
5. The screen reads from the database. **No sample data written into the code remains.**
6. **GID-254964 Data Confirmation** — The software shall allow configuration of the following user notifications: · Confirmation of saving · Confirmation of deletion · Data modification warning
7. **Data entered is written to the database and is still there after the application is closed and reopened.** This is the check that most screens fail today.
8. A success message appears only when something was actually saved.
9. **GID-255466 Custom Field Configuration** — The software shall provide customizable fields labeled "Available Field" that allow users to rename the fields.
10. The four checks above are satisfied. ---
11. The four checks above are satisfied.

### ASWD-2 F7 · Set a patient's risk factors and comments

*4.5 coding days*

**What it is** — Set risk factors with Yes / No / Unknown — read DF3 warning below; Assign a predefined comment, or write a patient-specific one

**Done when**

1. **GID-254888 Patient Risk Factor Selection** — The software shall provide an option to set risk factors in the patient details view with the following values: · Yes · No · Unknown
2. **GID-254889 Patient Comment Assignment** — The software shall provide an option to assign predefined or patient-specific comments to a patient record.
3. The four checks above are satisfied.

### ASWD-2 F8 · Manage and translate the risk factor list

*5.5 coding days*

**What it is** — Create the `RiskFactors` table; Create, view, edit and delete risk factors, blocking edit and delete when already used by a patient; Create `RiskFactorTranslations`; Enter translated name and description per language — sized by Q1

**Done when**

1. **GID-254950 Create Risk Factor** — The software shall allow administrative users to create predefined risk factors that can be assigned to patient records.
2. **GID-254951 View Risk Factor List** — The software shall allow administrative users to view a list of predefined risk factors that can be assigned to patient records.
3. **GID-254952 Edit Risk Factor** — The software shall allow administrative users to edit predefined risk factors that has not been assigned to a patient record.
4. **GID-254953 Delete Risk Factor** — The software shall allow administrative users to delete a list of predefined risk factors that is not assigned to a patient record.
5. The tables are created by a code-first migration, not by a hand-written script. A brand-new database is created correctly on first run.
6. Which database each table goes in is written into the epic design document and added to the running allocation table in the ASWD-77 design document (open item O1).
7. A repository can read, add, change and remove records, and is covered by tests.
8. **Data entered is written to the database and is still there after the application is closed and reopened.** This is the check that most screens fail today.
9. A success message appears only when something was actually saved.
10. **GID-254954 Risk Factor Translation** — The software shall allow administrative users to enter translated text for risk factor names and descriptions in each supported language.
11. A missing translation falls back to the default language rather than showing blank.
12. The four checks above are satisfied.

### ASWD-2 F9 · Manage and translate the comment list

*5.5 coding days*

**What it is** — Create `PredefinedComments`; Create, view, edit and delete comments, blocking delete when already used; Create `PredefinedCommentTranslations`; Enter translated text per language — sized by Q1

**Done when**

1. **GID-254955 Create Comment** — The software shall allow administrative users to create a comment that can be assigned to a patient record.
2. **GID-254956 View Comment List** — The software shall allow administrative users to view a list of predefined comments that can be assigned to a patient record.
3. **GID-254957 Edit Comment** — The software shall allow administrative users to edit a predefined comment that can be assigned to a patient record.
4. **GID-254958 Delete Comment** — The software shall allow administrative users to delete a predefined comment that is not assigned to a patient record.
5. The tables are created by a code-first migration, not by a hand-written script. A brand-new database is created correctly on first run.
6. Which database each table goes in is written into the epic design document and added to the running allocation table in the ASWD-77 design document (open item O1).
7. A repository can read, add, change and remove records, and is covered by tests.
8. **Data entered is written to the database and is still there after the application is closed and reopened.** This is the check that most screens fail today.
9. A success message appears only when something was actually saved.
10. **GID-254959 Comment Translation** — The software shall allow administrative users to enter translated text for comments in each supported language.
11. A missing translation falls back to the default language rather than showing blank.
12. The four checks above are satisfied.

### ASWD-2 F10 · Generate and print the patient test report

*3 coding days*

**What it is** — Generate and print the patient test report — may move to Epic 18, see Q5

**Done when**

1. **GID-254894 Patient Test Report Generation** — The software shall generate and print predefined patient test reports, including selected tests or all tests.
2. ⚠ **Blocked** — do not start until the open question is answered (see §8 of the plan). Starting on an assumption here means rework.
3. The four checks above are satisfied.

## Epic 3 · ASWD-32 — Data Exchange: import, export and S4H

*10 features · 81.9 days · milestone M1 · one developer: 2028-10-20*

> **One feature per format, import and export finished together** (D20). Slices: **32A** the file formats (M5), **32B** S4H (M7). Evidence for the reuse decisions: [AccuLink-DataExchange-Analysis.md](AccuLink-DataExchange-Analysis.md).

### ASWD-32 F1 · Exchange foundation: the record layer, settings, selection and de-identification P

*10.5 coding days · slice 32A*

**What it is** — Port the record-description and stream layer from AccuLink `Component.Core/Core/DataExchange` — column attributes, and the flat-file, binary and XML readers and writers; Create `ImportConfiguration` and `ExportConfiguration` and save the settings; Choose the export folder, format, and default format (GID-256274, GID-256286, GID-256512); Export new patients / all patients / selected entries / a test date range (GID-256275 to GID-256278); Strip demographics and identifiers before export (GID-256279); Connect every parser and writer to the patient repository, so an import is stored and an export reads real data

**Done when**

1. The record layer is ported and proven: a format is declared with column attributes, and the same declaration drives both reading and writing.
2. Flat-file, binary and XML readers and writers all work against a real file.
3. `ImportConfiguration` and `ExportConfiguration` exist and their settings survive a restart.
4. An export writes to the chosen folder, in the chosen format, and the default format is used when none is picked.
5. Each of the four selections works: new patients, all patients, the current selection, and a test date range.
6. With de-identification on, **no name, date of birth, identifier or contact detail reaches the file** — checked by reading the output, not the code.
7. An import saves to the database and the patients are still there after a restart. *(Today nothing is saved.)*
8. **GID-256274 Configurable Export Location** — The software shall allow users to specify the folder location for data export.
9. **GID-256275 Export New Patients with Data** — The software shall allow export of new patients with data.
10. **GID-256276 Export All Patients** — The software shall allow export of all patients.
11. **GID-256277 Export Selected Entries** — The device shall allow export of individually selected entries.
12. **GID-256278 Export by Test Date Range** — The software shall allow export of entries within a specified test date range.
13. **GID-256279 De-identified Data Export** — The software shall provide an export option to remove patient demographics and identifiers from the test data prior to sharing the data or for service review.
14. **GID-256286 Configurable Export Format** — The software shall allow users to select a default export format.
15. **GID-256512 Default Export Format** — The default export format shall be AccuSync JSON.

### ASWD-32 F2 · AccuSync XML and JSON — import and export

*5.5 coding days · slice 32A*

**What it is** — Agree and write down the AccuSync XML and JSON layout; AccuSync XML and JSON parsers; AccuSync XML and JSON writers with the required file names (GID-256280, GID-256281)

**Done when**

1. The AccuSync XML and JSON layouts are written down and agreed before either side is built.
2. A file AccuSync writes is a file AccuSync reads: **export then import returns the same patients and tests**.
3. File names match the requirement exactly.
4. Unit tests cover both formats in both directions, including an empty file and a malformed one.
5. **GID-254900 Supported Import Formats** — The software shall support the following formats to import:• AccuSync XML • AccuSync JSON• ALGO 5 XML• ALGO Pro JSON• AccuLink XML[Additional formats as required]
6. **GID-256280 AccuSync XML File Storage and Naming Convention** — The software shall store AccuSync XML files in ExportData/XML/ folder and named AccuSync_YYYY_MM_DD_HH_MM_SS.xml. The date and time should indicate when the AccuSync XML file was created.
7. **GID-256281 AccuSync JSON File Storage and Naming Convention** — The software shall store AccuSync JSON files in ExportData/JSON/ folder and named AccuSync_YYYY_MM_DD_HH_MM_SS.json. The date and time should indicate when the AccuSync JSON file was created.

### ASWD-32 F3 · CSV — import and export

*4 coding days · slice 32A*

**What it is** — 🔴 CSV writer (GID-256284) — *needs Q3; still the one format AccuLink cannot answer*; Export the on-screen patient list to CSV — Phase 1 per Natus 19 Aug 2026, covered by GID-254898, and it already works today; CSV import matching the writer

**Done when**

1. The on-screen patient list exports to CSV — this already works and must keep working.
2. CSV import reads back what CSV export writes.
3. 🔴 **Blocked until Q3 is answered.** GID-256283 and GID-256284 say *fixed width* for a format called CSV, and unlike HiTrack and OZ there is no AccuLink component to settle it. **This is the only format the AccuLink source could not answer.**
4. **GID-254898 Supported Export Formats** — The software shall support the following export formats:• AccuSync JSON • AccuSync XML• HiTrack• OZ• CSV• ALGO 5 XML[Additional formats as required]
5. **GID-256284 CSV Export File Storage and Naming Convention** — The software shall store CSV fixed width files in ExportData/CSV/ folder and named CSV_yyyymmdd_hhmmss.csv. The date and time should indicate when the CSV file was created.

### ASWD-32 F4 · ALGO 5 XML and ALGO Pro — import and export

*7 coding days · slice 32A*

**What it is** — Test harness plus tests for the three existing parsers; Fix the three format defects found in the samples: ALGO Pro duplicate `RiskFactors` keys, the missing ABR/DPOAE marker, and six fields encoded as text in one format and numbers in the other; ALGO 5 XML writer with the required file name (GID-256285); ALGO Pro writer

**Done when**

1. The three existing parsers have unit tests and a test harness. They have none today, across 3,430 lines.
2. **ALGO Pro's duplicate `RiskFactors` keys no longer lose 15 of 16 risk factors.**
3. A record with no ABR/DPOAE marker is classified correctly rather than guessed.
4. The six fields encoded as text in one format and numbers in the other read the same either way.
5. ALGO 5 XML and ALGO Pro files AccuSync writes are read back by AccuSync, and the ALGO 5 file name matches the requirement.
6. **GID-256285 ALGO 5 XML Export File Storage and Naming Convention** — The software shall store ALGO 5 XML files in ExportData/XML/ folder and named ALGO5_YYYY_MM_DD_HH_MM_SS.xml. The date and time should indicate when the ALGO 5 XML file was created.

### ASWD-32 F5 · AccuLink XML — import and export

*4.5 coding days · slice 32A*

**What it is** — AccuLink XML import through the existing parser, with tests; AccuLink XML writer and its typed document, from AccuLink `XmlPatientDataExporter` and the 35 classes generated from `AccuLinkXiMpLe.xsd`

**Done when**

1. AccuLink XML import works through the existing parser and has tests.
2. AccuLink XML export produces a document that matches `AccuLinkXiMpLe.xsd`.
3. **A file exported from AccuLink imports into AccuSync, and a file AccuSync exports is accepted where AccuLink files are accepted.** That round trip is the point of the format.

### ASWD-32 F6 · HiTrack — export, and pick-list import

*7.5 coding days · slice 32A*

**What it is** — HiTrack writer, including the duplicate `INTHS.txt` (GID-256282) — **the 114-column layout comes from AccuLink `HiTrackTest`, so Q3 no longer blocks it**; The export rules that go with it: result codes for deceased and discharged patients, risk-factor and comment aggregation, ethnicity and education code maps; HiTrack pick-list import: hospitals, physicians, audiologists, screeners, nursery types and race types

**Done when**

1. HiTrack export writes the 114 columns with the identifiers, lengths and date formats declared in AccuLink's `HiTrackTest`.
2. The duplicate `INTHS.txt` is written to both folders.
3. The export rules are carried over: result codes for deceased and discharged patients, risk-factor and comment aggregation, and the ethnicity and education code maps.
4. Pick-list import brings in hospitals, physicians, audiologists, screeners, nursery types and race types.
5. ⚠ **X2 must be confirmed:** HiTrack import is pick lists in AccuLink, not patient records. If Natus expects patient import, this feature is sized wrongly.
6. **GID-256282 HiTrack File Storage and Naming Convention** — The software shall store HiTrack files for the state in ExportData/SummaryFiles/YYYY/YYYY-MON/ folder and named state_yyyymmdd_hhmmss.txt. A duplicate file named INTHS.txt shall be stored here as well as in the folder HiTrackExportData. The date and time should indicate when the HiTrack file was created. MON indicates a three letter month abbreviation.

### ASWD-32 F7 · OZ — binary export

*3 coding days · slice 32A*

**What it is** — OZ writer (GID-256283) — **the binary record layout comes from AccuLink `Oz7Test`, so Q3 no longer blocks it**; ⚠ AccuLink has no OZ importer and none is specified. **X1 must be answered before any OZ import is planned**

**Done when**

1. OZ export writes the binary record declared in AccuLink's `Oz7Test`, including the termination rule on text columns.
2. A file AccuSync writes is byte-comparable with one AccuLink writes for the same patients.
3. ⚠ **X1 is open: there is no OZ import.** AccuLink has none and no format owner is named. If Natus wants one, it is new work and is not in this estimate.
4. **GID-256283 OZ Export File Storage and Naming Convention** — The software shall store OZ fixed width files for the state in ExportData/OZ/ folder and named state_yyyymmdd_hhmmss.txt. The date and time should indicate when the OZ file was created.

### ASWD-32 F8 · S4H: the connector and the sync engine

*6.5 coding days · slice 32B*

**What it is** — SOAP client for the SEDQ service — **the contract is `northgate.wsdl` with `uploadData` and `downloadSyncData`, so the 3-day discovery is replaced by a read**; Client plus a screen to set the web service URL (GID-254974); The shared sync framework and its trigger rules; 🔴 Send patient and test data to an external system over a web service (GID-254899) — *needs Q7*

**Done when**

1. The SOAP client talks to the SEDQ service using the operations in `northgate.wsdl`, `uploadData` and `downloadSyncData`.
2. The web service URL is configurable and is validated before use.
3. The sync framework runs on its agreed triggers, and a failed sync is reported and retried rather than lost.
4. ⚠ **The AccuLink client itself does not port.** `SoapHttpClientProtocol` is .NET Framework only; .NET 10 needs a generated WCF client or SOAP over `HttpClient`. **The contract ports, the client does not.**
5. 🔴 Sending patient and test data to an external system still needs **Q7**.
6. **GID-254899 Patient Data Export** — The software shall export patient and test data to supported external systems via file or web service.
7. **GID-254974 SEDQ Web Service URL** — The software shall provide an option to configure the SEDQ Web Service URL .

### ASWD-32 F9 · S4H: receive users, devices, facilities and risk factors

*10.5 coding days · slice 32B*

**What it is** — Receive the user list and store username and user ID, read-only (GID-254975, GID-254982, GID-254983); Auto-deactivate and reactivate, default profile and password, block admin adding users (GID-254977 to GID-254981); GID-254984 to GID-254987; GID-254988 to GID-254991; GID-254992 to GID-254995

**Done when**

1. Users, devices, facilities and risk factors received from S4H are stored and shown as read-only.
2. A record S4H deactivates is deactivated locally, and reactivated when S4H reactivates it.
3. An administrator cannot add a user locally, and cannot edit a synchronised field.
4. A new user gets the default profile and password.
5. ⚠ Four requirements in this block (GID-254984 to GID-254987) **have rotated titles in Jama**. The text is correct, the titles are not — traceability that matches on titles will point at the wrong requirement.
6. **GID-254975 User List Synchronization** — The software shall receive the user list from the SEDQ web service.
7. **GID-254977 Restrict User Configration** — The software shall prevent administrative users from adding a user account.
8. **GID-254978 Default Profile Assignment** — The software shall provide an option to select a default user profile to assign to all new users imported from S4H.
9. **GID-254979 Default Password Assignment** — The software shall provide an option to set the default password to apply to all new users dowloaded from S4H.
10. **GID-254980 Automatic User Deactivation** — The software shall automatically set the account status to "Inactive" for any active user accounts whose identifiers are not present in the received synchronisation data.
11. **GID-254981 Automatic User Reactivation** — The software shall automatically restore the account status to "Active" for any inactive user accounts whose identifiers are present in the received synchronisation data.
12. **GID-254982 User Attribute Synchronisation** — The software shall retrieve and store Username and User ID from the synchronisation data.
13. **GID-254983 Read-Only User Attributes** — The software shall prevent modification of Username and User ID except through synchronisation updates.
14. **GID-254984 Automatic Device Deactivation** — The software shall retrieve and store Device ID, Name, and Serial Number from the synchronisation data.
15. **GID-254985 Automatic Device Reactivation** — The software shall prevent modification of Device ID, Name, and Serial Number except through synchronisation updates.
16. **GID-254986 Device Attribute Synchronisation** — The software shall automatically set the device status to "Inactive" for any active devices whose identifiers are not present in the received synchronisation data.
17. **GID-254987 Read-Only Device Attributes** — The software shall automatically restore the device status to "Active" for any inactive devices whose identifiers are present in the received synchronisation data.
18. **GID-254988 Automatic Facility Deactivation** — The software shall automatically set the facility status to "Inactive" for any active facilities whose identifiers are not present in the received synchronisation data.
19. **GID-254989 Automatic Facility Reactivation** — The software shall automatically restore the facility status to "Active" for any inactive facilities whose identifiers are present in the received synchronisation data.
20. **GID-254990 Facility Attribute Synchronisation** — The software shall retrieve and store Facility ID, Name, and Type from the synchronisation data.
21. **GID-254991 Read-Only Facility Attributes** — The software shall prevent modification of Facility ID, Name, and Type except through synchronisation updates.
22. **GID-254992 Automatic Risk Factor Deactivation** — The software shall automatically set the risk factor status to "Inactive" for any active risk factors whose identifiers are not present in the received synchronisation data.
23. **GID-254993 Automatic Risk Factor Reactivation** — The software shall automatically restore the risk factor status to "Active" for any inactive risk factors whose identifiers are present in the received synchronisation data.
24. **GID-254994 Risk Factor Attribute Synchronisation** — The software shall retrieve and store Risk Factor ID and Risk Factor Value from the synchronisation data.
25. **GID-254995 Read-Only Risk Factor Attributes** — The software shall prevent modification of Risk Factor ID and Risk Factor Value except through synchronisation updates.

### ASWD-32 F10 · S4H: send identifiers, results and NHSP wording

*7 coding days · slice 32B*

**What it is** — Site ID, device ID, and inpatient/outpatient facility lists (GID-254972, GID-254973) — *needs Epic 21, which is why this epic now runs after it*; Test results including binary waveform data (GID-254976); NHSP wording: Pass → Clear Response, Refer → No Clear Response, Surname, Forename (GID-254971)

**Done when**

1. Site ID, device ID and the inpatient/outpatient facility lists are sent — which is why this feature runs after Epic 21.
2. Test results are sent including binary waveform data, and the service accepts them.
3. NHSP wording is applied: Pass → Clear Response, Refer → No Clear Response, and Surname and Forename replace the default labels.
4. **GID-254971 UK Terminology Standards** — The device shall use English (UK) terminology as defined by NHSP, including: Pass = Clear Response (CR), Refer = No Clear Response (NCR), Surname = Last Name, and Forename = First Name.
5. **GID-254972 Synchronize Identifiers and Facility Lists** — The software shall synchronize the following fields with the device:• Site Identifier• Device Identifier• List of inpatient facilities • List of outpatient facilities
6. **GID-254973 Site identifier Configuration** — The software shall provide an option to configure the Site identifier.
7. **GID-254976 Test Results Transfer** — The software shall support transfer of screening test results, including binary waveform data, into S4H.
## Epic 5 · ASWD-5 — OAE Test Result

*3 features · 13.1 days · milestone M1 · one developer: 2027-06-18*

### ASWD-5 F1 · Investigate the waveform data format and how to draw it

*2 coding days*

**What it is** — 2-day timeboxed look at the waveform data format and how to draw it

**Done when**

1. The investigation ends with a **written finding** that says what was learned and whether the estimate still holds. If it does not, the estimate is corrected before the next story starts.
2. The capability described above works and is demonstrable.
3. The four checks above are satisfied.

### ASWD-5 F2 · Create the OAE result tables and load real device data into them

*2 coding days*

**What it is** — Create `TEOAEResults` and `DPOAEResults` tables and waveform storage; Small utility that pushes a real device file through the existing ALGO 5 / AccuLink / ALGO Pro parsers straight into the test tables, so the result views can be demonstrated on genuine screening data before Import is built

**Done when**

1. The tables are created by a code-first migration, not by a hand-written script. A brand-new database is created correctly on first run.
2. Which database each table goes in is written into the epic design document and added to the running allocation table in the ASWD-77 design document (open item O1).
3. A repository can read, add, change and remove records, and is covered by tests.
4. **Data entered is written to the database and is still there after the application is closed and reopened.** This is the check that most screens fail today.
5. A success message appears only when something was actually saved.
6. The four checks above are satisfied. ---
7. The four checks above are satisfied.

### ASWD-5 F3 · Show TEOAE and DPOAE results with waveform, comments and device info

*5.5 coding days*

**What it is** — Details, waveform, comments and device info for TEOAE; Same for DPOAE

**Done when**

1. **GID-254901 TEOAE Test Result** — The software shall allow users to view patient test result details, waveform data, test comments, and device information for the TEOAE test.
2. **GID-254902 DPOAE Test Result** — The software shall allow users to view patient test result details, waveform data, test comments, and device information for the DPOAE test.
3. The four checks above are satisfied.

## Epic 6 · ASWD-6 — ABR Test Result

*1 feature · 8.5 days · milestone M1 · one developer: 2027-07-02*

### ASWD-6 F1 · Create the ABR result table and show results with EEG noise and impedance

*4 coding days*

**What it is** — Create the `ABRResults` table; Details, waveform, comments, device info, plus EEG noise and impedance

**Done when**

1. The tables are created by a code-first migration, not by a hand-written script. A brand-new database is created correctly on first run.
2. Which database each table goes in is written into the epic design document and added to the running allocation table in the ASWD-77 design document (open item O1).
3. A repository can read, add, change and remove records, and is covered by tests.
4. **Data entered is written to the database and is still there after the application is closed and reopened.** This is the check that most screens fail today.
5. A success message appears only when something was actually saved.
6. **GID-254903 ABR Test Result** — The software shall allow users to view patient test result details, waveform data, test comments, and device information for the ABR test.
7. The four checks above are satisfied. ---
8. The four checks above are satisfied.

## Epic 7 · ASWD-7 — User Account Management

*2 features · 28.8 days · milestone M1 · one developer: 2027-08-27*

### ASWD-7 F1 · Manage user accounts: create, edit, delete, activate, unlock and set language

*7.5 coding days*

**What it is** — View the user list; Create a user; Edit a user; Delete a user; Activate and deactivate; Unlock an account; Choose the display language for each user; Wire this screen's Add/Edit/Delete and Save/Revert/Undo onto Epic 2 F2's shared framework, replacing its own `Stack<UserSnapshot>` copy; Prevent transfer of deactivated user accounts to the device

**Done when**

1. **GID-254904 View User List** — The software shall allow administrative users to view a list of user accounts.
2. **GID-254905 Create User Account** — The software shall allow administrative users to create a user account.
3. **GID-254906 Edit User Account** — The software shall allow administrative users to edit a user account.
4. **Data entered is written to the database and is still there after the application is closed and reopened.** This is the check that most screens fail today.
5. A success message appears only when something was actually saved.
6. The screen reads from the database. **No sample data written into the code remains.**
7. **GID-254907 Unlock User Account** — The software shall allow administrative users to unlock a user account.
8. **GID-254908 Delete User Account** — The software shall allow administrative users to delete a user account.
9. **GID-254909 User Account Activation** — The software shall allow administrative users to activate/deactivate a user account.
10. **GID-254910 User Language Selection** — The software shall allow administrative users to select the display language per user.
11. This screen's own copy of undo or change-tracking code is deleted, not left alongside the shared one.
12. The capability described above works and is demonstrable.
13. The four checks above are satisfied. ---
14. **GID-255022 Deactivated User Transfer Prevention** — The software shall prevent transfer of deactivated user accounts to the device.
15. The four checks above are satisfied.

### ASWD-7 F2 · Apply and configure the password and lockout rules everywhere

*6 coding days*

**What it is** — Move the password rules into `Core` and apply them to every place a password is set. Remove the hardcoded `"1234"` defaults. Add a real entry point to the change-password screen for a signed-in user; Let an admin choose None / Simple / Complex; Screen to set the lockout duration; Decide what happens to the 90-day expiry that is in the code but in no requirement

**Done when**

1. **GID-255020 Password Complexity Requirements** — The software shall enforce password complexity requirements each user account: · At least 8 characters · At least one upper case letter (A to Z) · At least one lower case letter (a to z) · At least one numerical digit · The password cannot be the same as the three previously used passwords
2. **Data entered is written to the database and is still there after the application is closed and reopened.** This is the check that most screens fail today.
3. A success message appears only when something was actually saved.
4. **GID-254912 Password Complexity Configuration** — The software shall allow administrative users to configure the password complexity (None, Simple, Complex).
5. ⚠ **Blocked** — do not start until Q2 is answered (see §8 of the plan). Starting on an assumption here means rework.
6. **GID-254911 Account Lockout Duration Configuration** — The software shall allow administrative users to configure the lockout duration after 5 consecutive lockouts.
7. The capability described above works and is demonstrable.
8. ⚠ **Blocked** — do not start until Q4 is answered (see §8 of the plan). Starting on an assumption here means rework.
9. The four checks above are satisfied.

## Epic 8 · ASWD-8 — Profile Management

*3 features · 28.5 days · milestone M1 · one developer: 2027-09-24*

### ASWD-8 F1 · Create, view, edit and delete permission profiles

*4.5 coding days*

**What it is** — Create the `Profiles` table with its permission fields; View, create, edit and delete profiles, and show Name / Description / Permissions; Wire this screen's Add/Edit/Delete and Save/Revert/Undo onto Epic 2 F2's shared framework, replacing any local copy

**Done when**

1. **GID-254913 View Profile List** — The software shall allow administrative users to view a list of profiles and feature access rights.
2. **GID-254914 Create Profile** — The software shall allow administrative users to create a profile.
3. **GID-254915 Edit Profile** — The software shall allow administrative users to edit a profile.
4. **GID-254916 Delete Profile** — The software shall allow administrative users to delete a profile.
5. **GID-254917 Profile Display Fields** — The software shall display the following information for a selected profile: · Name · Description · Components and Permissions
6. The tables are created by a code-first migration, not by a hand-written script. A brand-new database is created correctly on first run.
7. Which database each table goes in is written into the epic design document and added to the running allocation table in the ASWD-77 design document (open item O1).
8. A repository can read, add, change and remove records, and is covered by tests.
9. **Data entered is written to the database and is still there after the application is closed and reopened.** This is the check that most screens fail today.
10. A success message appears only when something was actually saved.
11. This screen's own copy of undo or change-tracking code is deleted, not left alongside the shared one.
12. The capability described above works and is demonstrable.
13. The four checks above are satisfied. ---
14. The four checks above are satisfied.

### ASWD-8 F2 · Set permissions per profile, enforce them app-wide and retire the hardcoded roles

*5 coding days*

**What it is** — Set permissions per profile and enforce them across the app; Retire the fixed presets in `UserPermissionsViewModel` and read permissions from the database

**Done when**

1. **GID-254918 Profile Permission Configuration** — The software shall allow administrative users to configure permissions for each profile.
2. The capability described above works and is demonstrable.
3. The four checks above are satisfied.

### ASWD-8 F3 · Complete RBAC: one authorization mechanism, enforced everywhere P

*15 coding days*

**What it is** — Capture the **33 permission fields** from `Databases/SettingsDatabase.sql` before that file is deleted, and write them down as the permission model; One authorization service in `Core`: who is signed in, their effective permissions, and a single `Can(permission)` check that everything calls; Enforce in the UI — every command’s `CanExecute`, and a blocked action hidden rather than merely greyed out; Enforce again in the Application layer, so going around a screen does not go around the rule; Re-evaluate a signed-in user’s permissions when their profile changes, without a restart; Every denied action is logged and audited; The **permission matrix**: 33 permissions × every gated action, written down as both the specification and the test oracle; Tests proving each permission gates its action **at both layers**, and that patient delete stays available to every user per the 22 Aug decision

> ⚠ **Pulled forward to just before Epic 2.** Every epic after it needs gating; built at Epic 8’s own position it would arrive after Patient Management, the test views and user accounts were already built against a temporary role check. **This is X8z from §9.11 made real**, not an extra 14 days on top of it.

**Done when**

1. The **33 permission fields** are captured from `Databases/SettingsDatabase.sql` **before that file is deleted**, and written down as the permission model. If Epic 0 F1 skips this, the specification is gone.
2. There is **one** authorization service in `Core`. No screen, service or repository does its own role check.
3. Every command's `CanExecute` consults it, and a blocked action is **hidden, not merely greyed out** — today the sidebar's permission-hiding never fires at all (X1a).
4. **The Application layer enforces the same rule independently.** Calling a service directly, without going through the screen, is refused. A UI-only check is not access control.
5. Changing a user's profile changes what they can do **without a restart**.
6. **Every denial is logged and audited** — who, what, when, and which permission refused it.
7. The **permission matrix** exists: 33 permissions × every gated action, and it is the document the tests are written from.
8. Tests prove each permission gates its action **at both layers**.
9. **Patient delete remains available to every user**, per the Natus decision of 22 Aug 2026 — the matrix must not quietly turn it into an admin-only action.
10. `UserPermissionsViewModel`'s three hardcoded presets are gone, including `ReadOnly`, which is not in the SRS.
## Epic 9 · ASWD-9 — Device Management

*3 features · 20.8 days · milestone M1 · one developer: 2027-10-29*

### ASWD-9 F1 · Create, view, edit and delete screening devices

*5.5 coding days*

**What it is** — Create `Devices` and related tables; View the device list with Name / Serial / Last Seen; Add, edit and delete a device; Wire this screen's Add/Edit/Delete and Save/Revert/Undo onto Epic 2 F2's shared framework, replacing any local copy

**Done when**

1. **GID-254919 Device List Display Fields** — The software shall allow administrative users to view a list of devices with the following information: · Name · Serial number · Last Seen
2. **GID-254920 Add Device** — The software shall allow administrative users to add a new device.
3. **GID-254921 Edit Device** — The software shall allow administrative users to edit device information.
4. **GID-254922 Delete Device** — The software shall allow administrative users to delete a device.
5. The tables are created by a code-first migration, not by a hand-written script. A brand-new database is created correctly on first run.
6. Which database each table goes in is written into the epic design document and added to the running allocation table in the ASWD-77 design document (open item O1).
7. A repository can read, add, change and remove records, and is covered by tests.
8. **Data entered is written to the database and is still there after the application is closed and reopened.** This is the check that most screens fail today.
9. A success message appears only when something was actually saved.
10. The screen reads from the database. **No sample data written into the code remains.**
11. Tested against a real AccuScreen Pro, or an emulator agreed with the customer. Not against mocked data.
12. This screen's own copy of undo or change-tracking code is deleted, not left alongside the shared one.
13. The capability described above works and is demonstrable.
14. The four checks above are satisfied. ---
15. The four checks above are satisfied.

### ASWD-9 F2 · Assign users and facilities to a device

*3 coding days*

**What it is** — GID-254923; GID-254924

**Done when**

1. **GID-254923 Device User Assignment** — The software shall allow administrative users to assign users to each device.
2. **GID-254924 Device Facility Assignment** — The software shall allow administrative users to assign facilities to each device.
3. The four checks above are satisfied.

### ASWD-9 F3 · Configure on-device settings and show device system information

*4.5 coding days*

**What it is** — Configure the 7 device settings; Last Seen, Last Updated, Hardware Version, Firmware Version

**Done when**

1. **GID-254925 Device Settings Configuration** — The software shall allow administrative users to configure the following device settings: · Display timeout · Power timeout · Calibration/pause time · Result terminology · Automatic deletion · ABR Autostart · TEOAE Probe Fit Assistant ⚠ **safety-related (`U*`)**
2. Tested against a real AccuScreen Pro, or an emulator agreed with the customer. Not against mocked data.
3. ⚠ **Blocked** — do not start until Q8 is answered (see §8 of the plan). Starting on an assumption here means rework.
4. **GID-254926 Device System Information Display** — The software shall display the following System Information for a selected device: · Last Seen · Last Updated · Hardware Version · Firmware Version
5. The four checks above are satisfied.

## Epic 10 · ASWD-10 — Site Management

*2 features · 9.5 days · milestone M4 · one developer: 2027-11-19*

### ASWD-10 F1 · Create, view, edit and delete sites, and set what syncs to devices

*3.25 coding days*

**What it is** — Create the `Sites` table; View, add, edit and delete sites with Name / Description / Code; Wire this screen's Add/Edit/Delete and Save/Revert/Undo onto Epic 2 F2's shared framework, replacing any local copy; Setting for whether the site and facility list is pushed to devices during synchronisation — cannot be finished until Epic 21

**Done when**

1. **GID-254927 View Site List** — The software shall allow administrative users to view a list of sites and associated details.
2. **GID-254928 Site Detail Fields** — The software shall allow administrative users to enter the following site details: · Name · Description · Code
3. **GID-254929 Add Site** — The software shall allow administrative users to add a new site.
4. **GID-254930 Edit Site** — The software shall allow administrative users to edit site details.
5. **GID-254931 Delete Site** — The software shall allow administrative users to delete a site.
6. The tables are created by a code-first migration, not by a hand-written script. A brand-new database is created correctly on first run.
7. Which database each table goes in is written into the epic design document and added to the running allocation table in the ASWD-77 design document (open item O1).
8. A repository can read, add, change and remove records, and is covered by tests.
9. **Data entered is written to the database and is still there after the application is closed and reopened.** This is the check that most screens fail today.
10. A success message appears only when something was actually saved.
11. This screen's own copy of undo or change-tracking code is deleted, not left alongside the shared one.
12. The capability described above works and is demonstrable.
13. The four checks above are satisfied. ---
14. **GID-255001 Site Configuration Transfer** — The software provide an option to transfer Site configuation data to a connected AccuScreen Pro device.
15. **GID-255002 Facility Configuration Transfer** — The software provide an option to transfer Facility configuration data to a connected AccuScreen Pro device.
16. The four checks above are satisfied.

### ASWD-10 F2 · Shared “can this record be deleted?” check reused by seven screens

*2.75 coding days*

**What it is** — One shared rule set for "can this record be deleted?", reused by Facility, Location, Profile, Device, Protocol, Risk Factor and Comment

**Done when**

1. **Data entered is written to the database and is still there after the application is closed and reopened.** This is the check that most screens fail today.
2. A success message appears only when something was actually saved.
3. The capability described above works and is demonstrable.
4. The four checks above are satisfied.

## Epic 11 · ASWD-11 — Facility Management

*1 feature · 7.2 days · milestone M4 · one developer: 2027-12-03*

### ASWD-11 F1 · Create, view, edit and delete facilities

*3.5 coding days*

**What it is** — Create the `Facilities` table; View, add, edit and delete facilities with Name / Description / Code / Site / Location Type; Reuse Epic 10's "in use" check; Wire this screen's Add/Edit/Delete and Save/Revert/Undo onto Epic 2 F2's shared framework, replacing any local copy

**Done when**

1. **GID-254932 View Facility List** — The software shall allow administrative users to view a list of facilities and associated details.
2. **GID-254933 Facility Detail Fields** — The software shall allow administrative users to enter the following facility details: · Name · Description · Code · Site · Location Type
3. **GID-254934 Add Facility** — The software shall allow administrative users to add a facility.
4. **GID-254935 Edit Facility** — The software shall allow administrative users to edit a facility.
5. **GID-254936 Delete Facility** — The software shall allow administrative users to delete a facility.
6. The tables are created by a code-first migration, not by a hand-written script. A brand-new database is created correctly on first run.
7. Which database each table goes in is written into the epic design document and added to the running allocation table in the ASWD-77 design document (open item O1).
8. A repository can read, add, change and remove records, and is covered by tests.
9. **Data entered is written to the database and is still there after the application is closed and reopened.** This is the check that most screens fail today.
10. A success message appears only when something was actually saved.
11. This screen's own copy of undo or change-tracking code is deleted, not left alongside the shared one.
12. The capability described above works and is demonstrable.
13. The four checks above are satisfied. ---
14. The four checks above are satisfied.

## Epic 12 · ASWD-12 — Location Management

*1 feature · 7.2 days · milestone M4 · one developer: 2027-12-10*

### ASWD-12 F1 · Create, view, edit and delete locations, and assign one to every facility

*4.5 coding days*

**What it is** — Create the `Locations` table; View, add, edit and delete locations with Name / Description / Code; When adding a location, offer to assign it to every facility; Wire this screen's Add/Edit/Delete and Save/Revert/Undo onto Epic 2 F2's shared framework, replacing any local copy

**Done when**

1. **GID-254937 View Location List** — The software shall allow administrative users view a list of locations and associated details
2. **GID-254938 Location Detail Fields** — The software shall allow administrative users to enter the following location details: · Name · Description · Code
3. **GID-254939 Add Location** — The software shall allow administrative users to add a new location.
4. **GID-254940 Edit Location** — The software shall allow administrative users to edit a location.
5. **GID-254941 Delete Location** — The software shall allow administrative users to delete a location.
6. The tables are created by a code-first migration, not by a hand-written script. A brand-new database is created correctly on first run.
7. Which database each table goes in is written into the epic design document and added to the running allocation table in the ASWD-77 design document (open item O1).
8. A repository can read, add, change and remove records, and is covered by tests.
9. **Data entered is written to the database and is still there after the application is closed and reopened.** This is the check that most screens fail today.
10. A success message appears only when something was actually saved.
11. **GID-255465 Assign Location** — When adding a new location, the software shall provide an option to assign the location to all facilities.
12. This screen's own copy of undo or change-tracking code is deleted, not left alongside the shared one.
13. The capability described above works and is demonstrable.
14. The four checks above are satisfied. ---
15. The four checks above are satisfied.

## Epic 13 · ASWD-13 — ABR Test Protocol Configuration

*1 feature · 7 days · milestone M4 · one developer: 2027-12-24*

### ASWD-13 F1 · Create, view, edit and delete ABR test protocols

*4.5 coding days*

**What it is** — Create the `ABRProtocols` table with its parameter value ranges; View, create, edit and delete ABR protocols; Wire this screen's Add/Edit/Delete and Save/Revert/Undo onto Epic 2 F2's shared framework, replacing any local copy

**Done when**

1. **GID-254942 Create ABR Protocol** — The software shall allow administrative users create ABR test protocol configurations.
2. **GID-254943 View ABR Protocol List** — The software shall allow administrative users view a list of ABR test protocol configurations.
3. **GID-254944 Edit ABR Protocol** — The software shall allow administrative users to edit an existing ABR test protocol configuration.
4. **GID-254945 Delete ABR Protocol** — The software shall allow administrative users to delete an existing ABR test protocol configuration.
5. The tables are created by a code-first migration, not by a hand-written script. A brand-new database is created correctly on first run.
6. Which database each table goes in is written into the epic design document and added to the running allocation table in the ASWD-77 design document (open item O1).
7. A repository can read, add, change and remove records, and is covered by tests.
8. **Data entered is written to the database and is still there after the application is closed and reopened.** This is the check that most screens fail today.
9. A success message appears only when something was actually saved.
10. This screen's own copy of undo or change-tracking code is deleted, not left alongside the shared one.
11. The capability described above works and is demonstrable.
12. The four checks above are satisfied. ---
13. The four checks above are satisfied.

## Epic 14 · ASWD-14 — DPOAE Test Protocol Configuration

*1 feature · 8.5 days · milestone M4 · one developer: 2028-01-14*

### ASWD-14 F1 · Create, view, edit and delete DPOAE test protocols

*4.5 coding days*

**What it is** — Create the `DPOAEProtocols` table with its parameter value ranges; View, create, edit and delete DPOAE protocols; Wire this screen's Add/Edit/Delete and Save/Revert/Undo onto Epic 2 F2's shared framework, replacing any local copy

**Done when**

1. **GID-254946 Create DPOAE Protocol** — The software shall allow administrative users create DPOAE test protocol configurations.
2. **GID-254947 View DPOAE Protocol List** — The software shall allow administrative users view a list of DPOAE test protocol configurations.
3. **GID-254948 Edit DPOAE Protocol** — The software shall allow administrative users to edit an existing DPOAE test protocol configuration.
4. **GID-254949 Delete DPOAE Protocol** — The software shall allow administrative users to delete an existing DPOAE test protocol configuration.
5. The tables are created by a code-first migration, not by a hand-written script. A brand-new database is created correctly on first run.
6. Which database each table goes in is written into the epic design document and added to the running allocation table in the ASWD-77 design document (open item O1).
7. A repository can read, add, change and remove records, and is covered by tests.
8. **Data entered is written to the database and is still there after the application is closed and reopened.** This is the check that most screens fail today.
9. A success message appears only when something was actually saved.
10. This screen's own copy of undo or change-tracking code is deleted, not left alongside the shared one.
11. The capability described above works and is demonstrable.
12. The four checks above are satisfied. ---
13. The four checks above are satisfied.

## Epic 18 · ASWD-18 — Report Generation

*4 features · 19.2 days · milestone M6 · one developer: 2028-05-19*

> **Restructured 27 Aug 2026 so every feature is one workflow**, and **Epic 2 F10 absorbed into F2**, which answers Q5.

### ASWD-18 F1 · Choose the reporting library

*1.5 coding days*

**What it is** — 1.5-day timeboxed investigation — there is no reporting library in the solution today, and the choice constrains everything below

**Done when**

1. The comparison is **timeboxed to 1.5 days** and ends in a written decision, not an open-ended evaluation.
2. Each candidate is judged on the same points: licence cost, output formats, print support, template authoring, and whether it works on .NET 10 WPF.
3. The decision names what every later feature in this epic builds against.

### ASWD-18 F2 · Generate, preview and print a patient report P

*18.5 coding days*

**What it is** — Pick a patient, choose **selected tests or all tests**, and choose one of the predefined report types (GID-254894); Render the report with demographic information and test result details (GID-255112); The **ten predefined report templates** — Phase 1 per Natus 19 Aug 2026, *needs a new requirement naming the ten*; Preview it on screen before committing to paper; Print, with a progress indicator

**Done when**

1. A screener picks a patient, chooses **selected tests or all tests**, and chooses one of the predefined report types.
2. The report shows **demographic information and test result details** — the two things GID-255112 names.
3. **The ten predefined report types exist.** ⚠ Phase 1 per Natus 19 Aug 2026, but **no requirement names the ten** — one is still needed.
4. The report can be **previewed on screen** before anything is printed.
5. Printing shows a progress indicator, and a long report does not freeze the screen.
6. A report for a patient with no tests produces a sensible document, not an error.

### ASWD-18 F3 · Save a report to a file

*3 coding days*

**What it is** — 🔴 Export the report to a supported file format (GID-255113) — *the format list is still needed*

**Done when**

1. A report already generated can be saved to a file without regenerating it.
2. ⚠ **The list of supported formats is still needed.** GID-255113 says “supported file formats” without saying which.
3. The saved file opens correctly in whatever reads that format.

### ASWD-18 F4 · Set the report layout: logo and paper size

*5 coding days*

**What it is** — Add a logo from a graphics file and choose the paper format (GID-254965); Apply the layout to every report type, and show the change in the preview

**Done when**

1. An administrator can add a logo from a graphics file and choose the paper format.
2. The layout applies to **every** report type, not just the one being looked at.
3. The change is visible in the preview before anything is printed.
4. A logo that is too large for the paper is scaled or rejected with a clear message, not silently cropped.

## Epic 19 · ASWD-19 — Language

*6 features · 35.4 days · milestone M6 · one developer: 2029-03-16*

> **All 14 languages ship in this release** (Natus, 27 Aug 2026). Slices: **19A** switching and the string pack (M6), **19B** the three language phases (M8) — the phases cannot start until the strings are frozen, and they are not frozen while screens are still being added.

### ASWD-19 F1 · Investigate and build the language switching infrastructure P

*5 coding days · slice 19A*

**What it is** — 2-day timeboxed look at satellite assemblies, per-user language, and **Chinese and Japanese text layout — now in scope, not deferred**; Language switching, English as default, language picker and confirmation prompt (GID-254967, GID-256513, GID-254970)

**Done when**

1. A user picks a language, confirms, and **the whole application switches without a restart** — Natus’s preference, 27 Aug 2026. Where a screen cannot manage that, a restart is acceptable and is written down as a known exception.
2. English is the default on a fresh install.
3. The language is remembered per user, not per machine.
4. The investigation covers **Chinese and Japanese text layout** before any code is written — it is no longer deferred.

### ASWD-19 F2 · Merge the duplicated strings and cover every screen

*5.5 coding days · slice 19A*

**What it is** — The same text exists twice — 1,206 entries in `AccuSync.WPF` and 1,213 in `AccuSync.Application`, which de-duplicate to **860 distinct strings**. Merge them, and move English text out of the service classes; Go through all text, messages, menus, prompts and results (GID-254969)

**Done when**

1. The 860 distinct strings live in **one place**, not duplicated across two projects.
2. No English text remains in the service classes.
3. Every screen, message, menu, prompt and result reads from resources (GID-254969).
4. A missing resource key fails visibly in a test run, not silently at a customer site.

### ASWD-19 F3 · Review and freeze the strings, and produce the translation pack P

*5 coding days · slice 19A*

**What it is** — **Review and finalise all 860 strings before any translation starts** — Natus’s condition, 27 Aug 2026; Build the pack the distribution partners receive: every string with the screen it appears on, **plus a screenshot of each screen** — Natus asked for screenshots; Capture the screenshots once the screens are final; Import returned translations and flag anything missing, changed or untranslated

**Done when**

1. **All 860 strings are reviewed and finalised before anything is sent for translation** — Natus’s stated condition.
2. The pack gives each string **the screen it appears on**, and includes **a screenshot of every screen** — Natus asked for screenshots because context changes a translation.
3. Screenshots are captured after the screens are final, not from a design.
4. Returned translations import cleanly, and anything **missing, changed or still in English** is flagged rather than shipped.
5. ⚠ The pack does not go out while screens are still being added. Sending early means paying the partners to translate the same strings twice.

### ASWD-19 F4 · Phase 1: French, Italian, German, Spanish

*3 coding days · slice 19B*

**What it is** — Resource set, load and smoke-test every screen for each of the four (GID-254968 — the full 14-language list, delivered across F4, F5 and F6); Fix the layout truncation real translations expose — German and French run 20–30% longer than English

**Done when**

1. French, Italian, German and Spanish each load and every screen reads correctly (GID-254968, Phase 1).
2. **Layout truncation from real translations is fixed**, not accepted — German and French run 20–30% longer than English.
3. Switching between all five languages, including back to English, leaves no stale text on screen.

### ASWD-19 F5 · Phase 2: Brazilian Portuguese, Chinese (simplified), Chinese (traditional), Japanese

*6 coding days · slice 19B*

**What it is** — Resource set, load and smoke-test for each of the four; **Chinese and Japanese support**: fonts that carry the full character set, text measurement and line breaking, and no character loss through export, report or print; Verify the three dropdown value lists and all validation messages render correctly

**Done when**

1. Brazilian Portuguese, Chinese (simplified), Chinese (traditional) and Japanese each load and every screen reads correctly (GID-254968, Phase 2).
2. **A font carrying the full CJK character set is present and licensed**, and no character renders as a placeholder box.
3. Text measurement and line breaking work for languages that do not break on spaces.
4. **No character is lost through export, report or print** — checked on a real generated file, not on screen.
5. The three dropdown value lists and every validation message render correctly.

### ASWD-19 F6 · Phase 3: Norwegian, Danish, Finnish, Swedish, Turkish

*5 coding days · slice 19B*

**What it is** — Resource set, load and smoke-test for each of the five; **Turkish casing**: culture-aware upper and lower case and comparison, because Turkish has a dotless *i* and .NET’s default casing corrupts it; Finnish and Norwegian layout checks — long compound words break narrow columns

**Done when**

1. Norwegian, Danish, Finnish, Swedish and Turkish each load and every screen reads correctly (GID-254968, Phase 3).
2. **Turkish casing is correct.** Turkish has a dotless *i*, and .NET’s default `ToUpper` and `ToLower` corrupt it — every case-insensitive comparison in the product is culture-aware. A Turkish user gets the right search results and can log in.
3. Long Finnish and Norwegian compound words do not break narrow columns.

## Epic 21 · ASWD-21 — Device Communication (+ Firmware)

*5 features · 41.1 days · milestone M7 · one developer: 2028-08-18*

### ASWD-21 F1 · Discover the device protocol and create the communication project

*5 coding days*

**What it is** — 3-day timeboxed discovery against a real device; New `AccuSync.Adapters.DeviceCommunication` project and its interfaces in `Core`

**Done when**

1. The investigation ends with a **written finding** that says what was learned and whether the estimate still holds. If it does not, the estimate is corrected before the next story starts.
2. Tested against a real AccuScreen Pro, or an emulator agreed with the customer. Not against mocked data.
3. The capability described above works and is demonstrable.
4. ⚠ **Blocked** — do not start until Q10 is answered (see §8 of the plan). Starting on an assumption here means rework.
5. The four checks above are satisfied.

### ASWD-21 F2 · Connect to devices over USB and show connection status

*5 coding days*

**What it is** — USB transport, device discovery, connect and disconnect; Show connected / disconnected / connecting, with visual indicators

**Done when**

1. **GID-254996 Device Connection** — The software shall establish connections with AccuScreen Pro devices through USB.
2. Tested against a real AccuScreen Pro, or an emulator agreed with the customer. Not against mocked data.
3. **GID-254997 Connection Status** — The software shall display connection status indicating whether devices are connected, disconnected, or in the process of connecting.
4. **GID-255010 Connection Status Alerts** — The software shall provide visual indicators showing whether a device is connected or disconnected.
5. The four checks above are satisfied.

### ASWD-21 F3 · Read device information, patient demographics and test results

*5 coding days*

**What it is** — Firmware version and device identification; Patient demographics and test results

**Done when**

1. **GID-254998 Device Status** — The software shall display current status information from connected AccuScreen Pro devices including firmware version and device identification.
2. Tested against a real AccuScreen Pro, or an emulator agreed with the customer. Not against mocked data.
3. **GID-255000 Test Result Transfer from Device** — The software shall import patient demographic information and test results from a connected AccuScreen Pro device.
4. **GID-255044 AccuScreen Pro Patient Record Storage** — The software shall store and maintain patient test records received from the AccuScreen Pro device in a local patient database.
5. **GID-255045 AccuScreen Pro Device Configuration Storage** — The software shall store and update device configurations received from the AccuScreen Pro device in a local setting database.
6. The four checks above are satisfied.

### ASWD-21 F4 · Send patients and configuration to the device

*11 coding days*

**What it is** — GID-254999; Site config; Facility config; ABR protocols; DPOAE protocols

**Done when**

1. **GID-254999 Patient Record Transfer to Device** — The software shall transfer patient demographic information to a connected AccuScreen Pro device.
2. **GID-255001 Site Configuration Transfer** — The software provide an option to transfer Site configuation data to a connected AccuScreen Pro device.
3. **GID-255002 Facility Configuration Transfer** — The software provide an option to transfer Facility configuration data to a connected AccuScreen Pro device.
4. **GID-255003 ABR Protocol Configuration Transfer** — The software shall transfer ABR test protocol configurations to a connected AccuScreen Pro device.
5. **GID-255004 DPOAE Protocol Configuration Transfer** — The software shall transfer DPOAE test protocol configurations to a connected AccuScreen Pro device.
6. The four checks above are satisfied.

### ASWD-21 F5 · Send firmware to the device with warnings and progress

*5 coding days*

**What it is** — Send firmware to the device; Show warnings and progress before and during

**Done when**

1. **GID-255005 Firmware Update** — The software shall be able to perform a firmware update to a connected AccuScreen device.
2. **GID-255011 Firmware Update Warnings** — The software shall display warnings and precautions before initiating device firmware updates.
3. Tested against a real AccuScreen Pro, or an emulator agreed with the customer. Not against mocked data.
4. The four checks above are satisfied. ---
5. The four checks above are satisfied.

## Epic 24 · ASWD-24 — Alarms, Warnings, Operator Messages

*1 feature · 8.9 days · milestone M8 · one developer: 2028-11-03*

### ASWD-24 F1 · Show progress for slow operations and log the user out after inactivity

*4 coding days*

**What it is** — Shared progress indicator for slow operations: printing, transfers, firmware; Auto-logout after inactivity, with a warning first; GID-255008 → Epic 1; GID-255009 → Epic G; GID-255010 → Epic 21; GID-255011 → Epic 21; GID-255012 → Epic 17

**Done when**

1. **GID-255013 Operation Status Messages** — The software shall display progress indicators and status messages during time-consuming operations such as printing, data transfer, and device firmware updates.
2. **GID-255014 Session Timeout Warning** — The software shall display a warning message before automatically logging out users due to inactivity.
3. ⚠ **Blocked** — do not start until Q13 is answered (see §8 of the plan). Starting on an assumption here means rework.
4. **GID-255008 Authentication Warnings** — The software shall display warning messages after repeated failed login attempts and notify users when accounts are locked.
5. **GID-255009 Unsaved Data Warning** — The software shall warn users before closing windows or navigating away when unsaved data would be lost.
6. **GID-255010 Connection Status Alerts** — The software shall provide visual indicators showing whether a device is connected or disconnected.
7. **GID-255011 Firmware Update Warnings** — The software shall display warnings and precautions before initiating device firmware updates.
8. **GID-255012 Validation Error Messages** — The software shall display error messages identifying which fields contain invalid data and what corrections are needed.
9. The four checks above are satisfied. ---
10. The four checks above are satisfied.

## Epic 31 · ASWD-31 — Logging: retention and protection

*1 feature · 3.2 days · milestone M8 · one developer: 2028-11-10*

### ASWD-31 F1 · Roll, retain and protect the log files

*2.0 coding days*

**What it is** — Everything about the log files over time: rolling daily and at 5 MB, the file-name pattern, 366-day retention with oldest-first deletion, restricted read access, and no function anywhere in AccuSync that can delete or modify a log. Every number was set by Natus on 22 Aug 2026. **Split out of ASWD-26 on 25 Aug 2026** and placed next to Audit Trail, which needs the same approach.

**Done when**

1. **GID-255035 Log File Retention** — The software shall retain log files for a minimum of one year. **Built as 366 days**, then oldest-first deletion.
2. **GID-255036 Log File Security** — The software shall prevent unauthorized access to log files.
3. **GID-255037 Log Protection** — ~~The software shall prevent deletion of log files by any user~~ → **reworded by Natus on 22 Aug 2026 to: "The software shall not provide any user function to delete or modify log files."** ⚠ **The requirement has not yet been changed in Jama.**
4. Files roll **daily** and also at **5 MB**, whichever comes first.
5. File names follow **`accusync_log_{count}_{date:yyyy-MM-dd_HH-mm-ss}.log`**.
6. Pruning works after the machine has been switched off for a period — it does not require the application to have been running.
7. A deleted or altered log file is detected on the next write, so the gap is visible.
8. The same rolling, retention and protection approach is used by Epic 25 Audit Trail — designed once, not twice.
9. The four checks above are satisfied.

**Not done if**

- Any screen, menu item, service method or API in AccuSync can delete or modify a log file.
- GID-255037 is marked verified while DOC-076814 still carries the old, impossible wording.
- Retention is set to exactly 365 days, leaving no margin on a one-year minimum.
- This epic ships to a customer un-built while Epic 26 is live — logs would grow without limit.

## Epic 25 · ASWD-25 — Audit Trail

*2 features · 14.2 days · milestone M8 · one developer: 2028-12-08*

### ASWD-25 F1 · Create the audit table and keep records for at least a year

*4.5 coding days*

**What it is** — Audit table and writer with the 5 required fields: user ID, timestamp, description, device serial, status; Keep audit records for at least one year

**Done when**

1. **GID-255027 Audit Log Entry Fields** — The software shall associate all audit log entries with user ID, timestamp, description, device serial number or identifier, and status of the operation.
2. The tables are created by a code-first migration, not by a hand-written script. A brand-new database is created correctly on first run.
3. Which database each table goes in is written into the epic design document and added to the running allocation table in the ASWD-77 design document (open item O1).
4. A repository can read, add, change and remove records, and is covered by tests.
5. **GID-255029 Audit Log Retention** — The software shall retain audit logs for a minimum of one year.
6. The four checks above are satisfied.

### ASWD-25 F2 · Record login, configuration, patient, sync and firmware events

*5.5 coding days*

**What it is** — Login, logout, attempts, failures, password changes; GID-255025; Create, change, delete, export, import; Sync attempts and automatic user status changes; GID-255026

**Done when**

1. **GID-255023 User Activity Audit Logging** — The software shall maintain an audit log of all user login/logout, login attempts, failures, and password changes.
2. **GID-255025 Configuration Change Audit Logging** — The software shall shall maintain an audit log of all configuration and setting changes.
3. **GID-255024 Patient Record Audit Logging** — The software shall maintain an audit log of all patient record creation, modification, deletion, export, and import events.
4. **Data entered is written to the database and is still there after the application is closed and reopened.** This is the check that most screens fail today.
5. A success message appears only when something was actually saved.
6. **GID-255028 Synchronisation Audit Logging** — The software shall maintain an audit log of all successful and failed synchronisation attempts.
7. **GID-255030 User Status Audit Logging** — The software shall generate an audit trail entry for each automatic user status change, including user identifier, timestamp, and triggering synchronisation event.
8. **GID-255026 Firmware Upgrade Audit Logging** — The software shall maintain an audit log of all firmware upgrades.
9. The four checks above are satisfied. ---
10. The four checks above are satisfied.

## Epic 27 · ASWD-27 — About

*1 feature · 2.5 days · milestone M8 · one developer: 2028-12-08*

### ASWD-27 F1 · Show version number, manufacturer and website

*1 coding days*

**What it is** — Show version number, manufacturer name and website

**Done when**

1. **GID-255039 About** — The software shall display information about the application, including the version number, manufacturer name and website.
2. The four checks above are satisfied. ---
3. The four checks above are satisfied.

## Epic 28 · ASWD-28 — Help

*1 feature · 5.3 days · milestone M8 · one developer: 2028-12-22*

### ASWD-28 F1 · Open the user help and jump to the page for the current screen

*4.5 coding days*

**What it is** — Open user help from inside the app; Help icon opens the page for the current screen

**Done when**

1. **GID-255040 Help Documentation** — The software shall provide access to user help documentation from within the application.
2. ⚠ **Blocked** — do not start until Q15 is answered (see §8 of the plan). Starting on an assumption here means rework.
3. **GID-255041 Context Help** — The software shall provide context-sensitive help accessible via the Help icon.
4. The four checks above are satisfied. ---
5. The four checks above are satisfied.

## Epic 30 · ASWD-30 — Installation

*4 features · 34.9 days · milestone M8 · one developer: 2029-02-09*

### ASWD-30 F1 · Choose the installer technology and build the guided install

*4.5 coding days*

**What it is** — 1.5-day timeboxed comparison and a working minimal installer; Check prerequisites and system requirements before installing

**Done when**

1. The investigation ends with a **written finding** that says what was learned and whether the estimate still holds. If it does not, the estimate is corrected before the next story starts.
2. The capability described above works and is demonstrable.
3. **GID-255046 Installation** — The software shall provide a guided installation process that verifies prerequisites and system requirements before installation.
4. The four checks above are satisfied.

### ASWD-30 F2 · Uninstall cleanly, declare Windows 11 support and test a fresh install

*4.5 coding days*

**What it is** — Remove everything, with a choice to keep or delete the database; Confirm and declare support for Windows 11 Pro and Enterprise, and check the operating system at install time; Test a fresh install on clean Windows 11 Pro and Enterprise

**Done when**

1. **GID-255047 Uninstallation** — The software shall provide an uninstall process that removes all application components with an option to preserve or delete database files.
2. **Data entered is written to the database and is still there after the application is closed and reopened.** This is the check that most screens fail today.
3. A success message appears only when something was actually saved.
4. **GID-255007 Operating System Compatibility** — The software shall operate on Windows 11 (Pro and Enterprise editions).
5. The capability described above works and is demonstrable.
6. The four checks above are satisfied. ---
7. The four checks above are satisfied.

### ASWD-30 F3 · Encrypt the databases, check the network and confirm final storage

*4.5 coding days*

**What it is** — Detect no network before calling a web service and show a clear error; Encrypt both database files to AES-256 and manage the key at install time; Confirm patient records and settings land in the right database, including records received from a device

**Done when**

1. **GID-255006 Network Requirement Check** — The software shall require network access when exchanging data with web-service-based systems.
2. ⚠ **Blocked** — do not start until Q12 is answered (see §8 of the plan). Starting on an assumption here means rework.
3. **GID-256510 Patient Database Encryption** — The software shall encrypt all patient test records stored in the local patient database (minimum AES-256).
4. **GID-256511 Settings Database Encryption** — The software shall encrypt all settings configurations stored in the local settings database (minimum AES-256).
5. **GID-255042 Patient Record Storage** — The software shall store and maintain patient test records in a local patient database on the system.
6. **GID-255043 Device Configuration Storage** — The software shall store and maintain device configurations in a local settings database on the system.
7. **GID-255044 AccuScreen Pro Patient Record Storage** — The software shall store and maintain patient test records received from the AccuScreen Pro device in a local patient database.
8. **GID-255045 AccuScreen Pro Device Configuration Storage** — The software shall store and update device configurations received from the AccuScreen Pro device in a local setting database.
9. The four checks above are satisfied.
### ASWD-30 F4 · Database encryption research: the options, the cost and the decision P

*5.5 coding days*

**What it is** — Survey the options and what each one costs to licence: **SQLCipher** through SQLitePCLRaw, the commercial **SQLite Encryption Extension**, **column-level encryption** through EF Core value converters, and **OS-level** BitLocker or EFS; Spike each viable option against the real schema and a realistic data volume, and **measure** read and write cost rather than guess it; Key management: where the key lives, how it is protected at rest, how it rotates, and what happens when it is lost — a lost key on an encrypted patient database is unrecoverable data; Prove backup, restore and EF Core migrations still work on an encrypted file, and decide how support reads a customer database; Write the decision: the recommendation, the trade-offs, and **the design rules every later feature must follow** — including whether development proceeds against the encrypted provider from Epic 2 onward

> ⚠ **Pulled forward to week 3**, beside the Epic 21 investigation. Encryption still *ships* in F3 per D14 — only the decision moves earlier, because it constrains how every later feature is designed. Until now the only encryption research in the plan was a 1.5-day bullet inside Epic 0 F1, which stopped being visible when Epic 0 became a single hand-set line.

**Done when**

1. Each option is assessed on the same five points: **licence cost, performance, key management, backup and restore, and what it forces on the rest of the design.**
2. Performance is **measured** on the real schema at a realistic data volume, not quoted from a vendor page. Read and write cost is stated as a number.
3. Key management is answered concretely: where the key lives, how it is protected at rest, how it rotates, who can read it, and **what happens when it is lost** — on an encrypted patient database a lost key means unrecoverable data.
4. Backup, restore and **EF Core migrations** are proven to work on an encrypted file, and there is a stated answer for how support reads a customer database.
5. The output is a **written decision**, not a discussion: the recommendation, the trade-offs, and the design rules every later feature must follow.
6. The decision states explicitly **whether development proceeds against the encrypted provider from Epic 2 onward**. If it does, Epic 30 F3's regression pass shrinks and the plan is re-derived.
7. Two answers that would change design, not just deployment, are called out either way: **column-level encryption** through EF Core value converters would change how every entity is mapped and would break querying on encrypted columns; and a **per-machine key** would change what backup, restore and support access mean for every epic that touches data.
8. The decision is reviewed and agreed before Epic 2 starts, because that is the first epic whose design depends on it.
## Epic 33 · ASWD-33 — Release: release testing and documentation

*2 features · 32.5 days · milestone M8 · one developer: 2029-05-11*

> **No requirement in DOC-076814 covers this work.** Added 26 Aug 2026 at the code reviewer’s request, then scoped and timed by the lead the same day. **Nothing here changes the application.**

### ASWD-33 F1 Release testing: manual test against every requirement, and install on a new Windows machine P

*12.4 coding days*

**What it is** — Write the **test plan**: one manual test case per requirement, with the expected result and the evidence to capture; Execute the full manual pass across all **196 requirements** against a named build, and re-test what fails after each fix; Installer testing: install on a clean machine of each supported Windows version and run the pass there, not only on a developer machine

**Done when**

1. Every one of the **196 requirements** has a manual test case in the **test plan**, with an expected result and the evidence to capture.
2. The full pass is executed against a **named build**, not “the latest”, and the evidence is kept.
3. **What fails is re-tested after the fix**, inside the same 10 days — there is no separate regression task.
4. The pass is repeated on a **clean machine of each supported Windows version**. Passing on a developer machine proves only that it works where it was built.
5. **No requirement is signed off on the strength of a unit test.** Unit tests sit inside every feature's estimate already; this is the pass that tests the product.
6. Anything still failing at the end is **written down as a known issue**, with a reason, and handed to F2 for the release notes.

### ASWD-33 F2 User documentation: manual and release notes

*9 coding days*

**What it is** — Write the **user manual as a standalone document** covering every screen and workflow, against the finished product rather than the design; Release notes and the known-issue list; Check the user manual against the in-application help from Epic 28 so the two do not disagree — **a review of both, no application changes**

**Done when**

1. The user manual is a **standalone document**, not in-application content. It covers every screen and workflow, **written against the finished product** rather than the design.
2. Release notes list what is new and what is known-broken, using F1's known-issue list.
3. **The manual and the in-application help from Epic 28 agree.** Two sources that disagree are worse than one.
4. That check is a **review of two documents. It changes no application code** — if it finds a defect in the help content, that is raised against Epic 28, not fixed here.
