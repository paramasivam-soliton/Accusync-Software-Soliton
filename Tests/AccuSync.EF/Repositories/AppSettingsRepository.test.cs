// --------------------------------------------------------------------------------
// <copyright file="AppSettingsRepository.test.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.EF;
using AccuSync.EF.Contexts;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AccuSync.EF.Tests.Repositories
{
    /// <summary>
    /// Runs against a private, per-test Sqlite database, open only in memory for the
    /// lifetime of the test (schema created directly from the EF model). Unlike
    /// UserRepository, there's no encryption/hashing involved here.
    /// </summary>
    public class AppSettingsRepositoryTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly SettingsDbContext _context;
        private readonly AppSettingsRepository _sut;

        public AppSettingsRepositoryTests()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<SettingsDbContext>()
                .UseSqlite(_connection)
                .Options;
            _context = new SettingsDbContext(options);
            _context.Database.EnsureCreated();

            _sut = new AppSettingsRepository(_context);
        }

        [Fact]
        public async Task GetLockoutDurationMinutesAsync_GivenNoSettingsRowExistsYet_WhenCalled_ThenReturnsTheDefaultOfFifteenMinutes()
        {
            // Act
            int minutes = await _sut.GetLockoutDurationMinutesAsync();

            // Assert
            Assert.Equal(15, minutes);
        }

        [Fact]
        public async Task SetLockoutDurationMinutesAsync_GivenNoSettingsRowExistsYet_WhenSet_ThenCreatesItAndLaterReadsReturnTheNewValue()
        {
            // Act — no prior GetLockoutDurationMinutesAsync call, so no row exists yet.
            bool success = await _sut.SetLockoutDurationMinutesAsync(30);

            // Assert
            Assert.True(success);
            Assert.Equal(30, await _sut.GetLockoutDurationMinutesAsync());
        }

        [Fact]
        public async Task SetLockoutDurationMinutesAsync_GivenAnExistingSettingsRow_WhenSetAgain_ThenOverwritesRatherThanCreatingASecondRow()
        {
            // Arrange — creates the single settings row via its default-value path.
            await _sut.GetLockoutDurationMinutesAsync();

            // Act
            await _sut.SetLockoutDurationMinutesAsync(45);

            // Assert — still exactly one row, now holding the updated value.
            Assert.Equal(1, await _context.AppSettings.CountAsync());
            Assert.Equal(45, await _sut.GetLockoutDurationMinutesAsync());
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Dispose();
        }
    }
}
