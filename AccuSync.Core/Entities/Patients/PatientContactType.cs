// --------------------------------------------------------------------------------
// <copyright file="PatientContactType.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace AccuSync.Core.Entities.Patients
{
    /// <summary>
    /// Well-known <see cref="PatientContact.ContactType"/> values. A typo in a raw string
    /// literal (e.g. "caregiver" instead of "Caregiver") compiles fine and silently fails to
    /// match the existing row, producing a duplicate contact instead of updating one — these
    /// constants turn that into a compile error instead.
    /// </summary>
    public static class PatientContactType
    {
        public const string Patient = "Patient";
        public const string Mother = "Mother";
        public const string Caregiver = "Caregiver";
    }
}
