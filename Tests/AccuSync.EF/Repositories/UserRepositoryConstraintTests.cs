// --------------------------------------------------------------------------------
// <copyright file="UserRepositoryConstraintTests.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Core.Entities;
using AccuSync.EF.Contexts;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AccuSync.EF.Tests.Repositories
{
    /// <summary>
    /// Runs against a private, per-test Sqlite database, open only in memory for the
    /// lifetime of the test (schema created directly from the EF model — no migration
    /// history involved). Covers only the paths where UserRepository relies on the
    /// database itself enforcing a constraint (a unique-index collision, a NOT NULL
    /// column, a foreign-key violation) — a mocked DbContext can be told to throw but
    /// can't prove the schema actually enforces anything. See
    /// <see cref="UserRepositoryTests"/> for the mocked coverage of this repository's
    /// other behavior.
    /// </summary>
    public class UserRepositoryConstraintTests : IDisposable
    {
        // Hands out a fresh SettingsDbContext per call, all sharing the one open in-memory
        // connection below — mirrors IDbContextFactory's real per-call-context contract
        // (see UserRepository) without needing a DI container in this test.
        private class TestDbContextFactory : IDbContextFactory<SettingsDbContext>
        {
            private readonly DbContextOptions<SettingsDbContext> _options;

            public TestDbContextFactory(DbContextOptions<SettingsDbContext> options) => _options = options;

            public SettingsDbContext CreateDbContext() => new(_options);
        }

        private readonly SqliteConnection _connection;
        private readonly SettingsDbContext _context;
        private readonly UserRepository _userRepository;

        public UserRepositoryConstraintTests()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<SettingsDbContext>()
                .UseSqlite(_connection)
                .Options;
            _context = new SettingsDbContext(options);
            _context.Database.EnsureCreated();

            _userRepository = new UserRepository(new TestDbContextFactory(options));
        }

        private static User NewUser(string accountName, string usernameHash) => new()
        {
            AccountName = accountName,
            UsernameHash = usernameHash,
            FirstName = accountName,
            LastName = "User",
            ProfileId = (int)UserRole.Screener,
            ProfilePassword = "already-hashed-value"
        };

        [Fact]
        public async Task CreateUserAsync_UsernameHashThatAlreadyExists_ReturnsFalse()
        {
            await _userRepository.CreateUserAsync(NewUser("admin-account", "hash-1"));

            bool success = await _userRepository.CreateUserAsync(NewUser("other-account", "hash-1"));

            Assert.False(success);
        }

        [Fact]
        public async Task CreateUserAsync_UserWithARequiredFieldMissing_ThrowsRatherThanReturningFalse()
        {
            var user = NewUser("Admin", "hash-1");
            user.AccountName = null!;

            await Assert.ThrowsAnyAsync<Exception>(() => _userRepository.CreateUserAsync(user));
        }

        [Fact]
        public async Task UpdateUserProfileIdAsync_ProfileIdWithNoMatchingProfilesRow_ReturnsFalse()
        {
            var user = NewUser("Admin", "hash-1");
            await _userRepository.CreateUserAsync(user);

            bool success = await _userRepository.UpdateUserProfileIdAsync(user.Id, profileId: 999);

            Assert.False(success);
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Dispose();
        }
    }
}
