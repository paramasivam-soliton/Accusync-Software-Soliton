// --------------------------------------------------------------------------------
// <copyright file="TestRepository.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccuSync.Core.Abstractions.Repositories;
using AccuSync.Core.Entities.Patients;
using AccuSync.EF.Contexts;
using Microsoft.EntityFrameworkCore;

namespace AccuSync.EF
{
    /// <summary>
    /// EF Core-backed data access for test sessions/records (PatientDatabase.db).
    /// </summary>
    public class TestRepository : ITestRepository
    {
        private readonly PatientDbContext _context;

        public TestRepository(PatientDbContext context)
        {
            _context = context;
        }

        public async Task<List<TestRecord>> GetTestsForPatientAsync(int patientId)
        {
            return await _context.TestRecords
                .AsNoTracking()
                .Where(r => r.PatientId == patientId)
                .ToListAsync();
        }

        /// <summary>Hard delete — GID-254895. TestRecords has no IsDeleted column; see the HLD's Open Issues.</summary>
        public async Task<bool> DeleteTestRecordAsync(int testRecordId)
        {
            try
            {
                var tracked = await _context.TestRecords.FindAsync(testRecordId);
                if (tracked == null) return false;

                _context.TestRecords.Remove(tracked);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>GID-254896. Fails if either the test record or the target patient does not exist.</summary>
        public async Task<bool> ReassignTestRecordAsync(int testRecordId, int newPatientId)
        {
            try
            {
                var tracked = await _context.TestRecords.FindAsync(testRecordId);
                if (tracked == null) return false;

                bool targetPatientExists = await _context.Patients.AnyAsync(p => p.PatientId == newPatientId);
                if (!targetPatientExists) return false;

                tracked.PatientId = newPatientId;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
