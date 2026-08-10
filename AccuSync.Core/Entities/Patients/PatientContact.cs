// --------------------------------------------------------------------------------
// <copyright file="PatientContact.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;

namespace AccuSync.Core.Entities.Patients
{
    /// <summary>
    /// Persistence entity for the PatientContacts table. One row per contact per patient —
    /// ContactType distinguishes "Patient" (the patient's own name/demographics), "Mother",
    /// "Father", and "Caregiver" rows.
    /// </summary>
    public class PatientContact : IAuditableEntity
    {
        public int ContactId { get; set; }
        public int PatientId { get; set; }
        public string? SourceId { get; set; }
        public string ContactType { get; set; } = string.Empty;

        public string? Title { get; set; }
        public string? Forename1 { get; set; }
        public string? Forename2 { get; set; }
        public string? Surname { get; set; }

        public string? SocialSecurityNumber { get; set; }
        public string? IdNumber { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public DateTime? CalculatedDateOfBirth { get; set; }
        public string? Gender { get; set; }
        public double? Height { get; set; }
        public double? Weight { get; set; }
        public string? BirthLocation { get; set; }
        public string? LanguageCode { get; set; }
        public string? NationalityCode { get; set; }

        public string? Address1 { get; set; }
        public string? Zip { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }

        public string? Phone { get; set; }
        public string? CellPhone { get; set; }
        public string? Fax { get; set; }
        public string? Email { get; set; }

        public string? SourceCreatedAt { get; set; }
        public string? SourceModifiedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
