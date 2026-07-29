-- ============================================================
-- AccuSync — Settings Database Schema
-- Engine:    SQLite
--            (SQL Server equivalents: AUTOINCREMENT → IDENTITY,
--             DATETIME → DATETIME2, TEXT → NVARCHAR(MAX))
--
-- Database:  SettingsDatabase.db
-- Purpose:   Configuration, users, organisational structure
--
-- Cross-database foreign keys
--   SQLite does not support foreign keys across database files.
--   Columns in PatientDatabase that reference this database are
--   stored as plain INTEGER and annotated -- [XDB: TableName.Column].
--   Referential integrity is enforced at the application layer.
--
-- Audit logging
--   All audit events are written by Serilog to a rolling file
--   sink, not to any database table.
--   Path:      %ProgramData%\AccuSync\Logs\audit_YYYYMMDD.log
--   Retention: 90 days (configurable via Serilog settings)
--   Format:    one JSON object per line, e.g.
--     {"ts":"...","user":"...","action":"...","entity":"...","id":0,"note":"..."}
-- ============================================================


-- ============================================================
-- SECTION 1: ORGANISATIONAL STRUCTURE
-- ============================================================

-- Represents a top-level organisational unit.
-- Users, devices, and patients are scoped to a site.
CREATE TABLE IF NOT EXISTS Sites (
    SiteId      INTEGER PRIMARY KEY AUTOINCREMENT,
    Name        TEXT    NOT NULL,
    Code        TEXT    NOT NULL,
    Description TEXT,
    IsActive    INTEGER NOT NULL DEFAULT 1,
    CreatedAt   TEXT    NOT NULL DEFAULT (datetime('now')),
    ModifiedAt  TEXT    NOT NULL DEFAULT (datetime('now'))
);

-- A physical or operational unit that belongs to a site.
-- LocationType: 'Home Visit' | 'Inpatient' | 'Outpatient'
CREATE TABLE IF NOT EXISTS Facilities (
    FacilityId   INTEGER PRIMARY KEY AUTOINCREMENT,
    SiteId       INTEGER NOT NULL REFERENCES Sites(SiteId),
    Name         TEXT    NOT NULL,
    Code         TEXT    NOT NULL,
    Description  TEXT,
    LocationType TEXT,
    IsActive     INTEGER NOT NULL DEFAULT 1,
    CreatedAt    TEXT    NOT NULL DEFAULT (datetime('now')),
    ModifiedAt   TEXT    NOT NULL DEFAULT (datetime('now'))
);

-- A named sub-location within a facility (e.g. a screening room).
-- SiteId is denormalised here for efficient site-scoped queries
-- without requiring a join through Facilities.
CREATE TABLE IF NOT EXISTS Locations (
    LocationId  INTEGER PRIMARY KEY AUTOINCREMENT,
    FacilityId  INTEGER NOT NULL REFERENCES Facilities(FacilityId),
    SiteId      INTEGER NOT NULL REFERENCES Sites(SiteId),
    Name        TEXT    NOT NULL,
    Code        TEXT    NOT NULL,
    Description TEXT,
    IsActive    INTEGER NOT NULL DEFAULT 1,
    CreatedAt   TEXT    NOT NULL DEFAULT (datetime('now')),
    ModifiedAt  TEXT    NOT NULL DEFAULT (datetime('now'))
);


-- ============================================================
-- SECTION 2: USERS & PROFILES
-- ============================================================

-- Defines a named permission set that can be assigned to users.
-- Contains 33 granular BIT permission columns organised across
-- six functional areas: AccuScreen, Device, Patients, Sites,
-- SysConfig, and Users.
--
-- Columns marked "(placeholder)" have no corresponding UI in
-- the Profiles screen yet and must be added before they can
-- be set by an administrator.
CREATE TABLE IF NOT EXISTS Profiles (
    ProfileId                       INTEGER PRIMARY KEY AUTOINCREMENT,
    Name                            TEXT    NOT NULL,
    Description                     TEXT,
    -- AccuScreen Management
    AccuScreen_AllTests             INTEGER NOT NULL DEFAULT 0,
    AccuScreen_QuickTest            INTEGER NOT NULL DEFAULT 0,
    AccuScreen_BasicTests           INTEGER NOT NULL DEFAULT 0,
    AccuScreen_DeletePatients       INTEGER NOT NULL DEFAULT 0,
    AccuScreen_EditPatients         INTEGER NOT NULL DEFAULT 0,
    -- Device Management
    Device_AddEdit                  INTEGER NOT NULL DEFAULT 0,
    Device_ConfigureModules         INTEGER NOT NULL DEFAULT 0,
    Device_Delete                   INTEGER NOT NULL DEFAULT 0,
    Device_View                     INTEGER NOT NULL DEFAULT 0,
    -- Patients and Tests
    Patients_AddEdit                INTEGER NOT NULL DEFAULT 0,
    Patients_CommentMaintenance     INTEGER NOT NULL DEFAULT 0,
    Patients_ConfigureManagement    INTEGER NOT NULL DEFAULT 0,
    Patients_Delete                 INTEGER NOT NULL DEFAULT 0,
    Patients_ReassignTests          INTEGER NOT NULL DEFAULT 0,
    Patients_RiskFactorMaintenance  INTEGER NOT NULL DEFAULT 0,
    Patients_View                   INTEGER NOT NULL DEFAULT 0,
    Patients_Export                 INTEGER NOT NULL DEFAULT 0,  -- placeholder: add to Profiles screen UI
    Patients_ViewReports            INTEGER NOT NULL DEFAULT 0,  -- placeholder: add to Profiles screen UI
    -- Sites and Facilities
    Sites_AddEditFacilities         INTEGER NOT NULL DEFAULT 0,
    Sites_AddEditSites              INTEGER NOT NULL DEFAULT 0,
    Sites_ConfigureManagement       INTEGER NOT NULL DEFAULT 0,
    Sites_DeleteFacilities          INTEGER NOT NULL DEFAULT 0,
    Sites_DeleteSites               INTEGER NOT NULL DEFAULT 0,
    Sites_ViewFacilities            INTEGER NOT NULL DEFAULT 0,
    Sites_ViewSites                 INTEGER NOT NULL DEFAULT 0,
    -- System Configuration
    SysConfig_Configure             INTEGER NOT NULL DEFAULT 0,
    SysConfig_View                  INTEGER NOT NULL DEFAULT 0,
    SysConfig_AccessSettings        INTEGER NOT NULL DEFAULT 0,  -- placeholder: add to Profiles screen UI
    -- Users and Profiles
    Users_AddEditProfiles           INTEGER NOT NULL DEFAULT 0,
    Users_AddEditUsers              INTEGER NOT NULL DEFAULT 0,
    Users_ConfigureManagement       INTEGER NOT NULL DEFAULT 0,
    Users_DeleteProfiles            INTEGER NOT NULL DEFAULT 0,
    Users_DeleteUsers               INTEGER NOT NULL DEFAULT 0,
    Users_ResetUsers                INTEGER NOT NULL DEFAULT 0,
    Users_ViewProfiles              INTEGER NOT NULL DEFAULT 0,
    Users_ViewUsers                 INTEGER NOT NULL DEFAULT 0,
    IsActive                        INTEGER NOT NULL DEFAULT 1,
    CreatedAt                       TEXT    NOT NULL DEFAULT (datetime('now')),
    ModifiedAt                      TEXT    NOT NULL DEFAULT (datetime('now'))
);

-- A system user with login credentials and a profile assignment.
-- Language: 'English' | 'French' | 'Italian' | 'German' | 'Spanish'
--
-- Note: Language here is a per-user preference set on the Settings
-- screen. The app-wide default lives in SystemSettings.SystemLanguage.
-- At runtime, Users.Language takes precedence when a user is logged in.
CREATE TABLE IF NOT EXISTS Users (
    UserId      INTEGER PRIMARY KEY AUTOINCREMENT,
    ProfileId   INTEGER NOT NULL REFERENCES Profiles(ProfileId),
    SiteId      INTEGER          REFERENCES Sites(SiteId),
    Username    TEXT    NOT NULL UNIQUE,
    PasswordHash TEXT   NOT NULL,
    FirstName   TEXT,
    LastName    TEXT,
    Language    TEXT    NOT NULL DEFAULT 'English',
    IsActive    INTEGER NOT NULL DEFAULT 1,
    IsLocked    INTEGER NOT NULL DEFAULT 0,
    LockedAt    TEXT,
    LastLoginAt TEXT,
    CreatedAt   TEXT    NOT NULL DEFAULT (datetime('now')),
    ModifiedAt  TEXT    NOT NULL DEFAULT (datetime('now'))
);


-- ============================================================
-- SECTION 3: DEVICES
--
-- A physical screening device registered in the system.
--
-- Identity fields (SourceInstrumentId, InstrumentName,
-- InstrumentSignature, SerialNumber, DeviceType) are populated
-- from the import XML and are read-only in the UI.
--
-- Individual Setting fields (Name, Code, Language) and all
-- Common Configuration fields are user-editable on the
-- Configuration tab of the Device Management screen.
--
-- System Information fields (LastSeenAt, HardwareVersion,
-- FirmwareVersion) are read-only and updated by import or sync.
--
-- Transducer data is NOT stored here — it is denormalised per
-- test record in PatientDatabase.TestRecords.
--
-- FacilityId has been removed. Use DeviceFacilities for the
-- device-to-facility many-to-many relationship.
-- ============================================================

CREATE TABLE IF NOT EXISTS Devices (
    DeviceId                INTEGER PRIMARY KEY AUTOINCREMENT,

    -- Organisational assignment
    SiteId                  INTEGER          REFERENCES Sites(SiteId),

    -- Read-only identity fields populated from import XML
    SourceInstrumentId      TEXT,   -- <Instrument><Id> GUID
    InstrumentName          TEXT,   -- <InstrumentSignature> display text
    InstrumentSignature     TEXT,   -- <InstrumentSignature Signature="..."> GUID
    SerialNumber            TEXT    NOT NULL,
    DeviceType              TEXT,   -- 'ABR' | 'TEOAE' | 'DPOAE' | 'Multi'

    -- Individual Setting (Configuration tab — user-editable)
    Name                    TEXT    NOT NULL,
    Code                    TEXT,
    Language                TEXT    NOT NULL DEFAULT 'English',
                                    -- 'English' | 'French' | 'Italian' | 'German' | 'Spanish'

    -- Common Configuration (Configuration tab — user-editable)
    TestResultTerms         TEXT    NOT NULL DEFAULT 'PassRefer',
                                    -- 'PassRefer' | 'ClearResponse'
    PowerTimeout_min        INTEGER NOT NULL DEFAULT 5,
    DisplayTimeout_min      INTEGER NOT NULL DEFAULT 3,
    CalibrationPause_min    INTEGER NOT NULL DEFAULT 10,
    DataDeletion            TEXT    NOT NULL DEFAULT 'Manual',
                                    -- 'Manual' | 'AfterDownload'
    ABRAutostart            TEXT    NOT NULL DEFAULT 'NoAutostart',
                                    -- 'NoAutostart' | 'OnGreen' | 'OnGreenOrYellow'
    TEOAEProbeFitAssistant  TEXT    NOT NULL DEFAULT 'Enabled',
                                    -- 'Enabled' | 'Disabled'
    PatientIdRule           TEXT    NOT NULL DEFAULT 'None',
                                    -- 'None' | 'NHSP England'
                                    -- Per-device override of the system-wide
                                    -- SystemSettings.PatientIdRule default.

    -- System Information (read-only — updated by import/sync)
    LastSeenAt              TEXT,
    HardwareVersion         TEXT,
    FirmwareVersion         TEXT,

    IsActive                INTEGER NOT NULL DEFAULT 1,
    CreatedAt               TEXT    NOT NULL DEFAULT (datetime('now')),
    ModifiedAt              TEXT    NOT NULL DEFAULT (datetime('now'))
);

-- Resolves the many-to-many relationship between devices and users
-- (User Assignment tab on the Device Management screen).
-- CopyToDevice indicates whether this user should be pushed to
-- the physical device on next sync.
CREATE TABLE IF NOT EXISTS DeviceUsers (
    DeviceUserId INTEGER PRIMARY KEY AUTOINCREMENT,
    DeviceId     INTEGER NOT NULL REFERENCES Devices(DeviceId),
    UserId       INTEGER NOT NULL REFERENCES Users(UserId),
    CopyToDevice INTEGER NOT NULL DEFAULT 0,
    CreatedAt    TEXT    NOT NULL DEFAULT (datetime('now')),
    UNIQUE (DeviceId, UserId)
);

-- Resolves the many-to-many relationship between devices and
-- facilities (Facilities tab on the Device Management screen).
-- Replaces the former single FacilityId FK on Devices.
-- CopyToDevice indicates whether this facility should be pushed
-- to the physical device on next sync.
CREATE TABLE IF NOT EXISTS DeviceFacilities (
    DeviceFacilityId INTEGER PRIMARY KEY AUTOINCREMENT,
    DeviceId         INTEGER NOT NULL REFERENCES Devices(DeviceId),
    FacilityId       INTEGER NOT NULL REFERENCES Facilities(FacilityId),
    CopyToDevice     INTEGER NOT NULL DEFAULT 0,
    CreatedAt        TEXT    NOT NULL DEFAULT (datetime('now')),
    UNIQUE (DeviceId, FacilityId)
);

-- Per-device overrides for field visibility, mandatory state,
-- and display label. Rows only exist when a device's
-- configuration differs from the system-wide FieldSetup defaults.
--
-- If no DeviceFieldSetup row exists for a given device + field
-- combination, the app falls back to the system-wide FieldSetup
-- values.
--
-- CustomLabel: per-device display label override.
--   Resolution order: DeviceFieldSetup.CustomLabel
--                   → FieldSetup.CustomLabel
--                   → FieldSetup.Label (hardcoded default)
--   NULL means "inherit from the next level up".
--
-- IncludeInQR is intentionally omitted — QR configuration
-- is system-wide only (managed in System Configuration).
CREATE TABLE IF NOT EXISTS DeviceFieldSetup (
    DeviceFieldSetupId INTEGER PRIMARY KEY AUTOINCREMENT,
    DeviceId           INTEGER NOT NULL REFERENCES Devices(DeviceId),
    FieldSetupId       INTEGER NOT NULL REFERENCES FieldSetup(FieldSetupId),
    IsVisible          INTEGER NOT NULL DEFAULT 1,
    IsRequired         INTEGER NOT NULL DEFAULT 0,
    CustomLabel        TEXT,    -- NULL = inherit from FieldSetup.CustomLabel or .Label
    CreatedAt          TEXT    NOT NULL DEFAULT (datetime('now')),
    ModifiedAt         TEXT    NOT NULL DEFAULT (datetime('now')),
    UNIQUE (DeviceId, FieldSetupId)
);

-- ============================================================
-- SECTION 4: SYSTEM CONFIGURATION
-- ============================================================

-- -------------------------------------------------------
-- 4a. Risk Factors
-- Reference list displayed on the patient risk factor UI.
-- Code is used for import matching against the
-- PatientRiskFactors JSON array in PatientDatabase.Patients
-- and must not be changed after data has been imported.
-- -------------------------------------------------------
CREATE TABLE IF NOT EXISTS RiskFactors (
    RiskFactorId INTEGER PRIMARY KEY AUTOINCREMENT,
    Code         TEXT    NOT NULL UNIQUE,
    Name         TEXT    NOT NULL,
    Description  TEXT,
    IsActive     INTEGER NOT NULL DEFAULT 1,
    CreatedAt    TEXT    NOT NULL DEFAULT (datetime('now')),
    ModifiedAt   TEXT    NOT NULL DEFAULT (datetime('now'))
);

-- Per-language translations for a risk factor's Name and
-- Description. One row per language per risk factor (max 5).
-- Language: 'English' | 'French' | 'Italian' | 'German' | 'Spanish'
CREATE TABLE IF NOT EXISTS RiskFactorTranslations (
    TranslationId INTEGER PRIMARY KEY AUTOINCREMENT,
    RiskFactorId  INTEGER NOT NULL REFERENCES RiskFactors(RiskFactorId),
    Language      TEXT    NOT NULL,
    Name          TEXT,
    Description   TEXT,
    CreatedAt     TEXT    NOT NULL DEFAULT (datetime('now')),
    ModifiedAt    TEXT    NOT NULL DEFAULT (datetime('now')),
    UNIQUE (RiskFactorId, Language)
);

-- -------------------------------------------------------
-- 4b. Predefined Comments
-- Selectable comment text shown during patient data entry.
-- -------------------------------------------------------
CREATE TABLE IF NOT EXISTS PredefinedComments (
    CommentId   INTEGER PRIMARY KEY AUTOINCREMENT,
    CommentText TEXT    NOT NULL,
    IsActive    INTEGER NOT NULL DEFAULT 1,
    CreatedAt   TEXT    NOT NULL DEFAULT (datetime('now')),
    ModifiedAt  TEXT    NOT NULL DEFAULT (datetime('now'))
);

-- Per-language translation for a predefined comment's text.
-- One row per language per comment (max 5).
-- Language: 'English' | 'French' | 'Italian' | 'German' | 'Spanish'
CREATE TABLE IF NOT EXISTS PredefinedCommentTranslations (
    TranslationId INTEGER PRIMARY KEY AUTOINCREMENT,
    CommentId     INTEGER NOT NULL REFERENCES PredefinedComments(CommentId),
    Language      TEXT    NOT NULL,
    CommentText   TEXT,
    CreatedAt     TEXT    NOT NULL DEFAULT (datetime('now')),
    ModifiedAt    TEXT    NOT NULL DEFAULT (datetime('now')),
    UNIQUE (CommentId, Language)
);

-- -------------------------------------------------------
-- 4c. Field Setup
-- Controls the label, visibility, mandatory state, QR
-- inclusion, and optional custom display label for each
-- configurable patient field.
--
-- FieldKey matches the XML element name, e.g. 'FreeField1'.
--
-- Label is the hardcoded default display name and must never
-- be changed by application code. CustomLabel is the user-
-- editable override set on the System Configuration Field
-- Setup screen.
--
-- Display label resolution (system-wide context):
--   COALESCE(CustomLabel, Label)
--
-- Display label resolution (per-device context):
--   COALESCE(DeviceFieldSetup.CustomLabel,
--            FieldSetup.CustomLabel,
--            FieldSetup.Label)
-- -------------------------------------------------------
CREATE TABLE IF NOT EXISTS FieldSetup (
    FieldSetupId INTEGER PRIMARY KEY AUTOINCREMENT,
    FieldKey     TEXT    NOT NULL UNIQUE,
    Label        TEXT    NOT NULL,
    CustomLabel  TEXT,    -- NULL = use Label (the hardcoded default)
    IsVisible    INTEGER NOT NULL DEFAULT 1,
    IsRequired   INTEGER NOT NULL DEFAULT 0,
    IncludeInQR  INTEGER NOT NULL DEFAULT 0,
    CreatedAt    TEXT    NOT NULL DEFAULT (datetime('now')),
    ModifiedAt   TEXT    NOT NULL DEFAULT (datetime('now'))
);

-- -------------------------------------------------------
-- 4d. Import Configurations
-- Each row is a named import profile selectable in the
-- Import Configuration screen (list + detail pattern).
-- ImportFormat: 'AccuSync XML' | 'AccuSync JSON' |
--               'ALGO 5 XML'   | 'ALGO Pro JSON' | 'AccuLink XML'
-- ProfileId references the Profiles row whose permissions
-- apply when this import configuration is used.
-- PasswordHash is stored using the same hashing scheme as
-- Users.PasswordHash (not plain-text or reversible).
-- -------------------------------------------------------
CREATE TABLE IF NOT EXISTS ImportConfiguration (
    ImportConfigId   INTEGER PRIMARY KEY AUTOINCREMENT,
    Name             TEXT    NOT NULL,
    Description      TEXT,
    ImportFormat     TEXT    NOT NULL,
    ProfileId        INTEGER          REFERENCES Profiles(ProfileId),
    PasswordHash     TEXT,
    ImportFolderPath TEXT,
    IsActive         INTEGER NOT NULL DEFAULT 1,
    CreatedAt        TEXT    NOT NULL DEFAULT (datetime('now')),
    ModifiedAt       TEXT    NOT NULL DEFAULT (datetime('now'))
);

-- -------------------------------------------------------
-- 4e. Export Configurations
-- Each row is a named export profile selectable in the
-- Export Configuration screen (list + detail pattern).
-- ExportFormat: 'AccuSync JSON' | 'AccuSync XML' |
--               'HiTrack' | 'OZ' | 'CSV' | 'ALGO 5 XML'
-- ExportData:   'AllPatients' | 'ChangedPatients' |
--               'NewTestsOnly' | 'SelectedPatients'
-- -------------------------------------------------------
CREATE TABLE IF NOT EXISTS ExportConfiguration (
    ExportConfigId   INTEGER PRIMARY KEY AUTOINCREMENT,
    Name             TEXT    NOT NULL,
    Description      TEXT,
    ExportFormat     TEXT    NOT NULL,
    ExportData       TEXT    NOT NULL,
    ExportFolderPath TEXT,
    IsActive         INTEGER NOT NULL DEFAULT 1,
    CreatedAt        TEXT    NOT NULL DEFAULT (datetime('now')),
    ModifiedAt       TEXT    NOT NULL DEFAULT (datetime('now'))
);


-- ============================================================
-- SECTION 5: TEST PROTOCOLS
-- Defines the test parameters assigned to a device.
-- Protocols are created and managed in the Device Management
-- ABR/DPOAE Configuration overlay screens.
-- ============================================================

-- Defines an ABR test protocol.
-- Category:            'Basic' | 'Enhanced'
-- StimulusLevel_dB:    selected from 30 / 35 / 40 / 45 / 50 dB nHL
-- NotchFilter_Hz:      50 | 60
-- StimulusDuringPause: 1 = Yes, 0 = No
CREATE TABLE IF NOT EXISTS ABRProtocols (
    ABRProtocolId        INTEGER PRIMARY KEY AUTOINCREMENT,
    Name                 TEXT    NOT NULL,
    Category             TEXT    NOT NULL DEFAULT 'Basic',
    Description          TEXT,
    StimulusLevel_dB     REAL    NOT NULL,
    NotchFilter_Hz       INTEGER NOT NULL DEFAULT 50,
    StimulusDuringPause  INTEGER NOT NULL DEFAULT 0,
    IsActive             INTEGER NOT NULL DEFAULT 1,
    CreatedAt            TEXT    NOT NULL DEFAULT (datetime('now')),
    ModifiedAt           TEXT    NOT NULL DEFAULT (datetime('now'))
);

-- Defines a DPOAE test protocol.
-- Category:          'Basic' | 'Enhanced'
-- L2_dB:             50 | 55 | 60 | 65 dB SPL
-- L1:                'Auto' | 'L2+10dB' | 'L2+5dB' (relative formula, not a fixed value)
-- F2FrequenciesJson: JSON integer array of selected F2 frequencies, e.g. [1500,2000,3000,4000,5000,6000]
-- PassSNR_dB:        minimum SNR required to pass a frequency point — 6 | 9 | 12 dB
-- MinPassPoints:     number of frequency points that must pass — 1 through 6
-- MinLevel:          'Off' | '0dB' | '-5dB' | '-10dB' | '-15dB'
CREATE TABLE IF NOT EXISTS DPOAEProtocols (
    DPOAEProtocolId   INTEGER PRIMARY KEY AUTOINCREMENT,
    Name              TEXT    NOT NULL,
    Category          TEXT    NOT NULL DEFAULT 'Basic',
    Description       TEXT,
    L2_dB             REAL    NOT NULL DEFAULT 55,
    L1                TEXT    NOT NULL DEFAULT 'Auto',
    F2FrequenciesJson TEXT,
    RetestFrequencies INTEGER NOT NULL DEFAULT 0,
    PassSNR_dB        REAL    NOT NULL DEFAULT 6,
    MinPassPoints     INTEGER NOT NULL DEFAULT 3,
    AutoStop          INTEGER NOT NULL DEFAULT 1,
    MinLevel          TEXT    NOT NULL DEFAULT 'Off',
    IsActive          INTEGER NOT NULL DEFAULT 1,
    CreatedAt         TEXT    NOT NULL DEFAULT (datetime('now')),
    ModifiedAt        TEXT    NOT NULL DEFAULT (datetime('now'))
);

-- Links a device to one or more assigned test protocols.
-- A device may have multiple protocol rows, one per TestType.
-- IsDefault identifies the protocol selected by default for
-- that device/TestType combination.
-- TestType: 'ABR' | 'TEOAE' | 'DPOAE'
-- Only one of ABRProtocolId or DPOAEProtocolId will be set
-- per row, depending on TestType. TEOAE has no protocol table.
CREATE TABLE IF NOT EXISTS DeviceProtocols (
    DeviceProtocolId INTEGER PRIMARY KEY AUTOINCREMENT,
    DeviceId         INTEGER NOT NULL REFERENCES Devices(DeviceId),
    TestType         TEXT    NOT NULL,
    ABRProtocolId    INTEGER          REFERENCES ABRProtocols(ABRProtocolId),
    DPOAEProtocolId  INTEGER          REFERENCES DPOAEProtocols(DPOAEProtocolId),
    IsDefault        INTEGER NOT NULL DEFAULT 0,
    CreatedAt        TEXT    NOT NULL DEFAULT (datetime('now'))
);


-- ============================================================
-- SECTION 6: SYSTEM SETTINGS
-- Generic key-value store for application-wide configuration.
-- All known keys are seeded below with their default values.
-- The DAL should read settings by key name, not by row index.
-- DataType: 'String' | 'Bool' | 'Int' | 'Path'
-- ============================================================

CREATE TABLE IF NOT EXISTS SystemSettings (
    SettingId    INTEGER PRIMARY KEY AUTOINCREMENT,
    SettingKey   TEXT    NOT NULL UNIQUE,
    SettingValue TEXT,
    DataType     TEXT,
    Description  TEXT,
    CreatedAt    TEXT    NOT NULL DEFAULT (datetime('now')),
    ModifiedAt   TEXT    NOT NULL DEFAULT (datetime('now'))
);

-- Seed: default values for all known settings keys.
-- INSERT OR IGNORE ensures re-running initialisation does not
-- overwrite values that have already been changed by the user.
INSERT OR IGNORE INTO SystemSettings (SettingKey, SettingValue, DataType, Description) VALUES
    ('SystemLanguage',          'English',  'String', 'Application UI language — used as fallback when no per-user language is set'),
    ('ConfirmSave',             'Yes',      'Bool',   'Show confirmation dialog when saving a record'),
    ('ConfirmDelete',           'Yes',      'Bool',   'Show confirmation dialog when deleting a record'),
    ('DataModificationWarning', 'Yes',      'Bool',   'Warn the user when modifying existing data'),
    ('HospitalLogoPath',        NULL,       'Path',   'File path to the hospital logo image used in reports'),
    ('PaperFormat',             'A4',       'String', 'Report paper format — A4 | Letter'),
    ('PatientIdRule',           'None',     'String', 'Patient ID validation rule — None | NHSP England'),
    ('LockingTime_min',         '15',       'Int',    'Inactivity period in minutes before AccuSync locks the session'),
    ('PasswordSecurityRule',    'None',     'String', 'Password complexity rule — None | Simple | Complex'),
    ('TransferSitesToDevice',   'True',     'Bool',   'Transfer sites and facilities to the device on sync');


-- ============================================================
-- INDEXES
-- ============================================================

CREATE INDEX IF NOT EXISTS IX_Facilities_SiteId          ON Facilities(SiteId);
CREATE INDEX IF NOT EXISTS IX_Locations_FacilityId       ON Locations(FacilityId);
CREATE INDEX IF NOT EXISTS IX_Locations_SiteId           ON Locations(SiteId);
CREATE INDEX IF NOT EXISTS IX_Users_ProfileId            ON Users(ProfileId);
CREATE INDEX IF NOT EXISTS IX_Users_SiteId               ON Users(SiteId);
CREATE INDEX IF NOT EXISTS IX_Devices_SiteId             ON Devices(SiteId);
CREATE INDEX IF NOT EXISTS IX_Devices_Serial             ON Devices(SerialNumber);
CREATE INDEX IF NOT EXISTS IX_DeviceUsers_DeviceId       ON DeviceUsers(DeviceId);
CREATE INDEX IF NOT EXISTS IX_DeviceUsers_UserId         ON DeviceUsers(UserId);
CREATE INDEX IF NOT EXISTS IX_DeviceFacilities_DeviceId  ON DeviceFacilities(DeviceId);
CREATE INDEX IF NOT EXISTS IX_DeviceFacilities_FacId     ON DeviceFacilities(FacilityId);
CREATE INDEX IF NOT EXISTS IX_DeviceProtocols_DeviceId   ON DeviceProtocols(DeviceId);
CREATE INDEX IF NOT EXISTS IX_DeviceFieldSetup_DeviceId  ON DeviceFieldSetup(DeviceId);
CREATE INDEX IF NOT EXISTS IX_DeviceFieldSetup_FieldId   ON DeviceFieldSetup(FieldSetupId);
CREATE INDEX IF NOT EXISTS IX_PredefinedComments_Active  ON PredefinedComments(IsActive);
CREATE INDEX IF NOT EXISTS IX_PredefinedCommentTrans_CId ON PredefinedCommentTranslations(CommentId);
CREATE INDEX IF NOT EXISTS IX_RiskFactors_Code           ON RiskFactors(Code);
CREATE INDEX IF NOT EXISTS IX_RiskFactorTrans_RFId       ON RiskFactorTranslations(RiskFactorId);