# AccuSync — Requirements & Codebase Analysis (Reference)

**This is the detailed background reference.** The plan itself is in
[AccuSync-Delivery-Roadmap.md](AccuSync-Delivery-Roadmap.md).

Use this document when you need the detail behind a number in the plan:

| Part | What it gives you |
|---|---|
| Part 1 | All 196 requirements from the SRS, written out in full, plus 23 conflicts and document defects |
| Part 2 | What the code actually is today, with file paths for every claim |
| Part 3 | Requirement-by-requirement status: done / partly done / not started / conflicts / unclear |

Source: `DOC-076814 Rev 01 AccuSync Software Requirements.docx`
Code assessed at commit `700f380`, branch `users/chokkalingam/feat/AkkuSync-US5-Logout`

---

# PART 1 — ALL 196 REQUIREMENTS

**Source for every row:** `DOC-076814 Rev 01 AccuSync Software Requirements.docx`, Rev 01, Jama baseline B1 (01/23/2026), DCO#73885 Initial Release.

**Priority is "not stated" for all 196 requirements.** The document contains no priority or MoSCoW column. The only phasing signal anywhere is inside GID-254968 (language Phase 1/2/3). No priorities have been invented.

**Type key:** F = functional · NF = non-functional · I = integration · INF = infrastructure · C = compliance/regulatory
**Cat/HZ** = the document's own Category and Hazard ID columns, reproduced.

## §5.1 General (10)

| # | GID | Title | Requirement | Type | Cat/HZ |
|---|---|---|---|---|---|
| 1 | GID-254873 | Data Management Options | Shall provide data management options Add, Edit, Delete for creating, modifying, removing database records | F | S,U / None |
| 2 | GID-254874 | Add Function | Shall allow user to create a new record by selecting Add, which shall open a blank form for data entry | F | S,U / None |
| 3 | GID-254875 | Edit Function | Shall allow user to modify a selected record by selecting Edit, which shall open the record in an editable form | F | S,U / None |
| 4 | GID-254876 | Delete Function | Shall allow user to delete a selected record by selecting Delete, and shall require user confirmation before removal from the database | F | S,U / None |
| 5 | GID-254877 | Data Mgmt Screen Availability | Shall provide Add/Edit/Delete on: Patient, User, Profile, Device, Site, Facility, Location, ABR Protocols, DPOAE Protocols, Risk Factors, Comments | F | S,U / None |
| 6 | GID-254878 | Unsaved Data Mgmt Options | Shall provide unsaved data management options Save, Revert, Undo for managing uncommitted changes | F | S,U / None |
| 7 | GID-254879 | Save Function | Shall allow user to save all modified data by selecting Save, which shall persist changes to the database | F | S,U / None |
| 8 | GID-254880 | Revert Function | Shall allow user to discard all unsaved changes by selecting Revert, restoring the data to its last saved state | F | S,U / None |
| 9 | GID-254881 | Undo Function | Shall allow user to reverse the most recent unsaved change by selecting Undo | F | S,U / None |
| 10 | GID-254882 | Unsaved Data Screen Availability | Shall provide Save/Revert/Undo on: Patient, User, Profile, Device, Site, Facility, Location, ABR Protocols, DPOAE Protocols, Risk Factors, Comments, Patient Field Configuration | F | S,U / None |

## §5.2 Login (8)

| # | GID | Title | Requirement | Type | Cat/HZ |
|---|---|---|---|---|---|
| 11 | GID-255015 | User Login | Shall require users to enter username and password credentials to login to the system | F | S,U / None |
| 12 | GID-255016 | User Logout | Shall provide an option to logout of the system | F | S,U / None |
| 13 | GID-255017 | User Account Lockout | Shall lock out the user from logging into the system after a minimum of 5 failed sequential login attempts with improper credentials | F | S,U / None |
| 14 | GID-255018 | Credential Encryption | Shall securely hash all login passwords and encrypt all usernames stored in the system | NF, C | **S,U\* / HZ TBD** |
| 15 | GID-255019 | Role-Based Access Control | Shall support role-based access control with minimum two roles: Admin and Screener | F | S,U / None |
| 16 | GID-255020 | Password Complexity Requirements | Shall enforce per user account: ≥8 characters • ≥1 upper case letter (A–Z) • ≥1 lower case letter (a–z) • ≥1 numerical digit • the password cannot be the same as the three previously used passwords | F, NF | S,U / None |
| 17 | GID-255021 | Deactivated User Login Prevention | Shall prevent deactivated users from logging in | F | S,U / None |
| 18 | GID-255022 | Deactivated User Transfer Prevention | Shall prevent transfer of deactivated user accounts to the device | F, I | S,U / None |

## §5.3 Patient Management (15)

| # | GID | Title | Requirement | Type | Cat/HZ |
|---|---|---|---|---|---|
| 19 | GID-254883 | Create Patient Record | Shall allow users to create a patient record containing patient demographics, caregiver data, medical data, and consent information | F | S,U / None |
| 20 | GID-254884 | View Patient List | Shall allow users to view a list of patient records saved in the system | F | S,U / None |
| 21 | GID-254885 | Edit Patient Record | Shall allow users to select a patient from the list view and edit the patient record, unless locked by permissions | F | S,U / None |
| 22 | GID-254886 | Delete Patient Record | Shall allow administrative users to delete a patient record | F | S,U / None |
| 23 | GID-254887 | Patient Search | Shall provide search functionality on the patient list view, allowing users to search by patient ID, first name, last name, date of birth, or date of test range, and display the filtered results | F | S,U / None |
| 24 | GID-254888 | Patient Risk Factor Selection | Shall provide an option to set risk factors in the patient details view with the values: Yes • No • Unknown | F | S,U / None |
| 25 | GID-254889 | Patient Comment Assignment | Shall provide an option to assign predefined or patient-specific comments to a patient record | F | S,U / None |
| 26 | GID-254890 | Patient List Display Fields | Shall display for each patient: Patient ID / Hospital ID • Last Name • First Name • Date of Birth • Risk • Comment | F | S,U / None |
| 27 | GID-254891 | View Patient Information | Shall allow users to select a patient from the list view and view patient information | F | S,U / None |
| 28 | GID-254892 | Mandatory Field Indication | Shall indicate which fields are mandatory during patient data entry | F | S,U / None |
| 29 | GID-254893 | Patient Save Validation | Shall allow the user to save patient information when all mandatory fields are populated | F | S,U / None |
| 30 | GID-254894 | Patient Test Report Generation | Shall generate and print predefined patient test reports, including selected tests or all tests | F | S,U / None |
| 31 | GID-254895 | Delete Patient Test Entry | Shall allow administrative users to delete individual test entries from a patient's test result list | F | S,U / None |
| 32 | GID-254896 | Test Result Reassignment | Shall allow administrative users to reassign test results to the correct patient in the system | F | S,U / None |
| 33 | GID-254897 | Patient Test List Display Fields | Shall display for each test of a selected patient: Test Type • Left Ear Result • Right Ear Result • Date/Time of Test • Test Configuration • Duration • Examiner | F | S,U / None |

## §5.4 Import (1)

| # | GID | Title | Requirement | Type | Cat/HZ |
|---|---|---|---|---|---|
| 34 | GID-254900 | Supported Import Formats | Shall support the following formats to import: AccuSync XML • AccuSync JSON • ALGO 5 XML • ALGO Pro JSON • AccuLink XML • **[Additional formats as required]** | I | S,U / None |

## §5.5 Export (16)

| # | GID | Title | Requirement | Type | Cat/HZ |
|---|---|---|---|---|---|
| 35 | GID-254898 | Supported Export Formats | Shall support the following export formats: AccuSync JSON • AccuSync XML • HiTrack • OZ • CSV • ALGO 5 XML • **[Additional formats as required]** | I | S,U / None |
| 36 | GID-254899 | Patient Data Export | Shall export patient and test data to supported external systems via file or web service | I | S,U / None |
| 37 | GID-256274 | Configurable Export Location | Shall allow users to specify the folder location for data export | F | S,U / None |
| 38 | GID-256275 | Export New Patients with Data | Shall allow export of new patients with data | F | S,U / None |
| 39 | GID-256276 | Export All Patients | Shall allow export of all patients | F | S,U / None |
| 40 | GID-256277 | Export Selected Entries | **"The device shall"** allow export of individually selected entries | F | S,U / None |
| 41 | GID-256278 | Export by Test Date Range | Shall allow export of entries within a specified test date range | F | S,U / None |
| 42 | GID-256279 | De-identified Data Export | Shall provide an export option to remove patient demographics and identifiers from the test data prior to sharing the data or for service review | F, C | S,U / None |
| 43 | GID-256280 | AccuSync XML Storage & Naming | Shall store AccuSync XML files in `ExportData/XML/`, named `AccuSync_YYYY_MM_DD_HH_MM_SS.xml`; date/time indicates when the file was created | F | S,U / None |
| 44 | GID-256281 | AccuSync JSON Storage & Naming | Shall store AccuSync JSON files in `ExportData/JSON/`, named `AccuSync_YYYY_MM_DD_HH_MM_SS.json` | F | S,U / None |
| 45 | GID-256282 | HiTrack Storage & Naming | Shall store HiTrack files for the state in `ExportData/SummaryFiles/YYYY/YYYY-MON/`, named `state_yyyymmdd_hhmmss.txt`. A duplicate named `INTHS.txt` shall be stored here as well as in `HiTrackExportData`. MON = three-letter month abbreviation | F | S,U / None |
| 46 | GID-256283 | OZ Storage & Naming | Shall store OZ **fixed width** files for the state in `ExportData/OZ/`, named `state_yyyymmdd_hhmmss.txt` | F | S,U / None |
| 47 | GID-256284 | CSV Storage & Naming | Shall store CSV **fixed width** files in `ExportData/CSV/`, named `CSV_yyyymmdd_hhmmss.csv` | F | S,U / None |
| 48 | GID-256285 | ALGO 5 XML Storage & Naming | Shall store ALGO 5 XML files in `ExportData/XML/`, named `ALGO5_YYYY_MM_DD_HH_MM_SS.xml` | F | S,U / None |
| 49 | GID-256286 | Configurable Export Format | Shall allow users to select a default export format | F | S,U / None |
| 50 | GID-256512 | Default Export Format | The default export format shall be AccuSync JSON | F | S,U / None |

## §5.6 OAE Test Result (2) · §5.7 ABR Test Result (1)

| # | GID | Title | Requirement | Type | Cat/HZ |
|---|---|---|---|---|---|
| 51 | GID-254901 | TEOAE Test Result | Shall allow users to view patient test result details, waveform data, test comments, and device information for the TEOAE test | F | S,U / None |
| 52 | GID-254902 | DPOAE Test Result | Shall allow users to view patient test result details, waveform data, test comments, and device information for the DPOAE test | F | S,U / None |
| 53 | GID-254903 | ABR Test Result | Shall allow users to view patient test result details, waveform data, test comments, and device information for the ABR test | F | S,U / None |

## §5.8 User Account Management (9)

| # | GID | Title | Requirement | Type | Cat/HZ |
|---|---|---|---|---|---|
| 54 | GID-254904 | View User List | Shall allow administrative users to view a list of user accounts | F | S,U / None |
| 55 | GID-254905 | Create User Account | Shall allow administrative users to create a user account | F | S,U / None |
| 56 | GID-254906 | Edit User Account | Shall allow administrative users to edit a user account | F | S,U / None |
| 57 | GID-254907 | Unlock User Account | Shall allow administrative users to unlock a user account | F | S,U / None |
| 58 | GID-254908 | Delete User Account | Shall allow administrative users to delete a user account | F | S,U / None |
| 59 | GID-254909 | User Account Activation | Shall allow administrative users to activate/deactivate a user account | F | S,U / None |
| 60 | GID-254910 | User Language Selection | Shall allow administrative users to select the display language per user | F | S,U / None |
| 61 | GID-254911 | Account Lockout Duration Config | Shall allow administrative users to configure the lockout duration **after 5 consecutive lockouts** | F | S,U / None |
| 62 | GID-254912 | Password Complexity Config | Shall allow administrative users to configure the password complexity (**None, Simple, Complex**) | F | S,U / None |

## §5.9 Profile Management (6)

| # | GID | Title | Requirement | Type | Cat/HZ |
|---|---|---|---|---|---|
| 63 | GID-254913 | View Profile List | Shall allow administrative users to view a list of profiles and feature access rights | F | S,U / None |
| 64 | GID-254914 | Create Profile | Shall allow administrative users to create a profile | F | S,U / None |
| 65 | GID-254915 | Edit Profile | Shall allow administrative users to edit a profile | F | S,U / None |
| 66 | GID-254916 | Delete Profile | Shall allow administrative users to delete a profile | F | S,U / None |
| 67 | GID-254917 | Profile Display Fields | Shall display for a selected profile: Name • Description • Components and Permissions | F | S,U / None |
| 68 | GID-254918 | Profile Permission Configuration | Shall allow administrative users to configure permissions for each profile | F | S,U / None |

## §5.10 Device Management (8)

| # | GID | Title | Requirement | Type | Cat/HZ |
|---|---|---|---|---|---|
| 69 | GID-254919 | Device List Display Fields | Shall allow administrative users to view a list of devices with: Name • Serial number • Last Seen | F | S,U / None |
| 70 | GID-254920 | Add Device | Shall allow administrative users to add a new device | F | S,U / None |
| 71 | GID-254921 | Edit Device | Shall allow administrative users to edit device information | F | S,U / None |
| 72 | GID-254922 | Delete Device | Shall allow administrative users to delete a device | F | S,U / None |
| 73 | GID-254923 | Device User Assignment | Shall allow administrative users to assign users to each device | F | S,U / None |
| 74 | GID-254924 | Device Facility Assignment | Shall allow administrative users to assign facilities to each device | F | S,U / None |
| 75 | GID-254925 | Device Settings Configuration | Shall allow administrative users to configure: Display timeout • Power timeout • Calibration/pause time • Result terminology • Automatic deletion • ABR Autostart • TEOAE Probe Fit Assistant | F, C | **S,U\* / HZ 6.4** |
| 76 | GID-254926 | Device System Information Display | Shall display for a selected device: Last Seen • Last Updated • Hardware Version • Firmware Version | F | S,U / None |

## §5.11 Site (5) · §5.12 Facility (5) · §5.13 Location (6)

| # | GID | Title | Requirement | Type | Cat/HZ |
|---|---|---|---|---|---|
| 77 | GID-254927 | View Site List | Shall allow administrative users to view a list of sites and associated details | F | S,U / None |
| 78 | GID-254928 | Site Detail Fields | Shall allow administrative users to enter site details: Name • Description • Code | F | S,U / None |
| 79 | GID-254929 | Add Site | Shall allow administrative users to add a new site | F | S,U / None |
| 80 | GID-254930 | Edit Site | Shall allow administrative users to edit site details | F | S,U / None |
| 81 | GID-254931 | Delete Site | Shall allow administrative users to delete a site | F | S,U / None |
| 82 | GID-254932 | View Facility List | Shall allow administrative users to view a list of facilities and associated details | F | S,U / None |
| 83 | GID-254933 | Facility Detail Fields | Shall allow administrative users to enter facility details: Name • Description • Code • Site • Location Type | F | S,U / None |
| 84 | GID-254934 | Add Facility | Shall allow administrative users to add a facility | F | S,U / None |
| 85 | GID-254935 | Edit Facility | Shall allow administrative users to edit a facility | F | S,U / None |
| 86 | GID-254936 | Delete Facility | Shall allow administrative users to delete a facility | F | S,U / None |
| 87 | GID-254937 | View Location List | Shall allow administrative users view a list of locations and associated details | F | S,U / None |
| 88 | GID-254938 | Location Detail Fields | Shall allow administrative users to enter location details: Name • Description • Code | F | S,U / None |
| 89 | GID-254939 | Add Location | Shall allow administrative users to add a new location | F | S,U / None |
| 90 | GID-254940 | Edit Location | Shall allow administrative users to edit a location | F | S,U / None |
| 91 | GID-254941 | Delete Location | Shall allow administrative users to delete a location | F | S,U / None |
| 92 | GID-255465 | Assign Location | When adding a new location, shall provide an option to assign the location to **all** facilities | F | S,U / None |

## §5.14 ABR Test Protocol Configuration (4) · §5.15 DPOAE Test Protocol Configuration (4)

| # | GID | Title | Requirement | Type | Cat/HZ |
|---|---|---|---|---|---|
| 93 | GID-254942 | Create ABR Protocol | Shall allow administrative users create ABR test protocol configurations | F | S,U / None |
| 94 | GID-254943 | View ABR Protocol List | Shall allow administrative users view a list of ABR test protocol configurations | F | S,U / None |
| 95 | GID-254944 | Edit ABR Protocol | Shall allow administrative users to edit an existing ABR test protocol configuration | F | S,U / None |
| 96 | GID-254945 | Delete ABR Protocol | Shall allow administrative users to delete an existing ABR test protocol configuration | F | S,U / None |
| 97 | GID-254946 | Create DPOAE Protocol | Shall allow administrative users create DPOAE test protocol configurations | F | S,U / None |
| 98 | GID-254947 | View DPOAE Protocol List | Shall allow administrative users view a list of DPOAE test protocol configurations | F | S,U / None |
| 99 | GID-254948 | Edit DPOAE Protocol | Shall allow administrative users to edit an existing DPOAE test protocol configuration | F | S,U / None |
| 100 | GID-254949 | Delete DPOAE Protocol | Shall allow administrative users to delete an existing DPOAE test protocol configuration | F | S,U / None |

## §5.16 Risk Factor Configuration (5) · §5.17 Comments Configuration (5)

| # | GID | Title | Requirement | Type | Cat/HZ |
|---|---|---|---|---|---|
| 101 | GID-254950 | Create Risk Factor | Shall allow administrative users to create predefined risk factors that can be assigned to patient records | F | S,U / None |
| 102 | GID-254951 | View Risk Factor List | Shall allow administrative users to view a list of predefined risk factors that can be assigned to patient records | F | S,U / None |
| 103 | GID-254952 | Edit Risk Factor | Shall allow administrative users to edit predefined risk factors **that has not been assigned** to a patient record | F | S,U / None |
| 104 | GID-254953 | Delete Risk Factor | Shall allow administrative users to delete a list of predefined risk factors **that is not assigned** to a patient record | F | S,U / None |
| 105 | GID-254954 | Risk Factor Translation | Shall allow administrative users to enter translated text for risk factor names and descriptions in each supported language | F | S,U / None |
| 106 | GID-254955 | Create Comment | Shall allow administrative users to create a comment that can be assigned to a patient record | F | S,U / None |
| 107 | GID-254956 | View Comment List | Shall allow administrative users to view a list of predefined comments that can be assigned to a patient record | F | S,U / None |
| 108 | GID-254957 | Edit Comment | Shall allow administrative users to edit a predefined comment that can be assigned to a patient record | F | S,U / None |
| 109 | GID-254958 | Delete Comment | Shall allow administrative users to delete a predefined comment **that is not assigned** to a patient record | F | S,U / None |
| 110 | GID-254959 | Comment Translation | Shall allow administrative users to enter translated text for comments in each supported language | F | S,U / None |

## §5.18 Patient Field Configuration (8)

| # | GID | Title | Requirement | Type | Cat/HZ |
|---|---|---|---|---|---|
| 111 | GID-254960 | Patient ID Format Validation Config | Shall allow administrative users to configure patient ID rule format validation | F | S,U / None |
| 112 | GID-254961 | Patient Mandatory Field Config | Shall allow administrative users to configure which patient record fields are mandatory during data entry | F | S,U / None |
| 113 | GID-254962 | Patient Active Field Config | Shall allow administrative users to configure which patient record fields are active during data entry | F | S,U / None |
| 114 | GID-254963 | Patient List Sort Config | Shall allow administrative users to define how patient lists are sorted | F | S,U / None |
| 115 | GID-254964 | Data Confirmation | Shall allow configuration of the following user notifications: Confirmation of saving • Confirmation of deletion • Data modification warning | F | S,U / None |
| 116 | GID-254965 | Report Configuration | Shall provide an option to add logo to the layout of the report by using a graphics file and to select paper format | F | S,U / None |
| 117 | GID-254966 | Data Validation | Shall validate user inputs and prevent saving of invalid or incomplete data according to configured validation rules | F | S,U / None |
| 118 | GID-255466 | Custom Field Configuration | Shall provide customizable fields labeled "Available Field" that allow users to rename the fields | F | S,U / None |

## §5.19 Report Generation (2)

| # | GID | Title | Requirement | Type | Cat/HZ |
|---|---|---|---|---|---|
| 119 | GID-255112 | Patient Report Generation | Shall allow users to generate patient reports that include demographic information and test result details | F | S,U / None |
| 120 | GID-255113 | Export Reports | Shall allow exporting printed reports to supported file formats | F | S,U / None |

## §5.20 Language (5)

| # | GID | Title | Requirement | Type | Cat/HZ |
|---|---|---|---|---|---|
| 121 | GID-254967 | Default Language Selection | Shall allow the user to select the default language from a list of supported languages | F | S,U / None |
| 122 | GID-254968 | Supported Languages | **Phase 1:** English, French, Italian, German, Spanish · **Phase 2:** Brazilian Portuguese, Chinese (simplified), Chinese (traditional), Japanese · **Phase 3:** Norwegian, Danish, Finnish, Swedish, Turkish · **[Additional languages as required]** | F | S,U / None |
| 123 | GID-256513 | Default Language | The default language setting shall be English | F | S,U / None |
| 124 | GID-254969 | Localized Display | Shall display all on-screen text, messages, menus, prompts, and results in the configured language | F, NF | S,U / None |
| 125 | GID-254970 | Language Change Confirmation | Shall require user confirmation before applying the selected language as the default language for the application | F | S,U / None |

## §5.21 S4H (25)

| # | GID | Title | Requirement | Type | Cat/HZ |
|---|---|---|---|---|---|
| 126 | GID-254971 | UK Terminology Standards | **"The device shall"** use English (UK) terminology as defined by NHSP, including: Pass = Clear Response (CR), Refer = No Clear Response (NCR), Surname = Last Name, Forename = First Name | F, C | S,U / None |
| 127 | GID-254972 | Synchronize Identifiers & Facility Lists | Shall synchronize with the device: Site Identifier • Device Identifier • List of inpatient facilities • List of outpatient facilities | I | S,U / None |
| 128 | GID-254973 | Site Identifier Configuration | Shall provide an option to configure the Site identifier | F | S,U / None |
| 129 | GID-254974 | SEDQ Web Service URL | Shall provide an option to configure the SEDQ Web Service URL | I, INF | S,U / None |
| 130 | GID-254975 | User List Synchronization | Shall receive the user list from the SEDQ web service | I | S,U / None |
| 131 | GID-254976 | Test Results Transfer | Shall support transfer of screening test results, including binary waveform data, into S4H | I | S,U / None |
| 132 | GID-254977 | Restrict User Configration *(sic)* | Shall **prevent administrative users from adding a user account** | F | S,U / None |
| 133 | GID-254978 | Default Profile Assignment | Shall provide an option to select a default user profile to assign to all new users imported from S4H | F | S,U / None |
| 134 | GID-254979 | Default Password Assignment | Shall provide an option to set the default password to apply to all new users downloaded from S4H | F, NF | S,U / None |
| 135 | GID-254980 | Automatic User Deactivation | Shall automatically set the account status to "Inactive" for any active user accounts whose identifiers are not present in the received synchronisation data | F, I | S,U / None |
| 136 | GID-254981 | Automatic User Reactivation | Shall automatically restore the account status to "Active" for any inactive user accounts whose identifiers are present in the received synchronisation data | F, I | S,U / None |
| 137 | GID-254982 | User Attribute Synchronisation | Shall retrieve and store Username and User ID from the synchronisation data | I | S,U / None |
| 138 | GID-254983 | Read-Only User Attributes | Shall prevent modification of Username and User ID except through synchronisation updates | F | S,U / None |
| 139 | GID-254984 | *Titled* "Automatic Device Deactivation" | **Text:** Shall retrieve and store Device ID, Name, and Serial Number from the synchronisation data ⚠ title/text mismatch — see §1.1 A4 | I | S,U / None |
| 140 | GID-254985 | *Titled* "Automatic Device Reactivation" | **Text:** Shall prevent modification of Device ID, Name, and Serial Number except through synchronisation updates ⚠ | F | S,U / None |
| 141 | GID-254986 | *Titled* "Device Attribute Synchronisation" | **Text:** Shall automatically set the device status to "Inactive" for any active devices whose identifiers are not present in the received synchronisation data ⚠ | F, I | S,U / None |
| 142 | GID-254987 | *Titled* "Read-Only Device Attributes" | **Text:** Shall automatically restore the device status to "Active" for any inactive devices whose identifiers are present in the received synchronisation data ⚠ | F, I | S,U / None |
| 143 | GID-254988 | Automatic Facility Deactivation | Shall automatically set the facility status to "Inactive" for any active facilities whose identifiers are not present in the received synchronisation data | F, I | S,U / None |
| 144 | GID-254989 | Automatic Facility Reactivation | Shall automatically restore the facility status to "Active" for any inactive facilities whose identifiers are present in the received synchronisation data | F, I | S,U / None |
| 145 | GID-254990 | Facility Attribute Synchronisation | Shall retrieve and store Facility ID, Name, and Type from the synchronisation data | I | S,U / None |
| 146 | GID-254991 | Read-Only Facility Attributes | Shall prevent modification of Facility ID, Name, and Type except through synchronisation updates | F | S,U / None |
| 147 | GID-254992 | Automatic Risk Factor Deactivation | Shall automatically set the risk factor status to "Inactive" for any active risk factors whose identifiers are not present in the received synchronisation data | F, I | S,U / None |
| 148 | GID-254993 | Automatic Risk Factor Reactivation | Shall automatically restore the risk factor status to "Active" for any inactive risk factors whose identifiers are present in the received synchronisation data | F, I | S,U / None |
| 149 | GID-254994 | Risk Factor Attribute Synchronisation | Shall retrieve and store Risk Factor ID and Risk Factor Value from the synchronisation data | I | S,U / None |
| 150 | GID-254995 | Read-Only Risk Factor Attributes | Shall prevent modification of Risk Factor ID and Risk Factor Value except through synchronisation updates | F | S,U / None |

## §5.22 AccuScreen Pro Device Communication (9)

| # | GID | Title | Requirement | Type | Cat/HZ |
|---|---|---|---|---|---|
| 151 | GID-254996 | Device Connection | Shall establish connections with AccuScreen Pro devices through **USB** | I | S,F,U / None |
| 152 | GID-254997 | Connection Status | Shall display connection status indicating whether devices are connected, disconnected, or in the process of connecting | F, I | S,F,U / None |
| 153 | GID-254998 | Device Status | Shall display current status information from connected AccuScreen Pro devices including firmware version and device identification | F, I | S,F,U / None |
| 154 | GID-254999 | Patient Record Transfer to Device | Shall transfer patient demographic information to a connected AccuScreen Pro device | I | S,F,U / None |
| 155 | GID-255000 | Test Result Transfer from Device | Shall import patient demographic information and test results from a connected AccuScreen Pro device | I | S,F,U / None |
| 156 | GID-255001 | Site Configuration Transfer | Shall provide an option to transfer Site configuration data to a connected AccuScreen Pro device | I | S,F,U / None |
| 157 | GID-255002 | Facility Configuration Transfer | Shall provide an option to transfer Facility configuration data to a connected AccuScreen Pro device | I | S,F,U / None |
| 158 | GID-255003 | ABR Protocol Configuration Transfer | Shall transfer ABR test protocol configurations to a connected AccuScreen Pro device | I | S,F,U / None |
| 159 | GID-255004 | DPOAE Protocol Configuration Transfer | Shall transfer DPOAE test protocol configurations to a connected AccuScreen Pro device | I | S,F,U / None |

## §5.23 Network (1) · §5.24 Operating System (1)

| # | GID | Title | Requirement | Type | Cat/HZ |
|---|---|---|---|---|---|
| 160 | GID-255006 | Network Requirement Check | Shall require network access when exchanging data with web-service-based systems | NF, INF | S,U / None |
| 161 | GID-255007 | Operating System Compatibility | Shall operate on Windows 11 (Pro and Enterprise editions) | NF, INF | S,U / None |

## §5.25 Alarms, Warnings, Operator Messages (7)

| # | GID | Title | Requirement | Type | Cat/HZ |
|---|---|---|---|---|---|
| 162 | GID-255008 | Authentication Warnings | Shall display warning messages after repeated failed login attempts and notify users when accounts are locked | F | S,U / None |
| 163 | GID-255009 | Unsaved Data Warning | Shall warn users before closing windows or navigating away when unsaved data would be lost | F | S,U / None |
| 164 | GID-255010 | Connection Status Alerts | Shall provide visual indicators showing whether a device is connected or disconnected | F | S,U / None |
| 165 | GID-255011 | Firmware Update Warnings | Shall display warnings and precautions before initiating device firmware updates | F, C | S,U / None |
| 166 | GID-255012 | Validation Error Messages | Shall display error messages identifying which fields contain invalid data and what corrections are needed | F | S,U / None |
| 167 | GID-255013 | Operation Status Messages | Shall display progress indicators and status messages during time-consuming operations such as printing, data transfer, and device firmware updates | F | S,U / None |
| 168 | GID-255014 | Session Timeout Warning | Shall display a warning message before automatically logging out users due to inactivity | F | S,U / None |

## §5.26 Audit Trail (8)

| # | GID | Title | Requirement | Type | Cat/HZ |
|---|---|---|---|---|---|
| 169 | GID-255023 | User Activity Audit Logging | Shall maintain an audit log of all user login/logout, login attempts, failures, and password changes | C, NF | S / None |
| 170 | GID-255024 | Patient Record Audit Logging | Shall maintain an audit log of all patient record creation, modification, deletion, export, and import events | C, NF | S / None |
| 171 | GID-255025 | Configuration Change Audit Logging | Shall maintain an audit log of all configuration and setting changes | C, NF | S / None |
| 172 | GID-255026 | Firmware Upgrade Audit Logging | Shall maintain an audit log of all firmware upgrades | C, NF | S / None |
| 173 | GID-255027 | Audit Log Entry Fields | Shall associate all audit log entries with user ID, timestamp, description, device serial number or identifier, and status of the operation | C, NF | S / None |
| 174 | GID-255028 | Synchronisation Audit Logging | Shall maintain an audit log of all successful and failed synchronisation attempts | C, NF | S / None |
| 175 | GID-255029 | Audit Log Retention | Shall retain audit logs for a minimum of one year | C, NF | S / None |
| 176 | GID-255030 | User Status Audit Logging | Shall generate an audit trail entry for each automatic user status change, including user identifier, timestamp, and triggering synchronisation event | C, NF | S / None |

## §5.27 Logging (8)

| # | GID | Title | Requirement | Type | Cat/HZ |
|---|---|---|---|---|---|
| 177 | GID-255031 | Log Message Generation | Shall provide a mechanism to generate log messages during the operation of the software | NF | S / None |
| 178 | GID-255032 | Log Message Types | Shall provide a logging mechanism that supports informational, warning, and error message types | NF | S / None |
| 179 | GID-255033 | Log Message Storage | Shall store generated log messages on the system for troubleshooting purposes | NF | S / None |
| 180 | GID-255034 | Log Data Privacy | Shall **not** include or retain any patient-identifiable information in the log data | C, NF | S / None |
| 181 | GID-255035 | Log File Retention | Shall retain log files for a minimum of one year | C, NF | S / None |
| 182 | GID-255036 | Log File Security | Shall prevent unauthorized access to log files | C, NF | S / None |
| 183 | GID-255037 | Log Protection | Shall prevent deletion of log files **by any user** | C, NF | S / None |
| 184 | GID-255038 | Log Entry Fields | Shall associate all log entries with timestamp, description and status of the operation | NF | S / None |

## §5.28 About (1) · §5.29 Help (2)

| # | GID | Title | Requirement | Type | Cat/HZ |
|---|---|---|---|---|---|
| 185 | GID-255039 | About | Shall display information about the application, including the version number, manufacturer name and website | F | S,U / None |
| 186 | GID-255040 | Help Documentation | Shall provide access to user help documentation from within the application | F | S,U / None |
| 187 | GID-255041 | Context Help | Shall provide context-sensitive help accessible via the Help icon | F | S,U / None |

## §5.30 Database Storage (6)

| # | GID | Title | Requirement | Type | Cat/HZ |
|---|---|---|---|---|---|
| 188 | GID-255042 | Patient Record Storage | Shall store and maintain patient test records in **a local patient database** on the system | INF | S / None |
| 189 | GID-255043 | Device Configuration Storage | Shall store and maintain device configurations in **a local settings database** on the system | INF | S / None |
| 190 | GID-255044 | AccuScreen Pro Patient Record Storage | Shall store and maintain patient test records received from the AccuScreen Pro device in a local patient database | INF, I | S / None |
| 191 | GID-255045 | AccuScreen Pro Device Config Storage | Shall store and update device configurations received from the AccuScreen Pro device in a local setting database | INF, I | S / None |
| 192 | GID-256510 | Patient Database Encryption | Shall encrypt all patient test records stored in the local patient database (**minimum AES-256**) | NF, C | S / None |
| 193 | GID-256511 | Settings Database Encryption | Shall encrypt all settings configurations stored in the local settings database (**minimum AES-256**) | NF, C | S / None |

## §5.31 Firmware Update (1) · §5.32 Installation (2)

| # | GID | Title | Requirement | Type | Cat/HZ |
|---|---|---|---|---|---|
| 194 | GID-255005 | Firmware Update | Shall be able to perform a firmware update to a connected AccuScreen device | I | S,F,U / None |
| 195 | GID-255046 | Installation | Shall provide a guided installation process that verifies prerequisites and system requirements before installation | INF | S,U / None |
| 196 | GID-255047 | Uninstallation | Shall provide an uninstall process that removes all application components with an option to preserve or delete database files | INF | S,U / None |

**Total: 196 requirements.**

**ID-range cross-check.** GID-254873 → GID-255047 is a contiguous block of 175 IDs, plus 21 out-of-band IDs (GID-255112–255113, GID-255465–255466, GID-256274–256286, GID-256510–256513) = **196**. This matches the per-section sum independently. ✓

## §1.1 — Conflicts, ambiguities and document defects

Flagged, **not** silently resolved. Each maps to a clarification in §0 where a customer decision is needed.

| ID | Type | Requirements | Issue | →§0 |
|---|---|---|---|---|
| A1 | **Direct conflict** | GID-255020 vs GID-254912 | 255020 mandates fixed complexity ("shall enforce"). 254912 lets an admin set complexity to **None**, which violates it. *None*, *Simple*, *Complex* are never defined. | CQ-02 |
| A2 | **Direct conflict** | GID-254905 vs GID-254977 | 254905: admin *shall* create user accounts. 254977: software *shall prevent* admin adding a user account. Stated unconditionally with no mode/precondition. | CQ-16 |
| A3 | **Ambiguity** | GID-255017 vs GID-254911 | 255017 = lock after 5 failed *attempts*. 254911 = configure duration "after 5 consecutive **lockouts**". Literally different thresholds (5 attempts vs ~25). Code implements the first reading — `AuthenticationService.cs:29`. | CQ-02 |
| A4 | **Doc defect — titles rotated** | GID-254984, 254985, 254986, 254987 | All four device S4H titles are shuffled relative to their text (254984 titled "Deactivation" but text is attribute retrieval; 254986 titled "Attribute Synchronisation" but text is deactivation). If the Jama traceability matrix keys on titles, verification traces to the wrong requirement. Needs a Jama fix, not a code decision. | CQ-12 |
| A5 | **Scope error** | GID-256277, GID-254971 | Both say "**The device** shall…" in a *software* requirements document. Likely mis-assigned or copied from DOC-076750 (Product Requirements). | — |
| A6 | **Open architectural decision** | GID-255042/043/044/045 vs GID-256510/256511 | SRS says two databases; the repository's own analyses disagree with each other. Blocks all persistence work. | **CQ-01** |
| A7 | **Unbounded scope** | GID-254900, GID-254898 | "[Additional formats as required]" — not estimable as written. | CQ-18 |
| A8 | **Unbounded / unphased** | GID-254968 | 14 languages in 3 phases + "[Additional languages as required]". The doc never says which phase ships. | **CQ-08** |
| A9 | **Missing parent requirement** | GID-255014 | Requires a warning *before* inactivity auto-logout, but no requirement specifies the auto-logout, the timeout, or the unsaved-data behaviour at timeout. | CQ-13 |
| A10 | **Not implementable as stated** | GID-255037 + GID-255035 | "Prevent deletion of log files by any user" cannot hold against a local Windows admin. With 1-year retention and **no purge requirement**, storage growth is unbounded. | CQ-19 |
| A11 | **Untestable as stated** | GID-255006 | Specifies a precondition, not an observable behaviour. No pass/fail criterion. | CQ-15 |
| A12 | **Overlap / duplication** | GID-254894 vs GID-255112 + GID-255113 | Patient test report generation + print appears in §5.3 and again in §5.19. One feature traced twice, or two features? | CQ-05 |
| A13 | **Undefined relationship** | GID-255019 vs GID-254913–254918 | "Minimum two roles: Admin and Screener" vs a fully permission-configurable Profile system. The doc never states how role relates to profile. Code has already added a third role (`ReadOnly`) with no requirement. | CQ-17 |
| A14 | **Systematic gap** | GID-254886, 254908, 254916, 254922, 254931, 254936, 254941, 254945, 254949 | Nine delete requirements say nothing about referential integrity. Only risk factors and comments constrain deletion to unassigned items. | **CQ-06** |
| A15 | **Missing NFRs entirely** | — | No performance, capacity, concurrency, startup-time, backup/restore, or legacy-data-migration requirements anywhere. `AccuSync Architecture.md` §8 describes a DB auto-upgrade + backup/restore strategy that **no requirement asks for**. | RISK-06 |
| A16 | **Compliance gap** | GID-255018 | Category `S,U*` per §4 requires a Hazard ID *and* a Usability File entry. Hazard ID is **"TBD"**; the Usability Engineering File is itself listed **"TBD"** in §3. Cannot be verified as a mitigation. | **CQ-07** |
| A17 | **Missing input** | GID-254925 | HZ 6.4 references DOC-076518 (Risk Assessment Spreadsheet), **not provided**. Cannot confirm the mitigation required. | **CQ-07** |
| A18 | **Doc defect** | §4 Definition References | Category code **"I"** has a blank description. Legend incomplete. | — |
| A19 | **Unspecified interface** | GID-254899 | "export … via web service" — no contract defined except the S4H SEDQ URL. | CQ-10 |
| A20 | **Undefined format** | GID-256282, GID-256283, GID-256284 | HiTrack layout entirely unspecified; "OZ **fixed width**" and "CSV **fixed width**" given no field layout or spec reference. CSV and fixed-width are contradictory formats. | **CQ-09** |
| A21 | **Missing requirement (implied)** | GID-255020 | The "not same as previous three passwords" rule implies users change passwords, but **no requirement grants self-service password change**. Code implements it anyway. | CQ-04 |
| A22 | **Transport contradiction** | GID-254996 | Requires **USB**; `AccuSync Architecture.md` §2/§5 describes **serial-port** transport (`System.IO.Ports`). Materially different implementations. | **CQ-11** |
| A23 | **Missing epic coverage** | GID-254873–254882, GID-255005 | SRS §5.1 (10 reqs) and §5.31 (1 req) have **no corresponding ASWD Jira epic**. | **CQ-21** |

---

# PART 2 — WHAT THE CODE IS TODAY

Assessed at commit `700f380`, branch `users/chokkalingam/feat/AkkuSync-US5-Logout`.

## §2.1 Architecture: intended vs actual

`AccuSync Architecture.md` §2 specifies **seven** projects. `AccuSync.sln` contains **six** plus three test projects.

| Project | In arch doc | Exists | TFM | LOC (cs+xaml) | Files |
|---|---|---|---|---|---|
| `AccuSync.WPF` | ✔ | ✔ | net10.0-windows | 46,188 | 132 |
| `AccuSync.Presentation` | ✔ | ✔ | net10.0 | 2,029 | 6 |
| `AccuSync.Core` | ✔ | ✔ | net10.0 | 480 | 12 |
| `AccuSync.Application` | ✔ | ✔ | net10.0 | 13,120 | 58 |
| `AccuSync.EF` | ✔ | ✔ | net10.0 | 1,238 | 15 |
| `AccuSync.Adapters.DataParser` | ✔ | ✔ | net10.0-windows | 3,430 | 9 |
| **`AccuSync.Adapters.DeviceCommunication`** | ✔ | **✘ ABSENT** | — | **0** | **0** |
| `Tests/*` (3 projects) | — | ✔ | net10.0 | 1,659 | 10 |

**The architecture document is substantially aspirational.** These documented artefacts do not exist on disk:

| Documented in | Missing artefacts |
|---|---|
| §2 row 7, §5 | **`AccuSync.Adapters.DeviceCommunication`** — the entire project. Absent from disk *and* from `AccuSync.sln` |
| §5 `AccuSync.Core` | `ValueObjects/` (PhoneNumber, SerialNumber, SiteCode, **Waveform**), `Validators/` (PatientValidator, SiteValidator, **PasswordComplexityValidator**), `Policies/` (LockoutPolicy, PasswordExpiryPolicy), `Enums/` (6 files), `Helpers/Result.cs`, `Abstractions/Parsing/`, `Abstractions/Devices/`, `Abstractions/System/` (`IFileSystem`, **`IAuditLogger`**) — **~25 files, none exist**. Core has only `Entities/` (5) + `Abstractions/` (7) |
| §5 `AccuSync.Presentation` | `Dtos/`, `Converters/`, `Abstractions/` (`INavigationService`, `IDialogService`, `IFilePickerService`), `Common/` (`ObservableObject`, `ViewModelBase`, `ValidatableViewModelBase`, `AsyncRelayCommand`) — **14 files, none exist**. Presentation has 5 ViewModels + `Helpers/RelayCommand.cs` |
| §5 `AccuSync.WPF` | `Services/` (`WpfNavigationService`, `WpfDialogService`, `WpfFilePickerService`), `DependencyInjection/ServiceRegistration.cs` — **none exist**. DI is inline in `App.xaml.cs` |
| §5 `AccuSync.Adapters.DataParser` | Export writers (`CsvExportWriter`, `HiTrackExportWriter`, `OzExportWriter`), `ReportRenderer`, and the `Import/`+`Export/`+`Reporting/` folder split — **none exist**. `Services/` is flat, parsers only |
| §7 | ".NET Generic Host bootstrap" — actual is `new ServiceCollection()` + `BuildServiceProvider()` at `App.xaml.cs:26-28`. No `IHost` |

**[inference] This is the single most important estimation input in Phase 5.** Work items that the architecture document makes look like "add one more file to the established pattern" are in fact **greenfield**, because the pattern itself has not been built. Any estimate derived from reading the architecture doc alone would be badly optimistic.

## §2.2 Dependency-rule conformance — one violation

Architecture doc §3 states `AccuSync.Adapters.DataParser` references `AccuSync.Core` **only**. Actual, from `AccuSync.Adapters.DataParser/AccuSync.Adapters.DataParser.csproj`:

```xml
<ProjectReference Include="..\AccuSync.Application\AccuSync.Application.csproj" />
```

It references **Application**, not Core. Consequently the parsing abstractions sit in the wrong layer — `IImportService` and `IQrCodeGenerator` are in `AccuSync.Application/Abstractions/Parsing/`, whereas doc §5 places them in `AccuSync.Core/Abstractions/Parsing/`.

The doc's own stated test ("delete every project except `AccuSync.Core` and it must still compile") does pass, because Core is genuinely dependency-free. But the adapter layering is inverted relative to the design, and this must be corrected **before** export writers land or the violation doubles in size.

## §2.3 The real implementation surface: one vertical slice

Only **Authentication** is implemented end to end. Complete inventory of working backend code:

| Layer | Files | Note |
|---|---|---|
| Core entities | `User.cs`, `AppSettings.cs`, `UserRole.cs`, `PatientData.cs`, `TestRecord.cs` | `PatientData` (~110 fields) and `TestRecord` are **unpersisted DTOs** — no EF configuration, no `DbSet` |
| Core abstractions | `IUserRepository`, `IAppSettingsRepository`, `IAuthenticationService`, `ICurrentUserContext`, `IEncryptionService`, `IPasswordHasher`, `AuthenticationResult` | 7 files, all auth |
| Application services | `AuthenticationService`, `CurrentUserContext`, `EncryptionService`, `PasswordHasher` | 4 files, all auth |
| EF | `SettingsDbContext`, `UserConfiguration`, `AppSettingsConfiguration`, `UserRepository`, `AppSettingsRepository`, `TimestampInterceptor`, 3 migrations | Maps **2** tables |
| Presentation | `LoginViewModel`, `ChangePasswordViewModel`, `SplashViewModel`, `PatientViewModel`, `UserPermissionsViewModel`, `RelayCommand` | `PatientViewModel` has **no service behind it** (3 TODOs) |

**Verified absent across all `.cs` files** (grep, excluding `obj/` and `bin/`) — **zero matches for each**:

`PatientDbContext` · `IPatientRepository` · `IPatientService` · `IAuditLogger` · `IDeviceChannel` · `IExportWriter` · `IReportRenderer` · `IDataParser` · `ILogger` · `Serilog` · `NLog` · `HiTrack` · `OzExport` · `CsvExport` · `SEDQ` · `NHSP`

## §2.4 Data model — the largest single gap

| Source | Table count | Tables |
|---|---|---|
| `Databases/SettingsDatabase.sql` (545 lines) | **20** | Sites, Facilities, Locations, Profiles, Users, Devices, DeviceUsers, DeviceFacilities, DeviceFieldSetup, RiskFactors, RiskFactorTranslations, PredefinedComments, PredefinedCommentTranslations, FieldSetup, ImportConfiguration, ExportConfiguration, ABRProtocols, DPOAEProtocols, DeviceProtocols, SystemSettings |
| `Databases/PatientDatabase.sql` (477 lines) | **8** | ImportBatches, Patients, PatientContacts, TestSessions, TestRecords, ABRResults, TEOAEResults, DPOAEResults |
| **Actually modelled in EF** | **2** | `Users`, `AppSettings` — `AccuSync.EF/Contexts/SettingsDbContext.cs` |

**26 of 28 tables exist only as un-executed `.sql` DDL.** They are not code-first, not migrated, and per `AccuSync Architecture.md` §8 ("The schema is defined by the C# model and generated from it") they are slated for deletion.

`PATIENT_MANAGEMENT_STORIES.md` §3.10 flags that the `.sql` files are the **sole record** of 18 indexes, `Profiles`' **33 permission bits**, and protocol parameter value domains — and that capturing the settings-side design knowledge "**has no owner in this epic**."

**[inference]** This is the biggest cost driver in the plan. Essentially the entire data model is unbuilt, and the design knowledge required to build it correctly lives in two files scheduled for deletion. The knowledge-capture task is assigned an owner in Phase 4 as story **E2-S2**.

## §2.5 The 46k-line WPF layer is a UI shell, not a feature layer

132 files, 15,707 lines of code-behind. Only **12 of ~60** code-behind files reference any service, ViewModel, or DI:

`App.xaml.cs`, `AdminDashboardWindow`, `SidebarNavigation`, `LoginWindow`, `ChangePasswordWindow`, `SplashWindow`, `PatientsView`, `PatientInformationView`, `FacesheetReview`, `PatientDetailsTab`, `AdditionalInfoTab`, `RiskFactorsTab`

Every other view holds state in in-memory `ObservableCollection`s that vanish on close. Verified examples:

| File | Evidence |
|---|---|
| `Views/UsersProfiles/UsersContentView.xaml.cs:49-67` | `// TODO: Replace hardcoded users with data from DatabaseService` and `// BUG: Hardcoded "1234" default passwords`, followed by two literal `UserEntry` objects |
| `Views/SitesFacilities/SitesContentView.xaml.cs:47-53` | `// TODO: Replace with real data from DatabaseService`, empty collection |
| `Views/SystemConfiguration/ExportConfigView.xaml.cs:305` | `// TODO: HandleSave has no actual persistence — same pattern as all other config views.` |
| `Views/SystemConfiguration/ImportConfigView.xaml.cs:391` | `// TODO: HandleSave has no actual persistence.` |
| `Views/SystemConfiguration/ImportConfigView.xaml.cs:76` | `// TODO: Default password "1234" is hardcoded here and in HandleAdd.` |

**276 TODO / FIXME / BUG / NotImplementedException markers** across the codebase. Largest concentrations: `DocxParser.cs` (36), `OcrParser.cs` (32), `PdfParser.cs` (25), `PatientsView.xaml.cs` (10).

**[inference] The value here is real but partial.** The XAML layouts, the three style dictionaries, the ribbon control, 12 converters, and the screen inventory are genuine assets that reduce UI-side risk and make the estimates *more* confident. But every one of these screens needs a ViewModel, DTOs, a service, a repository, and an entity built beneath it — and per §2.1 the MVVM base classes those ViewModels should derive from do not exist either.

Expect the code-behind to be substantially **rewritten, not wired up**: `MessageBox.Show` calls and control-by-name access cannot move into `AccuSync.Presentation`, which deliberately carries no WPF reference (arch doc §2 row 2). Phase 5 estimates the screen stories on a rewrite basis, not a wire-up basis.

## §2.6 Test posture

Verified by running `dotnet test` on each project:

| Project | Tests | Result |
|---|---|---|
| `AccuSync.Application.Tests` | 52 | ✔ Passed, 0 failed |
| `AccuSync.EF.Tests` | 23 | ✔ Passed, 0 failed |
| `AccuSync.Presentation.Tests` | 28 | ✔ Passed, 0 failed |
| **Total** | **103** | **All passing** |

Stack: xunit 2.9.3 · Moq 4.20.72 · `Microsoft.Data.Sqlite` (real SQLite for EF tests) · `coverlet.collector` 6.0.4.

**Distribution is the problem, not the count.** All 103 tests cover the auth vertical plus three ViewModels. **Zero tests** for:

- `AccuSync.Adapters.DataParser` — 3,430 lines, **no test project exists**
- `AccuSync.WPF` — 46,188 lines, **no test project exists**
- `PatientData`, `TestRecord`, and all 58 files in `AccuSync.Application/Models/`

**[inference] Good news for estimating:** the pattern is established and clean — one `.test.cs` mirroring each source file, `Moq` for repositories, real in-memory SQLite for EF. Writing tests for *new backend* code is a well-trodden path, which is why the 0.4 × dev test ratio in Phase 5 is a Medium-to-High confidence figure. **Bad news:** there is no harness for parsers or for anything UI-shaped, and no coverage gate is configured anywhere.

## §2.7 Build / CI / CD — two verified defects

### Defect D1 — the solution does not build from a clean checkout

Verified by running `dotnet build AccuSync.sln -c Debug`:

```
error MSB3030: Could not copy the file
  "…\AccuSync.Adapters.DataParser\Resources\OCR\tessdata\eng.traineddata"
  because it was not found.
Build FAILED.  0 Warning(s)  1 Error(s)
```

**Cause.** `AccuSync.Adapters.DataParser.csproj` declares:

```xml
<Content Include="Resources\OCR\tessdata\eng.traineddata">
  <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
</Content>
```

with **no `Condition="Exists(…)"`**, while `.gitignore` excludes `*.traineddata` ("OCR training data (large binary, not source-controlled — provide locally)"). The file is genuinely absent from disk.

**Impact.** Any fresh clone, any new machine, and any CI runner fails at build. The three test projects build and pass only because none of them references `AccuSync.Adapters.DataParser`. This blocks CI setup (E1-S4) and is the first story in the plan.

Note this defect exists **only because of the facesheet OCR feature**, which no requirement asks for — see CQ-03.

### Defect D2 — the migration set is broken on a fresh install

Independently documented in `PATIENT_MANAGEMENT_STORIES_SINGLE_DB.md` §5, confirmed by the migration filenames on disk:

| Migration ID | Operation |
|---|---|
| `20260806041517_ChangeUserStatusToBoolean` | `AlterColumn` on `Users` |
| `20260806093232_AddAppSettingsTable` | `CreateTable AppSettings` |
| `20260810085959_InitialCreate` | `CreateTable Users` ← **latest timestamp** |

EF Core applies migrations in migration-ID order, so `InitialCreate` — which *creates* `Users` — runs **last**. On a database that does not yet exist, `ChangeUserStatusToBoolean` runs first and calls `AlterColumn` on a table that has not been created, and fails.

`UserRepository.InitializeDatabaseAsync()` calls `MigrateAsync()` on **every startup** (`AccuSync.EF/Repositories/UserRepository.cs:73`), so this is a live path, not a latent one. It is invisible on existing dev machines because `__EFMigrationsHistory` already records all three, so nothing re-applies. **It breaks only on a clean machine** — a fresh install, a new developer, or CI.

**Fix:** delete all three migrations and regenerate a single `InitialCreate` from the current model. This must happen **before** more migrations accumulate, and it interacts with CQ-01 (a one-DB decision would regenerate the set anyway).

### No CI/CD exists

Verified absent: `.github/`, `.azuredevops/`, `azure-pipelines.yml`, `.gitlab-ci.yml`. `build/` contains local output only.

There is **no automated build, no automated test run, no coverage gate, no packaging, and no installer**. GID-255046 and GID-255047 (Installation, Uninstallation) have zero infrastructure behind them.

## §2.8 Process state and measured velocity

### Nothing has been merged

`main` is at a single commit, `c4715ef "Accusync initial"`.

```
$ git rev-list --left-right --count main...HEAD
0    81
```

The current branch is **81 commits ahead; main is 0 ahead**. All work to date sits on a stack of 12 feature branches (`AkkuSync-US1-PasswordHashing` → … → `US5-Logout`, plus `US6-PatientPersistenceLayer` on origin), each merged into the next rather than into main.

**[inference]** This is a real schedule hazard, not a bookkeeping detail. 81 commits of unintegrated work across a 12-deep branch stack means integration risk has been accumulating silently for two weeks with no CI to catch it. A single big-bang merge to main is scheduled as story **E1-S3** and carries its own risk entry (RISK-02).

### Measured throughput

Commits span 2026-07-29 → 2026-08-10 = **9 working days** (Jul 29–31, Aug 3–7, Aug 10), 82 commits, one author.

| Date | Commits |
|---|---|
| 2026-07-29 | 4 |
| 2026-07-30 | 7 |
| 2026-07-31 | 1 |
| 2026-08-03 | 10 |
| 2026-08-04 | 7 |
| 2026-08-05 | 9 |
| 2026-08-06 | 12 |
| 2026-08-07 | 13 |
| 2026-08-10 | 19 |

Delivered in that window: the 6-project architecture split, WPF-agnostic Application layer, EF Core + SQLite persistence, DataParser extraction, five login stories (US1 Password Hashing → US5 Logout), 103 unit tests, and three HLD documents.

### Review cycles are real and cost time

Direct commit evidence, used to calibrate A-05, A-06 and A-08:

`fix: Address PR Comments` · `fix: Update Test Method Name - US1 PR Comment` · `PR 1-4 Changes: Merge branch …` · `doc: updated spec based on review` · `chore: clean xml summary - update latest logic spec` · `chore: remove ticket/spec references from code comments`

`PR 1-4 Changes` is the most informative: four PRs were reviewed as one batch, which means review turnaround is **not** same-day and the developer had to page four already-"finished" contexts back in.

## §2.9 Technical debt register — what will slow future work

| ID | Debt | Evidence | Effect on estimates | Addressed by |
|---|---|---|---|---|
| D1 | Clean checkout does not build | §2.7 | Blocks CI and any new developer | E1-S1 |
| D2 | Migration set broken on fresh DB | §2.7 | Blocks fresh-install testing and installer verification | E1-S2 |
| D3 | 26 of 28 tables unmodelled; `.sql` files are the sole record of index/permission/protocol design **and are slated for deletion** | §2.4 | Every data-touching epic pays a modelling cost first; knowledge-capture had **no owner** | E2-S2 … E2-S9 |
| D4 | One-DB vs two-DB unresolved | §1.1 A6 | Blocks **all** persistence work | **CQ-01** |
| D5 | No MVVM foundation — `ObservableObject`, `ViewModelBase`, `AsyncRelayCommand`, DTOs, converters, `INavigationService`, `IDialogService` all absent | §2.1 | Every screen story would otherwise re-invent these | E1-S5, E1-S6 |
| D6 | ~60 code-behind files hold business logic + `MessageBox.Show` + control-by-name access | §2.5 | Cannot be lifted into `AccuSync.Presentation` (no WPF ref) — **rewrite, not rewire** | Priced into every screen story |
| D7 | Navigation is static `App.GetService<T>()` + direct `Window` construction | `App.xaml.cs` `NavigateToDashboard`, `NavigateToTargetScreen`, `Logout` | Untestable; degrades with every screen added | E1-S6 |
| D8 | DataParser references Application, not Core; parsing abstractions in the wrong layer | §2.2 | Must be fixed **before** export writers land | E1-S8 |
| D9 | Dev-mode login bypass ships in the product | `DevModeConfig.cs`, `enable-dev.ps1/.bat`, `App.OnStartup` | Security and regulatory risk on a medical device | E1-S9 |
| D10 | Hardcoded `"1234"` default passwords | `UsersContentView.xaml.cs:59`, `ImportConfigView.xaml.cs:76` | Violates GID-255020 at every non-UI creation path | E6-S1 |
| D11 | 276 TODO/BUG markers, ~93 concentrated in the three untested, un-required OCR parsers | §2.5 | Sizes CQ-03 option costs | E25 |
| D12 | Zero CI; 81 commits unmerged on a 12-deep branch stack | §2.7, §2.8 | Silent integration risk | E1-S3, E1-S4 |
| D13 | Localisation: 1,204 resx entries **duplicated across two projects**; zero satellite files | `AccuSync.WPF/Resources/Strings.resx`, `AccuSync.Application/Resources/Strings.resx`; `find` for `*.??.resx` → no matches | Externalisation done (a real asset); translation and CJK/RTL layout not started; the duplication must be resolved first | E11-S3 |

---

# PART 3 — REQUIREMENT STATUS

Every one of the 196 requirements is classified. Where several requirements share one identical justification they are grouped into a single row, but **every ID is enumerated** — nothing is dropped.

| Class | Count |
|---|---|
| **DONE** | 6 |
| **PARTIAL** | 17 |
| **NOT STARTED** | 164 |
| **CONFLICTS WITH CODE** | 4 |
| **UNCLEAR** | 5 |
| **Total** | **196** ✓ |

## §3.1 DONE (6)

All six sit on the unmerged branch stack (§2.8), so "done" means **done-on-branch, not done-on-main**.

| GID | Evidence |
|---|---|
| **GID-255015** User Login | `AuthenticationService.cs:41-164` + `LoginViewModel.cs` + `LoginWindow.xaml`. 16 tests in `AuthenticationService.test.cs`, 5 in `LoginViewModel.test.cs`. Username enumeration deliberately prevented (`:56`, same error whether the user exists or not) |
| **GID-255016** User Logout | `App.Logout()` in `App.xaml.cs` → `ICurrentUserContext.SignOut()`; 4 tests in `CurrentUserContext.test.cs`. Commits `feat: logout`, `fix: Remove additional logout pop-up` |
| **GID-255017** User Account Lockout | `MaxFailedAttempts = 5` at `AuthenticationService.cs:29`; lockout window + auto-unlock at `:77-99`; failure streak timestamped from the first failure at `:104-112`. Admin override via `IUserRepository.UnlockUserAsync`. Duration is admin-configurable via `AppSettings.LockoutDurationMinutes` (default 15). Satisfies "minimum of 5" |
| **GID-255021** Deactivated User Login Prevention | `AuthenticationService.cs:66-73` — rejects `!user.Status` **before** the password check, with a generic message so account status is not leaked either |
| **GID-255018** Credential Encryption | Passwords: PBKDF2-HMAC-SHA256, **210,000 iterations** (OWASP 2023 minimum), 128-bit salt, 256-bit key — `PasswordHasher.cs:20-34`, 6 tests. Usernames: reversible encryption via `IDataProtector` — `EncryptionService.cs:26-40`, 5 tests. **Caveat:** functionally complete, but per CQ-07 the `S,U*` category obligations (Hazard ID, Usability File entry) are **TBD**, so it is not *verifiable* as a risk mitigation |
| **GID-255019** Role-Based Access Control | `UserRoleParser` (3 tests), `UserPermissionsViewModel.Admin()` / `.Screener()` (4 tests), consumed by `SidebarNavigation.SetPermissions()` and `AdminDashboardWindow.SetPermissions()`. Meets "minimum two roles". **Caveat:** roles are hardcoded C# presets, not the DB-driven Profiles of §5.9 — see CQ-17 and E9-S7 |

## §3.2 PARTIAL (17)

| GID(s) | What exists | What is missing |
|---|---|---|
| **GID-255020** Password Complexity | All four SRS rules implemented — `Length >= 8`, `Any(char.IsUpper)`, `Any(char.IsLower)`, `Any(char.IsDigit)` at `ChangePasswordViewModel.cs:174-177`; previous-3-password history at `:209-237` via `User.LastThreePasswords` (pipe-delimited hashes); 14 tests | **Enforced in the Presentation layer only, on the change-password path only.** No `PasswordComplexityValidator` in `AccuSync.Core` (arch doc §5 requires one; grep confirms absent). Every other creation path bypasses it — admin user creation, S4H default password (GID-254979), and the literal `"1234"` defaults at `UsersContentView.xaml.cs:59` and `ImportConfigView.xaml.cs:76`. Also enforces a **special-character** rule the SRS does not require (`:179`) → CQ-02 |
| **GID-254900** Supported Import Formats | 3 of 5 named formats parse: `algo5-xml` → `Algo5XmlParser`, `acculink-xml` → `AccuLinkParser`, `algopro-json` → `AlgoProJsonParser`, dispatched at `ImportService.cs:39-53` | AccuSync XML and JSON are **explicit stubs** returning `"…is not yet implemented."` at `ImportService.cs:102-115`, each preceded by `// TODO: Implement when AccuSync XML/JSON export format is designed` — **blocked on the export format design** (E13-S1). No parsed patient is persisted (no repository exists). **Zero tests** on 3,430 lines |
| **GID-255008** Authentication Warnings | `"{n} attempt(s) remaining"` and `"Account is now locked…"` at `AuthenticationService.cs:114-133` | No audit entry for these events (GID-255023 not started). Messages are **English string literals in the service layer**, not resx — conflicts with GID-254969 |
| **GID-254892** Mandatory Field Indication · **GID-254893** Patient Save Validation | `PatientDetailsTab.xaml`, `AdditionalInfoTab.xaml`, `PatientInformationView.xaml` field layouts exist; `FieldSetupTable.cs` helper present | No configuration source (`FieldSetup` table unmodelled), no validation service, no save target. `PatientViewModel.cs` carries 3 TODOs and has no service behind it |
| **GID-254883, 254884, 254885, 254887, 254890, 254891** Patient CRUD / list / search / display | `PatientsView.xaml.cs` (1,350 lines) + `PatientInformationView.xaml.cs` (477) + 3 tab views exist and do reference DI; `PatientData` DTO has ~110 fields | **Nothing persists.** No `Patients` table, no `PatientDbContext`, no `IPatientRepository`, no `IPatientService` — all four grep to zero. 10 TODOs in `PatientsView.xaml.cs`. Confirmed by the repo's own analysis, `PATIENT_MANAGEMENT_STORIES.md` §2.1: *"Headline: nothing in the Patient/Tests area is persisted"* |
| **GID-254888** Patient Risk Factor Selection · **GID-254889** Patient Comment Assignment | `RiskFactorsTab.xaml(.cs)`, `RiskFactorsConfigView.xaml(.cs)`, `CommentsConfigView.xaml(.cs)` UI exists; `RiskFactor`, `RiskFactorEntry`, `CommentEntry` models exist | Master lists unmodelled (`RiskFactors`, `RiskFactorTranslations`, `PredefinedComments`, `PredefinedCommentTranslations` are `.sql`-only). `PATIENT_MANAGEMENT_STORIES.md` §3.5 records risk-factor storage as *"still needs a decision"* |
| **GID-254919, 254920, 254921, 254922, 254923, 254924, 254926** Device Management | `DevicesContentView.xaml.cs` (608 lines), `DeviceFieldSetupView.xaml.cs` (493); `DeviceEntry`, `DeviceInfo`, `DeviceFieldEntry`, `DeviceUpdateInfo` models | UI-only, 4 TODOs. `Devices`, `DeviceUsers`, `DeviceFacilities`, `DeviceFieldSetup` unmodelled. "Last Seen" / "Last Updated" / "Hardware Version" / "Firmware Version" all require device communications — **project absent** (§2.1) |

## §3.3 CONFLICTS WITH CODE (4)

| GID | Conflict |
|---|---|
| **GID-256510** Patient DB Encryption · **GID-256511** Settings DB Encryption | Both require **minimum AES-256 at the database level**. The code encrypts **individual fields** via `IDataProtector` (`EncryptionService.cs`) and stores everything else in **plaintext SQLite**. `UserConfiguration.cs:25-45` applies no `HasConversion` to any property — `FirstName`, `LastName`, `ProfileId`, and all timestamps are plaintext on disk. No SQLCipher, no `SqliteConnection` password, no EF encryption provider anywhere in `Directory.Packages.props`. **The existing approach does not satisfy either requirement and is not a partial step toward it** — database-level encryption is a different mechanism, selected at provider level. Requires the E2-S1 spike |
| **GID-255019** Role-Based Access Control | SRS specifies "minimum two roles: Admin and Screener". Code adds a third, `ReadOnly()`, in `UserPermissionsViewModel.cs`. "Minimum two" *permits* more, so this is not strictly a violation — but it is an **undocumented role with no requirement**, and it interacts with the unresolved role-vs-profile question → CQ-17 |
| **GID-254912** Password Complexity Configuration | Requires admin-configurable complexity (None/Simple/Complex). Code hardcodes one fixed rule set — including a special-character rule the SRS never asks for — at `ChangePasswordViewModel.cs:174-179`, with `CanSave` gating on all five. Implementing GID-254912 means **reworking shipped, tested code**, not extending it, and requires resolving CQ-02 first |

## §3.4 UNCLEAR (5)

| GID | Why it cannot be classified yet |
|---|---|
| **GID-255042, GID-255043** Patient / Settings DB storage | Depends on **CQ-01**. `SettingsDbContext` exists with 2 tables; whether the target is one context or two is unresolved between the SRS text and the repository's own competing analyses |
| **GID-254905** Create User Account | Blocked by **CQ-16 / A2** — cannot classify against code until the S4H-mode precondition on GID-254977 is stated |
| **GID-255014** Session Timeout Warning | Blocked by **CQ-13 / A9** — the parent auto-logout requirement does not exist, and no inactivity timer exists in code. A warning for an unspecified feature cannot be assessed |
| **GID-254894** Patient Test Report Generation | Blocked by **CQ-05 / A12** — may be the same feature as GID-255112/255113. Classifying it separately risks double-counting an epic |

## §3.5 NOT STARTED (164)

Zero implementing code exists for any of these. Grouped by shared justification; **all IDs enumerated**.

| Group | GIDs | Count | Evidence of absence |
|---|---|---|---|
| **General CRUD & unsaved-data framework** | 254873, 254874, 254875, 254876, 254877, 254878, 254879, 254880, 254881, 254882 | **10** | Only per-view ad-hoc implementations: `Stack<UserSnapshot> _undoStack` at `UsersContentView.xaml.cs:35`, `Stack<SiteSnapshot>` at `SitesContentView.xaml.cs:30` — duplicated per screen, in-memory, no persistence (`ExportConfigView.xaml.cs:305`: *"same pattern as all other config views"*). No shared framework; nothing reaches a database |
| **Deactivated user transfer prevention** | 255022 | **1** | Requires device transfer — `AccuSync.Adapters.DeviceCommunication` absent |
| **Patient delete / test entry operations** | 254886, 254895, 254896 | **3** | No `Patients` / `TestRecords` tables, no repository |
| **Patient test list display** | 254897 | **1** | `TestResultRow` / `TestPreviewItem` models exist; `TestSessions` / `TestRecords` unmodelled; "Test Configuration" needs `ABRProtocols` / `DPOAEProtocols` (also unmodelled) |
| **Export — all** | 254898, 254899, 256274, 256275, 256276, 256277, 256278, 256279, 256280, 256281, 256282, 256283, 256284, 256285, 256286, 256512 | **16** | Grep for `ExportWriter` / `HiTrack` / `OzExport` / `CsvExport` matches only `Strings.Designer.cs` (UI labels). `ExportDialog.xaml.cs` validates date ranges then does nothing. No `ExportData/` writer, no naming-convention code |
| **Test result views (waveforms)** | 254901, 254902, 254903 | **3** | `TestResultsView.xaml(.cs)` exists as UI. No waveform storage (`ABRResults` / `TEOAEResults` / `DPOAEResults` are `.sql`-only), no `Waveform` value object (arch doc §5 — absent), no rendering |
| **User account management** | 254904, 254906, 254907, 254908, 254909, 254910, 254911 | **7** | 254907 has `UnlockUserAsync` on `IUserRepository` but **no admin UI path**; 254911 has `AppSettings.LockoutDurationMinutes` + `AppSettingsRepository` (3 tests) but **no configuration UI**. `UsersContentView` is hardcoded (§2.5) |
| **Profile management** | 254913, 254914, 254915, 254916, 254917, 254918 | **6** | `ProfilesContentView.xaml.cs` (573 lines) UI-only + 1 TODO. `ProfileEntry` / `ComponentPermissions` / `PermissionItem` models exist but `Profiles` is unmodelled — the `.sql` file is the **sole record of its 33 permission bits** |
| **Device settings configuration** | 254925 | **1** | `S,U*` / HZ 6.4. Needs device comms **and** `DeviceFieldSetup` table. Both absent. Blocked by CQ-07 |
| **Site / Facility / Location** | 254927, 254928, 254929, 254930, 254931, 254932, 254933, 254934, 254935, 254936, 254937, 254938, 254939, 254940, 254941, 255465 | **16** | Three views exist (`SitesContentView` 266 lines, `FacilitiesContentView` 296, `LocationsContentView`), all carrying `// TODO: Replace with real data from DatabaseService`. `Sites` / `Facilities` / `Locations` unmodelled |
| **ABR / DPOAE protocol configuration** | 254942, 254943, 254944, 254945, 254946, 254947, 254948, 254949 | **8** | `ABRConfigurationView.xaml.cs`, `DPOAEConfigurationView.xaml.cs` (310 lines) UI-only. `ABRProtocolEntry` / `DPOAEProtocolEntry` models exist; `ABRProtocols` / `DPOAEProtocols` / `DeviceProtocols` unmodelled — `.sql` is the sole record of parameter value domains |
| **Risk factor / comment configuration** | 254950, 254951, 254952, 254953, 254954, 254955, 254956, 254957, 254958, 254959 | **10** | Views exist with TODOs; master and translation tables unmodelled. The "not assigned" in-use checks (254952/254953/254958) require the Patients side — `PATIENT_MANAGEMENT_STORIES_SINGLE_DB.md` §2.3 flags that these checks run **backwards** across a two-DB boundary → CQ-01 |
| **Patient field configuration** | 254960, 254961, 254962, 254963, 254964, 254965, 254966, 255466 | **8** | `FieldSetupConfigView.xaml.cs` (533 lines) + `FieldSetupTable.cs` UI-only, 2 TODOs. `FieldSetup` / `SystemSettings` unmodelled. No validation engine exists |
| **Report generation** | 255112, 255113 | **2** | `PrintDialog.xaml(.cs)` UI-only. No `IReportRenderer` (grep → zero), no `ReportRenderer`, no report template, no PDF or print pipeline, **no reporting library in `Directory.Packages.props`** |
| **Language** | 254967, 254968, 256513, 254969, 254970 | **5** | 1,204 resx entries in each of two projects, but **zero satellite `.resx`** — verified by `find` for `*.??.resx` / `*.??-??.resx`. No `CultureInfo` switching, no per-user language persistence (254910), no restart/confirm flow. English literals still hardcoded in services (e.g. `AuthenticationService.cs:48`) |
| **S4H — all** | 254971, 254972, 254973, 254974, 254975, 254976, 254977, 254978, 254979, 254980, 254981, 254982, 254983, 254984, 254985, 254986, 254987, 254988, 254989, 254990, 254991, 254992, 254993, 254994, 254995 | **25** | No SEDQ client, no HTTP or web-service code, no sync engine, no NHSP terminology mapping, no status-reconciliation logic. Grep for `SEDQ` / `S4H` / `NHSP` → nothing outside resx labels. **Largest single untouched block** |
| **Device communication** | 254996, 254997, 254998, 254999, 255000, 255001, 255002, 255003, 255004 | **9** | `AccuSync.Adapters.DeviceCommunication` **does not exist** (§2.1). `System.IO.Ports 8.0.0` is pinned in `Directory.Packages.props` under a stale `<!-- AccuSync.Application -->` comment but referenced by **no project**. GID-254996 mandates USB; the arch doc describes serial → CQ-11 |
| **Network / OS** | 255006, 255007 | **2** | No connectivity check anywhere. Windows 11 targeting unverified — `net10.0-windows` is necessary but not sufficient; no target-OS declaration and no install-time OS check exist |
| **Alarms / warnings / messages** | 255009, 255010, 255011, 255012, 255013 | **5** | 255009: only ad-hoc per-view undo stacks; no unsaved-state tracking across navigation. 255010: `StatusToBrushConverter` exists but no live status source. 255011/255013: `FirmwareUpdateDialog.xaml(.cs)` is a UI shell with 2 TODOs, no progress plumbing. 255012: no validation engine |
| **Audit trail — all** | 255023, 255024, 255025, 255026, 255027, 255028, 255029, 255030 | **8** | Grep for `IAuditLogger` / `AuditLog` → **zero matches**. No audit table, no entity, no writer, no retention mechanism. `PATIENT_MANAGEMENT_STORIES.md` §3.3 records audit logging as *"required (GID-255024), reversing Rev A"* — a recent scope addition |
| **Logging — all** | 255031, 255032, 255033, 255034, 255035, 255036, 255037, 255038 | **8** | Grep for `ILogger` / `Serilog` / `NLog` → **zero matches**. Current diagnostics are `Debug.WriteLine` in `App.xaml.cs`, which is compiled out in Release. No log file, no retention, no ACLs, no tamper protection |
| **About / Help** | 255039, 255040, 255041 | **3** | `AboutContentView.xaml(.cs)` exists but with no version / manufacturer / website binding (no `AssemblyInformationalVersion` plumbing). No help content, no CHM/HTML help, no context-help mapping |
| **DB storage from device** | 255044, 255045 | **2** | Require device communications — absent |
| **Firmware update** | 255005 | **1** | `FirmwareUpdateDialog` UI shell only. No `IFirmwareUpdater`, no transfer protocol, no firmware file validation |
| **Installation** | 255046, 255047 | **2** | **No installer project of any kind** — no WiX, no MSIX, no Inno/NSIS, no `.wxs`. No prerequisite check. Blocked by defects D1 and D2 |
| | | **164** ✓ | |

## §3.6 Code that no requirement asks for

Flagged per the request. Roughly **20,000+ lines**. This is the subject of **CQ-03**.

| Feature | Files / size | Requirement status |
|---|---|---|
| **Dashboards** (Admin + Screener) and **14 dashboard dialogs** | `AccuSync.WPF/Views/Dashboard/` — `AdminDashboardWindow`, `ScreenerDashboardWindow`, `DashboardContentView`, `BaseScreenView`, `SidebarNavigation` (1,000 lines), + 11 dialogs; models `DashboardCardInfo`, `PendingScreening`, `CompletedScreening`, `AssignedPatient(Info)`, `UnassignedPatientInfo`, `NotExportedPatientInfo`, `ScreenerInfo`, `ScreenerNotExportedScreening`, `Badge`, `ScreeningBadge`. **~5,000+ lines** | **The SRS has no dashboard section at all.** Largest unrequested item |
| **Facesheet import via OCR / PDF / DOCX** | `OcrParser.cs`, `PdfParser.cs`, `DocxParser.cs` (~2,400 lines, **93 TODO markers**), `facesheet-image`/`-pdf`/`-docx` branches at `ImportService.cs:47-53`, `FacesheetReview.xaml(.cs)` (443 lines), `IsFacesheetFormat`, `GetFacesheetPreviewText`. Packages: `Tesseract`, `Docnet.Core`, `PdfPig`, `DocumentFormat.OpenXml`, `System.Drawing.Common` | GID-254900 lists **only** AccuSync XML/JSON, ALGO 5 XML, ALGO Pro JSON, AccuLink XML. Facesheet OCR is **not a listed import format**. It is also the **sole cause of build defect D1** and the only reason `AccuSync.Adapters.DataParser` is Windows-bound |
| **QR code generation** | `QrCodeGenerator.cs`, `IQrCodeGenerator.cs`, `QRCoder` package | **No requirement mentions QR codes** anywhere in the SRS |
| **90-day password expiry** | `AuthenticationService.cs:136-152` | No requirement. Arch doc §5 names a `PasswordExpiryPolicy`; the SRS does not. Its own TODO notes users have **no self-service reset**, so a feature the SRS never asked for can lock every user out on day 91 → CQ-04 |
| **Self-service password change + forced first-login change** | `ChangePasswordWindow`, `ChangePasswordViewModel` (14 tests), `User.FirstLogin` column | No requirement grants self-service change (A21). Implied by GID-255020's history rule but never stated → CQ-04 |
| **`ReadOnly` role** | `UserPermissionsViewModel.ReadOnly()` | Third role beyond the SRS's Admin/Screener → CQ-17 |
| **Dev-mode login / dashboard bypass** | `DevModeConfig.cs`, `enable-dev.ps1`, `enable-dev.bat`, `App.OnStartup`, `NavigateAfterSplash` | No requirement. **Security risk if it reaches Release** → E1-S9 |
| **Splash screen** | `SplashWindow`, `SplashViewModel` | No requirement — but it performs DB initialisation, so the function needs a home regardless |
| **Ribbon toolbar framework** | `RibbonToolbar.xaml(.cs)`, `RibbonDefinition(s).cs`, `RibbonGroup`, `RibbonItem`, `RibbonItemType`, `RibbonItemClickEventArgs`, `RibbonIcons.cs`, `RibbonToolbarStyles.xaml` | UI implementation choice, no requirement. Legitimate, but it is scope |
| **SSN, referral and extended contact fields** | `SSNFormatter.cs`, `PhoneNumberFormatter.cs`, `CountryDialCode.cs`; `PatientData` — `MotherSSN`, `CaregiverSSN`, `AudiologyReferral`, `ReferralDate/To/From/Phone`, `Medication`, `Physician`, `Audiologist`, ~30 mother/caregiver address fields | GID-254883 does not enumerate fields. **[inference]** Plausible readings, but SSN storage carries privacy weight and interacts with GID-256279, GID-255034 and GID-256510 → CQ-20 |
| **`ImportBatches` table** | `Databases/PatientDatabase.sql` | No requirement covers import batch tracking |

---
