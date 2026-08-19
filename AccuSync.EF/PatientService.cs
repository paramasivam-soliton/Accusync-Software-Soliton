// --------------------------------------------------------------------------------
// <copyright file="PatientService.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Threading.Tasks;
using AccuSync.Core.Abstractions;
using AccuSync.Core.Abstractions.Repositories;
using AccuSync.Core.Abstractions.Services;
using AccuSync.Core.Entities.Patients;

namespace AccuSync.EF
{
    /// <summary>
    /// Thin pass-through implementation of <see cref="IPatientService"/> over
    /// <see cref="IPatientRepository"/>. Intentionally has no logic of its own yet — its only
    /// job right now is to be the thing screens depend on instead of the repository directly.
    /// </summary>
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _patientRepository;

        public PatientService(IPatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }

        public Task<PagedResult<Patient>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, bool includeDeleted = false) =>
            _patientRepository.GetPagedAsync(pageNumber, pageSize, searchTerm, includeDeleted);

        public Task<Patient?> GetByIdAsync(int patientId) => _patientRepository.GetByIdAsync(patientId);

        public Task<bool> CreateAsync(Patient patient) => _patientRepository.CreateAsync(patient);

        public Task<bool> UpdateAsync(Patient patient) => _patientRepository.UpdateAsync(patient);

        public Task<bool> SoftDeleteAsync(int patientId) => _patientRepository.SoftDeleteAsync(patientId);
    }
}
