// --------------------------------------------------------------------------------
// <copyright file="TestRepository.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AccuSync.Core.Abstractions.Repositories;
using AccuSync.Core.Entities.Patients;
using AccuSync.EF.Contexts;
using Microsoft.EntityFrameworkCore;

namespace AccuSync.EF
{
    /// <summary>
    /// EF Core-backed data access for test sessions and test records (PatientDatabase.db).
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
            return await _context.TestRecords.AsNoTracking()
                .Where(r => r.PatientId == patientId)
                .OrderByDescending(r => r.TestDate)
                .ToListAsync();
        }

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
            catch (Exception ex)
            {
                Debug.WriteLine($"[TestRepository] DeleteTestRecordAsync failed: {ex}");
                return false;
            }
        }

        public async Task<bool> ReassignTestRecordAsync(int testRecordId, int newPatientId)
        {
            try
            {
                var tracked = await _context.TestRecords.FindAsync(testRecordId);
                if (tracked == null) return false;

                bool patientExists = await _context.Patients.AnyAsync(p => p.PatientId == newPatientId);
                if (!patientExists) return false;

                tracked.PatientId = newPatientId;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[TestRepository] ReassignTestRecordAsync failed: {ex}");
                return false;
            }
        }
    }
}
