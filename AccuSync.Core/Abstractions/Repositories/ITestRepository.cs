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
    /// Persistence contract for test sessions/records. Implemented by AccuSync.EF.
    /// </summary>
    public interface ITestRepository
    {
        /// <summary>Returns every test record for the given patient, across all of that patient's test sessions.</summary>
        Task<List<TestRecord>> GetTestsForPatientAsync(int patientId);

        /// <summary>Permanently removes a single test record. There is no soft-delete for test records — see the HLD's Open Issues.</summary>
        Task<bool> DeleteTestRecordAsync(int testRecordId);

        /// <summary>Reassigns a test record to a different patient. Returns false if the test record or the target patient does not exist.</summary>
        Task<bool> ReassignTestRecordAsync(int testRecordId, int newPatientId);
    }
}
