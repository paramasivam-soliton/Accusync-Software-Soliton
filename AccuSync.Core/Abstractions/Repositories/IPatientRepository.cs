// --------------------------------------------------------------------------------
// <copyright file="IPatientRepository.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Threading.Tasks;
using AccuSync.Core.Entities.Patients;

namespace AccuSync.Core.Abstractions.Repositories
{
    /// <summary>
    /// Persistence contract for patients, their contacts, and their risk-factor values.
    /// Implemented by AccuSync.EF (EF Core, SQLite-backed). Follows the same conventions
    /// as <see cref="IUserRepository"/>: <c>Task&lt;bool&gt;</c> for mutations, <c>Task&lt;T?&gt;</c>/
    /// <c>Task&lt;List&lt;T&gt;&gt;</c> for reads, no Result-wrapper type.
    /// </summary>
    public interface IPatientRepository
    {
        /// <summary>
        /// Returns one page of non-deleted patients (unless <paramref name="includeDeleted"/> is set),
        /// optionally filtered by <paramref name="searchTerm"/> against patient ID, hospital ID, or
        /// the associated "Patient" contact's name.
        /// </summary>
        Task<PagedResult<Patient>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, bool includeDeleted = false);

        /// <summary>Returns the patient with its contacts and test history, or <c>null</c> if not found.</summary>
        Task<Patient?> GetByIdAsync(int patientId);

        /// <summary>Creates a new patient record, including any contacts already attached.</summary>
        Task<bool> CreateAsync(Patient patient);

        /// <summary>Persists changes to an existing patient's scalar fields. Returns false if the patient no longer exists.</summary>
        Task<bool> UpdateAsync(Patient patient);

        /// <summary>Marks a patient as deleted (<see cref="Patient.IsDeleted"/>) without removing the row. Returns false if the patient no longer exists.</summary>
        Task<bool> SoftDeleteAsync(int patientId);
    }
}
