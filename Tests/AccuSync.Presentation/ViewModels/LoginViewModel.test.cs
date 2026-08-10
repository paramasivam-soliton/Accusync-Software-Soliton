// --------------------------------------------------------------------------------
// <copyright file="LoginViewModel.test.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Core.Abstractions.Repositories;
using AccuSync.Core.Abstractions.Services;
using AccuSync.Core.Entities;
using AccuSync.Presentation.ViewModels;
using Moq;

namespace AccuSync.Presentation.Tests.ViewModels
{
    /// <summary>
    /// Covers the username dropdown's active-only filtering and role-on-sign-in
    /// wiring — not credential verification or lockout, which belong to the
    /// authentication logic itself and are already covered where that logic
    /// actually lives (AuthenticationServiceTests).
    /// </summary>
    public class LoginViewModelTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IAuthenticationService> _authenticationServiceMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly Mock<ICurrentUserContext> _currentUserContextMock;

        public LoginViewModelTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _userRepositoryMock.Setup(r => r.GetAllUsersAsync()).ReturnsAsync(new List<User>());

            _authenticationServiceMock = new Mock<IAuthenticationService>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _currentUserContextMock = new Mock<ICurrentUserContext>();
        }

        private LoginViewModel CreateSut() => new(
            _userRepositoryMock.Object,
            _authenticationServiceMock.Object,
            _passwordHasherMock.Object,
            _currentUserContextMock.Object);

        private static User NewUser(string profileId, int firstLogin = 0, string accountName = "SomeUser", bool status = true) => new()
        {
            AccountName = accountName,
            ProfileId = profileId,
            FirstLogin = firstLogin,
            Status = status
        };

        [Fact]
        public async Task GivenAMixOfActiveAndDeactivatedAccounts_WhenTheDropdownLoads_ThenListsOnlyTheActiveOnes()
        {
            // Arrange — loading happens fire-and-forget from the constructor, so the
            // mock must be set up beforehand.
            _userRepositoryMock.Setup(r => r.GetAllUsersAsync()).ReturnsAsync(new List<User>
            {
                NewUser(profileId: "Admin", accountName: "ActiveAdmin", status: true),
                NewUser(profileId: "Screener", accountName: "DeactivatedScreener", status: false),
                NewUser(profileId: "Screener", accountName: "ActiveScreener", status: true)
            });

            // Act
            var sut = CreateSut();
            await Task.Delay(50); // LoadUsernamesAsync is async void, fired from the constructor

            // Assert
            Assert.Equal(new[] { "ActiveAdmin", "ActiveScreener" }, sut.Usernames);
        }

        [Fact]
        public async Task GivenAUserWhoseStoredProfileIdIsAdmin_WhenSignedIn_ThenTheCurrentUserContextIsSignedInWithTheAdminRole()
        {
            // Arrange — the freshly-fetched User carries whatever role an admin most
            // recently assigned; the ViewModel must not use a cached/stale role.
            var sut = CreateSut();
            var user = NewUser(profileId: "Admin");
            _authenticationServiceMock
                .Setup(a => a.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new AuthenticationResult { Success = true, User = user });

            // Act
            sut.SignInCommand.Execute(null);
            await Task.Delay(50); // SignInCommand's handler is async void via RelayCommand

            // Assert
            _currentUserContextMock.Verify(c => c.SignIn(user, UserRole.Admin), Times.Once);
        }

        [Fact]
        public async Task GivenAUserWhoseStoredProfileIdIsScreener_WhenSignedIn_ThenTheCurrentUserContextIsSignedInWithTheScreenerRole()
        {
            // Arrange
            var sut = CreateSut();
            var user = NewUser(profileId: "Screener");
            _authenticationServiceMock
                .Setup(a => a.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new AuthenticationResult { Success = true, User = user });

            // Act
            sut.SignInCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _currentUserContextMock.Verify(c => c.SignIn(user, UserRole.Screener), Times.Once);
        }

        [Fact]
        public async Task GivenASuccessfulNonFirstLoginSignIn_WhenSignedIn_ThenTheLoginSucceededEventCarriesTheParsedRoleAsText()
        {
            // Arrange
            var sut = CreateSut();
            var user = NewUser(profileId: "Admin", firstLogin: 0);
            _authenticationServiceMock
                .Setup(a => a.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new AuthenticationResult { Success = true, User = user });

            string? raisedRole = null;
            sut.LoginSucceeded += (_, role) => raisedRole = role;

            // Act
            sut.SignInCommand.Execute(null);
            await Task.Delay(50);

            // Assert — this is what App.NavigateAfterLogin uses to pick a dashboard,
            // so it must reflect the freshly-parsed role, not a hardcoded string.
            Assert.Equal(nameof(UserRole.Admin), raisedRole);
        }

        [Fact]
        public async Task GivenAFailedSignIn_WhenAuthenticationFails_ThenTheCurrentUserContextIsNeverSignedIn()
        {
            // Arrange
            var sut = CreateSut();
            _authenticationServiceMock
                .Setup(a => a.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new AuthenticationResult { Success = false, ErrorMessage = "Invalid username or password." });

            // Act
            sut.SignInCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _currentUserContextMock.Verify(c => c.SignIn(It.IsAny<User>(), It.IsAny<UserRole>()), Times.Never);
        }
    }
}
