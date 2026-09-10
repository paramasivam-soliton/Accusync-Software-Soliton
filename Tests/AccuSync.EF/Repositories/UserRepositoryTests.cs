// --------------------------------------------------------------------------------
// <copyright file="UserRepositoryTests.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Threading;
using AccuSync.Core.Entities;
using AccuSync.Core.Exceptions;
using AccuSync.EF.Contexts;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;

namespace AccuSync.EF.Tests.Repositories
{
    /// <summary>
    /// Mocks IDbContextFactory/DbContext/DbSet (Moq + MockQueryable.Moq for the async LINQ
    /// surface — ToListAsync/FirstOrDefaultAsync/etc.) rather than running against a real
    /// database, backed by a plain in-memory list standing in for the Users table. See
    /// <see cref="UserRepositoryConstraintTests"/> for the tests that specifically verify
    /// real DB constraint enforcement and so stay on a real in-memory Sqlite connection.
    /// </summary>
    public class UserRepositoryTests
    {
        private readonly List<User> _users;
        private readonly UserRepository _userRepository;

        public UserRepositoryTests()
        {
            _users = new List<User>();

            var usersDbSetMock = _users.BuildMockDbSet();
            usersDbSetMock
                .Setup(d => d.FindAsync(It.IsAny<object[]>()))
                .ReturnsAsync((object[] keys) => _users.FirstOrDefault(u => u.Id == (string)keys[0]));
            usersDbSetMock
                .Setup(d => d.Add(It.IsAny<User>()))
                .Callback<User>(u => _users.Add(u));

            var contextMock = new Mock<SettingsDbContext>(new DbContextOptionsBuilder<SettingsDbContext>().Options);
            contextMock.Setup(c => c.Users).Returns(usersDbSetMock.Object);
            contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var contextFactoryMock = new Mock<IDbContextFactory<SettingsDbContext>>();
            contextFactoryMock
                .Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(contextMock.Object);

            _userRepository = new UserRepository(contextFactoryMock.Object);
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
        public async Task CreateUserAsync_ANewUser_FieldValuesArePersistedUnchanged()
        {
            var user = NewUser("encrypted-account-name", "hash-1");

            bool success = await _userRepository.CreateUserAsync(user);

            Assert.True(success);
            var stored = await _userRepository.GetUserByUsernameHashAsync("hash-1");
            Assert.NotNull(stored);
            Assert.Equal("encrypted-account-name", stored!.AccountName);
            Assert.Equal("hash-1", stored.UsernameHash);
            Assert.Equal("already-hashed-value", stored.ProfilePassword);
        }

        [Fact]
        public async Task GetUserByUsernameHashAsync_AnExistingUsernameHash_ReturnsTheMatchingUser()
        {
            var user = NewUser("encrypted-account-name", "hash-1");
            await _userRepository.CreateUserAsync(user);

            var result = await _userRepository.GetUserByUsernameHashAsync("hash-1");

            Assert.NotNull(result);
            Assert.Equal(user.Id, result!.Id);
        }

        [Fact]
        public async Task GetUserByUsernameHashAsync_AUsernameHashThatDoesNotExist_ReturnsNull()
        {
            var result = await _userRepository.GetUserByUsernameHashAsync("no-such-hash");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllUsersAsync_MultipleUsers_ReturnsAllOfThemUnchanged()
        {
            await _userRepository.CreateUserAsync(NewUser("admin-account", "hash-admin"));
            await _userRepository.CreateUserAsync(NewUser("screener-account", "hash-screener"));

            var result = await _userRepository.GetAllUsersAsync();

            Assert.Equal(2, result.Count);
            Assert.Contains(result, u => u.AccountName == "admin-account");
            Assert.Contains(result, u => u.AccountName == "screener-account");
        }

        [Fact]
        public async Task UpdateUserAsync_AnExistingUserWithFieldsChanged_LaterLookupsReflectTheNewValues()
        {
            var user = NewUser("encrypted-account-name", "hash-1");
            await _userRepository.CreateUserAsync(user);
            var created = await _userRepository.GetUserByUsernameHashAsync("hash-1");

            created!.AccountName = "changed-encrypted-value";
            created.UsernameHash = "hash-2";
            bool success = await _userRepository.UpdateUserAsync(created);

            Assert.True(success);
            Assert.Null(await _userRepository.GetUserByUsernameHashAsync("hash-1"));
            var updated = await _userRepository.GetUserByUsernameHashAsync("hash-2");
            Assert.NotNull(updated);
            Assert.Equal("changed-encrypted-value", updated!.AccountName);
        }

        [Fact]
        public async Task UpdateUserAsync_UserIdThatDoesNotExist_ThrowsUserNotFoundException()
        {
            var nonExistentUser = NewUser("no-such-account", "hash-missing");

            await Assert.ThrowsAsync<UserNotFoundException>(() => _userRepository.UpdateUserAsync(nonExistentUser));
        }

        [Fact]
        public async Task CreateUserAsync_RoleAssignedAtCreation_RoleIsPersistedAndRetrievable()
        {
            var user = NewUser("Admin", "hash-1");
            user.ProfileId = (int)UserRole.Admin;

            await _userRepository.CreateUserAsync(user);

            var result = await _userRepository.GetUserByUsernameHashAsync("hash-1");
            Assert.Equal((int)UserRole.Admin, result!.ProfileId);
        }

        [Fact]
        public async Task UpdateUserProfileIdAsync_ExistingUser_LaterLookupsReflectTheNewValue()
        {
            var user = NewUser("Admin", "hash-1");
            user.ProfileId = (int)UserRole.Screener;
            await _userRepository.CreateUserAsync(user);

            bool success = await _userRepository.UpdateUserProfileIdAsync(user.Id, (int)UserRole.Admin);

            Assert.True(success);
            var result = await _userRepository.GetUserByUsernameHashAsync("hash-1");
            Assert.Equal((int)UserRole.Admin, result!.ProfileId);
        }

        [Fact]
        public async Task UpdateUserProfileIdAsync_UserIdThatDoesNotExist_ThrowsUserNotFoundException()
        {
            await Assert.ThrowsAsync<UserNotFoundException>(() => _userRepository.UpdateUserProfileIdAsync(Guid.NewGuid().ToString(), (int)UserRole.Admin));
        }

        [Fact]
        public async Task UpdateUserProfileIdAsync_ProfileIdChange_OtherFieldsAreLeftUntouched()
        {
            var user = NewUser("Admin", "hash-1");
            user.ProfileId = (int)UserRole.Screener;
            await _userRepository.CreateUserAsync(user);

            await _userRepository.UpdateUserProfileIdAsync(user.Id, (int)UserRole.Admin);

            var result = await _userRepository.GetUserByUsernameHashAsync("hash-1");
            Assert.Equal("Admin", result!.AccountName);
            Assert.Equal("Admin", result.FirstName);
        }

        [Fact]
        public async Task SetUserActiveStatusAsync_ActiveUserDeactivated_LaterLookupsReflectTheDeactivation()
        {
            var user = NewUser("Admin", "hash-1");
            await _userRepository.CreateUserAsync(user);

            bool success = await _userRepository.SetUserActiveStatusAsync(user.Id, isActive: false);

            Assert.True(success);
            var result = await _userRepository.GetUserByUsernameHashAsync("hash-1");
            Assert.False(result!.IsActive);
            Assert.Equal("Admin", result.AccountName);
        }

        [Fact]
        public async Task SetUserActiveStatusAsync_UserIdThatDoesNotExist_ThrowsUserNotFoundException()
        {
            await Assert.ThrowsAsync<UserNotFoundException>(() => _userRepository.SetUserActiveStatusAsync(Guid.NewGuid().ToString(), isActive: false));
        }

        [Fact]
        public async Task UnlockUserAsync_LockedOutUser_ClearsTheFailedAttemptCounterAndTimestampTogether()
        {
            var user = NewUser("Admin", "hash-1");
            await _userRepository.CreateUserAsync(user);
            var lockedOut = await _userRepository.GetUserByUsernameHashAsync("hash-1");
            lockedOut!.FailedLoginAttemptCount = 5;
            lockedOut.FirstFailedLoginTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            await _userRepository.UpdateUserAsync(lockedOut);

            bool success = await _userRepository.UnlockUserAsync(user.Id);

            Assert.True(success);
            var result = await _userRepository.GetUserByUsernameHashAsync("hash-1");
            Assert.Equal(0, result!.FailedLoginAttemptCount);
            Assert.Equal(0L, result.FirstFailedLoginTime);
        }

        [Fact]
        public async Task UnlockUserAsync_UserIdThatDoesNotExist_ThrowsUserNotFoundException()
        {
            await Assert.ThrowsAsync<UserNotFoundException>(() => _userRepository.UnlockUserAsync(Guid.NewGuid().ToString()));
        }
    }
}
