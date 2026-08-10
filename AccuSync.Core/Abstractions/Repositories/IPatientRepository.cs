// --------------------------------------------------------------------------------
// <copyright file="IPatientRepository.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Threading.Tasks;
using AccuSync.Core.Abstractions;
using AccuSync.Core.Entities.Patients;

namespace AccuSync.Core.Abstractions.Repositories
{
    /// <summary>
    /// Persistence contract for patient records, contacts, and risk factor values.
    /// Implemented by AccuSync.EF (EF Core, SQLite-backed, PatientDatabase.db).
    /// </summary>
    public interface IPatientRepository
    {
        Task InitializeDatabaseAsync();

        /// <summary>
        /// Searches by patient record number, hospital ID, or the "Patient"-type contact's
        /// first/last name. Excludes soft-deleted patients unless <paramref name="includeDeleted"/>
        /// is true.
        /// </summary>
        Task<PagedResult<Patient>> GetPagedAsync(int pageNumber, int pageSize, string searchTerm = null, bool includeDeleted = false);

        /// <summary>
        /// Returns the patient including its contacts, risk factor values, and test
        /// sessions/records. Returns null if no patient with this id exists.
        /// </summary>
        Task<Patient> GetByIdAsync(int patientId);

        Task<bool> CreateAsync(Patient patient);

        /// <summary>
        /// Updates the patient's own scalar fields. Contacts and risk factor values are
        /// managed separately, not synchronized by this call.
        /// </summary>
        Task<bool> UpdateAsync(Patient patient);

        Task<bool> SoftDeleteAsync(int patientId);
    }
}
