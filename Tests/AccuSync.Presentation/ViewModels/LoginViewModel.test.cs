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
    /// Scoped narrowly to profile-on-sign-in behavior — a freshly assigned profile
    /// takes effect on the user's next login — rather than re-testing credential
    /// verification, lockout, or dropdown loading, which belong to the login flow
    /// itself and are already covered where that logic actually lives
    /// (AuthenticationServiceTests).
    /// </summary>
    public class LoginViewModelTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IAuthenticationService> _authenticationServiceMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly Mock<ICurrentUserContext> _currentUserContextMock;
        private readonly Mock<IProfileRepository> _profileRepositoryMock;

        public LoginViewModelTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _userRepositoryMock.Setup(r => r.GetAllUsersAsync()).ReturnsAsync(new List<User>());

            _authenticationServiceMock = new Mock<IAuthenticationService>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _currentUserContextMock = new Mock<ICurrentUserContext>();
            _profileRepositoryMock = new Mock<IProfileRepository>();
        }

        private LoginViewModel CreateSut() => new(
            _userRepositoryMock.Object,
            _authenticationServiceMock.Object,
            _passwordHasherMock.Object,
            _currentUserContextMock.Object,
            _profileRepositoryMock.Object);

        private static User NewUser(string profileId, int firstLogin = 0) => new()
        {
            AccountName = "SomeUser",
            ProfileId = profileId,
            FirstLogin = firstLogin
        };

        private void SetUpProfile(string profileId) =>
            _profileRepositoryMock
                .Setup(r => r.GetProfileByIdAsync(profileId))
                .ReturnsAsync(new Profile { Id = profileId, Name = profileId });

        [Fact]
        public async Task GivenAUserWhoseStoredProfileIdIsAdmin_WhenSignedIn_ThenTheCurrentUserContextIsSignedInWithTheAdminProfile()
        {
            // Arrange — the freshly-fetched User carries whatever profile an admin most
            // recently assigned; the ViewModel must not use a cached/stale profile.
            var sut = CreateSut();
            var user = NewUser(profileId: "Admin");
            SetUpProfile("Admin");
            _authenticationServiceMock
                .Setup(a => a.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new AuthenticationResult { Success = true, User = user });

            // Act
            sut.SignInCommand.Execute(null);
            await Task.Delay(50); // SignInCommand's handler is async void via RelayCommand

            // Assert
            _currentUserContextMock.Verify(c => c.SignIn(user, It.Is<Profile>(p => p.Id == "Admin")), Times.Once);
        }

        [Fact]
        public async Task GivenAUserWhoseStoredProfileIdIsScreener_WhenSignedIn_ThenTheCurrentUserContextIsSignedInWithTheScreenerProfile()
        {
            // Arrange
            var sut = CreateSut();
            var user = NewUser(profileId: "Screener");
            SetUpProfile("Screener");
            _authenticationServiceMock
                .Setup(a => a.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new AuthenticationResult { Success = true, User = user });

            // Act
            sut.SignInCommand.Execute(null);
            await Task.Delay(50);

            // Assert
            _currentUserContextMock.Verify(c => c.SignIn(user, It.Is<Profile>(p => p.Id == "Screener")), Times.Once);
        }

        [Fact]
        public async Task GivenASuccessfulNonFirstLoginSignIn_WhenSignedIn_ThenTheLoginSucceededEventCarriesTheResolvedProfileName()
        {
            // Arrange
            var sut = CreateSut();
            var user = NewUser(profileId: "Admin", firstLogin: 0);
            SetUpProfile("Admin");
            _authenticationServiceMock
                .Setup(a => a.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new AuthenticationResult { Success = true, User = user });

            string? raisedProfileName = null;
            sut.LoginSucceeded += (_, profileName) => raisedProfileName = profileName;

            // Act
            sut.SignInCommand.Execute(null);
            await Task.Delay(50);

            // Assert — this is what App.NavigateAfterLogin uses to pick a dashboard,
            // so it must reflect the freshly-resolved profile, not a hardcoded string.
            Assert.Equal("Admin", raisedProfileName);
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
            _currentUserContextMock.Verify(c => c.SignIn(It.IsAny<User>(), It.IsAny<Profile>()), Times.Never);
        }
    }
}
