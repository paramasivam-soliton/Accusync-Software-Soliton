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
    /// Scoped narrowly to ASWD-36's role-on-sign-in behavior — "role changes take
    /// effect on the user's next login" — rather than re-testing credential
    /// verification, lockout, or dropdown loading, which belong to the login flow
    /// itself (ASWD-41) and are already covered where that logic actually lives
    /// (AuthenticationServiceTests).
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

        private static User NewUser(string profileId, int firstLogin = 0) => new()
        {
            AccountName = "SomeUser",
            ProfileId = profileId,
            FirstLogin = firstLogin
        };

        [Fact]
        public async Task SignInCommand_GivenAUserWhoseStoredProfileIdIsAdmin_WhenSignedIn_ThenTheCurrentUserContextIsSignedInWithTheAdminRole()
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
        public async Task SignInCommand_GivenAUserWhoseStoredProfileIdIsScreener_WhenSignedIn_ThenTheCurrentUserContextIsSignedInWithTheScreenerRole()
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
        public async Task SignInCommand_GivenASuccessfulNonFirstLoginSignIn_WhenSignedIn_ThenTheLoginSucceededEventCarriesTheParsedRoleAsText()
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
        public async Task SignInCommand_GivenAFailedSignIn_WhenAuthenticationFails_ThenTheCurrentUserContextIsNeverSignedIn()
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
