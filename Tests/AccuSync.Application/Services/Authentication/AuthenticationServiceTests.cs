// --------------------------------------------------------------------------------
// <copyright file="AuthenticationServiceTests.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Application.Resources;
using AccuSync.Application.Services.Authentication;
using AccuSync.Core.Abstractions.Repositories;
using AccuSync.Core.Abstractions.Services;
using AccuSync.Core.Entities;
using Moq;

namespace AccuSync.Application.Tests.Services.Authentication
{
    /// <summary>
    /// IUserService and IAppSettingsRepository are mocked (no real persistence
    /// needed to exercise the authentication rules), but PasswordHasher is real —
    /// so these tests verify AuthenticationService's actual hash-verification
    /// behavior, not just that some mock returned what it was told to.
    /// </summary>
    public class AuthenticationServiceTests
    {
        private const string AccountName = "Screener";
        private const string CorrectPassword = "Password@123";
        private const int DefaultLockoutDurationMinutes = 15;

        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IAppSettingsRepository> _appSettingsRepositoryMock;
        private readonly PasswordHasher _passwordHasher;
        private readonly AuthenticationService _authenticationService;

        public AuthenticationServiceTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _appSettingsRepositoryMock = new Mock<IAppSettingsRepository>();
            _appSettingsRepositoryMock
                .Setup(a => a.GetLockoutDurationMinutesAsync())
                .ReturnsAsync(DefaultLockoutDurationMinutes);

            _passwordHasher = new PasswordHasher();
            _authenticationService = new AuthenticationService(_userServiceMock.Object, _passwordHasher, _appSettingsRepositoryMock.Object);
        }

        private User CreateUser(
            string plainPassword,
            int failedLoginAttemptCount = 0,
            long firstFailedLoginTime = 0L,
            long? passwordModificationDate = null,
            bool isActive = true)
        {
            return new User
            {
                AccountName = AccountName,
                ProfilePassword = _passwordHasher.Hash(plainPassword),
                FailedLoginAttemptCount = failedLoginAttemptCount,
                FirstFailedLoginTime = firstFailedLoginTime,
                PasswordModificationDate = passwordModificationDate ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                IsActive = isActive
            };
        }

        private void GivenUserExists(User user)
        {
            _userServiceMock
                .Setup(r => r.GetUserByAccountNameAsync(AccountName))
                .ReturnsAsync(user);
            _userServiceMock
                .Setup(r => r.UpdateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(true);
        }

        private void GivenLockoutDurationMinutes(int minutes)
        {
            _appSettingsRepositoryMock
                .Setup(a => a.GetLockoutDurationMinutesAsync())
                .ReturnsAsync(minutes);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task AuthenticateAsync_NullOrWhitespaceAccountName_ReturnsRequiredFieldsError(
            string? accountName)
        {
            var result = await _authenticationService.AuthenticateAsync(accountName!, CorrectPassword);

            Assert.False(result.Success);
            Assert.Equal("Username and password are required.", result.ErrorMessage);
            _userServiceMock.Verify(r => r.GetUserByAccountNameAsync(It.IsAny<string>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task AuthenticateAsync_NullOrWhitespacePassword_ReturnsRequiredFieldsError(
            string? password)
        {
            var result = await _authenticationService.AuthenticateAsync(AccountName, password!);

            Assert.False(result.Success);
            Assert.Equal("Username and password are required.", result.ErrorMessage);
            _userServiceMock.Verify(r => r.GetUserByAccountNameAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AuthenticateAsync_AccountNameThatDoesNotExist_ReturnsTheSameGenericInvalidCredentialsErrorAsAWrongPassword()
        {
            _userServiceMock
                .Setup(r => r.GetUserByAccountNameAsync(AccountName))
                .ReturnsAsync((User)null!);

            var result = await _authenticationService.AuthenticateAsync(AccountName, CorrectPassword);

            Assert.False(result.Success);
            Assert.Equal("Invalid username or password.", result.ErrorMessage);
        }

        [Fact]
        public async Task AuthenticateAsync_DeactivatedAccountWithCorrectPassword_ReturnsInvalidCredentialsError()
        {
            var user = CreateUser(CorrectPassword, isActive: false);
            GivenUserExists(user);

            var result = await _authenticationService.AuthenticateAsync(AccountName, CorrectPassword);

            Assert.False(result.Success);
            Assert.Equal("Invalid username or password.", result.ErrorMessage);
        }

        [Fact]
        public async Task AuthenticateAsync_CorrectPassword_ReturnsSuccessWithTheUser()
        {
            var user = CreateUser(CorrectPassword);
            GivenUserExists(user);

            var result = await _authenticationService.AuthenticateAsync(AccountName, CorrectPassword);

            Assert.True(result.Success);
            Assert.Same(user, result.User);
            Assert.False(result.IsLocked);
        }

        [Fact]
        public async Task AuthenticateAsync_CorrectPasswordAfterPriorFailures_ResetsTheFailedAttemptCounter()
        {
            var user = CreateUser(CorrectPassword, failedLoginAttemptCount: 3, firstFailedLoginTime: DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            GivenUserExists(user);

            var result = await _authenticationService.AuthenticateAsync(AccountName, CorrectPassword);

            Assert.True(result.Success);
            Assert.Equal(0, user.FailedLoginAttemptCount);
            Assert.Equal(0L, user.FirstFailedLoginTime);
            _userServiceMock.Verify(r => r.UpdateUserAsync(user), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_WrongPassword_ReturnsFailureAndIncrementsTheFailedAttemptCounter()
        {
            var user = CreateUser(CorrectPassword);
            GivenUserExists(user);

            var result = await _authenticationService.AuthenticateAsync(AccountName, "some-wrong-password");

            Assert.False(result.Success);
            Assert.False(result.IsLocked);
            Assert.Equal(1, user.FailedLoginAttemptCount);
            _userServiceMock.Verify(r => r.UpdateUserAsync(user), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_UserWithACorruptedStoredPasswordHash_ReturnsAnUnexpectedFailureError()
        {
            var user = CreateUser(CorrectPassword);
            user.ProfilePassword = "600000.not-valid-base64!!!.c29tZWhhc2g=";
            GivenUserExists(user);

            var result = await _authenticationService.AuthenticateAsync(AccountName, CorrectPassword);

            Assert.False(result.Success);
            Assert.False(result.IsLocked);
            Assert.Equal(
                string.Format(Strings.AuthenticationService_UnexpectedFailure, ErrorCode.Unexpected.ToDisplayCode()),
                result.ErrorMessage);
            Assert.Equal(0, user.FailedLoginAttemptCount);
            _userServiceMock.Verify(r => r.UpdateUserAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task AuthenticateAsync_UserServiceThrowsWhilePersistingAFailedAttempt_ReturnsAnUnexpectedFailureError()
        {
            var user = CreateUser(CorrectPassword);
            _userServiceMock
                .Setup(r => r.GetUserByAccountNameAsync(AccountName))
                .ReturnsAsync(user);
            _userServiceMock
                .Setup(r => r.UpdateUserAsync(It.IsAny<User>()))
                .ThrowsAsync(new InvalidOperationException("Simulated database write failure."));

            var result = await _authenticationService.AuthenticateAsync(AccountName, "some-wrong-password");

            Assert.False(result.Success);
            Assert.False(result.IsLocked);
            Assert.Equal(
                string.Format(Strings.AuthenticationService_UnexpectedFailure, ErrorCode.Unexpected.ToDisplayCode()),
                result.ErrorMessage);
        }

        [Fact]
        public async Task AuthenticateAsync_WrongPasswordWithAttemptsStillRemaining_TheErrorMessageStatesHowManyAttemptsRemain()
        {
            var user = CreateUser(CorrectPassword, failedLoginAttemptCount: 3, firstFailedLoginTime: DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            GivenUserExists(user);

            var result = await _authenticationService.AuthenticateAsync(AccountName, "some-wrong-password");

            Assert.False(result.Success);
            Assert.False(result.IsLocked);
            Assert.Equal("Invalid username or password. 1 attempt(s) remaining.", result.ErrorMessage);
        }

        [Fact]
        public async Task AuthenticateAsync_WrongPasswordOnTheFifthConsecutiveAttempt_LocksTheAccountForTheConfiguredDuration()
        {
            var user = CreateUser(CorrectPassword, failedLoginAttemptCount: 4, firstFailedLoginTime: DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            GivenUserExists(user);
            GivenLockoutDurationMinutes(15);

            var result = await _authenticationService.AuthenticateAsync(AccountName, "some-wrong-password");

            Assert.False(result.Success);
            Assert.True(result.IsLocked);
            Assert.Equal(5, user.FailedLoginAttemptCount);
            Assert.Equal("Account is now locked due to 5 failed attempts. Try again in 15 minute(s), or contact your administrator.", result.ErrorMessage);
        }

        [Fact]
        public async Task AuthenticateAsync_AccountLockedWithinTheConfiguredDurationEvenWithCorrectPassword_StillReturnsLocked()
        {
            var user = CreateUser(
                CorrectPassword,
                failedLoginAttemptCount: 5,
                firstFailedLoginTime: DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            GivenUserExists(user);
            GivenLockoutDurationMinutes(15);

            var result = await _authenticationService.AuthenticateAsync(AccountName, CorrectPassword);

            Assert.False(result.Success);
            Assert.True(result.IsLocked);
        }

        [Fact]
        public async Task AuthenticateAsync_AccountLockedWithAdministratorConfiguredLongerDuration_StillReturnsLockedPastTheDefaultFifteenMinutes()
        {
            long twentyMinutesAgo = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - (20 * 60);
            var user = CreateUser(CorrectPassword, failedLoginAttemptCount: 5, firstFailedLoginTime: twentyMinutesAgo);
            GivenUserExists(user);
            GivenLockoutDurationMinutes(30);

            var result = await _authenticationService.AuthenticateAsync(AccountName, CorrectPassword);

            Assert.False(result.Success);
            Assert.True(result.IsLocked);
        }

        [Fact]
        public async Task AuthenticateAsync_ThirtySecondsRemainingOnTheLockout_RemainingTimeRoundsUpToOneMinuteRatherThanZero()
        {
            long fourteenAndHalfMinutesAgo = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - (14 * 60 + 30);
            var user = CreateUser(CorrectPassword, failedLoginAttemptCount: 5, firstFailedLoginTime: fourteenAndHalfMinutesAgo);
            GivenUserExists(user);
            GivenLockoutDurationMinutes(15);

            var result = await _authenticationService.AuthenticateAsync(AccountName, CorrectPassword);

            Assert.False(result.Success);
            Assert.Equal("Account locked. Try again in 1 minute(s), or contact your administrator.", result.ErrorMessage);
        }

        [Fact]
        public async Task AuthenticateAsync_AccountLockedButConfiguredDurationHasElapsedWithCorrectPassword_AutoResetsAndSucceeds()
        {
            long sixteenMinutesAgo = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - (16 * 60);
            var user = CreateUser(
                CorrectPassword,
                failedLoginAttemptCount: 5,
                firstFailedLoginTime: sixteenMinutesAgo);
            GivenUserExists(user);
            GivenLockoutDurationMinutes(15);

            var result = await _authenticationService.AuthenticateAsync(AccountName, CorrectPassword);

            Assert.True(result.Success);
            Assert.False(result.IsLocked);
            Assert.Equal(0, user.FailedLoginAttemptCount);
            Assert.Equal(0L, user.FirstFailedLoginTime);
        }

        [Fact]
        public async Task AuthenticateAsync_AccountLockedButConfiguredDurationHasElapsedWithWrongPassword_StartsANewFailureStreak()
        {
            long sixteenMinutesAgo = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - (16 * 60);
            var user = CreateUser(
                CorrectPassword,
                failedLoginAttemptCount: 5,
                firstFailedLoginTime: sixteenMinutesAgo);
            GivenUserExists(user);
            GivenLockoutDurationMinutes(15);

            var result = await _authenticationService.AuthenticateAsync(AccountName, "some-wrong-password");

            Assert.False(result.Success);
            Assert.False(result.IsLocked);
            Assert.Equal(1, user.FailedLoginAttemptCount);
        }

        [Fact]
        public async Task AuthenticateAsync_PasswordOlderThanNinetyDaysWithCorrectPassword_SucceedsButFlagsThePasswordAsExpired()
        {
            long ninetyOneDaysAgo = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - (91L * 24 * 60 * 60);
            var user = CreateUser(CorrectPassword, passwordModificationDate: ninetyOneDaysAgo);
            GivenUserExists(user);

            var result = await _authenticationService.AuthenticateAsync(AccountName, CorrectPassword);

            Assert.True(result.Success);
            Assert.True(result.IsPasswordExpired);
            Assert.Same(user, result.User);
        }

        [Fact]
        public async Task AuthenticateAsync_PasswordExactlyNinetyDaysOldWithCorrectPassword_IsTreatedAsExpired()
        {
            long exactlyNinetyDaysAgo = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - (90L * 24 * 60 * 60);
            var user = CreateUser(CorrectPassword, passwordModificationDate: exactlyNinetyDaysAgo);
            GivenUserExists(user);

            var result = await _authenticationService.AuthenticateAsync(AccountName, CorrectPassword);

            Assert.True(result.Success);
            Assert.True(result.IsPasswordExpired);
        }

        [Fact]
        public async Task AuthenticateAsync_PasswordWithinNinetyDaysWithCorrectPassword_SucceedsWithoutFlaggingExpiry()
        {
            long eightyNineDaysAgo = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - (89L * 24 * 60 * 60);
            var user = CreateUser(CorrectPassword, passwordModificationDate: eightyNineDaysAgo);
            GivenUserExists(user);

            var result = await _authenticationService.AuthenticateAsync(AccountName, CorrectPassword);

            Assert.True(result.Success);
            Assert.False(result.IsPasswordExpired);
        }

        [Fact]
        public async Task AuthenticateAsync_UserWithNoRecordedPasswordModificationDateWithCorrectPassword_IsTreatedAsExpired()
        {
            var user = CreateUser(CorrectPassword, passwordModificationDate: 0L);
            GivenUserExists(user);

            var result = await _authenticationService.AuthenticateAsync(AccountName, CorrectPassword);

            Assert.True(result.Success);
            Assert.True(result.IsPasswordExpired);
        }

        [Fact]
        public async Task AuthenticateAsync_PasswordModificationDateRecordedThroughANonUtcOffsetWithCorrectPassword_StillComputesAgeCorrectly()
        {
            var ninetyOneDaysAgoUtc = DateTimeOffset.UtcNow.AddDays(-91);
            var sameInstantNonUtcOffset = ninetyOneDaysAgoUtc.ToOffset(TimeSpan.FromHours(5.5));
            var user = CreateUser(CorrectPassword, passwordModificationDate: sameInstantNonUtcOffset.ToUnixTimeSeconds());
            GivenUserExists(user);

            var result = await _authenticationService.AuthenticateAsync(AccountName, CorrectPassword);

            Assert.True(result.Success);
            Assert.True(result.IsPasswordExpired);
        }
    }
}
