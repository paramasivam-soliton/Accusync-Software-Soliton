using System.Collections.Generic;

namespace AccuSync.Application.Models
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
        public string SourceId { get; set; } = string.Empty;
        public string SourceCreatedAt { get; set; } = string.Empty;
        public string SourceModifiedAt { get; set; } = string.Empty;

        // Patient information
        public string PatientId { get; set; } = string.Empty;
        public string HospitalId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string MiddleInitial { get; set; } = string.Empty;
        public string DateOfBirth { get; set; } = string.Empty;
        public string Gender { get; set; } = "Unknown";
        public string GestationalAge { get; set; } = string.Empty;
        public string Weight { get; set; } = string.Empty;
        public string Height { get; set; } = string.Empty;
        public string BirthLocation { get; set; } = string.Empty;
        public string Nationality { get; set; } = string.Empty;
        public string ScreeningConsent { get; set; } = "No";
        public string ConsentState { get; set; } = "No";
        public string NICU { get; set; } = "No";
        public string Discharged { get; set; } = string.Empty;
        public string Deceased { get; set; } = string.Empty;
        public string TrackingConsent { get; set; } = "No tracking";
        public string Comments { get; set; } = string.Empty;

        // Aliases for import compatibility — some device files use different field names
        // for the same data. These redirect to the canonical properties above.
        public string MedicalRecordNumber
        {
            get => PatientId;
            set => PatientId = value;
        }

        public string BirthWeight
        {
            get => Weight;
            set => Weight = value;
        }

        public string DischargeDate
        {
            get => Discharged;
            set => Discharged = value;
        }

        // Risk factors
        public Dictionary<string, string> RiskFactors { get; set; } = new Dictionary<string, string>();

        // Mother information
        public string MotherTitle { get; set; } = string.Empty;
        public string MotherSSN { get; set; } = string.Empty;
        public string MotherId { get; set; } = string.Empty;
        public string MotherFirstName { get; set; } = string.Empty;
        public string MotherLastName { get; set; } = string.Empty;
        public string MotherDOB { get; set; } = string.Empty;
        public string MotherLanguage { get; set; } = string.Empty;
        public string MotherAddress1 { get; set; } = string.Empty;
        public string MotherAddress2 { get; set; } = string.Empty;
        public string MotherCity { get; set; } = string.Empty;
        public string MotherState { get; set; } = string.Empty;
        public string MotherZip { get; set; } = string.Empty;
        public string MotherCountry { get; set; } = string.Empty;
        public string MotherPhone { get; set; } = string.Empty;
        public string MotherMobile { get; set; } = string.Empty;
        public string MotherFax { get; set; } = string.Empty;
        public string MotherEmail { get; set; } = string.Empty;

        public string MotherDateOfBirth
        {
            get => MotherDOB;
            set => MotherDOB = value;
        }

        public string MotherMedicalRecordNumber
        {
            get => MotherId;
            set => MotherId = value;
        }

        // Caregiver information
        public string CaregiverTitle { get; set; } = string.Empty;
        public string CaregiverSSN { get; set; } = string.Empty;
        public string CaregiverFirstName { get; set; } = string.Empty;
        public string CaregiverLastName { get; set; } = string.Empty;
        public string CaregiverLanguage { get; set; } = string.Empty;
        public string CaregiverAddress1 { get; set; } = string.Empty;
        public string CaregiverAddress2 { get; set; } = string.Empty;
        public string CaregiverCity { get; set; } = string.Empty;
        public string CaregiverState { get; set; } = string.Empty;
        public string CaregiverZip { get; set; } = string.Empty;
        public string CaregiverCountry { get; set; } = string.Empty;
        public string CaregiverPhone { get; set; } = string.Empty;
        public string CaregiverMobile { get; set; } = string.Empty;
        public string CaregiverFax { get; set; } = string.Empty;
        public string CaregiverEmail { get; set; } = string.Empty;

        // Referral information
        public string AudiologyReferral { get; set; } = string.Empty;
        public string ReferralDate { get; set; } = string.Empty;
        public string ReferralTo { get; set; } = string.Empty;
        public string ReferralFrom { get; set; } = string.Empty;
        public string ReferralPhone { get; set; } = string.Empty;

        // Medical information
        public string Medication { get; set; } = string.Empty;
        public string Physician { get; set; } = string.Empty;
        public string Audiologist { get; set; } = string.Empty;

        // Computed display helpers
        public string Initials => $"{(string.IsNullOrEmpty(FirstName) ? "" : FirstName[0])}{(string.IsNullOrEmpty(LastName) ? "" : LastName[0])}";
        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}