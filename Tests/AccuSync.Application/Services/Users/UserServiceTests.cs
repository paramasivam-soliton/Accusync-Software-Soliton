// --------------------------------------------------------------------------------
// <copyright file="UserServiceTests.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Application.Resources;
using AccuSync.Application.Services.Authentication;
using AccuSync.Application.Services.Users;
using AccuSync.Core.Abstractions.Repositories;
using AccuSync.Core.Entities;
using AccuSync.Core.Exceptions;
using Microsoft.AspNetCore.DataProtection;
using Moq;

namespace AccuSync.Application.Tests.Services.Users
{
    /// <summary>
    /// IUserRepository is mocked (a real persistence boundary), but EncryptionService and
    /// PasswordHasher are real — backed by a private, per-test Data Protection key ring —
    /// so these tests verify UserService's actual encrypt/decrypt/hash orchestration, not
    /// just that a mock returned what it was told to.
    /// </summary>
    public class UserServiceTests : IDisposable
    {
        private readonly string _keyRingPath;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly EncryptionService _encryptionService;
        private readonly PasswordHasher _passwordHasher;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _keyRingPath = Path.Combine(Path.GetTempPath(), "AccuSyncUserServiceTests_" + Guid.NewGuid());
            var provider = DataProtectionProvider.Create(new DirectoryInfo(_keyRingPath));

            _userRepositoryMock = new Mock<IUserRepository>();
            _encryptionService = new EncryptionService(provider);
            _passwordHasher = new PasswordHasher();
            _userService = new UserService(_userRepositoryMock.Object, _encryptionService, _passwordHasher);
        }

        private static User NewUser(string accountName, string plainTextPassword = "Password@123") => new()
        {
            AccountName = accountName,
            FirstName = accountName,
            LastName = "User",
            ProfileId = (int)UserRole.Screener,
            ProfilePassword = plainTextPassword
        };

        [Fact]
        public async Task CreateUserAsync_ANewUser_TheRepositoryReceivesAnEncryptedAccountNameNotThePlaintext()
        {
            var user = NewUser("Admin");
            User persisted = null!;
            _userRepositoryMock
                .Setup(r => r.CreateUserAsync(It.IsAny<User>()))
                .Callback<User>(u => persisted = u)
                .ReturnsAsync(true);

            bool success = await _userService.CreateUserAsync(user);

            Assert.True(success);
            Assert.NotEqual("Admi", persisted.AccountName);
        }

        [Fact]
        public async Task CreateUserAsync_ANewUser_TheRepositoryReceivesAOneWayHashNotThePlaintextPassword()
        {
            var user = NewUser("Admin", plainTextPassword: "Password@123");
            User persisted = null!;
            _userRepositoryMock
                .Setup(r => r.CreateUserAsync(It.IsAny<User>()))
                .Callback<User>(u => persisted = u)
                .ReturnsAsync(true);

            await _userService.CreateUserAsync(user);

            Assert.NotEqual("Password@123", persisted.ProfilePassword);
            Assert.True(_passwordHasher.Verify("Password@123", persisted.ProfilePassword));
        }

        [Fact]
        public async Task CreateUserAsync_ANewUser_TheRepositoryReceivesANonEmptyUsernameHash()
        {
            _userRepositoryMock
                .Setup(r => r.CreateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(true);

            await _userService.CreateUserAsync(NewUser("Admin"));

            _userRepositoryMock.Verify(r => r.CreateUserAsync(It.Is<User>(u => !string.IsNullOrEmpty(u.UsernameHash))), Times.Once);
        }

        [Fact]
        public async Task GetUserByAccountNameAsync_AnExistingAccountName_QueriesTheRepositoryByTheComputedUsernameHashAndReturnsTheDecryptedUser()
        {
            string ciphertext = _encryptionService.Encrypt("Admin");
            string expectedHash = null!;
            _userRepositoryMock
                .Setup(r => r.GetUserByUsernameHashAsync(It.IsAny<string>()))
                .Callback<string>(hash => expectedHash = hash)
                .ReturnsAsync(new User { AccountName = ciphertext, UsernameHash = "irrelevant-in-this-test" });

            var result = await _userService.GetUserByAccountNameAsync("Admin");

            Assert.NotNull(result);
            Assert.Equal("Admin", result!.AccountName);
            Assert.NotNull(expectedHash);
        }

        [Fact]
        public async Task GetUserByAccountNameAsync_AnAccountNameThatDoesNotExist_ReturnsNullWithoutDecrypting()
        {
            _userRepositoryMock
                .Setup(r => r.GetUserByUsernameHashAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null!);

            var result = await _userService.GetUserByAccountNameAsync("NoSuchAccount");

            Assert.Null(result);
        }

        [Theory]
        [InlineData("ADMIN")]
        [InlineData("admin")]
        [InlineData(" Admin ")]
        public async Task GetUserByAccountNameAsync_AccountNameDifferingOnlyByCasingOrSurroundingWhitespace_TheSameUsernameHashIsQueried(
            string suppliedAccountName)
        {
            var queriedHashes = new List<string>();
            _userRepositoryMock
                .Setup(r => r.GetUserByUsernameHashAsync(It.IsAny<string>()))
                .Callback<string>(hash => queriedHashes.Add(hash))
                .ReturnsAsync((User)null!);

            await _userService.GetUserByAccountNameAsync("Admin");
            await _userService.GetUserByAccountNameAsync(suppliedAccountName);

            Assert.Equal(2, queriedHashes.Count);
            Assert.Equal(queriedHashes[0], queriedHashes[1]);
        }

        [Fact]
        public async Task UpdateUserAsync_AUserToUpdate_TheRepositoryReceivesEncryptedFieldsAndThePasswordPassesThroughUnchanged()
        {
            string alreadyHashedPassword = _passwordHasher.Hash("Password@123");
            var user = new User { AccountName = "Admin", FirstName = "Admin", LastName = "User", ProfileId = (int)UserRole.Screener, ProfilePassword = alreadyHashedPassword };
            User persisted = null!;
            _userRepositoryMock
                .Setup(r => r.UpdateUserAsync(It.IsAny<User>()))
                .Callback<User>(u => persisted = u)
                .ReturnsAsync(true);

            bool success = await _userService.UpdateUserAsync(user);

            Assert.True(success);
            Assert.NotEqual("Admin", persisted.AccountName);
            Assert.Equal(alreadyHashedPassword, persisted.ProfilePassword);
        }

        [Fact]
        public async Task UpdateUserAsync_TheRepositoryThrowsUserNotFoundException_ReturnsFalse()
        {
            _userRepositoryMock
                .Setup(r => r.UpdateUserAsync(It.IsAny<User>()))
                .ThrowsAsync(new UserNotFoundException("No user found."));

            bool success = await _userService.UpdateUserAsync(NewUser("NoSuchAccount"));

            Assert.False(success);
        }

        [Fact]
        public async Task GetAllUsersAsync_MultipleStoredUsers_ReturnsAllOfThemDecrypted()
        {
            _userRepositoryMock
                .Setup(r => r.GetAllUsersAsync())
                .ReturnsAsync(new List<User>
                {
                    new() { AccountName = _encryptionService.Encrypt("Admin") },
                    new() { AccountName = _encryptionService.Encrypt("Screener") }
                });

            var result = await _userService.GetAllUsersAsync();

            Assert.Equal(2, result.Count);
            Assert.Contains(result, u => u.AccountName == "Admin");
            Assert.Contains(result, u => u.AccountName == "Screener");
        }

        [Fact]
        public async Task UpdateUserRoleAsync_RoleChange_RepositoryReceivesTheRolesUnderlyingIntValue()
        {
            string userId = Guid.NewGuid().ToString();
            int persistedProfileId = -1;
            _userRepositoryMock
                .Setup(r => r.UpdateUserProfileIdAsync(userId, It.IsAny<int>()))
                .Callback<string, int>((_, profileId) => persistedProfileId = profileId)
                .ReturnsAsync(true);

            var result = await _userService.UpdateUserRoleAsync(userId, UserRole.Admin);

            Assert.True(result.Success);
            Assert.Equal((int)UserRole.Admin, persistedProfileId);
        }

        [Fact]
        public async Task UpdateUserRoleAsync_RepositoryThrowsUserNotFoundException_ReturnsAFailureResultWithAUserFacingMessage()
        {
            _userRepositoryMock
                .Setup(r => r.UpdateUserProfileIdAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync(new UserNotFoundException("No user found."));

            var result = await _userService.UpdateUserRoleAsync(Guid.NewGuid().ToString(), UserRole.Admin);

            Assert.False(result.Success);
            Assert.Equal(Strings.UserService_UserNotFound, result.ErrorMessage);
        }

        [Fact]
        public async Task UpdateUserRoleAsync_RepositoryReturnsFalseForAnInvalidProfileId_ReturnsAFailureResultWithAUserFacingMessage()
        {
            _userRepositoryMock
                .Setup(r => r.UpdateUserProfileIdAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(false);

            var result = await _userService.UpdateUserRoleAsync(Guid.NewGuid().ToString(), UserRole.Admin);

            Assert.False(result.Success);
            Assert.Equal(Strings.UserService_RoleAssignmentFailed, result.ErrorMessage);
        }

        [Fact]
        public async Task SetUserActiveStatusAsync_ActiveStatusChange_RepositoryReceivesIt()
        {
            string userId = Guid.NewGuid().ToString();
            _userRepositoryMock
                .Setup(r => r.SetUserActiveStatusAsync(userId, false))
                .ReturnsAsync(true);

            var result = await _userService.SetUserActiveStatusAsync(userId, false);

            Assert.True(result.Success);
        }

        [Fact]
        public async Task SetUserActiveStatusAsync_RepositoryThrowsUserNotFoundException_ReturnsAFailureResultWithAUserFacingMessage()
        {
            _userRepositoryMock
                .Setup(r => r.SetUserActiveStatusAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ThrowsAsync(new UserNotFoundException("No user found."));

            var result = await _userService.SetUserActiveStatusAsync(Guid.NewGuid().ToString(), false);

            Assert.False(result.Success);
            Assert.Equal(Strings.UserService_UserNotFound, result.ErrorMessage);
        }

        [Fact]
        public async Task UnlockUserAsync_LockedOutUser_RepositoryReceivesIt()
        {
            string userId = Guid.NewGuid().ToString();
            _userRepositoryMock
                .Setup(r => r.UnlockUserAsync(userId))
                .ReturnsAsync(true);

            var result = await _userService.UnlockUserAsync(userId);

            Assert.True(result.Success);
        }

        [Fact]
        public async Task UnlockUserAsync_RepositoryThrowsUserNotFoundException_ReturnsAFailureResultWithAUserFacingMessage()
        {
            _userRepositoryMock
                .Setup(r => r.UnlockUserAsync(It.IsAny<string>()))
                .ThrowsAsync(new UserNotFoundException("No user found."));

            var result = await _userService.UnlockUserAsync(Guid.NewGuid().ToString());

            Assert.False(result.Success);
            Assert.Equal(Strings.UserService_UserNotFound, result.ErrorMessage);
        }

        public void Dispose()
        {
            if (Directory.Exists(_keyRingPath))
            {
                Directory.Delete(_keyRingPath, recursive: true);
            }
        }
    }
}
