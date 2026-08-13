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
    /// history involved), mirroring UserRepositoryTests' style.
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

        private static Patient NewPatient(string recordNumber, string firstName, string lastName) => new()
        {
            PatientRecordNumber = recordNumber,
            HospitalId = "HOSP-001",
            Contacts = new List<PatientContact>
            {
                new() { ContactType = "Patient", Forename1 = firstName, Surname = lastName }
            }
        };

        [Fact]
        public async Task GivenANewPatient_WhenCreated_ThenItCanBeReadBackById()
        {
            // Arrange
            var patient = NewPatient("REC-1", "John", "Smith");

            // Act
            bool success = await _sut.CreateAsync(patient);

            // Assert
            Assert.True(success);
            var result = await _sut.GetByIdAsync(patient.PatientId);
            Assert.NotNull(result);
            Assert.Equal("REC-1", result!.PatientRecordNumber);
            Assert.Single(result.Contacts);
            Assert.Equal("John", result.Contacts[0].Forename1);
        }

        [Fact]
        public async Task GivenAPatientWithContactsAndTests_WhenReadById_ThenContactsAndTestHistoryAreIncluded()
        {
            // Arrange
            var patient = NewPatient("REC-2", "Jane", "Doe");
            patient.TestSessions.Add(new TestSession
            {
                SessionDate = new DateTime(2026, 1, 1),
                TestRecords = new List<TestRecord>
                {
                    new() { TestType = "TEOAE", TestObject = "Left Ear", TestDate = new DateTime(2026, 1, 1) }
                }
            });
            await _sut.CreateAsync(patient);

            // Act
            var result = await _sut.GetByIdAsync(patient.PatientId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result!.Contacts);
            Assert.Single(result.TestSessions);
            Assert.Single(result.TestSessions[0].TestRecords);
        }

        [Fact]
        public async Task GivenAPatientIdThatDoesNotExist_WhenReadById_ThenReturnsNull()
        {
            // Act
            var result = await _sut.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GivenAnExistingPatient_WhenUpdated_ThenTheChangeIsPersisted()
        {
            // Arrange
            var patient = NewPatient("REC-3", "Bob", "Lee");
            await _sut.CreateAsync(patient);
            var toUpdate = await _sut.GetByIdAsync(patient.PatientId);
            toUpdate!.HospitalId = "HOSP-999";

            // Act
            bool success = await _sut.UpdateAsync(toUpdate);

            // Assert
            Assert.True(success);
            var result = await _sut.GetByIdAsync(patient.PatientId);
            Assert.Equal("HOSP-999", result!.HospitalId);
        }

        [Fact]
        public async Task GivenAPatientIdThatDoesNotExist_WhenUpdated_ThenReturnsFalse()
        {
            // Arrange
            var nonExistent = new Patient { PatientId = 999, HospitalId = "X" };

            // Act
            bool success = await _sut.UpdateAsync(nonExistent);

            // Assert
            Assert.False(success);
        }

        [Fact]
        public async Task GivenAnActivePatient_WhenSoftDeleted_ThenItIsExcludedFromTheDefaultPagedList()
        {
            // Arrange
            var patient = NewPatient("REC-4", "Amy", "Chen");
            await _sut.CreateAsync(patient);

            // Act
            bool success = await _sut.SoftDeleteAsync(patient.PatientId);

            // Assert
            Assert.True(success);
            var defaultList = await _sut.GetPagedAsync(pageNumber: 1, pageSize: 10);
            Assert.DoesNotContain(defaultList.Items, p => p.PatientId == patient.PatientId);
        }

        [Fact]
        public async Task GivenASoftDeletedPatient_WhenIncludeDeletedIsTrue_ThenItStillAppearsInThePagedList()
        {
            // Arrange
            var patient = NewPatient("REC-5", "Tom", "Reed");
            await _sut.CreateAsync(patient);
            await _sut.SoftDeleteAsync(patient.PatientId);

            // Act
            var result = await _sut.GetPagedAsync(pageNumber: 1, pageSize: 10, includeDeleted: true);

            // Assert
            Assert.Contains(result.Items, p => p.PatientId == patient.PatientId);
        }

        [Fact]
        public async Task GivenAPatientIdThatDoesNotExist_WhenSoftDeleted_ThenReturnsFalse()
        {
            // Act
            bool success = await _sut.SoftDeleteAsync(999);

            // Assert
            Assert.False(success);
        }

        [Fact]
        public async Task GivenPatientsWithDifferentSurnames_WhenSearchedBySurname_ThenOnlyTheMatchingPatientIsReturned()
        {
            // Arrange
            await _sut.CreateAsync(NewPatient("REC-6", "Alice", "Walker"));
            await _sut.CreateAsync(NewPatient("REC-7", "Zack", "Nguyen"));

            // Act
            var result = await _sut.GetPagedAsync(pageNumber: 1, pageSize: 10, searchTerm: "Walker");

            // Assert
            Assert.Single(result.Items);
            Assert.Equal("Walker", result.Items[0].Contacts[0].Surname);
        }

        [Fact]
        public async Task GivenMorePatientsThanFitOnOnePage_WhenPaged_ThenTotalCountReflectsAllMatchesNotJustThePage()
        {
            // Arrange
            for (int i = 0; i < 5; i++)
            {
                await _sut.CreateAsync(NewPatient($"REC-{i}", $"First{i}", "Same"));
            }

            // Act
            var result = await _sut.GetPagedAsync(pageNumber: 1, pageSize: 2);

            // Assert
            Assert.Equal(2, result.Items.Count);
            Assert.Equal(5, result.TotalCount);
        }

        [Fact]
        public async Task GivenANewPatient_WhenSavedWithTheRealInterceptorWired_ThenCreatedAtAndModifiedAtAreSetAutomatically()
        {
            // Arrange — a separate context with TimestampInterceptor actually attached,
            // since the shared fixture's _context does not wire one.
            using var connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();
            var options = new DbContextOptionsBuilder<PatientDbContext>()
                .UseSqlite(connection)
                .AddInterceptors(new TimestampInterceptor())
                .Options;
            using var context = new PatientDbContext(options);
            context.Database.EnsureCreated();
            var repository = new PatientRepository(context);
            var patient = NewPatient("REC-8", "Nora", "Kim");

            // Act
            await repository.CreateAsync(patient);

            // Assert
            var result = await repository.GetByIdAsync(patient.PatientId);
            Assert.NotEqual(default, result!.CreatedAt);
            Assert.NotEqual(default, result.ModifiedAt);
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Dispose();
        }
    }
}
