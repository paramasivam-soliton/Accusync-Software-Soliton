// --------------------------------------------------------------------------------
// <copyright file="AuthenticationService.test.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Application.Services.Authentication;
using AccuSync.Core.Abstractions.Repositories;
using AccuSync.Core.Entities;
using Moq;

namespace AccuSync.Application.Tests.Services.Authentication
{
    /// <summary>
    /// IUserRepository and IAppSettingsRepository are mocked (no real persistence
    /// needed to exercise the authentication rules), but PasswordHasher is real —
    /// so these tests verify AuthenticationService's actual hash-verification
    /// behavior, not just that some mock returned what it was told to.
    /// </summary>
    public class AuthenticationServiceTests
    {
        private const string AccountName = "Screener";
        private const string CorrectPassword = "Password@123";
        private const int DefaultLockoutDurationMinutes = 15;

        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IAppSettingsRepository> _appSettingsRepositoryMock;
        private readonly PasswordHasher _passwordHasher;
        private readonly AuthenticationService _sut;

        public AuthenticationServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _appSettingsRepositoryMock = new Mock<IAppSettingsRepository>();
            _appSettingsRepositoryMock
                .Setup(a => a.GetLockoutDurationMinutesAsync())
                .ReturnsAsync(DefaultLockoutDurationMinutes);

            _passwordHasher = new PasswordHasher();
            _sut = new AuthenticationService(_userRepositoryMock.Object, _passwordHasher, _appSettingsRepositoryMock.Object);
        }

        private User CreateUser(
            string plainPassword,
            int failedLoginAttemptCount = 0,
            long firstFailedLoginTime = 0L,
            long? passwordModificationDate = null,
            bool status = true)
        {
            return new User
            {
                AccountName = AccountName,
                ProfilePassword = _passwordHasher.Hash(plainPassword),
                FailedLoginAttemptCount = failedLoginAttemptCount,
                FirstFailedLoginTime = firstFailedLoginTime,
                PasswordModificationDate = passwordModificationDate ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Status = status
            };
        }

        private void GivenUserExists(User user)
        {
            _userRepositoryMock
                .Setup(r => r.GetUserByAccountNameAsync(AccountName))
                .ReturnsAsync(user);
            _userRepositoryMock
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
        public async Task AuthenticateAsync_GivenNullOrWhitespaceAccountName_WhenAuthenticating_ThenReturnsRequiredFieldsErrorWithoutTouchingTheRepository(
            string? accountName)
        {
            // Act
            var result = await _sut.AuthenticateAsync(accountName!, CorrectPassword);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Username and password are required.", result.ErrorMessage);
            _userRepositoryMock.Verify(r => r.GetUserByAccountNameAsync(It.IsAny<string>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task AuthenticateAsync_GivenNullOrWhitespacePassword_WhenAuthenticating_ThenReturnsRequiredFieldsErrorWithoutTouchingTheRepository(
            string? password)
        {
            // Act
            var result = await _sut.AuthenticateAsync(AccountName, password!);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Username and password are required.", result.ErrorMessage);
            _userRepositoryMock.Verify(r => r.GetUserByAccountNameAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AuthenticateAsync_GivenAnAccountNameThatDoesNotExist_WhenAuthenticating_ThenReturnsTheSameGenericInvalidCredentialsErrorAsAWrongPassword()
        {
            // Arrange
            _userRepositoryMock
                .Setup(r => r.GetUserByAccountNameAsync(AccountName))
                .ReturnsAsync((User)null!);

            // Act
            var result = await _sut.AuthenticateAsync(AccountName, CorrectPassword);

            // Assert — same message a wrong password gets, so a caller can't
            // tell "no such account" from "wrong password" (no enumeration).
            Assert.False(result.Success);
            Assert.Equal("Invalid username or password.", result.ErrorMessage);
        }

        [Fact]
        public async Task AuthenticateAsync_GivenADeactivatedAccount_WhenAuthenticatingWithTheCorrectPassword_ThenReturnsTheSameGenericInvalidCredentialsErrorAsAWrongPassword()
        {
            // Arrange
            var user = CreateUser(CorrectPassword, status: false);
            GivenUserExists(user);

            // Act
            var result = await _sut.AuthenticateAsync(AccountName, CorrectPassword);

            // Assert — deactivated is rejected regardless of password correctness, with
            // the same generic message a wrong password gets (active status isn't leaked).
            Assert.False(result.Success);
            Assert.Equal("Invalid username or password.", result.ErrorMessage);
        }

        [Fact]
        public async Task AuthenticateAsync_GivenTheCorrectPassword_WhenAuthenticating_ThenReturnsSuccessWithTheUser()
        {
            // Arrange
            var user = CreateUser(CorrectPassword);
            GivenUserExists(user);

            // Act
            var result = await _sut.AuthenticateAsync(AccountName, CorrectPassword);

            // Assert
            Assert.True(result.Success);
            Assert.Same(user, result.User);
            Assert.False(result.IsLocked);
        }

        [Fact]
        public async Task AuthenticateAsync_GivenTheCorrectPasswordAfterPriorFailures_WhenAuthenticating_ThenResetsTheFailedAttemptCounter()
        {
            // Arrange
            var user = CreateUser(CorrectPassword, failedLoginAttemptCount: 3, firstFailedLoginTime: DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            GivenUserExists(user);

            // Act
            var result = await _sut.AuthenticateAsync(AccountName, CorrectPassword);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(0, user.FailedLoginAttemptCount);
            Assert.Equal(0L, user.FirstFailedLoginTime);
            _userRepositoryMock.Verify(r => r.UpdateUserAsync(user), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_GivenAWrongPassword_WhenAuthenticating_ThenReturnsFailureAndIncrementsTheFailedAttemptCounter()
        {
            // Arrange
            var user = CreateUser(CorrectPassword);
            GivenUserExists(user);

            // Act
            var result = await _sut.AuthenticateAsync(AccountName, "some-wrong-password");

            // Assert
            Assert.False(result.Success);
            Assert.False(result.IsLocked);
            Assert.Equal(1, user.FailedLoginAttemptCount);
            _userRepositoryMock.Verify(r => r.UpdateUserAsync(user), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_GivenAWrongPasswordWithAttemptsStillRemaining_WhenAuthenticating_ThenTheErrorMessageStatesHowManyAttemptsRemain()
        {
            // Arrange — 3 prior failures; this 4th failure leaves 1 attempt before the 5-attempt lockout.
            var user = CreateUser(CorrectPassword, failedLoginAttemptCount: 3, firstFailedLoginTime: DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            GivenUserExists(user);

            // Act
            var result = await _sut.AuthenticateAsync(AccountName, "some-wrong-password");

            // Assert
            Assert.False(result.Success);
            Assert.False(result.IsLocked);
            Assert.Equal("Invalid username or password. 1 attempt(s) remaining.", result.ErrorMessage);
        }

        [Fact]
        public async Task AuthenticateAsync_GivenAWrongPasswordOnTheFifthConsecutiveAttempt_WhenAuthenticating_ThenLocksTheAccountForTheConfiguredDuration()
        {
            // Arrange — 4 prior failures; this 5th failure crosses the lockout threshold.
            var user = CreateUser(CorrectPassword, failedLoginAttemptCount: 4, firstFailedLoginTime: DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            GivenUserExists(user);
            GivenLockoutDurationMinutes(15);

            // Act
            var result = await _sut.AuthenticateAsync(AccountName, "some-wrong-password");

            // Assert
            Assert.False(result.Success);
            Assert.True(result.IsLocked);
            Assert.Equal(5, user.FailedLoginAttemptCount);
            Assert.Equal("Account is now locked due to 5 failed attempts. Try again in 15 minute(s), or contact your administrator.", result.ErrorMessage);
        }

        [Fact]
        public async Task AuthenticateAsync_GivenAnAccountLockedWithinTheConfiguredDuration_WhenAuthenticatingEvenWithTheCorrectPassword_ThenStillReturnsLocked()
        {
            // Arrange — locked just now; the configured 15-minute duration hasn't elapsed.
            var user = CreateUser(
                CorrectPassword,
                failedLoginAttemptCount: 5,
                firstFailedLoginTime: DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            GivenUserExists(user);
            GivenLockoutDurationMinutes(15);

            // Act
            var result = await _sut.AuthenticateAsync(AccountName, CorrectPassword);

            // Assert
            Assert.False(result.Success);
            Assert.True(result.IsLocked);
        }

        [Fact]
        public async Task AuthenticateAsync_GivenAnAdministratorHasConfiguredALongerDuration_WhenAuthenticatingWithinThatWindow_ThenStillReturnsLockedEvenPastTheOldFixedFifteenMinutes()
        {
            // Arrange — locked 20 minutes ago, but the admin has configured a 30-minute
            // duration. A hardcoded 15-minute assumption would incorrectly treat this as
            // expired; the actual duration must come from IAppSettingsRepository.
            long twentyMinutesAgo = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - (20 * 60);
            var user = CreateUser(CorrectPassword, failedLoginAttemptCount: 5, firstFailedLoginTime: twentyMinutesAgo);
            GivenUserExists(user);
            GivenLockoutDurationMinutes(30);

            // Act
            var result = await _sut.AuthenticateAsync(AccountName, CorrectPassword);

            // Assert
            Assert.False(result.Success);
            Assert.True(result.IsLocked);
        }

        [Fact]
        public async Task AuthenticateAsync_GivenThirtySecondsRemainingOnTheLockout_WhenAuthenticating_ThenTheRemainingTimeRoundsUpToOneMinuteRatherThanZero()
        {
            // Arrange — 14.5 minutes into a 15-minute lockout: 30 seconds remain.
            long fourteenAndHalfMinutesAgo = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - (14 * 60 + 30);
            var user = CreateUser(CorrectPassword, failedLoginAttemptCount: 5, firstFailedLoginTime: fourteenAndHalfMinutesAgo);
            GivenUserExists(user);
            GivenLockoutDurationMinutes(15);

            // Act
            var result = await _sut.AuthenticateAsync(AccountName, CorrectPassword);

            // Assert — a user should never be told "try again in 0 minutes".
            Assert.False(result.Success);
            Assert.Equal("Account locked. Try again in 1 minute(s), or contact your administrator.", result.ErrorMessage);
        }

        [Fact]
        public async Task AuthenticateAsync_GivenAnAccountLockedButTheConfiguredDurationHasElapsed_WhenAuthenticatingWithTheCorrectPassword_ThenAutoResetsAndSucceeds()
        {
            // Arrange — locked 16 minutes ago; the configured 15-minute duration has elapsed.
            long sixteenMinutesAgo = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - (16 * 60);
            var user = CreateUser(
                CorrectPassword,
                failedLoginAttemptCount: 5,
                firstFailedLoginTime: sixteenMinutesAgo);
            GivenUserExists(user);
            GivenLockoutDurationMinutes(15);

            // Act
            var result = await _sut.AuthenticateAsync(AccountName, CorrectPassword);

            // Assert
            Assert.True(result.Success);
            Assert.False(result.IsLocked);
            Assert.Equal(0, user.FailedLoginAttemptCount);
            Assert.Equal(0L, user.FirstFailedLoginTime);
        }

        [Fact]
        public async Task AuthenticateAsync_GivenAnAccountLockedButTheConfiguredDurationHasElapsed_WhenAuthenticatingWithAWrongPassword_ThenStartsANewFailureStreakInsteadOfStayingLocked()
        {
            // Arrange — locked 16 minutes ago; duration elapsed, but this attempt uses the wrong password.
            long sixteenMinutesAgo = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - (16 * 60);
            var user = CreateUser(
                CorrectPassword,
                failedLoginAttemptCount: 5,
                firstFailedLoginTime: sixteenMinutesAgo);
            GivenUserExists(user);
            GivenLockoutDurationMinutes(15);

            // Act
            var result = await _sut.AuthenticateAsync(AccountName, "some-wrong-password");

            // Assert — auto-unlock clears the old streak, then this failure starts a fresh one at 1, not 6.
            Assert.False(result.Success);
            Assert.False(result.IsLocked);
            Assert.Equal(1, user.FailedLoginAttemptCount);
        }

        [Fact]
        public async Task AuthenticateAsync_GivenAPasswordOlderThanNinetyDays_WhenAuthenticatingWithTheCorrectPassword_ThenReturnsAnExpiredError()
        {
            // Arrange
            long ninetyOneDaysAgo = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - (91L * 24 * 60 * 60);
            var user = CreateUser(CorrectPassword, passwordModificationDate: ninetyOneDaysAgo);
            GivenUserExists(user);

            // Act
            var result = await _sut.AuthenticateAsync(AccountName, CorrectPassword);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("expired", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task AuthenticateAsync_GivenAPasswordWithinNinetyDays_WhenAuthenticatingWithTheCorrectPassword_ThenSucceeds()
        {
            // Arrange
            long eightyNineDaysAgo = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - (89L * 24 * 60 * 60);
            var user = CreateUser(CorrectPassword, passwordModificationDate: eightyNineDaysAgo);
            GivenUserExists(user);

            // Act
            var result = await _sut.AuthenticateAsync(AccountName, CorrectPassword);

            // Assert
            Assert.True(result.Success);
        }
    }
}
