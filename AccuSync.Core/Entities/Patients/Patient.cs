// --------------------------------------------------------------------------------
// <copyright file="Patient.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace AccuSync.Core.Entities.Patients
{
    /// <summary>
    /// EF entity for the <c>Patients</c> table (Databases/PatientDatabase.sql). Name/DOB/address
    /// fields live on <see cref="PatientContact"/> (ContactType = "Patient"), not here.
    /// Risk factors (<see cref="PatientRiskFactors"/>) and comments (<see cref="PredefinedComments"/>)
    /// use the schema's existing columns as-is — no redesign in this story.
    /// </summary>
    public class Patient : IAuditableEntity
    {
        public int PatientId { get; set; }

        /// <summary>GUID from the source device's <c>&lt;Patient&gt;&lt;Id&gt;</c>, used for import dedup.</summary>
        public string? SourceId { get; set; }

        /// <summary>Not modeled with an EF navigation — <c>ImportBatches</c> is out of scope for this story.</summary>
        public int? ImportBatchId { get; set; }

        /// <summary>Cross-database reference — [XDB: SettingsDatabase.Sites.SiteId]. No FK; validated at the repository layer.</summary>
        public int? SiteId { get; set; }

        /// <summary>Cross-database reference — [XDB: SettingsDatabase.Users.UserId]. No FK; validated at the repository layer.</summary>
        public int? AssignedUserId { get; set; }

        public string? PatientRecordNumber { get; set; }
        public string? HospitalId { get; set; }

        /// <summary>'Yes' | 'No' | 'Unknown'.</summary>
        public string? NicuStatus { get; set; }
        public DateTime? Discharged { get; set; }

        /// <summary>1 = true, 0 = false in the schema; mapped to a nullable bool here.</summary>
        public bool? Deceased { get; set; }
        public string? Medication { get; set; }

        /// <summary>'Full' | 'Partial' | 'None'.</summary>
        public string? ConsentState { get; set; }
        public string? TrackingConsent { get; set; }

        /// <summary>'Yes' | 'No'.</summary>
        public string? ScreeningConsent { get; set; }

        /// <summary>Weeks.</summary>
        public int? GestationalAge { get; set; }
        public string? RaceReferenceId { get; set; }

        /// <summary>JSON array of RiskFactor.Code values — carried over unchanged from the existing schema.</summary>
        public string? PatientRiskFactors { get; set; }

        public string? ReferralFrom { get; set; }
        public string? ReferralTo { get; set; }
        public string? ReferralPhone { get; set; }

        public string? FreeText1 { get; set; }
        public string? FreeText2 { get; set; }
        public string? FreeText3 { get; set; }

        /// <summary>Labels for FreeField1-4 are configured in SettingsDatabase.FieldSetup.</summary>
        public string? FreeField1Value { get; set; }
        public string? FreeField2Value { get; set; }
        public string? FreeField3Value { get; set; }
        public string? FreeField4Value { get; set; }

        public string? PredefinedComments { get; set; }

        public bool IsDeleted { get; set; }
        public bool IsExported { get; set; }
        public DateTime? ExportedAt { get; set; }

        public DateTime? SourceCreatedAt { get; set; }
        public DateTime? SourceModifiedAt { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }

        public List<PatientContact> Contacts { get; set; } = new();
        public List<TestSession> TestSessions { get; set; } = new();
    }
}
