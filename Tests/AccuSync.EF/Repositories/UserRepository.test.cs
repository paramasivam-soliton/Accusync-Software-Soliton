// --------------------------------------------------------------------------------
// <copyright file="UserRepository.test.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Application.Services.Authentication;
using AccuSync.Core.Entities;
using AccuSync.EF;
using AccuSync.EF.Contexts;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AccuSync.EF.Tests.Repositories
{
    /// <summary>
    /// Runs against a private, per-test Sqlite database, open only in memory for the
    /// lifetime of the test (schema created directly from the EF model — no migration
    /// history involved). EncryptionService/PasswordHasher are the real implementations
    /// (backed by a private, per-test Data Protection key ring), not fakes, so these
    /// tests confirm UserRepository's encrypt-on-write/decrypt-on-read round-trip and
    /// the username blind-index pattern actually work end to end.
    /// </summary>
    public class UserRepositoryTests : IDisposable
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
        private readonly string _keyRingPath;
        private readonly UserRepository _sut;

        public UserRepositoryTests()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<SettingsDbContext>()
                .UseSqlite(_connection)
                .Options;
            _context = new SettingsDbContext(options);
            _context.Database.EnsureCreated();

            _keyRingPath = Path.Combine(Path.GetTempPath(), "AccuSyncUserRepositoryTests_" + Guid.NewGuid());
            var dataProtectionProvider = DataProtectionProvider.Create(new DirectoryInfo(_keyRingPath));

            _sut = new UserRepository(new TestDbContextFactory(options), new EncryptionService(dataProtectionProvider), new PasswordHasher());
        }

        private static User NewUser(string accountName, string plainTextPassword = "Password@123") => new()
        {
            AccountName = accountName,
            FirstName = accountName,
            LastName = "User",
            ProfileId = "Screener",
            ProfilePassword = plainTextPassword
        };

        [Fact]
        public async Task GivenANewUser_WhenCreated_ThenTheStoredAccountNameIsEncryptedNotPlaintext()
        {
            // Arrange
            var user = NewUser("Admin");

            // Act
            bool success = await _sut.CreateUserAsync(user);

            // Assert
            Assert.True(success);
            var stored = await _context.Users.AsNoTracking().SingleAsync(u => u.Id == user.Id);
            Assert.NotEqual("Admin", stored.AccountName);
        }

        [Fact]
        public async Task GivenANewUser_WhenCreated_ThenTheStoredPasswordIsAOneWayHashNotThePlaintext()
        {
            // Arrange
            var user = NewUser("Admin", plainTextPassword: "Password@123");

            // Act
            await _sut.CreateUserAsync(user);

            // Assert
            var stored = await _context.Users.AsNoTracking().SingleAsync(u => u.Id == user.Id);
            Assert.NotEqual("Password@123", stored.ProfilePassword);
            Assert.True(new PasswordHasher().Verify("Password@123", stored.ProfilePassword));
        }

        [Fact]
        public async Task GivenAnExistingAccountName_WhenLookedUp_ThenReturnsTheDecryptedUser()
        {
            // Arrange
            var user = NewUser("Admin");
            await _sut.CreateUserAsync(user);

            // Act
            var result = await _sut.GetUserByAccountNameAsync("Admin");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Admin", result!.AccountName);
            Assert.Equal(user.Id, result.Id);
        }

        [Fact]
        public async Task GivenAnAccountNameThatDoesNotExist_WhenLookedUp_ThenReturnsNull()
        {
            // Act
            var result = await _sut.GetUserByAccountNameAsync("NoSuchAccount");

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData("ADMIN")]
        [InlineData("admin")]
        [InlineData(" Admin ")]
        public async Task GivenAccountNameDifferingOnlyByCasingOrSurroundingWhitespace_WhenLookedUp_ThenStillMatchesTheSameAccount(
            string suppliedAccountName)
        {
            // Arrange
            var user = NewUser("Admin");
            await _sut.CreateUserAsync(user);

            // Act
            var result = await _sut.GetUserByAccountNameAsync(suppliedAccountName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result!.Id);
        }

        [Fact]
        public async Task GivenMultipleUsers_WhenRetrieved_ThenReturnsAllOfThemDecrypted()
        {
            // Arrange
            await _sut.CreateUserAsync(NewUser("Admin"));
            await _sut.CreateUserAsync(NewUser("Screener"));

            // Act
            var result = await _sut.GetAllUsersAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, u => u.AccountName == "Admin");
            Assert.Contains(result, u => u.AccountName == "Screener");
        }

        [Fact]
        public async Task GivenAnAccountNameThatAlreadyExists_WhenCreated_ThenReturnsFalseInsteadOfViolatingTheUniqueUsernameConstraint()
        {
            // Arrange
            await _sut.CreateUserAsync(NewUser("Admin"));

            // Act
            bool success = await _sut.CreateUserAsync(NewUser("Admin"));

            // Assert
            Assert.False(success);
        }

        [Fact]
        public async Task GivenAnExistingUser_WhenTheAccountNameIsChanged_ThenLaterLookupsFindItUnderTheNewName()
        {
            // Arrange
            var user = NewUser("Admin");
            await _sut.CreateUserAsync(user);
            var created = await _sut.GetUserByAccountNameAsync("Admin");

            // Act
            created!.AccountName = "SiteAdmin";
            bool success = await _sut.UpdateUserAsync(created);

            // Assert
            Assert.True(success);
            Assert.Null(await _sut.GetUserByAccountNameAsync("Admin"));
            Assert.NotNull(await _sut.GetUserByAccountNameAsync("SiteAdmin"));
        }

        [Fact]
        public async Task GivenAUserIdThatDoesNotExist_WhenUpdated_ThenReturnsFalse()
        {
            // Arrange
            var nonExistentUser = NewUser("NoSuchAccount");

            // Act
            bool success = await _sut.UpdateUserAsync(nonExistentUser);

            // Assert
            Assert.False(success);
        }

        [Fact]
        public async Task GivenTheAccountNameIsSetBackToItsOwnCurrentValue_WhenUpdated_ThenTheStoredCiphertextChangesButTheUsernameHashDoesNot()
        {
            // Arrange
            var user = NewUser("Admin");
            await _sut.CreateUserAsync(user);
            var beforeUpdate = await _context.Users.AsNoTracking().SingleAsync(u => u.Id == user.Id);
            var current = await _sut.GetUserByAccountNameAsync("Admin");

            // Act — re-save with the exact same account name.
            await _sut.UpdateUserAsync(current!);
            var afterUpdate = await _context.Users.AsNoTracking().SingleAsync(u => u.Id == user.Id);

            // Assert — encryption is non-deterministic (a fresh ciphertext every write),
            // while the deterministic blind-index hash used for lookup stays the same.
            Assert.NotEqual(beforeUpdate.AccountName, afterUpdate.AccountName);
            Assert.Equal(beforeUpdate.UsernameHash, afterUpdate.UsernameHash);
        }

        [Fact]
        public async Task GivenARoleAssignedAtCreation_WhenCreated_ThenTheRoleIsPersistedAndRetrievable()
        {
            // Arrange — users can be assigned a role at account creation.
            var user = NewUser("Admin");
            user.ProfileId = "Admin";

            // Act
            await _sut.CreateUserAsync(user);

            // Assert
            var result = await _sut.GetUserByAccountNameAsync("Admin");
            Assert.Equal("Admin", result!.ProfileId);
        }

        [Fact]
        public async Task GivenAnExistingUser_WhenTheRoleIsChanged_ThenLaterLookupsReflectTheNewRole()
        {
            // Arrange — created as Screener.
            var user = NewUser("Admin");
            user.ProfileId = "Screener";
            await _sut.CreateUserAsync(user);

            // Act — promoted to Admin via the narrow admin-exception method
            // (no Users-management UI exists yet).
            bool success = await _sut.UpdateUserRoleAsync(user.Id, UserRole.Admin);

            // Assert
            Assert.True(success);
            var result = await _sut.GetUserByAccountNameAsync("Admin");
            Assert.Equal("Admin", result!.ProfileId);
        }

        [Fact]
        public async Task GivenAUserIdThatDoesNotExist_WhenCalled_ThenReturnsFalse()
        {
            // Act
            bool success = await _sut.UpdateUserRoleAsync(System.Guid.NewGuid().ToString(), UserRole.Admin);

            // Assert
            Assert.False(success);
        }

        [Fact]
        public async Task GivenARoleChange_WhenUpdated_ThenOtherFieldsAreLeftUntouched()
        {
            // Arrange
            var user = NewUser("Admin");
            user.ProfileId = "Screener";
            await _sut.CreateUserAsync(user);

            // Act
            await _sut.UpdateUserRoleAsync(user.Id, UserRole.Admin);

            // Assert — a role change is not supposed to be a disguised full profile overwrite.
            var result = await _sut.GetUserByAccountNameAsync("Admin");
            Assert.Equal("Admin", result!.AccountName);
            Assert.Equal("Admin", result.FirstName);
        }

        [Fact]
        public async Task SetUserActiveStatusAsync_GivenAnActiveUser_WhenDeactivated_ThenLaterLookupsReflectTheDeactivationWithoutTouchingOtherFields()
        {
            // Arrange — active by default (see User's constructor).
            var user = NewUser("Admin");
            await _sut.CreateUserAsync(user);

            // Act — deactivated via the narrow admin-exception method
            // (no Users-management UI exists yet).
            bool success = await _sut.SetUserActiveStatusAsync(user.Id, isActive: false);

            // Assert
            Assert.True(success);
            var result = await _sut.GetUserByAccountNameAsync("Admin");
            Assert.False(result!.IsActive);
            Assert.Equal("Admin", result.AccountName);
        }

        [Fact]
        public async Task SetUserActiveStatusAsync_GivenAUserIdThatDoesNotExist_WhenCalled_ThenReturnsFalse()
        {
            // Act
            bool success = await _sut.SetUserActiveStatusAsync(System.Guid.NewGuid().ToString(), isActive: false);

            // Assert
            Assert.False(success);
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Dispose();

            if (Directory.Exists(_keyRingPath))
            {
                Directory.Delete(_keyRingPath, recursive: true);
            }
        }
    }
}
