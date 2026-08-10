// --------------------------------------------------------------------------------
// <copyright file="PatientRepository.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AccuSync.Core.Abstractions;
using AccuSync.Core.Abstractions.Repositories;
using AccuSync.Core.Entities.Patients;
using AccuSync.EF.Contexts;
using Microsoft.EntityFrameworkCore;

namespace AccuSync.EF
{
    /// <summary>
    /// EF Core-backed data access for patient records (PatientDatabase.db). Follows the same
    /// backup-then-migrate-with-rollback initialization as <c>UserRepository</c>.
    /// </summary>
    public class PatientRepository : IPatientRepository
    {
        private readonly PatientDbContext _context;

        public PatientRepository(PatientDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Applies pending migrations — backing up the database file first and restoring it
        /// if the migration fails. Safe to call on every startup.
        /// </summary>
        public async Task InitializeDatabaseAsync()
        {
            string databasePath = _context.Database.GetDbConnection().DataSource;
            bool databaseExisted = File.Exists(databasePath);
            string backupPath = databasePath + ".bak";

            if (databaseExisted)
            {
                File.Copy(databasePath, backupPath, overwrite: true);
            }

            try
            {
                // See UserRepository.InitializeDatabaseAsync for why clearing a stale
                // migrations lock row is safe and necessary for this single-instance app.
                await ClearStaleMigrationsLockAsync();
                await _context.Database.MigrateAsync();
            }
            catch
            {
                if (databaseExisted)
                {
                    File.Copy(backupPath, databasePath, overwrite: true);
                }

                throw;
            }
        }

        private async Task ClearStaleMigrationsLockAsync()
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"__EFMigrationsLock\";");
            }
            catch (DbException)
            {
                // Table doesn't exist yet — this is the very first run, nothing to clear.
            }
        }

        public async Task<PagedResult<Patient>> GetPagedAsync(int pageNumber, int pageSize, string searchTerm = null, bool includeDeleted = false)
        {
            var query = _context.Patients.AsNoTracking().Include(p => p.Contacts).AsQueryable();

            if (!includeDeleted)
            {
                query = query.Where(p => !p.IsDeleted);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p =>
                    (p.PatientRecordNumber != null && p.PatientRecordNumber.Contains(searchTerm)) ||
                    (p.HospitalId != null && p.HospitalId.Contains(searchTerm)) ||
                    p.Contacts.Any(c => c.ContactType == "Patient" &&
                        ((c.Forename1 != null && c.Forename1.Contains(searchTerm)) ||
                         (c.Surname != null && c.Surname.Contains(searchTerm)))));
            }

            int totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(p => p.PatientId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Patient> { Items = items, TotalCount = totalCount };
        }

        public async Task<Patient> GetByIdAsync(int patientId)
        {
            return await _context.Patients
                .Include(p => p.Contacts)
                .Include(p => p.RiskFactorValues)
                .Include(p => p.TestSessions).ThenInclude(s => s.TestRecords)
                .FirstOrDefaultAsync(p => p.PatientId == patientId);
        }

        public async Task<bool> CreateAsync(Patient patient)
        {
            try
            {
                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PatientRepository] CreateAsync failed: {ex}");
                return false;
            }
        }

        public async Task<bool> UpdateAsync(Patient patient)
        {
            try
            {
                var tracked = await _context.Patients.FindAsync(patient.PatientId);
                if (tracked == null) return false;

                tracked.HospitalId = patient.HospitalId;
                tracked.PatientRecordNumber = patient.PatientRecordNumber;
                tracked.SiteId = patient.SiteId;
                tracked.AssignedUserId = patient.AssignedUserId;
                tracked.NicuStatus = patient.NicuStatus;
                tracked.Discharged = patient.Discharged;
                tracked.Deceased = patient.Deceased;
                tracked.Medication = patient.Medication;
                tracked.ConsentState = patient.ConsentState;
                tracked.TrackingConsent = patient.TrackingConsent;
                tracked.ScreeningConsent = patient.ScreeningConsent;
                tracked.GestationalAge = patient.GestationalAge;
                tracked.RaceReferenceId = patient.RaceReferenceId;
                tracked.ReferralFrom = patient.ReferralFrom;
                tracked.ReferralTo = patient.ReferralTo;
                tracked.ReferralPhone = patient.ReferralPhone;
                tracked.FreeText1 = patient.FreeText1;
                tracked.FreeText2 = patient.FreeText2;
                tracked.FreeText3 = patient.FreeText3;
                tracked.FreeField1Value = patient.FreeField1Value;
                tracked.FreeField2Value = patient.FreeField2Value;
                tracked.FreeField3Value = patient.FreeField3Value;
                tracked.FreeField4Value = patient.FreeField4Value;
                tracked.PredefinedComments = patient.PredefinedComments;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PatientRepository] UpdateAsync failed: {ex}");
                return false;
            }
        }

        public async Task<bool> SoftDeleteAsync(int patientId)
        {
            try
            {
                var tracked = await _context.Patients.FindAsync(patientId);
                if (tracked == null) return false;

                tracked.IsDeleted = true;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PatientRepository] SoftDeleteAsync failed: {ex}");
                return false;
            }
        }
    }
}
