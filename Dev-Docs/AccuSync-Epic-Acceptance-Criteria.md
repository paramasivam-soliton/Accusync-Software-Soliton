# AccuSync — Epic Acceptance Criteria

**What has to be true before an epic can be called done.**

| | |
|---|---|
| Epics | **25** |
| Requirements | **196** |
| Total | **536.1 developer-days** |
| Plan | [AccuSync-Delivery-Roadmap.md](AccuSync-Delivery-Roadmap.md) |
| Feature-level criteria | [AccuSync-Feature-Acceptance-Criteria.md](AccuSync-Feature-Acceptance-Criteria.md) |
| Story-level criteria | [Patient Management stories](EPIC%202%20Patient%20Management/PATIENT_MANAGEMENT_STORIES.md) |

---

## How to use this

There are three levels of acceptance criteria, and they answer different questions.

| Level | Question it answers | Where |
|---|---|---|
| **Epic** | Is this whole area of the product finished and trustworthy? | This file |
| **Feature** | Does this one capability work? | [Feature file](AccuSync-Feature-Acceptance-Criteria.md) |
| **Story** | Is this one piece of work done? | Story files, per epic |

**Epic criteria are outcomes, not tasks.** They are written so a person who did not build it can check them. Where a criterion says "verified by opening the file" or "demonstrated on real data", that is deliberate — several things in this codebase currently look finished and are not.

Each epic below has:

- **Done when** — the outcomes that must all be true
- **Not done if** — traps specific to this epic, drawn from what the code does today
- **Always** — the seven checks that apply to every epic

---

## The seven checks that apply to every epic

Every epic must satisfy all of these, in addition to its own list.

1. Every feature in this epic meets its own acceptance criteria (see the feature file).
2. Unit tests reach 80% on new code in `Core`, `Application`, `EF` and `Presentation`. Screens are excluded.
3. All existing unit tests still pass. The count never goes down.
4. Every requirement listed above is recorded against the work that delivers it.
5. The epic design document is updated to describe what was actually built, not what was planned.
6. Everything is merged to `main` with CI green.
7. No new TODO or BUG comment is left in the code without a ticket.

---

## Epics

| # | Epic | Milestone | Target | 1 developer | Days | Reqs |
|---|---|---|---|---|---|---|
| 0 | [ASWD-77 Revamp Code Base](#epic-0) | M1 | Sep-26 | 2026-09-04 | 9.1 | 0 |
| 1 | [ASWD-1 Login](#epic-1) | M1 | Sep-26 | 2026-10-23 | 8.9 | 6 |
| 2 | [ASWD-2 Patient Management](#epic-2) | M1-M2 | Sep-26 / Oct-26 | 2027-05-21 | 96.1 | 44 |
| 3 | [ASWD-32 Data Exchange: import, export and S4H](#epic-3) ⚠ *needs a Jira epic* | M5-M7 | Apr-27 / Jul-27 | 2028-10-20 | 81.9 | 42 |
| 5 | [ASWD-5 OAE Test Result](#epic-5) | M3 | Dec-26 | 2027-06-18 | 13.1 | 2 |
| 6 | [ASWD-6 ABR Test Result](#epic-6) | M3 | Dec-26 | 2027-07-02 | 8.5 | 1 |
| 7 | [ASWD-7 User Account Management](#epic-7) | M4 | Feb-27 | 2027-08-27 | 28.8 | 11 |
| 8 | [ASWD-8 Profile Management](#epic-8) | M4 | Feb-27 | 2027-09-24 | 28.5 | 6 |
| 9 | [ASWD-9 Device Management](#epic-9) | M4 | Feb-27 | 2027-10-29 | 20.8 | 8 |
| 10 | [ASWD-10 Site Management](#epic-10) | M4 | Feb-27 | 2027-11-19 | 9.5 | 7 |
| 11 | [ASWD-11 Facility Management](#epic-11) | M4 | Feb-27 | 2027-12-03 | 7.2 | 5 |
| 12 | [ASWD-12 Location Management](#epic-12) | M4 | Feb-27 | 2027-12-10 | 7.2 | 6 |
| 13 | [ASWD-13 ABR Test Protocol Configuration](#epic-13) | M4 | Feb-27 | 2027-12-24 | 7 | 4 |
| 14 | [ASWD-14 DPOAE Test Protocol Configuration](#epic-14) | M4 | Feb-27 | 2028-01-14 | 8.5 | 4 |
| 18 | [ASWD-18 Report Generation](#epic-18) | M6 | Jun-27 | 2028-05-19 | 19.2 | 3 |
| 19 | [ASWD-19 Language](#epic-19) | M6 | Jun-27 | 2029-03-16 | 35.4 | 5 |
| 21 | [ASWD-21 Device Communication (+ Firmware)](#epic-21) | M7 | Jul-27 | 2028-08-18 | 41.1 | 14 |
| 24 | [ASWD-24 Alarms, Warnings, Operator Messages](#epic-24) | M8 | Sep-27 | 2028-11-03 | 8.9 | 7 |
| 31 | [ASWD-31 Logging: retention and protection](#epic-31) ⚠ *needs a Jira epic* | M8 | Sep-27 | 2028-11-10 | 3.2 | 3 |
| 25 | [ASWD-25 Audit Trail](#epic-25) | M8 | Sep-27 | 2028-12-08 | 14.2 | 8 |
| 26 | [ASWD-26 Logging: the basics](#epic-26) | M1 | Sep-26 | 2026-10-30 | 3.8 | 5 |
| 27 | [ASWD-27 About](#epic-27) | M8 | Sep-27 | 2028-12-08 | 2.5 | 1 |
| 28 | [ASWD-28 Help](#epic-28) | M8 | Sep-27 | 2028-12-22 | 5.3 | 2 |
| 30 | [ASWD-30 Installation](#epic-30) | M8 | Sep-27 | 2029-02-09 | 34.9 | 10 |
| 33 | [ASWD-33 Release: release testing and documentation](#epic-33) ⚠ *needs a Jira epic* | M8 | Sep-27 | 2029-05-11 | 32.5 | 0 |

---

<a id="epic-0"></a>

## Epic 0 · ASWD-77 — Revamp Code Base

| | |
|---|---|
| Milestone | **M1** |
| Target date | Sep-26 |
| One developer | **2026-09-04** |
| Estimate | **9.1 days** |
| Features | 1 |
| Requirements | **0** — — |

### Done when

1. `git clone` followed by `dotnet build AccuSync.sln` succeeds on a machine that has never built this project.
2. A brand-new database is created correctly on first run. The three out-of-order migrations are gone, replaced by one clean `InitialCreate`.
3. `main` contains all the work. The 12 stacked feature branches are merged and retired, and no branch is more than 5 working days old.
4. Every pull request is built and tested automatically, and the coverage figure is reported.
5. A developer can write a new screen without inventing base classes: `ObservableObject`, `ViewModelBase`, `ValidatableViewModelBase`, `AsyncRelayCommand` and `Result<T>` all exist and are used.
6. No screen opens a window directly or calls `App.GetService<T>()` to navigate. Navigation, dialogs and file pickers all go through interfaces.
7. `AccuSync.Adapters.DataParser` references `AccuSync.Core`, not `AccuSync.Application`, and the parsing interfaces live in `Core`.
8. A Release build **cannot** be made to skip the login screen. The dev-mode bypass is excluded at compile time, not just switched off.
9. The design knowledge in `Databases/*.sql` is captured in a reviewed document **before** those files are deleted — 18 indexes, the Profiles permission fields, and every protocol parameter range.
10. The seven checks above are satisfied.

### Not done if

- A new developer cannot build the project on their first day.
- The dev-mode login bypass is still reachable in a Release build.

---

<a id="epic-1"></a>

## Epic 1 · ASWD-1 — Login

| | |
|---|---|
| Milestone | **M1** |
| Target date | Sep-26 |
| One developer | **2026-10-23** |
| Estimate | **8.9 days** |
| Features | 1 |
| Requirements | **6** — GID-255015–255019, GID-255021 |

### Done when

1. The six built Login requirements are re-verified after the branch merge and still pass.
2. All 103 existing unit tests still pass on `main`.
3. Each of the six requirements is recorded against the work that delivers it, so verification can trace it.
4. **Every one of the 6 requirements below is met and can be demonstrated:**

   - **GID-255015 User Login** — The software shall require users to enter username and password credentials to login to the system.
   - **GID-255016 User Logout** — The software shall provide an option to logout of the system.
   - **GID-255017 User Account Lockout** — The software shall lock out the user from logging into the system after a minimum of 5 failed sequential login attempts with improper credentials.
   - **GID-255018 Credential Encryption** — The software shall securely hash all login passwords and encrypt all usernames stored in the system. ⚠ **safety-related (`U*`)**
   - **GID-255019 Role-Based Access Control** — The software shall support role-based access control with minimum two roles: Admin and Screener.
   - **GID-255021 Deactivated User Login Prevention** — The software shall prevent deactivated users from logging in.

5. The seven checks above are satisfied.

---

<a id="epic-2"></a>

## Epic 26 · ASWD-26 — Logging: the basics

| | |
|---|---|
| Milestone | **M1** |
| Target date | Sep-26 |
| One developer | **2026-10-30** |
| Estimate | **3.8 days** |
| Features | 1 |
| Requirements | **5** — GID-255031, GID-255032, GID-255033, GID-255034, GID-255038 |

### Done when

1. Informational, warning and error messages are written to a log file with timestamps — plus **Debug**, a fourth level Natus asked for on 22 Aug 2026.
2. Every entry follows the format Natus set: **`{date:yyyy-MM-dd HH:mm:ss} [{level}] {class-name}.{method}() {message}`**.
3. **No patient demographic data appears in any log file** — Natus's ruling is the whole demographic set, not a named subset. Checked by inspecting real output, not by reading the code.
5. Log messages are **English only** and are not part of the translated resource set.
8. The 163 existing `Debug.WriteLine` and `Console.WriteLine` calls are converted or removed; none is left as the only record of anything that matters.
9. Epic 0 and Epic 1 code logs like everything else, and **no password, hash or token is ever logged**.
10. **The audit trail remains a separate record** in `%ProgramData%\Natus\AccuSync\auditTrail` with its own format. Logging a login does not satisfy GID-255023.
11. **Every one of the 5 requirements below is met and can be demonstrated:**

   - **GID-255031 Log Message Generation** — The software shall provide a mechanism to generate log messages during the operation of the software.
   - **GID-255032 Log Message Types** — The software shall provide a logging mechanism that supports informational, warning, and error message types.
   - **GID-255033 Log Message Storage** — The software shall store generated log messages on the system for troubleshooting purposes.
   - **GID-255034 Log Data Privacy** — The software shall **not** include or retain any patient-identifiable information in the log data.
   - **GID-255038 Log Entry Fields** — The software shall associate all log entries with timestamp, description and status of the operation.

### Not done if

- A log file produced by a real run contains any patient demographic value.
- Debug level ships without a requirement or a recorded exception covering it.

> **Rotation, retention and file protection are not in this epic.** They moved to **ASWD-31** on 25 Aug 2026, late in the plan next to Audit Trail, because they produce nothing a stakeholder can see. **Between the two epics logs are written but never pruned** — acceptable on a development machine, not acceptable in a release.


## Epic 2 · ASWD-2 — Patient Management

| | |
|---|---|
| Milestone | **M1-M2** |
| Target date | Sep-26 / Oct-26 |
| One developer | **2027-05-21** |
| Estimate | **96.1 days** |
| Features | 9 |
| Requirements | **44** — GID-254873–254897, GID-254950–254964, GID-254966, GID-255009, GID-255012, GID-255466 |

### Done when

1. A patient created in the application is still there after the application is closed and reopened.
2. The patient list shows real saved patients, not sample data written into the code.
3. An administrator can add a risk factor and it appears on the patient screen without a code change.
4. Deleting a patient hides them everywhere but does not remove the row from the database.
5. A screener without edit permission can open a patient but cannot change anything.
6. Saving is blocked while a mandatory field is empty, and the empty field is marked with a message saying what is needed.
7. No screen in this epic reports success for something it did not do.
8. **The two databases are split as Natus set on 22 Aug 2026** — patient data and the risks and comments on a patient in the **patient** database; all configuration, including the risk factor and comment **lists**, in the **settings** database.
9. **The field set matches AccuLink as its baseline**, plus the additions from ALGO 5 and ALGO Pro. Natus confirmed AccuLink is the reference and that the current schema design does not yet match it.
10. **The 19 ALGO device fields are on the patient screens and saved** (PM-39, in F1). The three coded dropdowns use the values Natus supplied on 22 Aug 2026, and `InsuranceType` is a text box rather than a dropdown. Importing an ALGO 5 or ALGO Pro file no longer silently discards patient data, and the `Physician` / pediatrician import defect is fixed. *No requirement covers these fields — scope confirmed by Natus on 18 Aug 2026.*
9. **Every one of the 44 requirements below is met and can be demonstrated:**

   - **GID-254873 Data Management Options** — The software shall provide data management options Add, Edit, and Delete for creating, modifying, and removing database records.
   - **GID-254874 Add Function** — The software shall allow the user to create a new record by selecting Add, which shall open a blank form for data entry.
   - **GID-254875 Edit Function** — The software shall allow the user to modify a selected record by selecting Edit, which shall open the record in an editable form.
   - **GID-254876 Delete Function** — The software shall allow the user to delete a selected record by selecting Delete, and shall require user confirmation before removal from the database.
   - **GID-254877 Data Management Screen Availability** — The software shall provide data management options (Add, Edit, Delete) on the following screens: Patient, User, Profile, Device, Site, Facility, Location, ABR Protocols, DPOAE Protocols, Risk Factors, Comments.
   - **GID-254878 Unsaved Data Management Options** — The software shall provide unsaved data management options Save, Revert, and Undo for managing uncommitted changes.
   - **GID-254879 Save Function** — The software shall allow the user to save all modified data by selecting Save, which shall persist changes to the database.
   - **GID-254880 Revert Function** — The software shall allow the user to discard all unsaved changes by selecting Revert, restoring the data to its last saved state.
   - **GID-254881 Undo Function** — The software shall allow the user to reverse the most recent unsaved change by selecting Undo.
   - **GID-254882 Unsaved Data Management Screen Availability** — The software shall provide unsaved data management options (Save, Revert, Undo) on the following screens: Patient, User, Profile, Device, Site, Facility, Location, ABR Protocols, DPOAE Protocols, Risk Factors, Comments, Patient Field Configuration.
   - **GID-254883 Create Patient Record** — The software shall allow users to create a patient record containing patient demographics, caregiver data, medical data, and consent information.
   - **GID-254884 View Patient List** — The software shall allow users to view a list of patient records saved in the system.
   - **GID-254885 Edit Patient Record** — The software shall allow users to select a patient from the list view and edit the patient record, unless locked by permissions.
   - **GID-254886 Delete Patient Record** — The software shall allow administrative users to delete a patient record.
   - **GID-254887 Patient Search** — The software shall provide search functionality on the patient list view, allowing users to search by patient ID, first name, last name, date of birth, or date of test range, and display the filtered results.
   - **GID-254888 Patient Risk Factor Selection** — The software shall provide an option to set risk factors in the patient details view with the following values: · Yes · No · Unknown
   - **GID-254889 Patient Comment Assignment** — The software shall provide an option to assign predefined or patient-specific comments to a patient record.
   - **GID-254890 Patient List Display Fields** — The software shall display the following information for each patient in the patient list view: · Patient ID / Hospital ID · Last Name · First Name · Date of Birth · Risk · Comment
   - **GID-254891 View Patient Information** — The software shall allow users select a patient from the list view and view patient information.
   - **GID-254892 Mandatory Field Indication** — The software shall indicate which fields are mandatory during patient data entry.
   - **GID-254893 Patient Save Validation** — The software shall allow the user to save patient information when all mandatory fields are populated.
   - **GID-254894 Patient Test Report Generation** — The software shall generate and print predefined patient test reports, including selected tests or all tests.
   - **GID-254895 Delete Patient Test Entry** — The software shall allow administrative users to delete individual test entries from a patient's test result list.
   - **GID-254896 Test Result Reassignment** — The software shall allow administrative users to reassign test results to the correct patient in the system.
   - **GID-254897 Patient Test List Display Fields** — The software shall display the following information for each test of a selected patient: · Test Type · Left Ear Result · Right Ear Result · Date/Time of Test · Test Configuration · Duration · Examiner
   - **GID-254950 Create Risk Factor** — The software shall allow administrative users to create predefined risk factors that can be assigned to patient records.
   - **GID-254951 View Risk Factor List** — The software shall allow administrative users to view a list of predefined risk factors that can be assigned to patient records.
   - **GID-254952 Edit Risk Factor** — The software shall allow administrative users to edit predefined risk factors that has not been assigned to a patient record.
   - **GID-254953 Delete Risk Factor** — The software shall allow administrative users to delete a list of predefined risk factors that is not assigned to a patient record.
   - **GID-254954 Risk Factor Translation** — The software shall allow administrative users to enter translated text for risk factor names and descriptions in each supported language.
   - **GID-254955 Create Comment** — The software shall allow administrative users to create a comment that can be assigned to a patient record.
   - **GID-254956 View Comment List** — The software shall allow administrative users to view a list of predefined comments that can be assigned to a patient record.
   - **GID-254957 Edit Comment** — The software shall allow administrative users to edit a predefined comment that can be assigned to a patient record.
   - **GID-254958 Delete Comment** — The software shall allow administrative users to delete a predefined comment that is not assigned to a patient record.
   - **GID-254959 Comment Translation** — The software shall allow administrative users to enter translated text for comments in each supported language.
   - **GID-254960 Patient ID Format Validation Configuration** — The software shall allow administrative users to configure patient ID rule format validation.
   - **GID-254961 Patient Mandatory Field Configuration** — The software shall allow administrative users to configure which patient record fields are mandatory during data entry.
   - **GID-254962 Patient Active Field Configuration** — The software shall allow administrative users to configure which patient record fields are active during data entry.
   - **GID-254963 Patient List Sort Configuration** — The software shall allow administrative users to define how patient lists are sorted.
   - **GID-254964 Data Confirmation** — The software shall allow configuration of the following user notifications: · Confirmation of saving · Confirmation of deletion · Data modification warning
   - **GID-254966 Data Validation** — The software shall validate user inputs and prevent saving of invalid or incomplete data according to configured validation rules.
   - **GID-255009 Unsaved Data Warning** — The software shall warn users before closing windows or navigating away when unsaved data would be lost.
   - **GID-255012 Validation Error Messages** — The software shall display error messages identifying which fields contain invalid data and what corrections are needed.
   - **GID-255466 Custom Field Configuration** — The software shall provide customizable fields labeled "Available Field" that allow users to rename the fields.

9. The seven checks above are satisfied.

### Not done if

- Any screen still shows data written into the code instead of read from the database.
- “Patient saved” appears when nothing was written.
- The 16 risk factor questions are still hardcoded on the patient screen.

---

<a id="epic-5"></a>

## Epic 3 · ASWD-32 — Data Exchange: import, export and S4H

| | |
|---|---|
| Milestone | **M5-M7** |
| Target date | Apr-27 / Jul-27 |
| One developer | **2028-10-20** |
| Estimate | **81.9 days** |
| Features | 10 |
| Requirements | **42** — the 1 from ASWD-3, 16 from ASWD-4 and 25 from ASWD-20 |
| Jira | ⚠ **needs an epic.** `ASWD-32` is a placeholder |
| Slices | **32A** file formats (M5) · **32B** S4H (M7) |

### Done when

1. **Every format works in both directions**: AccuSync XML, AccuSync JSON, CSV, ALGO 5 XML, ALGO Pro, AccuLink XML, HiTrack and OZ — with the two documented exceptions, OZ import (X1) and HiTrack patient import (X2).
2. An imported patient is **saved to the database** and is still there after a restart.
3. Every export writes to the exact folder and file name the requirements specify.
4. De-identified export leaks nothing — verified by reading real output.
5. AccuSync signs on to the SEDQ service, receives users, devices, facilities and risk factors, and sends results back including waveform data.
6. The record and stream layer is shared by every file format rather than written per format.
7. **All 42 requirements are met and can be demonstrated.**

### Not done if

- A format reads but does not write, or writes but does not read. **A format is finished when it round-trips**, which is the whole reason these three epics became one.
- AccuLink source is copied into AccuSync before **X5** is answered. Reading it to learn the formats is safe; lifting it is a licensing question.
- HiTrack or OZ is marked blocked on Q3. Their layouts are settled — only CSV is still open.
- A file exports on screen and the folder is empty, or an import shows patients that are gone after a restart.
- S4H is signed off while GID-254984 to GID-254987 still carry rotated titles in Jama.

> **Replaces ASWD-3, ASWD-4 and ASWD-20**, merged 25 Aug 2026 (D20). Reading `C:\Drive\natus\AccuLink\src\Component.DataExchange` removed the 3-day SEDQ discovery and settled the HiTrack and OZ layouts — see [AccuLink-DataExchange-Analysis.md](AccuLink-DataExchange-Analysis.md).

---

<a id="epic-5"></a>

## Epic 5 · ASWD-5 — OAE Test Result

| | |
|---|---|
| Milestone | **M3** |
| Target date | Dec-26 |
| One developer | **2027-06-18** |
| Estimate | **13.1 days** |
| Features | 3 |
| Requirements | **2** — GID-254901–254902 |

### Done when

1. A TEOAE and a DPOAE result can be opened from a patient and show the result, the waveform, the comments and which device recorded it.
2. The waveform is drawn from stored data, not from a picture or sample values.
3. The screens are demonstrated using a **real** device file loaded through the existing parsers.
4. **Every one of the 2 requirements below is met and can be demonstrated:**

   - **GID-254901 TEOAE Test Result** — The software shall allow users to view patient test result details, waveform data, test comments, and device information for the TEOAE test.
   - **GID-254902 DPOAE Test Result** — The software shall allow users to view patient test result details, waveform data, test comments, and device information for the DPOAE test.

5. The seven checks above are satisfied.

---

<a id="epic-6"></a>

## Epic 6 · ASWD-6 — ABR Test Result

| | |
|---|---|
| Milestone | **M3** |
| Target date | Dec-26 |
| One developer | **2027-07-02** |
| Estimate | **8.5 days** |
| Features | 1 |
| Requirements | **1** — GID-254903 |

### Done when

1. An ABR result can be opened and shows the result, the waveform, EEG noise, impedance, comments and device information.
2. The waveform drawing code is shared with the OAE views rather than duplicated.
3. **Every one of the 1 requirements below is met and can be demonstrated:**

   - **GID-254903 ABR Test Result** — The software shall allow users to view patient test result details, waveform data, test comments, and device information for the ABR test.

4. The seven checks above are satisfied.

---

<a id="epic-7"></a>

## Epic 7 · ASWD-7 — User Account Management

| | |
|---|---|
| Milestone | **M4** |
| Target date | Feb-27 |
| One developer | **2027-08-27** |
| Estimate | **28.8 days** |
| Features | 2 |
| Requirements | **11** — GID-254904–254912, GID-255020, GID-255022 |

### Done when

1. An administrator can create a real user account, and that account can log in. *(There is no working way to do this today.)*
2. Password rules are enforced everywhere a password is set — admin creation, import, and self-service change.
3. No account anywhere is created with a default password of `1234` or `12345`.
4. A signed-in user can reach the change-password screen. The last-three-passwords rule can actually fire.
5. An administrator can activate, deactivate and unlock accounts, and set the lockout duration.
6. **Every one of the 11 requirements below is met and can be demonstrated:**

   - **GID-254904 View User List** — The software shall allow administrative users to view a list of user accounts.
   - **GID-254905 Create User Account** — The software shall allow administrative users to create a user account.
   - **GID-254906 Edit User Account** — The software shall allow administrative users to edit a user account.
   - **GID-254907 Unlock User Account** — The software shall allow administrative users to unlock a user account.
   - **GID-254908 Delete User Account** — The software shall allow administrative users to delete a user account.
   - **GID-254909 User Account Activation** — The software shall allow administrative users to activate/deactivate a user account.
   - **GID-254910 User Language Selection** — The software shall allow administrative users to select the display language per user.
   - **GID-254911 Account Lockout Duration Configuration** — The software shall allow administrative users to configure the lockout duration after 5 consecutive lockouts.
   - **GID-254912 Password Complexity Configuration** — The software shall allow administrative users to configure the password complexity (None, Simple, Complex).
   - **GID-255020 Password Complexity Requirements** — The software shall enforce password complexity requirements each user account: · At least 8 characters · At least one upper case letter (A to Z) · At least one lower case letter (a to z) · At least one numerical digit · The password cannot be the same as the three previously used passwords
   - **GID-255022 Deactivated User Transfer Prevention** — The software shall prevent transfer of deactivated user accounts to the device.

7. The seven checks above are satisfied.

### Not done if

- The only accounts that exist are the two seeded ones.

---

<a id="epic-8"></a>

## Epic 8 · ASWD-8 — Profile Management

| | |
|---|---|
| Milestone | **M4** |
| Target date | Feb-27 |
| One developer | **2027-09-24** |
| Estimate | **28.5 days** |
| Features | 3 |
| Requirements | **6** — GID-254913–254918 |

### Done when

1. An administrator can build a profile by ticking individual permissions, save it, and assign it to a user.
2. What a user can see and do follows their profile from the database, not the hardcoded presets in `UserPermissionsViewModel`.
3. Permission is enforced in the service layer, not only by hiding buttons.
4. **Every one of the 6 requirements below is met and can be demonstrated:**

   - **GID-254913 View Profile List** — The software shall allow administrative users to view a list of profiles and feature access rights.
   - **GID-254914 Create Profile** — The software shall allow administrative users to create a profile.
   - **GID-254915 Edit Profile** — The software shall allow administrative users to edit a profile.
   - **GID-254916 Delete Profile** — The software shall allow administrative users to delete a profile.
   - **GID-254917 Profile Display Fields** — The software shall display the following information for a selected profile: · Name · Description · Components and Permissions
   - **GID-254918 Profile Permission Configuration** — The software shall allow administrative users to configure permissions for each profile.

5. The seven checks above are satisfied.

### Not done if

- **Permissions are enforced only in the user interface.** A rule a service call can walk around is not access control.
- The permission matrix is not written down, so nobody can say what the 33 permissions are meant to gate.
- `UserPermissionsViewModel` still returns hardcoded presets, including `ReadOnly`, which is not in the SRS.
- Patient delete becomes admin-only. Natus decided on 22 Aug 2026 that every user can delete a patient.

---

<a id="epic-9"></a>


## Epic 9 · ASWD-9 — Device Management

| | |
|---|---|
| Milestone | **M4** |
| Target date | Feb-27 |
| One developer | **2027-10-29** |
| Estimate | **20.8 days** |
| Features | 3 |
| Requirements | **8** — GID-254919–254926 |

### Done when

1. An administrator can add, edit and delete devices, and assign users and facilities to them.
2. The device list shows real saved devices with name, serial number and last seen.
3. Device settings configuration is **not** built until hazard 6.4 is known (question Q8).
4. **Every one of the 8 requirements below is met and can be demonstrated:**

   - **GID-254919 Device List Display Fields** — The software shall allow administrative users to view a list of devices with the following information: · Name · Serial number · Last Seen
   - **GID-254920 Add Device** — The software shall allow administrative users to add a new device.
   - **GID-254921 Edit Device** — The software shall allow administrative users to edit device information.
   - **GID-254922 Delete Device** — The software shall allow administrative users to delete a device.
   - **GID-254923 Device User Assignment** — The software shall allow administrative users to assign users to each device.
   - **GID-254924 Device Facility Assignment** — The software shall allow administrative users to assign facilities to each device.
   - **GID-254925 Device Settings Configuration** — The software shall allow administrative users to configure the following device settings: · Display timeout · Power timeout · Calibration/pause time · Result terminology · Automatic deletion · ABR Autostart · TEOAE Probe Fit Assistant ⚠ **safety-related (`U*`)**
   - **GID-254926 Device System Information Display** — The software shall display the following System Information for a selected device: · Last Seen · Last Updated · Hardware Version · Firmware Version

5. The seven checks above are satisfied.

---

<a id="epic-10"></a>

## Epic 10 · ASWD-10 — Site Management

| | |
|---|---|
| Milestone | **M4** |
| Target date | Feb-27 |
| One developer | **2027-11-19** |
| Estimate | **9.5 days** |
| Features | 2 |
| Requirements | **7** — GID-254927–254931, GID-255001–255002 |

### Done when

1. An administrator can add, edit and delete sites, and the list shows real saved data.
2. One shared “is this record in use?” check exists and is reused by Facility, Location, Profile, Device, Protocol, Risk Factor and Comment — not copied per screen.
3. **Every one of the 7 requirements below is met and can be demonstrated:**

   - **GID-254927 View Site List** — The software shall allow administrative users to view a list of sites and associated details.
   - **GID-254928 Site Detail Fields** — The software shall allow administrative users to enter the following site details: · Name · Description · Code
   - **GID-254929 Add Site** — The software shall allow administrative users to add a new site.
   - **GID-254930 Edit Site** — The software shall allow administrative users to edit site details.
   - **GID-254931 Delete Site** — The software shall allow administrative users to delete a site.
   - **GID-255001 Site Configuration Transfer** — The software provide an option to transfer Site configuation data to a connected AccuScreen Pro device.
   - **GID-255002 Facility Configuration Transfer** — The software provide an option to transfer Facility configuration data to a connected AccuScreen Pro device.

4. The seven checks above are satisfied.

---

<a id="epic-11"></a>

## Epic 11 · ASWD-11 — Facility Management

| | |
|---|---|
| Milestone | **M4** |
| Target date | Feb-27 |
| One developer | **2027-12-03** |
| Estimate | **7.2 days** |
| Features | 1 |
| Requirements | **5** — GID-254932–254936 |

### Done when

1. An administrator can add, edit and delete facilities with name, description, code, site and location type.
2. A facility that is in use cannot be deleted, using the shared check from ASWD-10.
3. **Every one of the 5 requirements below is met and can be demonstrated:**

   - **GID-254932 View Facility List** — The software shall allow administrative users to view a list of facilities and associated details.
   - **GID-254933 Facility Detail Fields** — The software shall allow administrative users to enter the following facility details: · Name · Description · Code · Site · Location Type
   - **GID-254934 Add Facility** — The software shall allow administrative users to add a facility.
   - **GID-254935 Edit Facility** — The software shall allow administrative users to edit a facility.
   - **GID-254936 Delete Facility** — The software shall allow administrative users to delete a facility.

4. The seven checks above are satisfied.

---

<a id="epic-12"></a>

## Epic 12 · ASWD-12 — Location Management

| | |
|---|---|
| Milestone | **M4** |
| Target date | Feb-27 |
| One developer | **2027-12-10** |
| Estimate | **7.2 days** |
| Features | 1 |
| Requirements | **6** — GID-254937–254941, GID-255465 |

### Done when

1. An administrator can add, edit and delete locations.
2. When adding a location, there is an option to assign it to every facility at once.
3. **Every one of the 6 requirements below is met and can be demonstrated:**

   - **GID-254937 View Location List** — The software shall allow administrative users view a list of locations and associated details
   - **GID-254938 Location Detail Fields** — The software shall allow administrative users to enter the following location details: · Name · Description · Code
   - **GID-254939 Add Location** — The software shall allow administrative users to add a new location.
   - **GID-254940 Edit Location** — The software shall allow administrative users to edit a location.
   - **GID-254941 Delete Location** — The software shall allow administrative users to delete a location.
   - **GID-255465 Assign Location** — When adding a new location, the software shall provide an option to assign the location to all facilities.

4. The seven checks above are satisfied.

---

<a id="epic-13"></a>

## Epic 13 · ASWD-13 — ABR Test Protocol Configuration

| | |
|---|---|
| Milestone | **M4** |
| Target date | Feb-27 |
| One developer | **2027-12-24** |
| Estimate | **7 days** |
| Features | 1 |
| Requirements | **4** — GID-254942–254945 |

### Done when

1. An administrator can create, view, edit and delete ABR test protocols.
2. Every protocol parameter accepts only values inside its allowed range, taken from the design captured in ASWD-77.
3. **Every one of the 4 requirements below is met and can be demonstrated:**

   - **GID-254942 Create ABR Protocol** — The software shall allow administrative users create ABR test protocol configurations.
   - **GID-254943 View ABR Protocol List** — The software shall allow administrative users view a list of ABR test protocol configurations.
   - **GID-254944 Edit ABR Protocol** — The software shall allow administrative users to edit an existing ABR test protocol configuration.
   - **GID-254945 Delete ABR Protocol** — The software shall allow administrative users to delete an existing ABR test protocol configuration.

4. The seven checks above are satisfied.

---

<a id="epic-14"></a>

## Epic 14 · ASWD-14 — DPOAE Test Protocol Configuration

| | |
|---|---|
| Milestone | **M4** |
| Target date | Feb-27 |
| One developer | **2028-01-14** |
| Estimate | **8.5 days** |
| Features | 1 |
| Requirements | **4** — GID-254946–254949 |

### Done when

1. An administrator can create, view, edit and delete DPOAE test protocols.
2. Protocol parameter ranges are enforced, as for ABR.
3. **Every one of the 4 requirements below is met and can be demonstrated:**

   - **GID-254946 Create DPOAE Protocol** — The software shall allow administrative users create DPOAE test protocol configurations.
   - **GID-254947 View DPOAE Protocol List** — The software shall allow administrative users view a list of DPOAE test protocol configurations.
   - **GID-254948 Edit DPOAE Protocol** — The software shall allow administrative users to edit an existing DPOAE test protocol configuration.
   - **GID-254949 Delete DPOAE Protocol** — The software shall allow administrative users to delete an existing DPOAE test protocol configuration.

4. The seven checks above are satisfied.

---

<a id="epic-3"></a>

## Epic 18 · ASWD-18 — Report Generation

| | |
|---|---|
| Milestone | **M6** |
| Target date | Jun-27 |
| One developer | **2028-05-19** |
| Estimate | **19.2 days** |
| Features | 4 |
| Requirements | **4** — GID-255112, GID-255113, GID-254965, GID-254894 |

### Done when

1. A screener can **generate, preview, print and save** a patient report covering selected tests or all tests.
2. The report carries demographic information and test result details.
3. The ten predefined report types exist.
4. An administrator can set the logo and paper size, and it applies to every report type.

### Not done if

- The report prints but cannot be previewed, so the first sight of a mistake is on paper.
- The layout settings apply to one report type and not the rest.
- The ten report types ship without a requirement naming them.

> **Absorbed Epic 2 F10 on 27 Aug 2026**, which answers **Q5**: GID-254894 and GID-255112 describe the same report, so it is one workflow and it lives here. The epic was restructured at the same time so every feature is something a person does.

---


## Epic 19 · ASWD-19 — Language

| | |
|---|---|
| Milestone | **M6-M8** |
| Target date | Jun-27 / Sep-27 |
| One developer | **2029-03-16** |
| Estimate | **35.4 days** |
| Features | 6 |
| Requirements | **5** — GID-254967 to GID-254970, GID-256513 |
| Slices | **19A** switching and the string pack (M6) · **19B** the three language phases (M8) |

### Done when

1. **All 14 languages named in GID-254968 load and work** — Phase 1, Phase 2 and Phase 3. Natus confirmed on 27 Aug 2026 that every phase must be complete at product launch.
2. A user switches language and the application follows **without a restart**, or the exception is recorded.
3. The 860 strings exist once, are reviewed and frozen, and were packed for the distribution partners **with the screen and a screenshot for each**.
4. Chinese, Japanese and Turkish are correct — characters render, text wraps, and casing does not corrupt Turkish.

### Not done if

- A language loads but a screen still shows English.
- The pack went to the partners before the strings were frozen, so some are translated twice.
- CJK characters render as placeholder boxes, or are lost in an exported file or a printed report.
- Turkish search or login misbehaves because a comparison used the default culture.
- Phases 2 and 3 are treated as optional. Natus removed that option on 27 Aug 2026.

> **Scope set by Natus on 27 Aug 2026:** *"We start with Phase 1. By product launch, all phases have to be completed."* Translation is done by **regional distribution partners**, and *"strings have to be reviewed/finalized before they are translated."*

> ⚠ **The translation itself is not in the estimate** — 860 strings, about 2,413 English words, across 13 new languages is roughly **31,400 words** of partner work. What is in the estimate is the pack we owe them. **Thirteen partner sets have to come back**, and a slow region holds up the release regardless of development effort (R12).

---


## Epic 21 · ASWD-21 — Device Communication (+ Firmware)

| | |
|---|---|
| Milestone | **M7** |
| Target date | Jul-27 |
| One developer | **2028-08-18** |
| Estimate | **41.1 days** |
| Features | 5 |
| Requirements | **14** — GID-254996–255005, GID-255010–255011, GID-255044–255045 |

### Done when

1. An AccuScreen Pro connects over USB and the screen shows connecting, connected and disconnected states correctly.
2. Patient records transfer to the device, and completed test results transfer back and are saved.
3. Site, facility and both protocol configurations transfer to the device.
4. A firmware update runs, with warnings shown before it starts and progress shown while it runs.
5. Nothing in this epic is estimated as committed work until the week 13 investigation has confirmed the protocol and a device is available.
6. **Every one of the 14 requirements below is met and can be demonstrated:**

   - **GID-254996 Device Connection** — The software shall establish connections with AccuScreen Pro devices through USB.
   - **GID-254997 Connection Status** — The software shall display connection status indicating whether devices are connected, disconnected, or in the process of connecting.
   - **GID-254998 Device Status** — The software shall display current status information from connected AccuScreen Pro devices including firmware version and device identification.
   - **GID-254999 Patient Record Transfer to Device** — The software shall transfer patient demographic information to a connected AccuScreen Pro device.
   - **GID-255000 Test Result Transfer from Device** — The software shall import patient demographic information and test results from a connected AccuScreen Pro device.
   - **GID-255001 Site Configuration Transfer** — The software provide an option to transfer Site configuation data to a connected AccuScreen Pro device.
   - **GID-255002 Facility Configuration Transfer** — The software provide an option to transfer Facility configuration data to a connected AccuScreen Pro device.
   - **GID-255003 ABR Protocol Configuration Transfer** — The software shall transfer ABR test protocol configurations to a connected AccuScreen Pro device.
   - **GID-255004 DPOAE Protocol Configuration Transfer** — The software shall transfer DPOAE test protocol configurations to a connected AccuScreen Pro device.
   - **GID-255005 Firmware Update** — The software shall be able to perform a firmware update to a connected AccuScreen device.
   - **GID-255010 Connection Status Alerts** — The software shall provide visual indicators showing whether a device is connected or disconnected.
   - **GID-255011 Firmware Update Warnings** — The software shall display warnings and precautions before initiating device firmware updates.
   - **GID-255044 AccuScreen Pro Patient Record Storage** — The software shall store and maintain patient test records received from the AccuScreen Pro device in a local patient database.
   - **GID-255045 AccuScreen Pro Device Configuration Storage** — The software shall store and update device configurations received from the AccuScreen Pro device in a local setting database.

7. The seven checks above are satisfied.

### Not done if

- The device screens are demonstrated with invented data because no device was available.

---

<a id="epic-24"></a>

## Epic 24 · ASWD-24 — Alarms, Warnings, Operator Messages

| | |
|---|---|
| Milestone | **M8** |
| Target date | Sep-27 |
| One developer | **2028-11-03** |
| Estimate | **8.9 days** |
| Features | 1 |
| Requirements | **7** — GID-255008–255014 |

### Done when

1. Slow operations — printing, data transfer, firmware update — show progress while they run.
2. The user is warned before an inactivity logout, once the parent requirement for that logout exists (question Q13).
3. **Every one of the 7 requirements below is met and can be demonstrated:**

   - **GID-255008 Authentication Warnings** — The software shall display warning messages after repeated failed login attempts and notify users when accounts are locked.
   - **GID-255009 Unsaved Data Warning** — The software shall warn users before closing windows or navigating away when unsaved data would be lost.
   - **GID-255010 Connection Status Alerts** — The software shall provide visual indicators showing whether a device is connected or disconnected.
   - **GID-255011 Firmware Update Warnings** — The software shall display warnings and precautions before initiating device firmware updates.
   - **GID-255012 Validation Error Messages** — The software shall display error messages identifying which fields contain invalid data and what corrections are needed.
   - **GID-255013 Operation Status Messages** — The software shall display progress indicators and status messages during time-consuming operations such as printing, data transfer, and device firmware updates.
   - **GID-255014 Session Timeout Warning** — The software shall display a warning message before automatically logging out users due to inactivity.

4. The seven checks above are satisfied.

---

<a id="epic-25"></a>

## Epic 31 · ASWD-31 — Logging: retention and protection

| | |
|---|---|
| Milestone | **M8** |
| Target date | Sep-27 |
| One developer | **2028-11-10** |
| Estimate | **3.2 days** |
| Features | 1 |
| Requirements | **3** — GID-255035, GID-255036, GID-255037 |
| Jira | ⚠ **needs an epic.** `ASWD-31` is a placeholder |

### Done when

1. Log files roll **daily** and also at **5 MB**, named `accusync_log_{count}_{date:yyyy-MM-dd_HH-mm-ss}.log`.
2. Files are kept **366 days**, then deleted oldest-first, automatically.
3. Pruning works on a machine that has been switched off — it does not depend on the application running.
4. An ordinary logged-in user cannot read the log folder.
5. **AccuSync provides no user function to delete or modify a log file** — no screen, menu item, service method or API.
6. A log file deleted or altered outside the application is detected on the next write.
7. The rolling, retention and protection approach is shared with Epic 25 Audit Trail rather than built twice.
8. **Every one of the 3 requirements below is met and can be demonstrated:**

   - **GID-255035 Log File Retention** — The software shall retain log files for a minimum of one year.
   - **GID-255036 Log File Security** — The software shall prevent unauthorized access to log files.
   - **GID-255037 Log Protection** — ~~The software shall prevent deletion of log files by any user.~~ ⚠ **Reworded by Natus 22 Aug 2026 to "The software shall not provide any user function to delete or modify log files." Not yet changed in Jama.**

### Not done if

- GID-255037 is signed off while DOC-076814 still carries the old, impossible wording.
- The epic is deferred past release. It is late in the order **by choice**, not optional — without it the log folder grows without limit on every installed machine.

## Epic 25 · ASWD-25 — Audit Trail

| | |
|---|---|
| Milestone | **M8** |
| Target date | Sep-27 |
| One developer | **2028-12-08** |
| Estimate | **14.2 days** |
| Features | 2 |
| Requirements | **8** — GID-255023–255030 |

### Done when

1. Audit records are written to **`%ProgramData%\Natus\AccuSync\auditTrail`** in the format **`{date: yyyy-MM-dd HH:mm:ss} {level} {user} {message}`** — set by Natus on 22 Aug 2026. This is a **separate record from the application log**, in its own folder with its own format.
2. Every login, logout, failed attempt and password change is recorded in the audit log. **Logging a login does not satisfy this** — the audit record is separate.
2. Every patient record created, changed, deleted, exported or imported is recorded.
3. Every configuration change, synchronisation and firmware update is recorded.
4. Every audit entry carries user ID, timestamp, description, device identifier and outcome.
5. Audit records are kept for at least a year and cannot be edited through the application.
6. **Every one of the 8 requirements below is met and can be demonstrated:**

   - **GID-255023 User Activity Audit Logging** — The software shall maintain an audit log of all user login/logout, login attempts, failures, and password changes.
   - **GID-255024 Patient Record Audit Logging** — The software shall maintain an audit log of all patient record creation, modification, deletion, export, and import events.
   - **GID-255025 Configuration Change Audit Logging** — The software shall shall maintain an audit log of all configuration and setting changes.
   - **GID-255026 Firmware Upgrade Audit Logging** — The software shall maintain an audit log of all firmware upgrades.
   - **GID-255027 Audit Log Entry Fields** — The software shall associate all audit log entries with user ID, timestamp, description, device serial number or identifier, and status of the operation.
   - **GID-255028 Synchronisation Audit Logging** — The software shall maintain an audit log of all successful and failed synchronisation attempts.
   - **GID-255029 Audit Log Retention** — The software shall retain audit logs for a minimum of one year.
   - **GID-255030 User Status Audit Logging** — The software shall generate an audit trail entry for each automatic user status change, including user identifier, timestamp, and triggering synchronisation event.

7. The seven checks above are satisfied.

### Not done if

- Audit entries exist for some events but not all of the ones the requirements name.

---

<a id="epic-26"></a>

## Epic 27 · ASWD-27 — About

| | |
|---|---|
| Milestone | **M8** |
| Target date | Sep-27 |
| One developer | **2028-12-08** |
| Estimate | **2.5 days** |
| Features | 1 |
| Requirements | **1** — GID-255039 |

### Done when

1. The About screen shows the real version number, manufacturer name and website — not a hardcoded string.
2. **Every one of the 1 requirements below is met and can be demonstrated:**

   - **GID-255039 About** — The software shall display information about the application, including the version number, manufacturer name and website.

3. The seven checks above are satisfied.

---

<a id="epic-28"></a>

## Epic 28 · ASWD-28 — Help

| | |
|---|---|
| Milestone | **M8** |
| Target date | Sep-27 |
| One developer | **2028-12-22** |
| Estimate | **5.3 days** |
| Features | 1 |
| Requirements | **2** — GID-255040–255041 |

### Done when

1. Help opens from inside the application.
2. Pressing the Help icon on a screen opens the page for that screen.
3. The help content itself is supplied by the customer (question Q15); this epic delivers the viewer and the mapping.
4. **Every one of the 2 requirements below is met and can be demonstrated:**

   - **GID-255040 Help Documentation** — The software shall provide access to user help documentation from within the application.
   - **GID-255041 Context Help** — The software shall provide context-sensitive help accessible via the Help icon.

5. The seven checks above are satisfied.

---

<a id="epic-30"></a>

## Epic 30 · ASWD-30 — Installation

| | |
|---|---|
| Milestone | **M8** |
| Target date | Sep-27 |
| One developer | **2029-02-09** |
| Estimate | **34.9 days** |
| Features | 4 |
| Requirements | **10** — GID-255006–255007, GID-255042–255047, GID-256510–256511 |

### Done when

1. A clean Windows 11 Pro machine and a clean Windows 11 Enterprise machine can both install and run AccuSync.
2. The installer checks prerequisites and the operating system before installing, and stops with a clear message if they are not met.
3. Both database files are encrypted to AES-256 after installation, verified by opening the file outside the application.
4. A full regression pass is run **after** encryption is switched on — everything before this epic is developed against unencrypted databases.
5. Uninstalling removes the application and the encryption key folder, and offers to keep or delete the database.
6. The application detects a missing network before calling a web service and says so clearly.
7. **Every one of the 10 requirements below is met and can be demonstrated:**

   - **GID-255006 Network Requirement Check** — The software shall require network access when exchanging data with web-service-based systems.
   - **GID-255007 Operating System Compatibility** — The software shall operate on Windows 11 (Pro and Enterprise editions).
   - **GID-255042 Patient Record Storage** — The software shall store and maintain patient test records in a local patient database on the system.
   - **GID-255043 Device Configuration Storage** — The software shall store and maintain device configurations in a local settings database on the system.
   - **GID-255044 AccuScreen Pro Patient Record Storage** — The software shall store and maintain patient test records received from the AccuScreen Pro device in a local patient database.
   - **GID-255045 AccuScreen Pro Device Configuration Storage** — The software shall store and update device configurations received from the AccuScreen Pro device in a local setting database.
   - **GID-255046 Installation** — The software shall provide a guided installation process that verifies prerequisites and system requirements before installation.
   - **GID-255047 Uninstallation** — The software shall provide an uninstall process that removes all application components with an option to preserve or delete database files.
   - **GID-256510 Patient Database Encryption** — The software shall encrypt all patient test records stored in the local patient database (minimum AES-256).
   - **GID-256511 Settings Database Encryption** — The software shall encrypt all settings configurations stored in the local settings database (minimum AES-256).

8. The seven checks above are satisfied.

### Not done if

- Encryption is switched on but nothing was re-tested afterwards.
- The encryption approach was decided at install time rather than in week 3, so the whole product was designed without knowing what it costs.
- The regression pass in F3 is skipped on the assumption that switching provider is harmless. It changes the connection string, and can change file locking and performance.


---

## Epic 33 · ASWD-33 — Release: release testing and documentation

| | |
|---|---|
| Milestone | **M8** |
| Target date | Sep-27 |
| One developer | **2029-05-11** |
| Estimate | **32.5 days** |
| Features | 2 |
| Requirements | **none** — no requirement in DOC-076814 covers release, testing or documentation |
| Jira | ⚠ **needs an epic.** `ASWD-33` is a placeholder |

### Done when

1. **All 196 requirements are tested by hand** against a named build, with the evidence recorded, and what fails re-tested after the fix.
2. The product is **installed and proven on a clean machine** of each supported Windows version.
3. Anything still failing is a **recorded known issue**, not a silence.
4. The **user manual and release notes** are written against the finished product, and the manual agrees with the in-application help.

### Not done if

- A requirement is signed off on the strength of a unit test.
- The test pass only ever ran on a developer machine.
- The manual describes screens that changed after it was written.
- The release notes are silent about issues the test pass found.

> **Nothing in this epic changes the application.** The user manual is a separate document and the help check is a review, so a task here is one piece of work rather than service, ViewModel and screen layers.

> ⚠ **Risk R3 is reopened for what was cut** on 26 Aug 2026: the integration test suite, requirement traceability and the verification report, the IEC 62304 lifecycle records, and the ISO 14971 risk file. The last two are mandatory on a medical device, so the likely answer is that **Natus’s own quality and regulatory function owns them**. If it turns out to be Soliton, about 50 days come back into the plan.

---



