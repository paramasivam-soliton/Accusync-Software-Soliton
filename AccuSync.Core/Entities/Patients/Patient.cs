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
    /// Persistence entity for the Patients table (PatientDatabase.db). Demographic name
    /// fields live on <see cref="PatientContact"/> (ContactType == "Patient"), not here —
    /// matches the target schema, where Patients holds clinical/administrative data only.
    /// SiteId/AssignedUserId reference SettingsDatabase and are not database-enforced
    /// foreign keys (SQLite cannot FK across database files); validate at the repository
    /// layer.
    /// </summary>
    public class Patient : IAuditableEntity
    {
        public int PatientId { get; set; }
        public string? SourceId { get; set; }
        public int? ImportBatchId { get; set; }

        public int? SiteId { get; set; }
        public int? AssignedUserId { get; set; }

        public string? PatientRecordNumber { get; set; }
        public string? HospitalId { get; set; }

        public string? NicuStatus { get; set; }
        public DateTime? Discharged { get; set; }
        public bool? Deceased { get; set; }
        public string? Medication { get; set; }

        public string? ConsentState { get; set; }
        public string? TrackingConsent { get; set; }
        public string? ScreeningConsent { get; set; }

        public int? GestationalAge { get; set; }
        public string? RaceReferenceId { get; set; }

        public string? ReferralFrom { get; set; }
        public string? ReferralTo { get; set; }
        public string? ReferralPhone { get; set; }

        public string? FreeText1 { get; set; }
        public string? FreeText2 { get; set; }
        public string? FreeText3 { get; set; }

        public string? FreeField1Value { get; set; }
        public string? FreeField2Value { get; set; }
        public string? FreeField3Value { get; set; }
        public string? FreeField4Value { get; set; }

        public string? PredefinedComments { get; set; }

        public bool IsDeleted { get; set; }
        public bool IsExported { get; set; }
        public DateTime? ExportedAt { get; set; }

        public string? SourceCreatedAt { get; set; }
        public string? SourceModifiedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }

        public List<PatientContact> Contacts { get; set; } = new List<PatientContact>();
        public List<TestSession> TestSessions { get; set; } = new List<TestSession>();
        public List<PatientRiskFactorValue> RiskFactorValues { get; set; } = new List<PatientRiskFactorValue>();
    }
}
