// --------------------------------------------------------------------------------
// <copyright file="PatientContact.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;

namespace AccuSync.Core.Entities.Patients
{
    /// <summary>
    /// EF entity for the <c>PatientContacts</c> table. One row per contact per patient —
    /// ContactType: 'Patient' | 'Mother' | 'Father' | 'Caregiver'. Name/DOB/address fields
    /// live here rather than on <see cref="Patient"/> itself.
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
        public DateTime? TimeOfBirth { get; set; }
        public string? Gender { get; set; }
        public double? Height { get; set; }
        public double? Weight { get; set; }
        public string? BirthLocation { get; set; }
        public string? LanguageCode { get; set; }
        public string? NationalityCode { get; set; }

        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? Zip { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }

        public string? Phone { get; set; }
        public string? CellPhone { get; set; }
        public string? Fax { get; set; }
        public string? Email { get; set; }

        public DateTime? SourceCreatedAt { get; set; }
        public DateTime? SourceModifiedAt { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }

        /// <summary>Set when the parent <see cref="Patient"/> is soft-deleted — not independently settable elsewhere.</summary>
        public bool IsDeleted { get; set; }

        public Patient? Patient { get; set; }

        /// <summary>
        /// Deliberately excludes <see cref="SocialSecurityNumber"/> — a guard against it ever
        /// reaching a log line through string interpolation (e.g. <c>$"{contact}"</c>) or a
        /// message-template logger call. Does not protect against a serializer that reflects
        /// over every public property instead of calling this method.
        /// </summary>
        public override string ToString() => $"PatientContact(ContactId={ContactId}, PatientId={PatientId}, ContactType={ContactType})";
    }
}
