// --------------------------------------------------------------------------------
// <copyright file="PatientRepository.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

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
    /// EF Core-backed data access for patients (PatientDatabase.db).
    /// </summary>
    public class PatientRepository : IPatientRepository
    {
        private readonly PatientDbContext _context;

        public PatientRepository(PatientDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Searches patient ID/hospital ID and, for the associated "Patient" contact row,
        /// first/last name — per GID-254887. Date-of-birth/date-of-test filtering is left
        /// for whichever consumer needs it, as a separate optional parameter.
        /// </summary>
        public async Task<PagedResult<Patient>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, bool includeDeleted = false)
        {
            var query = _context.Patients.Include(p => p.Contacts).AsNoTracking().AsQueryable();

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

        public async Task<Patient?> GetByIdAsync(int patientId)
        {
            return await _context.Patients
                .Include(p => p.Contacts)
                .Include(p => p.TestSessions).ThenInclude(s => s.TestRecords)
                .AsNoTracking()
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
            catch
            {
                return false;
            }
        }

        /// <summary>Updates the patient's own scalar fields. Contacts/test history are not touched here.</summary>
        public async Task<bool> UpdateAsync(Patient patient)
        {
            try
            {
                var tracked = await _context.Patients.FindAsync(patient.PatientId);
                if (tracked == null) return false;

                tracked.SourceId = patient.SourceId;
                tracked.ImportBatchId = patient.ImportBatchId;
                tracked.SiteId = patient.SiteId;
                tracked.AssignedUserId = patient.AssignedUserId;
                tracked.PatientRecordNumber = patient.PatientRecordNumber;
                tracked.HospitalId = patient.HospitalId;
                tracked.NicuStatus = patient.NicuStatus;
                tracked.Discharged = patient.Discharged;
                tracked.Deceased = patient.Deceased;
                tracked.Medication = patient.Medication;
                tracked.ConsentState = patient.ConsentState;
                tracked.TrackingConsent = patient.TrackingConsent;
                tracked.ScreeningConsent = patient.ScreeningConsent;
                tracked.GestationalAge = patient.GestationalAge;
                tracked.RaceReferenceId = patient.RaceReferenceId;
                tracked.PatientRiskFactors = patient.PatientRiskFactors;
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
                tracked.IsDeleted = patient.IsDeleted;
                tracked.IsExported = patient.IsExported;
                tracked.ExportedAt = patient.ExportedAt;
                tracked.SourceCreatedAt = patient.SourceCreatedAt;
                tracked.SourceModifiedAt = patient.SourceModifiedAt;
                // PatientId/CreatedAt/ModifiedAt are never overwritten here — ModifiedAt is
                // set by TimestampInterceptor, not by the caller.

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
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
            catch
            {
                return false;
            }
        }
    }
}
