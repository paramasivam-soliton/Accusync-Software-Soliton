// --------------------------------------------------------------------------------
// <copyright file="LoginViewModelTests.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Application.Resources;
using AccuSync.Core.Abstractions.Services;
using AccuSync.Core.Entities;
using AccuSync.Presentation.ViewModels;
using Moq;

namespace AccuSync.Presentation.Tests.ViewModels
{
    /// <summary>
    /// IUserService, IAuthenticationService, IPasswordHasher, and ICurrentUserContext
    /// are all mocked — this ViewModel has no pure-logic collaborator to keep real,
    /// only orchestration over these service boundaries.
    /// </summary>
    public class LoginViewModelTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IAuthenticationService> _authenticationServiceMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly Mock<ICurrentUserContext> _currentUserContextMock;

        public LoginViewModelTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _authenticationServiceMock = new Mock<IAuthenticationService>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _currentUserContextMock = new Mock<ICurrentUserContext>();

            _userServiceMock
                .Setup(s => s.GetAllUsersAsync())
                .ReturnsAsync(new List<User>());
        }

        private LoginViewModel CreateLoginViewModel()
        {
            return new LoginViewModel(
                _userServiceMock.Object,
                _authenticationServiceMock.Object,
                _passwordHasherMock.Object,
                _currentUserContextMock.Object);
        }

        private static User NewUser(UserRole role, int firstLogin = 0, string accountName = "SomeUser", bool isActive = true) => new()
        {
            AccountName = accountName,
            ProfileId = (int)role,
            FirstLogin = firstLogin,
            IsActive = isActive
        };

        [Fact]
        public void LoadUsernamesAsync_RegisteredUsersExist_UsernamesArePopulatedAndTheFirstOneIsSelected()
        {
            _userServiceMock
                .Setup(s => s.GetAllUsersAsync())
                .ReturnsAsync(new List<User>
                {
                    new() { AccountName = "Admin" },
                    new() { AccountName = "Screener" }
                });

            var loginViewModel = CreateLoginViewModel();

            Assert.Equal(2, loginViewModel.Usernames.Count);
            Assert.Equal("Admin", loginViewModel.SelectedUsername);
        }

        [Fact]
        public void LoadUsernamesAsync_MixOfActiveAndDeactivatedAccounts_ListsOnlyTheActiveOnes()
        {
            _userServiceMock
                .Setup(s => s.GetAllUsersAsync())
                .ReturnsAsync(new List<User>
                {
                    NewUser(role: UserRole.Admin, accountName: "ActiveAdmin", isActive: true),
                    NewUser(role: UserRole.Screener, accountName: "DeactivatedScreener", isActive: false),
                    NewUser(role: UserRole.Screener, accountName: "ActiveScreener", isActive: true)
                });

            var loginViewModel = CreateLoginViewModel();

            Assert.Equal(new[] { "ActiveAdmin", "ActiveScreener" }, loginViewModel.Usernames);
        }

        [Fact]
        public void LoadUsernamesAsync_TheUserServiceThrows_SetsGenericFailedToLoadUsersErrorMessage()
        {
            _userServiceMock
                .Setup(s => s.GetAllUsersAsync())
                .ThrowsAsync(new InvalidOperationException("Simulated database failure."));

            var loginViewModel = CreateLoginViewModel();

            Assert.Equal(
                string.Format(Strings.LoginViewModel_FailedToLoadUsers, ErrorCode.Unexpected.ToDisplayCode()),
                loginViewModel.ErrorMessage);
            Assert.DoesNotContain("Simulated database failure.", loginViewModel.ErrorMessage);
        }

        [Fact]
        public async Task SignInAsync_SuccessfulAuthenticationForAReturningUser_RaisesLoginSucceededAndClearsThePassword()
        {
            var loginViewModel = CreateLoginViewModel();
            loginViewModel.SelectedUsername = "Screener";
            loginViewModel.Password = "Password@123";

            var user = new User { AccountName = "Screener", ProfileId = (int)UserRole.Screener, FirstLogin = 0 };
            _authenticationServiceMock
                .Setup(a => a.AuthenticateAsync("Screener", "Password@123"))
                .ReturnsAsync(new AuthenticationResult { Success = true, User = user });

            (string AccountName, string Role)? raised = null;
            loginViewModel.LoginSucceeded += (accountName, role) => raised = (accountName, role);

            await loginViewModel.SignInAsync();

            Assert.NotNull(raised);
            Assert.Equal("Screener", raised!.Value.AccountName);
            Assert.Equal(string.Empty, loginViewModel.Password);
        }

        [Fact]
        public async Task SignInAsync_SuccessfulAuthenticationForAFirstLoginUser_RaisesFirstLoginPasswordChangeRequiredInstead()
        {
            var loginViewModel = CreateLoginViewModel();
            loginViewModel.SelectedUsername = "Admin";
            loginViewModel.Password = "Password@123";

            var user = new User { AccountName = "Admin", ProfileId = (int)UserRole.Admin, FirstLogin = 1 };
            _authenticationServiceMock
                .Setup(a => a.AuthenticateAsync("Admin", "Password@123"))
                .ReturnsAsync(new AuthenticationResult { Success = true, User = user });

            ChangePasswordViewModel raisedViewModel = null!;
            bool loginSucceededRaised = false;
            loginViewModel.FirstLoginPasswordChangeRequired += vm => raisedViewModel = vm;
            loginViewModel.LoginSucceeded += (_, _) => loginSucceededRaised = true;

            await loginViewModel.SignInAsync();

            Assert.NotNull(raisedViewModel);
            Assert.False(loginSucceededRaised);
        }

        [Fact]
        public async Task SignInAsync_SuccessfulAuthenticationWithAnExpiredPassword_RaisesPasswordExpiredPasswordChangeRequiredInstead()
        {
            var loginViewModel = CreateLoginViewModel();
            loginViewModel.SelectedUsername = "Screener";
            loginViewModel.Password = "Password@123";

            var user = new User { AccountName = "Screener", ProfileId = (int)UserRole.Screener, FirstLogin = 0 };
            _authenticationServiceMock
                .Setup(a => a.AuthenticateAsync("Screener", "Password@123"))
                .ReturnsAsync(new AuthenticationResult { Success = true, User = user, IsPasswordExpired = true });

            ChangePasswordViewModel raisedViewModel = null!;
            bool loginSucceededRaised = false;
            bool firstLoginRaised = false;
            loginViewModel.PasswordExpiredPasswordChangeRequired += vm => raisedViewModel = vm;
            loginViewModel.LoginSucceeded += (_, _) => loginSucceededRaised = true;
            loginViewModel.FirstLoginPasswordChangeRequired += _ => firstLoginRaised = true;

            await loginViewModel.SignInAsync();

            Assert.NotNull(raisedViewModel);
            Assert.False(loginSucceededRaised);
            Assert.False(firstLoginRaised);
        }

        [Fact]
        public async Task SignInAsync_FailedAuthentication_SetsTheReturnedErrorMessageAndClearsThePassword()
        {
            var loginViewModel = CreateLoginViewModel();
            loginViewModel.SelectedUsername = "Screener";
            loginViewModel.Password = "wrong-password";

            _authenticationServiceMock
                .Setup(a => a.AuthenticateAsync("Screener", "wrong-password"))
                .ReturnsAsync(new AuthenticationResult { Success = false, ErrorMessage = "Invalid username or password." });

            await loginViewModel.SignInAsync();

            Assert.Equal("Invalid username or password.", loginViewModel.ErrorMessage);
            Assert.Equal(string.Empty, loginViewModel.Password);
        }

        [Fact]
        public async Task SignInAsync_TheAuthenticationServiceThrows_SetsGenericUnexpectedErrorMessage()
        {
            var loginViewModel = CreateLoginViewModel();
            loginViewModel.SelectedUsername = "Screener";
            loginViewModel.Password = "Password@123";

            _authenticationServiceMock
                .Setup(a => a.AuthenticateAsync("Screener", "Password@123"))
                .ThrowsAsync(new InvalidOperationException("Simulated failure."));

            await loginViewModel.SignInAsync();

            Assert.Equal(
                string.Format(Strings.LoginViewModel_UnexpectedError, ErrorCode.Unexpected.ToDisplayCode()),
                loginViewModel.ErrorMessage);
            Assert.DoesNotContain("Simulated failure.", loginViewModel.ErrorMessage);
        }

        [Fact]
        public async Task SignInAsync_ASignInAttemptCompletes_IsLoadingEndsFalse()
        {
            var loginViewModel = CreateLoginViewModel();
            loginViewModel.SelectedUsername = "Screener";
            loginViewModel.Password = "Password@123";

            _authenticationServiceMock
                .Setup(a => a.AuthenticateAsync("Screener", "Password@123"))
                .ReturnsAsync(new AuthenticationResult { Success = false, ErrorMessage = "Invalid username or password." });

            await loginViewModel.SignInAsync();

            Assert.False(loginViewModel.IsLoading);
        }

        // --- Role-on-sign-in behavior: a freshly assigned role takes effect on the
        // user's next login. Scoped narrowly to that; credential verification and
        // dropdown loading are covered above and in AuthenticationServiceTests.

        [Fact]
        public async Task SignInAsync_UserWhoseStoredProfileIdIsAdmin_SetsCurrentUserContextWithAdminRole()
        {
            var loginViewModel = CreateLoginViewModel();
            var user = NewUser(role: UserRole.Admin);
            _authenticationServiceMock
                .Setup(a => a.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new AuthenticationResult { Success = true, User = user });

            await loginViewModel.SignInAsync();

            _currentUserContextMock.Verify(c => c.Set(user, UserRole.Admin), Times.Once);
        }

        [Fact]
        public async Task SignInAsync_UserWhoseStoredProfileIdIsScreener_SetsCurrentUserContextWithScreenerRole()
        {
            var loginViewModel = CreateLoginViewModel();
            var user = NewUser(role: UserRole.Screener);
            _authenticationServiceMock
                .Setup(a => a.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new AuthenticationResult { Success = true, User = user });

            await loginViewModel.SignInAsync();

            _currentUserContextMock.Verify(c => c.Set(user, UserRole.Screener), Times.Once);
        }

        [Fact]
        public async Task SignInAsync_SuccessfulNonFirstLoginSignIn_LoginSucceededEventCarriesTheParsedRoleAsText()
        {
            var loginViewModel = CreateLoginViewModel();
            var user = NewUser(role: UserRole.Admin, firstLogin: 0);
            _authenticationServiceMock
                .Setup(a => a.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new AuthenticationResult { Success = true, User = user });

            string? raisedRole = null;
            loginViewModel.LoginSucceeded += (_, role) => raisedRole = role;

            await loginViewModel.SignInAsync();

            Assert.Equal(nameof(UserRole.Admin), raisedRole);
        }

        [Fact]
        public async Task SignInAsync_FailedAuthentication_NeverSetsCurrentUserContext()
        {
            var loginViewModel = CreateLoginViewModel();
            _authenticationServiceMock
                .Setup(a => a.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new AuthenticationResult { Success = false, ErrorMessage = "Invalid username or password." });

            await loginViewModel.SignInAsync();

            _currentUserContextMock.Verify(c => c.Set(It.IsAny<User>(), It.IsAny<UserRole>()), Times.Never);
        }
    }
}
