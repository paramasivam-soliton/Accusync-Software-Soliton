// --------------------------------------------------------------------------------
// <copyright file="PatientRepository.test.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Core.Entities.Patients;
using AccuSync.EF;
using AccuSync.EF.Contexts;
using AccuSync.EF.Interceptors;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AccuSync.EF.Tests.Repositories
{
    /// <summary>
    /// Runs against a private, per-test Sqlite database, open only in memory for the
    /// lifetime of the test (schema created directly from the EF model — no migration
    /// history involved), mirroring UserRepositoryTests.
    /// </summary>
    public class PatientRepositoryTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly PatientDbContext _context;
        private readonly PatientRepository _sut;

        public PatientRepositoryTests()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<PatientDbContext>()
                .UseSqlite(_connection)
                .Options;
            _context = new PatientDbContext(options);
            _context.Database.EnsureCreated();

            _sut = new PatientRepository(_context);
        }

        private static Patient NewPatient(string firstName, string lastName, string hospitalId = "HOSP-001") => new()
        {
            HospitalId = hospitalId,
            PatientRecordNumber = "REC-" + Guid.NewGuid().ToString("N")[..8],
            Contacts = new List<PatientContact>
            {
                new PatientContact
                {
                    ContactType = "Patient",
                    Forename1 = firstName,
                    Surname = lastName,
                    DateOfBirth = new DateTime(2026, 1, 1)
                }
            }
        };

        [Fact]
        public async Task CreateAsync_GivenANewPatient_WhenCreated_ThenCanBeReadBackById()
        {
            // Arrange
            var patient = NewPatient("John", "Smith");

            // Act
            bool success = await _sut.CreateAsync(patient);

            // Assert
            Assert.True(success);
            var result = await _sut.GetByIdAsync(patient.PatientId);
            Assert.NotNull(result);
            Assert.Equal("HOSP-001", result!.HospitalId);
            Assert.Contains(result.Contacts, c => c.ContactType == "Patient" && c.Forename1 == "John" && c.Surname == "Smith");
        }

        [Fact]
        public async Task GetByIdAsync_GivenAPatientWithContactsAndTests_WhenRetrieved_ThenIncludesBoth()
        {
            // Arrange
            var patient = NewPatient("Sarah", "Johnson");
            patient.TestSessions.Add(new TestSession
            {
                SessionDate = new DateTime(2026, 1, 17),
                TestRecords = new List<TestRecord>
                {
                    new TestRecord { TestType = "TEOAE", TestObject = "Right Ear", TestResult = "Pass", TestDate = new DateTime(2026, 1, 17) }
                }
            });
            await _sut.CreateAsync(patient);

            // Act
            var result = await _sut.GetByIdAsync(patient.PatientId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result!.Contacts);
            Assert.Single(result.TestSessions);
            Assert.Single(result.TestSessions.Single().TestRecords);
            Assert.Equal("TEOAE", result.TestSessions.Single().TestRecords.Single().TestType);
        }

        [Fact]
        public async Task GetByIdAsync_GivenAPatientIdThatDoesNotExist_WhenRetrieved_ThenReturnsNull()
        {
            // Act
            var result = await _sut.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateAsync_GivenAnExistingPatient_WhenScalarFieldsChanged_ThenPersisted()
        {
            // Arrange
            var patient = NewPatient("Michael", "Williams");
            await _sut.CreateAsync(patient);

            // Act
            patient.HospitalId = "HOSP-999";
            patient.ScreeningConsent = "Yes";
            bool success = await _sut.UpdateAsync(patient);

            // Assert
            Assert.True(success);
            var result = await _sut.GetByIdAsync(patient.PatientId);
            Assert.Equal("HOSP-999", result!.HospitalId);
            Assert.Equal("Yes", result.ScreeningConsent);
        }

        [Fact]
        public async Task UpdateAsync_GivenAPatientIdThatDoesNotExist_WhenUpdated_ThenReturnsFalse()
        {
            // Act
            bool success = await _sut.UpdateAsync(new Patient { PatientId = 999 });

            // Assert
            Assert.False(success);
        }

        [Fact]
        public async Task SoftDeleteAsync_GivenAnExistingPatient_WhenDeleted_ThenExcludedFromTheDefaultPagedListQuery()
        {
            // Arrange
            var patient = NewPatient("David", "Jones");
            await _sut.CreateAsync(patient);

            // Act
            bool success = await _sut.SoftDeleteAsync(patient.PatientId);

            // Assert — the full round-trip: create -> read back -> update -> soft-delete -> excluded.
            Assert.True(success);
            var defaultList = await _sut.GetPagedAsync(pageNumber: 1, pageSize: 50);
            Assert.DoesNotContain(defaultList.Items, p => p.PatientId == patient.PatientId);
        }

        [Fact]
        public async Task SoftDeleteAsync_GivenAnExistingPatient_WhenDeletedAndIncludeDeletedIsTrue_ThenStillReturned()
        {
            // Arrange
            var patient = NewPatient("Jessica", "Garcia");
            await _sut.CreateAsync(patient);
            await _sut.SoftDeleteAsync(patient.PatientId);

            // Act
            var listIncludingDeleted = await _sut.GetPagedAsync(pageNumber: 1, pageSize: 50, includeDeleted: true);

            // Assert
            Assert.Contains(listIncludingDeleted.Items, p => p.PatientId == patient.PatientId);
        }

        [Fact]
        public async Task SoftDeleteAsync_GivenAPatientIdThatDoesNotExist_WhenDeleted_ThenReturnsFalse()
        {
            // Act
            bool success = await _sut.SoftDeleteAsync(999);

            // Assert
            Assert.False(success);
        }

        [Fact]
        public async Task GetPagedAsync_GivenASearchTermMatchingAContactSurname_WhenSearched_ThenReturnsOnlyThatPatient()
        {
            // Arrange
            await _sut.CreateAsync(NewPatient("John", "Smith"));
            await _sut.CreateAsync(NewPatient("Sarah", "Johnson"));

            // Act
            var result = await _sut.GetPagedAsync(pageNumber: 1, pageSize: 50, searchTerm: "Johnson");

            // Assert
            Assert.Equal(1, result.TotalCount);
            Assert.Equal("Johnson", result.Items.Single().Contacts.Single().Surname);
        }

        [Fact]
        public async Task GetPagedAsync_GivenMultiplePatients_WhenPaged_ThenTotalCountReflectsAllMatchesNotJustThePage()
        {
            // Arrange
            for (int i = 0; i < 5; i++)
            {
                await _sut.CreateAsync(NewPatient($"First{i}", $"Last{i}"));
            }

            // Act
            var result = await _sut.GetPagedAsync(pageNumber: 1, pageSize: 2);

            // Assert
            Assert.Equal(2, result.Items.Count);
            Assert.Equal(5, result.TotalCount);
        }

        [Fact]
        public async Task CreateAsync_GivenAPatientWithTriStateRiskFactorValues_WhenReadBack_ThenAllThreeDistinctValuesArePreserved()
        {
            // Arrange — this is the gap the earlier JSON-array-of-codes design couldn't cover:
            // Yes/No/Unknown must each be distinguishable, not just "present" vs. "absent".
            var patient = NewPatient("Amanda", "Martinez");
            patient.RiskFactorValues.Add(new PatientRiskFactorValue { RiskFactorId = 1, Value = "Yes" });
            patient.RiskFactorValues.Add(new PatientRiskFactorValue { RiskFactorId = 2, Value = "No" });
            patient.RiskFactorValues.Add(new PatientRiskFactorValue { RiskFactorId = 3, Value = "Unknown" });

            // Act
            await _sut.CreateAsync(patient);
            var result = await _sut.GetByIdAsync(patient.PatientId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result!.RiskFactorValues.Count);
            Assert.Equal("Yes", result.RiskFactorValues.Single(v => v.RiskFactorId == 1).Value);
            Assert.Equal("No", result.RiskFactorValues.Single(v => v.RiskFactorId == 2).Value);
            Assert.Equal("Unknown", result.RiskFactorValues.Single(v => v.RiskFactorId == 3).Value);
        }

        [Fact]
        public async Task CreateAsync_GivenANewPatient_WhenSavedThroughAContextWithTimestampInterceptorAttached_ThenCreatedAtAndModifiedAtAreSetAutomatically()
        {
            // Arrange — a separate context/repository instance with the interceptor attached,
            // exactly as AddSqlitePersistence wires it for the real app.
            var options = new DbContextOptionsBuilder<PatientDbContext>()
                .UseSqlite(_connection)
                .AddInterceptors(new TimestampInterceptor())
                .Options;
            using var interceptedContext = new PatientDbContext(options);
            var interceptedRepository = new PatientRepository(interceptedContext);
            var patient = NewPatient("Daniel", "Miller");

            // Act
            await interceptedRepository.CreateAsync(patient);

            // Assert
            Assert.NotEqual(default, patient.CreatedAt);
            Assert.NotEqual(default, patient.ModifiedAt);
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Dispose();
        }
    }
}
