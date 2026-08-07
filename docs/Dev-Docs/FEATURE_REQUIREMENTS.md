# AccuSync — Feature Requirements

**Source:** Reverse-engineered from the existing WPF proof-of-concept (POC) codebase (`AccuSync/`), built to validate the client's requirements with them. This document is the feature baseline for the from-scratch WPF rebuild (multi-project solution).

**How to read this document:** Each section describes a functional area as it should work as a product feature. Where the POC's implementation is a stub, hardcoded, or otherwise not representative of the intended final behavior, this is called out explicitly in a **"POC gap"** note so the rebuild does not mistake a placeholder for a spec. Database column names are cited where they pin down a field's type/enum precisely.

**Product domain:** Newborn hearing screening clinical software. Patients (newborns) are screened using ABR (Auditory Brainstem Response), TEOAE (Transient Evoked Otoacoustic Emissions), and DPOAE (Distortion Product Otoacoustic Emissions) hardware devices. Results (Pass / Refer / Incomplete) are tracked per ear, aggregated to the patient/session level, and exported to hospital/state reporting systems. The POC includes real, working parsers for three device data formats: **AccuLink XML** (Natus AccuScreen family), **ALGO 5 XML**, and **ALGO Pro JSON** (Natus ALGO family) — confirming genuine device-integration requirements, not placeholder formats.

---

## 1. Application Shell & Startup

### 1.1 Splash Screen
- On launch, show a branded splash window while the app initializes.
- Sequence: initialize/open the local database → show "Initializing database..." → show "Loading application..." → proceed to Login (or Dashboard, in dev mode).
- If database initialization fails, show a clear error and exit gracefully (no partially-initialized app state).

### 1.2 Login
- Username selection from the list of provisioned accounts (dropdown of active users) + password entry with show/hide toggle.
- "Sign In" authenticates via the Authentication Service (see §2).
- Inline error messaging for invalid credentials, locked accounts, and expired passwords.
- On first login (`FirstLogin` flag), force a password change before entering the app.
- Role (Admin vs. Screener) is resolved from the user's assigned **Profile**, and determines whether the Admin Dashboard or Screener Dashboard opens after login.
- **POC gap:** role is currently inferred by string-matching the username against `"Admin"` rather than reading the user's actual `Profile`/permission set. The rebuild must derive role/landing screen from `Users.ProfileId → Profiles`, not from username.

### 1.3 Forced Password Change
- Triggered on first login. Requires: current password, new password, confirmation.
- Live checklist while typing shows pass/fail for each rule (see §2.2 password policy).
- Save is disabled until all rules pass. Rejects reuse of the last 3 passwords.
- On success, routes directly into the correct dashboard for the account's role.

### 1.4 Application Shell (post-login)
- Persistent **Sidebar Navigation** with these destinations, gated per-user by Profile permissions (see §12.3 for the permission-to-nav mapping):
  - Dashboard, Patients, Users (Users & Profiles), Sites (Sites, Facilities & Locations), Devices (Device Management), System Configuration, Settings, About.
- Each screen is hosted in a consistent shell: a top **Ribbon Toolbar** (screen-specific actions, grouped and labeled) above a content area.
- "Takeover" sub-screens (e.g., Profiles inside Users, Facilities/Locations inside Sites, ABR/DPOAE/Field Setup inside Devices, the seven System Configuration sub-screens) replace the parent screen's content area and ribbon in place, with a **Back** action to return — this is a deliberate in-place drill-down pattern, not a modal or new window, and should be preserved in the rebuild's navigation architecture.
- Common ribbon action vocabulary reused across screens: **Add, Edit, Delete, Save, Revert, Undo, Back, Help**, plus screen-specific groups (Import/Export/Print, Send/Receive/Update Device, Unlock, etc.).
- A dev-only bypass mechanism exists (flag files under `%ProgramData%\AccuSync\`) to skip Login and/or jump straight to a named screen, for faster manual testing. This should be preserved as an internal developer/QA convenience, gated so it cannot be enabled in a production build.

### 1.5 Roles
Two operational roles are modeled by the POC's Admin/Screener dashboards, with a `Profile`-driven permission system underneath capable of finer-grained roles (see §12):
- **Admin** — full access to all navigation destinations and the full permission set.
- **Screener** — day-to-day screening workflow: Dashboard, Patients, About by default; everything else gated by assigned Profile permissions.
- The permission model also anticipates a **Supervisor**-style profile (seen as a third seed profile in the POC) and a fully **read-only** profile — the Profile/permission system (§12) is the correct general mechanism; hardcoded role checks are a POC-only shortcut to remove.

---

## 2. Authentication, Passwords & Security

### 2.1 Login & Lockout Rules
- **Lockout:** after **10 consecutive failed login attempts**, the account locks for **15 minutes**, then automatically unlocks on the next attempt after the cooldown elapses. Failure count resets to zero on any successful login.
- Login failure messages never reveal whether a given username exists (generic "Invalid username or password" for both "no such user" and "wrong password").
- **Password expiration:** passwords expire **90 days** after last change. An expired password blocks login; the account requires an administrator to intervene (no self-service reset flow yet — a real product needs one; see §2.4).
- **Admin unlock:** an administrator can manually unlock a locked account from the Users screen. This must clear the failed-attempt counter and the failure-window start time, not just the locked flag.

### 2.2 Password Policy (forced-change / self-service change)
Enforced live in the UI, all rules required to save a new password:
- Minimum length: 8 characters.
- At least one uppercase letter, one lowercase letter, one digit, one special (non-alphanumeric) character.
- New password must match its confirmation.
- New password must differ from the current password.
- New password must not match any of the **last 3** passwords used.
- A system-wide "Password Security Rule" setting (`SystemSettings.PasswordSecurityRule`: None / Simple / Complex) is configurable in System Configuration → User/Profile Config, intended to vary how strict this policy is (POC only stores the setting; the rebuild should make the live checklist actually respect it rather than always enforcing the fixed 6-rule set above).

### 2.3 Encryption & Credential Storage
- Sensitive values at rest (passwords, and any PII such as SSN) must be protected using a **non-reversible, salted hash** (e.g., bcrypt/PBKDF2/Argon2) — **not** reversible encryption.
- **POC gap (must fix, not replicate):** the POC uses reversible AES-256-CBC "encryption" for passwords with a machine-name-derived key and a **hardcoded salt**, a **static IV**, and it is silently disabled outside Release builds. This is explicitly flagged in the POC's own code comments as unsafe and must not be carried into the rebuild. Passwords must be hashed one-way at rest; comparison must be hash-to-hash, never decrypt-and-compare.
- Any key material must live in a proper secret store (e.g., Windows DPAPI, Credential Manager), never derived from a hardcoded string in source.

### 2.4 Account/User Security Fields (data model)
Per user: account name, hashed password, first-login flag, failed-login-attempt count + first-failure timestamp, password-last-changed timestamp, last 3 password hashes (for reuse checking), active/inactive flag, locked flag + locked-at timestamp, last-login timestamp, assigned Profile, assigned Site, language preference.

### 2.5 Session Locking
- A configurable **inactivity locking timer** (`SystemSettings.LockingTime_min`, default 15 minutes) should lock the session (require re-authentication) after the configured idle period. (POC stores the setting but does not implement the actual idle-lock behavior — this is a real requirement to build.)

### 2.6 Audit Logging
- Every create/update/delete/import/export/login-relevant action should be written to an append-only audit trail — one structured (JSON) record per event with timestamp, acting user, action, entity type/id, and a note — rolling daily, retained 90 days. (Modeled in the POC's schema comments as a file-based Serilog sink rather than a DB table; the rebuild should keep audit events out of the transactional tables but must actually implement writing them — the POC does not yet emit any.)

---

## 3. Dashboard

Two role-specific dashboards, sharing a common visual pattern (greeting bar with time-of-day-aware message and current date, a row of clickable KPI stat cards opening detail dialogs, and a Quick Actions row) but differing in content:

### 3.1 Admin Dashboard
KPI cards (each opens a filtered patient/screening list dialog):
- **Referred (7 days)** — patients referred in the trailing 7 days.
- **Pass (7 days)** — patients who passed in the trailing 7 days.
- **Incomplete (7 days)** — screenings started but not completed.
- **Screenings Today** — opens a per-screener activity list (screener name, screening count, last-activity time) for today.
- **Not Exported** — patients with completed results not yet exported, across all screeners (shows which screener owns each).
- **Devices Needing Update** — devices whose firmware is behind the required version.
- Quick Actions: Add Patient, Import, Export, (Reports and Upload are planned but not yet implemented).
- A statistics section (average session time, per-screener test/refer/pass counts) for a "today" or trailing-window view — the POC's version is a static table; the rebuild should back this with a real trend visualization (e.g., a small chart) fed by actual data.

### 3.2 Screener Dashboard
KPI cards (scoped to the logged-in screener):
- **Assigned Today** — patients assigned to this screener today, not yet screened.
- **Pending** — screenings waiting to be performed, with elapsed wait time and risk-factor badges (high-priority flags such as NICU stay, family history).
- **Completed** — screenings this screener finished today, with result and duration.
- **Not Exported** — this screener's completed screenings not yet exported.
- **Referred / Pass** — same rollups as Admin, scoped to this screener (planned — not implemented in the POC beyond the card shell).
- Quick actions equivalent to Admin's for this screener's own workload.

### 3.3 Screening Lifecycle (state model)
The product must model a single, unambiguous lifecycle per patient/screening, resolving the POC's inconsistencies (documented in §3.4):

1. **Unassigned** — registered patient awaiting assignment to a screener.
2. **Assigned / Pending** — owned by a screener, screening not yet performed. Should be a *single* state (the POC splits this into two different dashboards/models — "Assigned Today" vs. "Pending" — that describe the same thing).
3. **Completed** — screening performed, with a definitive result.
4. **Result:** exactly one of **Pass**, **Refer**, or **Incomplete** (the last meaning the test could not be completed — e.g., probe error — and requires rescreening; it is a result value, not a separate pipeline stage).
5. **Exported / Not Exported** — a boolean/flag on the completed screening (or on the patient's most recent batch of results) indicating whether it has been synced to the destination system. This must be a real field on the screening/session record (`TestSessions.IsExported` / `Patients.IsExported` already exist in the schema for this) — not a derived ad hoc list.

### 3.4 Known inconsistencies to resolve in the rebuild
- The Admin and Screener dashboards currently use overlapping-but-different KPI vocabularies and separate dialog implementations for conceptually identical data (e.g., "Not Exported" is colored/labeled differently on each). Decide on one canonical, role-scoped presentation of the lifecycle in §3.3, not two parallel designs.
- The POC uses two incompatible ID schemes for the same conceptual patient ("MRN" e.g. `P-1201` in some dialogs vs. a "PatientId" e.g. `PT-2025-003` in others). The rebuild's data model must have one canonical patient identifier (the DB schema's auto-increment `PatientId` plus the clinical `PatientRecordNumber`/`HospitalId` fields — see §13) used everywhere.
- "Incomplete" is used inconsistently as both a *result value* and a *status distinct from Pending*. Standardize per §3.3.

---

## 4. Patient Management

### 4.1 Patient List
- Master list of patients with: avatar/initials, First/Last Name, Patient ID, Gender, Date of Birth, Date of Screen.
- **Two card layouts**: Detailed and Compact, user-toggleable.
- **Search**: free-text match across First Name, Last Name, Patient ID, Gender, Date of Birth.
- **Field-specific filters** (togglable via a filter panel): First Name, Last Name, Date of Birth (range), Patient ID, Gender — combinable (AND) with the free-text search.
- **Sort**: Last Name (A–Z, default), First Name (A–Z), Date of Birth (newest/oldest), Date of Screen (recent first), Patient ID.
- **Pagination**: configurable page size (5/10/25/50, default 10), numbered page navigation.
- **Bulk selection mode**: per-row checkboxes, select-all (tri-state), bulk delete with confirmation.
- Selecting a patient loads their full record into the Patient Information panel and their test history into the Test Results panel (see §4.2, §5) in the same screen — a three-panel master/detail/detail layout, not separate windows.

### 4.2 Patient Information (detail/edit form)
Three tabs, plus a header summary strip showing a **QR code** encoding key identifiers (name, patient ID, DOB, gender, hospital ID) for physical chart/label printing or device scanning.

**Patient tab**
- Core identifiers: Patient ID *(required)*, Hospital ID *(required)*, First Name, Last Name *(required)*, Date of Birth (with a "set to today" shortcut), Gender (Male/Female/Other), Gestational Age (weeks, 20–45), Weight (g), Height (cm), Birth Location (Hospital/Home/Birth Center/Other), Nationality.
- Consent & status: Screening Consent (Yes/No), Consent State (No/Screening/Full), NICU (Yes/No), Discharged date, Deceased date, Tracking Consent (No tracking/Simple/Full).
- Free-text Comments field (max 1000 chars, with a live counter that warns as the limit approaches).
- Required-field validation (Patient ID, Hospital ID, Last Name) is enforced before save, with a visible count-badge and a popup listing exactly which required fields are still empty.

**Additional tab** (with in-page "jump to" quick links)
- **Mother Information**: Title, SSN (auto-formatted, must be masked in the UI e.g. `•••-••-1234` — the POC formats but does not mask this field, which must be corrected for PII protection), Mother ID, First/Last Name, DOB, Language, Address (1/2, City, State, Zip, Country), Phone + Mobile (with per-country dial code and live formatting), Fax, Email.
- **Caregiver Information**: same field set as Mother, minus DOB.
- **Referral Information**: Audiology Referral, Referral Date, Referral To, Referral From, Referral Phone.
- **Medical Information**: Medication, Physician, Audiologist.
- Names auto-capitalize as typed (title case). Phone numbers auto-format per the selected country's dial code (14 countries supported: US/Canada, Mexico, UK, France, Germany, Japan, China, India, Australia, Brazil, Italy, Spain, Russia, South Korea), enforcing each country's correct maximum digit count.

**Risks tab**
- 16 standardized JCIH-style risk factors, each set to **Yes / No / Unknown** via a tri-state control: Family History of Hearing Loss, Low Birth Weight, Hyperbilirubinemia, Asphyxia/HIE, Craniofacial Anomalies, Syndromes Associated with Hearing Loss, In-Utero Infections, Bacterial Meningitis, Perinatal/Postnatal Infection, Ototoxic Medications, Aminoglycosides, Prolonged Ventilation (>5 days), ECMO, NICU Stay >5 Days, Head Trauma, Caregiver Concern.
- Bulk-set actions: "All Yes" / "All No" / "All Unknown".
- A running summary shows how many of the 16 have been answered and how many are "Yes", split into Perinatal / Postnatal / Other categories.
- This exact 16-item list is also the canonical taxonomy used by facesheet OCR/PDF/DOCX extraction (§6.4) and must stay in sync everywhere it appears.

### 4.3 Editing Workflow
- **View mode** (default): read-only overlay ("click to edit").
- **Edit mode**: unlocks all fields.
- **Add mode**: blank form, immediately in edit mode, with a visible "new patient" indicator; canceling discards the whole draft.
- **Save**: blocked with a validation prompt if required fields are empty; otherwise commits the edit session and regenerates the QR code.
- **Revert**: discards all unsaved changes back to the last saved state (with confirmation).
- **Undo**: rolls back the single most recent field-level edit (independent, granular undo — not the same as Revert).
- **Delete**: removes the patient record (with confirmation).
- All of the above must be backed by real persistence to the database in the rebuild — the POC's Save/Revert/Undo/Delete on this screen only mutate in-memory state.

### 4.4 Test Results Panel (per selected patient)
See §5 for full detail — this panel is the third column of the Patients screen, always showing the currently selected patient's test history.

---

## 5. Test Results

### 5.1 Test List
- A read-only, single-select table of every test performed for the selected patient: Test Type (ABR/TEOAE/DPOAE), Left-ear result symbol, Right-ear result symbol (✓ Pass / ✗ Refer / ? Incomplete, colored, blank if the row's test wasn't performed on that ear), Date & Time, Configuration/Protocol, Duration, Examiner.

### 5.2 Test Detail (for the selected test row)
- **Test Result tab**: waveform visualization (the POC only has a placeholder here — a real implementation must render the actual waveform, from `ABRResults.WaveformPointsJson` / `TEOAEResults.WaveformPointsJson` / DPOAE's `ReListJson`/`ImListJson`, see §13.3), ABR-specific electrode impedance readings (White/Red, kΩ) and an EEG-noise indicator, and the overall Pass/Refer/Incomplete status with duration and ear.
- **DP Details tab** (DPOAE tests only): per-frequency level/noise-floor/SNR/pass detail (backed by `DPOAEResults`, see §13.3) — the POC only stubs this tab.
- **Test Comments tab**: attach one of the standardized predefined comments (14 canned phrases such as "Patient Restless," "Noisy Conditions," "Wax in Ear Canal," "Retest Recommended," etc. — the same list configurable in System Configuration → Comments, §10.2) to the test record; must persist to the record (POC only shows a confirmation, doesn't save).
- **Device Information tab**: device serial, firmware build, test facility, location, probe serial, probe calibration date — all captured from the originating device/import, read-only here.

### 5.3 Data model implications
Each individual test record must carry: type (ABR/TEOAE/DPOAE — should be a real enum, not a free string), which ear (Left/Right/Binaural), result (Pass/Refer/Incomplete), date/time, duration, device identity + firmware, probe identity + calibration dates, facility/location/examiner, and a link back to the raw imported binary "TestDetail" blob for full-fidelity preservation plus a pre-parsed summary for fast UI rendering (see §13.3 — this two-tier storage strategy from the POC's schema design should be kept).

---

## 6. Import

### 6.1 Supported Formats
| Format | Tag | Scope | Status |
|---|---|---|---|
| AccuLink XML (Natus AccuScreen family) | `acculink-xml` | Batch, multi-patient | Implemented parser |
| ALGO 5 XML (Natus ALGO 5) | `algo5-xml` | Batch, multi-patient, ABR only | Implemented parser |
| ALGO Pro JSON (Natus ALGO Pro) | `algopro-json` | Batch, multi-patient, ABR only (DPOAE/TEOAE anticipated for future firmware) | Implemented parser |
| AccuSync XML / AccuSync JSON | `accusync-xml` / `accusync-json` | Batch — the app's own round-trip export format | **Not yet implemented** — required for re-import of previously exported data |
| Facesheet — scanned image (OCR) | `facesheet-image` | Single patient | Implemented (Tesseract OCR) |
| Facesheet — PDF | `facesheet-pdf` | Single patient | Implemented (PdfPig text extraction) |
| Facesheet — DOCX | `facesheet-docx` | Single patient | Implemented (OpenXML text/table extraction) |

### 6.2 Import Flow — Batch Device Formats (XML/JSON)
1. **Select format + file** (drag-and-drop or browse; validates file extension matches the chosen format).
2. **Preview & select**: a table of all patients found in the file, each flagged **New** or **Duplicate** (matched against existing records by device-assigned source ID), with per-patient test counts; supports select-all and per-row selection. Clicking a row opens a slide-in detail panel listing every test in that patient's file with date/ear/type/result.
3. **Duplicate resolution**: for each duplicate, choose one of — **Replace Existing**, **Create New** (import as a separate patient), **Add Tests** (merge new tests onto the existing patient, default choice), **Skip Patient**.
4. **Results**: three collapsible outcome groups — Imported (with a badge: Tests Added / Replaced / New Record), Errors (with the reason), Skipped (deselected or user-skipped).
5. Every batch import is recorded as an **Import Batch** (source file name, source system, source version, file timestamp, counts of patients found/imported/skipped) for audit/traceability (`ImportBatches` table, §13.2).
6. **POC gap:** duplicate detection and actual database persistence during import are stubbed (`existsInDatabase`/`saveToDatabase` delegates are always `null`) — the rebuild must wire real duplicate lookup (by `SourceId`) and real writes.

### 6.3 Import Flow — Facesheet Formats (Image/PDF/DOCX)
Single-patient flow for scanned/typed hospital intake forms:
1. Select format + file.
2. **Facesheet Review**: split view — original document preview (image viewer, or extracted text for PDF/DOCX) alongside an editable 3-tab form (mirrors the Patient/Risks/Additional tabs from §4.2) pre-populated from extraction.
3. For OCR (image) imports, an **extraction-confidence indicator** is shown so the screener knows how much to double-check.
4. User corrects any misread fields, then imports directly as a new/updated patient record (no duplicate-resolution step needed — always a single patient).

### 6.4 Facesheet Field Extraction Rules
Regex/keyword-based extraction (consistent across the three parsers) must recognize, at minimum:
- Name formats: standard newborn hospital naming conventions (e.g., "LASTNAME, BABY", "Lastname, Girl A Firstname", "Lastname, Baby 1 day old"), plus generic labeled fields.
- Patient ID: Medical Record #, Chart ID, MRN, or Patient ID labels, stripping any hospital-specific suffix.
- Date of Birth: multiple common date formats; must not accidentally match an insurance "Subscriber DOB" field.
- Gender: Male/Female/M/F in various label contexts.
- Weight/Height: with guards against false positives (e.g., a 4-digit weight that's actually a year, a 5-digit weight that's actually a ZIP code).
- Mother's name and phone.
- All 16 canonical risk factors (§4.2 Risks tab), scored Yes/No/Unknown by keyword + context-window matching (e.g., "positive"/"present" → Yes; "negative"/"absent"/"denied" → No).
- **PII handling requirement:** none of the extracted PII (names, SSNs, DOB, etc.) may be written to debug/trace logs in a production build — the POC currently logs full extracted text and field values, which must be removed/gated.

### 6.5 Import Configuration (admin-managed profiles)
System Configuration → Import Configuration lets an admin define named, reusable import profiles: Name, Description, Import Format (one of the formats in §6.1's batch/facesheet set), an associated Profile (permissions to apply when this configuration runs), and a password (hashed, not stored plaintext), plus a default Import Folder to watch/browse. (POC seeds one example profile; no persistence.)

---

## 7. Export

### 7.1 Supported Formats
AccuSync JSON, AccuSync XML (round-trip formats — **not yet implemented**, needed to close the loop with §6.1's matching import formats), HiTrack, OZ, CSV, ALGO 5 XML.
- CSV export is the only fully working format in the POC today (columns: Last Name, First Name, DOB, Patient ID, Gender, Date of Screen, Left Ear result, Right Ear result), used as the reference for what every other format must eventually support.

### 7.2 Export Scope
One of: **New Results** (not yet exported since last export), **All Patients**, **By Date Range** (with from/to validation), **Selected Patients** (from a prior selection).

### 7.3 De-identification
An optional **"De-identify Patient Records"** toggle strips/masks direct identifiers for research or non-clinical downstream use — the exact de-identification rule set (which fields get removed vs. masked) needs to be defined precisely with the client during rebuild.

### 7.4 Export Configuration (admin-managed profiles)
System Configuration → Export Configuration lets an admin define named export profiles: Name, Description, Export Format (§7.1's list), Export Data scope (All Patients / Changed Patients / New Tests Only / Selected Patients), and a default Export Folder.

### 7.5 Consequence of a completed export
A successful export must set the exported flag/timestamp on the affected patient/session records (`IsExported`, `ExportedAt` — see §13.2), which drives the "Not Exported" dashboard KPIs (§3).

---

## 8. Printing / Reports

- A **Print** dialog lets the user choose one report to print for the current context, from 10 combinations: report category (**Basic** or **Detail**) × report type (**Best Tests**, **Best TEOAE Test**, **Best DPOAE Test**, **Best ABR Test**, **Selected Tests**).
- "Best" reports presumably mean the best/most recent passing result per ear/test type for the patient — this needs to be confirmed precisely with the client, as the POC only defines the selector UI, not the report content/layout.
- Report paper format (A4/Letter) and an optional hospital logo (uploaded once, used on all printed reports) are configured system-wide in System Configuration (§10.1).
- **POC gap:** no actual report rendering/PDF generation exists yet — this whole area (report layouts, data binding, print-preview, PDF export of a report) is new work for the rebuild, not just data-wiring.

---

## 9. Device Management

### 9.1 Devices List & Configuration
Two-panel list (Name, Serial, Last Seen) / detail editor with three tabs:

**Configuration tab**
- *Individual Setting*: Name *(required)*, Serial *(required)*, Code, Site (assignment), UI Language (English/French/Italian/German/Spanish).
- *Common Configuration*: Test Result Terminology (Pass/Refer vs. Clear Response/No Clear Response), Power Timeout (1–30 min, default 5), Display Timeout (1–5 min, default 3), Calibration Pause (1–30 min, default 10), Data Deletion policy (Manual vs. After Successful Download), ABR Autostart behavior (No Autostart / On Green / On Green or Yellow), TEOAE Probe Fit Assistant (Enabled/Disabled).
- *System Information* (read-only, populated from device sync/import): Last Seen, Last Updated, Hardware Version, Firmware Version.
- Device identity fields (source instrument ID/signature, serial, **device type**: ABR/TEOAE/DPOAE/Multi) are populated from import data and not user-editable.

**User Assignment tab**
- Assign system users to a device (many-to-many), with a per-user "Copy to Device" flag indicating whether that user account should be pushed to the physical device on next sync.

**Facilities tab**
- Assign facilities to a device (many-to-many, replacing an earlier single-facility design), each with its own "Copy to Device" flag.

**Actions**: Add, Delete (with confirmation), Save, Revert, Undo — standard pattern across the app (§1.4). Full change history while editing is tracked so Undo/Revert both work correctly.

### 9.2 Protocol Assignment
- A device can have one or more assigned **test protocols**, one per test type it supports (ABR and/or DPOAE — TEOAE has no separate protocol concept), each protocol optionally marked as the device's default for that test type.
- **POC gap:** the protocol-definition screens (§9.3, §9.4) exist and work, but there is no UI on the Devices screen itself to actually *assign* a protocol to a specific device — this picker needs to be added in the rebuild.

### 9.3 ABR Protocol Configuration
List/detail editor for named ABR test protocols: Name *(required)*, Category (Basic/Enhanced, required), Description, Status (Active/Inactive), and the clinical parameters — **ABR Level** (30/35/40/45/50 dB nHL), **Notch Filter** (50/60 Hz), **Stimulus During Pause** (Yes/No).

### 9.4 DPOAE Protocol Configuration
List/detail editor for named DPOAE test protocols: Name *(required)*, Category (Basic/Enhanced, required), Description, Status (Active/Inactive), and clinical parameters — **L2** (50/55/60/65 dB SPL), **L1** (Auto / L2+10dB / L2+5dB), **F2 Frequencies** (any combination of 1000/1500/2000/3000/4000/5000/6000 Hz as toggle buttons), **Retest Frequencies** (Yes/No), **Pass Criterion / Min Pass Points** (1 through 6 of 6 frequency points must pass), **Auto Stop** (Yes/No), **Min Level** (Off/0/-5/-10/-15 dB), **SNR** required to pass a frequency point (6/9/12 dB).

### 9.5 Per-Device Field Setup
A device-scoped override of the system-wide Field Setup (§10.3): for each patient/clinical field, override whether it's Active (visible) and Mandatory on that specific device, and optionally a device-specific custom display label. Falls back to the system-wide setting when no override exists. Includes a "Patient ID Rule" per-device override (None / NHSP England — a UK newborn-screening ID validation rule). Does **not** include the "Include in QR" option — that stays system-wide only.

### 9.6 Firmware Update
- A dialog to select a folder, auto-detect a firmware file within it (recognized extensions: `.afw`, `.bin`, `.pkg`, `.hex`, `.fw`), show the detected file name/size alongside the device's current firmware/hardware version, and confirm the update.
- The Dashboard's "Devices Needing Update" KPI (§3.1) surfaces devices whose firmware is behind the fleet's required version and lets an admin push the update from there too.
- **POC gap:** only the file-selection UI exists; the actual firmware transfer/flash mechanism to the physical device is out of scope for the POC and must be designed with hardware/firmware engineering input.

---

## 10. System Configuration

A settings hub screen plus seven drill-down sub-screens (all follow the standard Add/Edit/Delete/Save/Revert/Undo pattern where they're list/detail screens).

### 10.1 Hub screen (own settings)
- System-wide UI Language (default for any user without a personal language preference).
- Data confirmation behavior: whether to show a confirmation prompt on Save, on Delete, and when modifying existing data (each independently Yes/No).
- Report configuration: hospital logo (upload/drag-drop, with restore-default), default paper format (A4/Letter).

### 10.2 Risk Factors Configuration
Manage the master list of clinical risk factors (the same 16 shown on the Patient's Risks tab, §4.2): Name, Description, Active flag, plus full 5-language translation (English/French/Italian/German/Spanish) of Name and Description. A risk factor **in active use** on any patient record cannot be deleted (delete-protection, enforced by an "in use" flag) — the only config screen in the POC with this protection, and a pattern that should extend to Comments (§10.2 below) and any other reference data with foreign-key usage.

### 10.3 Predefined Comments Configuration
Manage the master list of quick-select comments used on Test Results (§5.2) and elsewhere: Comment text, Active flag, full 5-language translation. Same in-use delete protection as Risk Factors.

### 10.4 Field Setup Configuration (system-wide)
A comprehensive table (~68 rows covering Patient, Mother, Caregiver, Referral, and Other field groups, plus 4 generic "Available/Free Field" slots for site-specific custom fields) controlling, per field: a custom display label override (falls back to the hardcoded default name), Active/visible, Mandatory, and **Include in QR Code** (whether this field's value is embedded in the patient's printed QR code, §4.2). Includes select-all header checkboxes per column and a "reset all custom labels" action. Also configures the system-wide **Patient ID Rule** (None / NHSP England).

### 10.5 User & Profile Configuration
Two small global settings: **AccuSync session locking time** (1–60 minutes, default 15 — drives §2.5's idle-lock behavior) and the system's **Password Security Rule** (None / Simple / Complex, drives §2.2's policy strictness), with a description of what each rule level means.

### 10.6 Site & Facility Configuration
One setting: whether to automatically **transfer sites and facilities to devices** on sync (on by default).

### 10.7 Import Configuration
See §6.5.

### 10.8 Export Configuration
See §7.4.

---

## 11. Sites, Facilities & Locations

A three-level organizational hierarchy that scopes users, devices, and patients:

- **Site** — top-level organizational unit (e.g., a hospital network or region). Fields: Name *(required)*, Code *(required)*, Description, Active flag.
- **Facility** — belongs to exactly one Site. Fields: Name *(required)*, Code *(required)*, Site *(required)*, Location Type (**Home Visit / Inpatient / Outpatient**), Description, Active flag.
- **Location** — a named sub-location within a facility (e.g., a specific screening room), belonging to exactly one Facility and (denormalized for query efficiency) its parent Site. Fields: Name *(required)*, Code *(required)*, Description, Active flag.
- Navigation: Sites is the top-level screen; Facilities and Locations are "takeover" drill-down screens reached from it, each with their own Add/Edit/Delete/Save/Revert/Undo ribbon.
- **POC gap — must fix in rebuild:** the POC's Locations screen has no Facility/Site picker at all (it's disconnected from the hierarchy in the UI, even though the underlying schema correctly requires both `FacilityId` and `SiteId`). The rebuild's Locations editor must let the user choose the parent Facility (and derive/confirm the Site from it).
- None of the three screens currently expose the Active/Inactive toggle or audit timestamps in the UI, despite the data model having them — the rebuild should surface Active (to support retiring a site/facility/location without deleting history) at minimum.

---

## 12. Users & Profiles (Permissions)

### 12.1 Users
List/detail management of user accounts: Login Name *(required)*, Profile assignment *(required)*, First/Last Name, Password + confirmation (with live match indicator), per-user Language preference (overrides the system default, §10.1), Status (Active/Inactive), and a read-only Locked indicator with an **Unlock** action (see §2.1's note on what a correct unlock must reset).

### 12.2 Profiles (permission sets)
A named, reusable permission bundle assignable to users. The full permission surface — organized into 6 functional groups, 33 individually grantable permissions — is:

| Group | Permissions |
|---|---|
| **AccuScreen Management** | All Tests, Quick Test, Basic Tests, Delete Patients, Edit Patients |
| **Device Management** | Add/Edit, Configure Modules, Delete, View |
| **Patients and Tests** | Add/Edit, Comment Maintenance, Configure Management, Delete, Reassign Tests, Risk Factor Maintenance, View, **Export**, **View Reports** |
| **Sites and Facilities** | Add/Edit Facilities, Add/Edit Sites, Configure Management, Delete Facilities, Delete Sites, View Facilities, View Sites |
| **System Configuration** | Configure, View, **Access Settings** |
| **Users and Profiles** | Add/Edit Profiles, Add/Edit Users, Configure Management, Delete Profiles, Delete Users, Reset Users, View Profiles, View Users |

The permissions in **bold** above (Patients: Export, View Reports; System Configuration: Access Settings) are defined in the data model but have **no UI control yet** in the POC's Profiles editor — these must be added in the rebuild so every stored permission bit is actually assignable by an administrator.

Each profile's editor presents a component-grouped checklist (tri-state "select all" per group, live granted-count badge) — this UX pattern should be preserved.

### 12.3 Navigation-to-Permission Mapping
The sidebar (§1.4) shows/hides each destination based on the logged-in user's effective permissions:

| Nav item | Gate |
|---|---|
| Dashboard | always visible |
| Patients | `Patients_View` (or similar "can access patients" gate) |
| Users | any Users_* permission |
| Sites | any Sites_* permission |
| Devices | any Device_* permission |
| System Configuration | `SysConfig_View` / `SysConfig_Configure` |
| Settings | always visible (per-user preferences) |
| About | always visible |

Feature-level gating within a screen (e.g., can this user Delete a patient vs. only View) must also be checked against the specific permission bit, not just screen-level access.

---

## 13. Data Model (Reference)

The POC ships a two-database SQLite design (clinical data separate from configuration data) — a reasonable separation of concerns to carry into the rebuild, whatever the final RDBMS choice (SQLite/SQL Server/etc.):

### 13.1 SettingsDatabase — organizational & configuration data
`Sites`, `Facilities` (→Sites), `Locations` (→Facilities, denormalized →Sites); `Profiles` (33 permission bits, §12.2), `Users` (→Profiles, →Sites); `Devices` (→Sites; identity fields from import; individual + common configuration fields per §9.1; `DeviceType`: ABR/TEOAE/DPOAE/Multi), `DeviceUsers` (M:N, +CopyToDevice), `DeviceFacilities` (M:N, +CopyToDevice), `DeviceFieldSetup` (per-device field overrides, §9.5); `RiskFactors` + `RiskFactorTranslations`; `PredefinedComments` + `PredefinedCommentTranslations`; `FieldSetup` (system-wide field config, §10.4); `ImportConfiguration`, `ExportConfiguration`; `ABRProtocols`, `DPOAEProtocols`, `DeviceProtocols` (M:N device↔protocol, §9.2); `SystemSettings` (generic key/value store — language, confirmation prompts, logo path, paper format, patient ID rule, locking time, password rule, transfer-to-device flag).

### 13.2 PatientDatabase — clinical data
- `ImportBatches` — one row per import operation, for full audit trail (source file, source system/version, counts imported/skipped, timestamp).
- `Patients` — demographics, NICU/discharge/deceased status, consent fields, gestational age, risk factors (as a JSON array of `RiskFactors.Code` values), referral info, 3 free-text fields + 4 configurable "free fields" (labels from `FieldSetup`), soft-delete flag, export flag + timestamp, source vs. local created/modified timestamps.
- `PatientContacts` — one row per contact (Patient/Mother/Father/Caregiver) per patient: name, SSN/ID, demographics, address, phone/mobile/fax/email.
- `TestSessions` — groups same-day tests for a patient; carries the aggregate result (Pass/Refer/Incomplete) and export flag/timestamp.
- `TestRecords` — one row per individual ear-level test (ABR/TEOAE/DPOAE): result, duration, device/probe identity and calibration snapshot (denormalized per-test, since each test carries its own calibration state), links to a matched Device/Protocol (resolved at import time), the raw base64 `TestDetail` blob (full-fidelity original) and a pre-parsed `SummaryJson` (fast UI rendering without deserializing the blob).
- `ABRResults`, `TEOAEResults`, `DPOAEResults` — 1:1 detail tables per `TestRecord`, populated by deserializing the blob: waveform point data, per-frequency/per-band metrics (SNR, reproducibility, DP level/noise), electrode impedance, calibration reference data. This is the data the real waveform/DP-gram visualizations in §5.2 must render from.

### 13.3 Cross-database references
Columns like `Patients.SiteId` or `TestRecords.MatchedDeviceId` reference the other database by plain integer ID (no cross-file FK enforcement in SQLite) — the rebuild should decide whether to keep this two-database split (and enforce referential integrity at the application/service layer, as the POC does) or consolidate into one database with real FK constraints; either is valid, but the choice affects the repository/service layer design significantly.

---

## 14. Cross-Cutting Requirements

### 14.1 Localization
Five UI languages throughout: **English, French, Italian, German, Spanish**. Applies to: system-wide default language, per-user language preference (takes precedence when set), per-device language, and full translation of Risk Factors and Predefined Comments (Name/Description/Text in each language). All other UI strings should be resource-based (the POC already centralizes strings in a `.resx` file) to support this.

### 14.2 Formatting & Validation Utilities
- **Names**: auto-capitalize to title case as typed (with known limitations to solve properly in the rebuild: names with apostrophes like O'Brien, mixed-case surnames like McDonald, hyphenated names, and suffixes like Jr./III/Sr. all need correct handling, not just naive title-casing).
- **Phone numbers**: live-format per selected country dial code, with correct max-digit enforcement per country (14 countries in the POC — see §4.2).
- **SSN**: auto-format as `XXX-XX-XXXX` while typing; **must be masked in the UI** (e.g., `•••-••-1234`) since it's direct PII — not implemented in the POC and a hard requirement for the rebuild.
- **QR Codes**: generated per patient, encoding the fields marked "Include in QR" in Field Setup (§10.4). Needs one finalized, documented payload format/contract (the POC has two competing encodings — delimited key:value text vs. JSON — pick one and document it so any consuming scanner/importer has a stable contract).

### 14.3 Confirmation Dialogs
A consistent themed dialog (replacing the OS-native message box) for all confirmations, warnings, and errors — supports OK, OK/Cancel, Yes/No, Yes/No/Cancel button sets with consistent iconography (info/warning/error/question) and accent coloring. This visual system should be one of the first shared UI components built in the rebuild, since virtually every screen depends on it.

### 14.4 Undo / Revert / Save pattern
Every editable screen in the app (Devices, ABR/DPOAE Protocols, Field Setup, Sites/Facilities/Locations, Users/Profiles, all System Configuration sub-screens, Patient editing) follows the same three-tier change-management pattern and should share one implementation in the rebuild rather than being re-implemented per screen (the POC explicitly flags this duplication as technical debt to fix):
- **Save** — commits the current edit session as the new baseline.
- **Revert** — discards all changes back to the last saved baseline.
- **Undo** — rolls back only the single most recent field-level change, independent of the overall dirty/baseline state.

### 14.5 Persistence
Every list/detail/config screen in the POC operates on in-memory collections only — **no screen in the POC actually persists to a database.** This is the single largest gap between the POC and a shippable product: the rebuild's core engineering effort is building the real data-access layer (repositories/services) behind every one of the screens described in this document, plus real duplicate-detection and save logic for Import (§6.2), and real transfer/flash logic for Firmware Update (§9.6).

---

## Appendix A — Explicitly Orphaned/Incomplete POC Screens
These exist in the POC's code but are not reachable from any button today; evaluate whether each represents a real intended feature to finish, or should be dropped:
- **Add New (chooser dialog)** — a 4-option quick-add chooser (Patient/User/Site/Device) with no wiring to any of the four actual Add flows.
- **Assign Screenings dialog** — lets a screener select from unassigned patients (with a high-priority/NICU filter) and assign them; the assignment itself is unimplemented.
- **Screener Details dialog** — a per-screener daily patient/result breakdown, intended to open from the "Screenings Today" list but never wired to it.

## Appendix B — Glossary
- **ABR** — Auditory Brainstem Response (a hearing screening test type/device).
- **TEOAE** — Transient Evoked Otoacoustic Emissions (a hearing screening test type).
- **DPOAE** — Distortion Product Otoacoustic Emissions (a hearing screening test type/device).
- **NICU** — Neonatal Intensive Care Unit.
- **MRN** — Medical Record Number.
- **Pass / Refer / Incomplete** — the three possible screening result values.
