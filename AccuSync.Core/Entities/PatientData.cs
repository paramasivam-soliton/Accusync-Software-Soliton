using System.Collections.Generic;

namespace AccuSync.Core.Entities
{
    /// <summary>
    /// Flat data transfer object populated from device import files.
    /// Maps 1:1 to the fields exported by screening devices.
    /// Not used directly in the UI — ViewModels wrap this for binding and validation.
    /// </summary>
    // TODO: Extract a shared ContactInfo class for Mother and Caregiver —
    //       both have the same 15 fields (Title, SSN, Name, Address, Phone, Email, etc.).
    public class PatientData
    {
        // Source identity — used for deduplication across imports (matches Patients.SourceId in DB)
        /// <summary>The source system's unique identifier for this patient, used for deduplication across imports.</summary>
        public string SourceId { get; set; } = string.Empty;
        /// <summary>When the source record was created.</summary>
        public string SourceCreatedAt { get; set; } = string.Empty;
        /// <summary>When the source record was last modified.</summary>
        public string SourceModifiedAt { get; set; } = string.Empty;

        // Patient information
        /// <summary>The patient's identifier.</summary>
        public string PatientId { get; set; } = string.Empty;
        /// <summary>The patient's hospital identifier.</summary>
        public string HospitalId { get; set; } = string.Empty;
        /// <summary>The patient's first name.</summary>
        public string FirstName { get; set; } = string.Empty;
        /// <summary>The patient's last name.</summary>
        public string LastName { get; set; } = string.Empty;
        /// <summary>The patient's middle initial.</summary>
        public string MiddleInitial { get; set; } = string.Empty;
        /// <summary>The patient's date of birth.</summary>
        public string DateOfBirth { get; set; } = string.Empty;
        /// <summary>The patient's gender.</summary>
        public string Gender { get; set; } = "Unknown";
        /// <summary>The patient's gestational age at birth.</summary>
        public string GestationalAge { get; set; } = string.Empty;
        /// <summary>The patient's weight.</summary>
        public string Weight { get; set; } = string.Empty;
        /// <summary>The patient's height.</summary>
        public string Height { get; set; } = string.Empty;
        /// <summary>The patient's birth location.</summary>
        public string BirthLocation { get; set; } = string.Empty;
        /// <summary>The patient's nationality.</summary>
        public string Nationality { get; set; } = string.Empty;
        /// <summary>Whether screening consent has been given for the patient.</summary>
        public string ScreeningConsent { get; set; } = "No";
        /// <summary>The patient's consent state.</summary>
        public string ConsentState { get; set; } = "No";
        /// <summary>Whether the patient was in the NICU (Neonatal Intensive Care Unit).</summary>
        public string NICU { get; set; } = "No";
        /// <summary>Whether and when the patient was discharged.</summary>
        public string Discharged { get; set; } = string.Empty;
        /// <summary>Whether and when the patient is recorded as deceased.</summary>
        public string Deceased { get; set; } = string.Empty;
        /// <summary>Whether tracking consent has been given for the patient.</summary>
        public string TrackingConsent { get; set; } = "No tracking";
        /// <summary>Free-form comments about the patient.</summary>
        public string Comments { get; set; } = string.Empty;

        // Aliases for import compatibility — some device files use different field names
        // for the same data. These redirect to the canonical properties above.
        /// <summary>Alias for <see cref="PatientId"/>, used by import sources that name the field differently.</summary>
        public string MedicalRecordNumber
        {
            get => PatientId;
            set => PatientId = value;
        }

        /// <summary>Alias for <see cref="Weight"/>, used by import sources that name the field differently.</summary>
        public string BirthWeight
        {
            get => Weight;
            set => Weight = value;
        }

        /// <summary>Alias for <see cref="Discharged"/>, used by import sources that name the field differently.</summary>
        public string DischargeDate
        {
            get => Discharged;
            set => Discharged = value;
        }

        // Risk factors
        /// <summary>Risk factor values keyed by risk factor name.</summary>
        public Dictionary<string, string> RiskFactors { get; set; } = new Dictionary<string, string>();

        // Mother information
        /// <summary>The mother's title.</summary>
        public string MotherTitle { get; set; } = string.Empty;
        /// <summary>The mother's social security number.</summary>
        public string MotherSSN { get; set; } = string.Empty;
        /// <summary>The mother's identifier.</summary>
        public string MotherId { get; set; } = string.Empty;
        /// <summary>The mother's first name.</summary>
        public string MotherFirstName { get; set; } = string.Empty;
        /// <summary>The mother's last name.</summary>
        public string MotherLastName { get; set; } = string.Empty;
        /// <summary>The mother's date of birth.</summary>
        public string MotherDOB { get; set; } = string.Empty;
        /// <summary>The mother's preferred language.</summary>
        public string MotherLanguage { get; set; } = string.Empty;
        /// <summary>The mother's first address line.</summary>
        public string MotherAddress1 { get; set; } = string.Empty;
        /// <summary>The mother's second address line.</summary>
        public string MotherAddress2 { get; set; } = string.Empty;
        /// <summary>The mother's city.</summary>
        public string MotherCity { get; set; } = string.Empty;
        /// <summary>The mother's state.</summary>
        public string MotherState { get; set; } = string.Empty;
        /// <summary>The mother's zip code.</summary>
        public string MotherZip { get; set; } = string.Empty;
        /// <summary>The mother's country.</summary>
        public string MotherCountry { get; set; } = string.Empty;
        /// <summary>The mother's phone number.</summary>
        public string MotherPhone { get; set; } = string.Empty;
        /// <summary>The mother's mobile number.</summary>
        public string MotherMobile { get; set; } = string.Empty;
        /// <summary>The mother's fax number.</summary>
        public string MotherFax { get; set; } = string.Empty;
        /// <summary>The mother's email address.</summary>
        public string MotherEmail { get; set; } = string.Empty;

        /// <summary>Alias for <see cref="MotherDOB"/>, used by import sources that name the field differently.</summary>
        public string MotherDateOfBirth
        {
            get => MotherDOB;
            set => MotherDOB = value;
        }

        /// <summary>Alias for <see cref="MotherId"/>, used by import sources that name the field differently.</summary>
        public string MotherMedicalRecordNumber
        {
            get => MotherId;
            set => MotherId = value;
        }

        // Caregiver information
        /// <summary>The caregiver's title.</summary>
        public string CaregiverTitle { get; set; } = string.Empty;
        /// <summary>The caregiver's social security number.</summary>
        public string CaregiverSSN { get; set; } = string.Empty;
        /// <summary>The caregiver's first name.</summary>
        public string CaregiverFirstName { get; set; } = string.Empty;
        /// <summary>The caregiver's last name.</summary>
        public string CaregiverLastName { get; set; } = string.Empty;
        /// <summary>The caregiver's preferred language.</summary>
        public string CaregiverLanguage { get; set; } = string.Empty;
        /// <summary>The caregiver's first address line.</summary>
        public string CaregiverAddress1 { get; set; } = string.Empty;
        /// <summary>The caregiver's second address line.</summary>
        public string CaregiverAddress2 { get; set; } = string.Empty;
        /// <summary>The caregiver's city.</summary>
        public string CaregiverCity { get; set; } = string.Empty;
        /// <summary>The caregiver's state.</summary>
        public string CaregiverState { get; set; } = string.Empty;
        /// <summary>The caregiver's zip code.</summary>
        public string CaregiverZip { get; set; } = string.Empty;
        /// <summary>The caregiver's country.</summary>
        public string CaregiverCountry { get; set; } = string.Empty;
        /// <summary>The caregiver's phone number.</summary>
        public string CaregiverPhone { get; set; } = string.Empty;
        /// <summary>The caregiver's mobile number.</summary>
        public string CaregiverMobile { get; set; } = string.Empty;
        /// <summary>The caregiver's fax number.</summary>
        public string CaregiverFax { get; set; } = string.Empty;
        /// <summary>The caregiver's email address.</summary>
        public string CaregiverEmail { get; set; } = string.Empty;

        // Referral information
        /// <summary>Whether the patient has been referred to audiology.</summary>
        public string AudiologyReferral { get; set; } = string.Empty;
        /// <summary>The date of the audiology referral.</summary>
        public string ReferralDate { get; set; } = string.Empty;
        /// <summary>Who the patient was referred to.</summary>
        public string ReferralTo { get; set; } = string.Empty;
        /// <summary>Who referred the patient.</summary>
        public string ReferralFrom { get; set; } = string.Empty;
        /// <summary>The phone number for the referral.</summary>
        public string ReferralPhone { get; set; } = string.Empty;

        // Medical information
        /// <summary>The patient's current medication.</summary>
        public string Medication { get; set; } = string.Empty;
        /// <summary>The patient's physician.</summary>
        public string Physician { get; set; } = string.Empty;
        /// <summary>The patient's audiologist.</summary>
        public string Audiologist { get; set; } = string.Empty;

        // Computed display helpers
        /// <summary>The patient's first and last name initials.</summary>
        public string Initials => $"{(string.IsNullOrEmpty(FirstName) ? "" : FirstName[0])}{(string.IsNullOrEmpty(LastName) ? "" : LastName[0])}";
        /// <summary>The patient's full name.</summary>
        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}