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
    /// IUserRepository is mocked (no real persistence needed to exercise the
    /// authentication rules), but PasswordHasher is real — so these tests verify
    /// AuthenticationService's actual hash-verification behavior, not just that
    /// some mock returned what it was told to.
    /// </summary>
    public class AuthenticationServiceTests
    {
        private const string AccountName = "Screener";
        private const string CorrectPassword = "Password@123";

        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly PasswordHasher _passwordHasher;
        private readonly AuthenticationService _sut;

        public AuthenticationServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _passwordHasher = new PasswordHasher();
            _sut = new AuthenticationService(_userRepositoryMock.Object, _passwordHasher);
        }

        private User CreateUser(
            string plainPassword,
            int failedLoginAttemptCount = 0,
            long firstFailedLoginTime = 0L,
            long? passwordModificationDate = null)
        {
            return new User
            {
                AccountName = AccountName,
                ProfilePassword = _passwordHasher.Hash(plainPassword),
                FailedLoginAttemptCount = failedLoginAttemptCount,
                FirstFailedLoginTime = firstFailedLoginTime,
                PasswordModificationDate = passwordModificationDate ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds()
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
            // Arrange — 7 prior failures; this 8th failure leaves 2 attempts before the 10-attempt lockout.
            var user = CreateUser(CorrectPassword, failedLoginAttemptCount: 7, firstFailedLoginTime: DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            GivenUserExists(user);

            // Act
            var result = await _sut.AuthenticateAsync(AccountName, "some-wrong-password");

            // Assert
            Assert.False(result.Success);
            Assert.False(result.IsLocked);
            Assert.Equal("Invalid username or password. 2 attempt(s) remaining.", result.ErrorMessage);
        }

        [Fact]
        public async Task AuthenticateAsync_GivenAWrongPasswordOnTheTenthConsecutiveAttempt_WhenAuthenticating_ThenLocksTheAccount()
        {
            // Arrange — 9 prior failures; this 10th failure crosses the lockout threshold.
            var user = CreateUser(CorrectPassword, failedLoginAttemptCount: 9, firstFailedLoginTime: DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            GivenUserExists(user);

            // Act
            var result = await _sut.AuthenticateAsync(AccountName, "some-wrong-password");

            // Assert
            Assert.False(result.Success);
            Assert.True(result.IsLocked);
            Assert.Equal(10, user.FailedLoginAttemptCount);
            Assert.Contains("locked", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task AuthenticateAsync_GivenAnAccountLockedWithinTheCooldownWindow_WhenAuthenticatingEvenWithTheCorrectPassword_ThenStillReturnsLocked()
        {
            // Arrange — locked just now; the 15-minute cooldown hasn't elapsed.
            var user = CreateUser(
                CorrectPassword,
                failedLoginAttemptCount: 10,
                firstFailedLoginTime: DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            GivenUserExists(user);

            // Act
            var result = await _sut.AuthenticateAsync(AccountName, CorrectPassword);

            // Assert
            Assert.False(result.Success);
            Assert.True(result.IsLocked);
        }

        [Fact]
        public async Task AuthenticateAsync_GivenAnAccountLockedButTheCooldownHasElapsed_WhenAuthenticatingWithTheCorrectPassword_ThenAutoResetsAndSucceeds()
        {
            // Arrange — locked 16 minutes ago; the 15-minute cooldown has elapsed.
            long sixteenMinutesAgo = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - (16 * 60);
            var user = CreateUser(
                CorrectPassword,
                failedLoginAttemptCount: 10,
                firstFailedLoginTime: sixteenMinutesAgo);
            GivenUserExists(user);

            // Act
            var result = await _sut.AuthenticateAsync(AccountName, CorrectPassword);

            // Assert
            Assert.True(result.Success);
            Assert.False(result.IsLocked);
            Assert.Equal(0, user.FailedLoginAttemptCount);
            Assert.Equal(0L, user.FirstFailedLoginTime);
        }

        [Fact]
        public async Task AuthenticateAsync_GivenAnAccountLockedButTheCooldownHasElapsed_WhenAuthenticatingWithAWrongPassword_ThenStartsANewFailureStreakInsteadOfStayingLocked()
        {
            // Arrange — locked 16 minutes ago; cooldown elapsed, but this attempt uses the wrong password.
            long sixteenMinutesAgo = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - (16 * 60);
            var user = CreateUser(
                CorrectPassword,
                failedLoginAttemptCount: 10,
                firstFailedLoginTime: sixteenMinutesAgo);
            GivenUserExists(user);

            // Act
            var result = await _sut.AuthenticateAsync(AccountName, "some-wrong-password");

            // Assert — auto-unlock clears the old streak, then this failure starts a fresh one at 1, not 11.
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
