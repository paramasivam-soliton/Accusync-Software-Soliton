// --------------------------------------------------------------------------------
// <copyright file="ITestRepository.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using AccuSync.Core.Entities.Patients;

namespace AccuSync.Core.Abstractions.Repositories
{
    /// <summary>
    /// Persistence contract for test sessions and test records. Implemented by AccuSync.EF
    /// (EF Core, SQLite-backed, PatientDatabase.db).
    /// </summary>
    public interface ITestRepository
    {
        Task<List<TestRecord>> GetTestsForPatientAsync(int patientId);

        /// <summary>Hard delete — TestRecords has no soft-delete column in this phase.</summary>
        Task<bool> DeleteTestRecordAsync(int testRecordId);

        /// <summary>Returns false if either the test record or the target patient does not exist.</summary>
        Task<bool> ReassignTestRecordAsync(int testRecordId, int newPatientId);
    }
}
