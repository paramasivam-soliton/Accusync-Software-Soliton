// --------------------------------------------------------------------------------
// <copyright file="IPatientService.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Threading.Tasks;
using AccuSync.Core.Entities.Patients;

namespace AccuSync.Core.Abstractions.Services
{
    /// <summary>
    /// The layer screens talk to for patient data — sits above
    /// <see cref="Repositories.IPatientRepository"/> so no screen depends on the persistence
    /// layer directly. Mirrors the repository's method shapes for now; grows real
    /// orchestration/validation only once something actually needs it.
    /// </summary>
    public interface IPatientService
    {
        /// <summary>Returns one page of non-deleted patients (unless <paramref name="includeDeleted"/> is set), optionally filtered by <paramref name="searchTerm"/>.</summary>
        Task<PagedResult<Patient>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, bool includeDeleted = false);

        /// <summary>Returns the patient with its contacts and test history, or <c>null</c> if not found or soft-deleted.</summary>
        Task<Patient?> GetByIdAsync(int patientId);

        /// <summary>Creates a new patient record, including any contacts already attached.</summary>
        Task<bool> CreateAsync(Patient patient);

        /// <summary>Persists changes to an existing patient's scalar fields. Returns false if the patient no longer exists.</summary>
        Task<bool> UpdateAsync(Patient patient);

        /// <summary>Marks a patient and all of its contacts as deleted, without removing any rows. Returns false if the patient no longer exists.</summary>
        Task<bool> SoftDeleteAsync(int patientId);
    }
}
