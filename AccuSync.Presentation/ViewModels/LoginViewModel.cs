// --------------------------------------------------------------------------------
// <copyright file="LoginViewModel.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Presentation.Helpers;
using AccuSync.Application.Helpers;
using AccuSync.Core.Abstractions.Services;
using AccuSync.Core.Entities;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using AccuSync.Application.Resources;

namespace AccuSync.Presentation.ViewModels
{
    /// <summary>
    /// Drives the login screen. Loads available usernames into a dropdown
    /// and delegates credential verification to <see cref="IAuthenticationService"/>.
    /// Raises <see cref="LoginSucceeded"/>, <see cref="FirstLoginPasswordChangeRequired"/>,
    /// or <see cref="PasswordExpiredPasswordChangeRequired"/> so the hosting View decides
    /// how to navigate — this VM has no dependency on any concrete Window type, so it
    /// works whether the View lives in this project or another.
    /// </summary>
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly IUserService _userService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ICurrentUserContext _currentUserContext;

        /// <summary>
        /// Creates the view model and begins loading available usernames.
        /// </summary>
        /// <param name="userService">Service used to fetch the list of registered users.</param>
        /// <param name="authenticationService">Service used to verify credentials.</param>
        /// <param name="passwordHasher">Service passed through to the password-change flow.</param>
        /// <param name="currentUserContext">Signed in with the authenticated user and their role on a successful, non-first-login sign-in.</param>
        public LoginViewModel(IUserService userService, IAuthenticationService authenticationService, IPasswordHasher passwordHasher, ICurrentUserContext currentUserContext)
        {
            _userService = userService;
            _authenticationService = authenticationService;
            _passwordHasher = passwordHasher;
            _currentUserContext = currentUserContext;

            Usernames = new ObservableCollection<string>();
            _isPasswordVisible = false;
            SignInCommand = new RelayCommand(async () => await SignInAsync(), () => !IsLoading);

            // BUG: fire-and-forget — if this throws after the constructor returns,
            //      the exception is unobserved and the dropdown stays empty with no
            //      error shown. Consider an async initialization pattern.
            _ = LoadUsernamesAsync();
        }

        /// <summary>Account names for the dropdown, loaded once on construction.</summary>
        public ObservableCollection<string> Usernames { get; set; }

        private string _selectedUsername;

        /// <summary>The username currently selected in the dropdown.</summary>
        public string SelectedUsername
        {
            get => _selectedUsername;
            set
            {
                _selectedUsername = value;
                OnPropertyChanged();
                ErrorMessage = string.Empty;
            }
        }

        private string _password = string.Empty;

        /// <summary>The password as typed.</summary>
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
                ErrorMessage = string.Empty;
            }
        }

        private string _errorMessage = string.Empty;

        /// <summary>Message shown to the user when loading usernames or signing in fails.</summary>
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        private bool _isLoading;

        /// <summary>Whether a sign-in attempt is in progress.</summary>
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        private bool _isPasswordVisible;

        /// <summary>Whether the password field shows plaintext instead of masked characters.</summary>
        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set
            {
                _isPasswordVisible = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Command bound to the Sign In button.</summary>
        public ICommand SignInCommand { get; }

        /// <summary>Raised after a successful, non-first-login sign-in. Carries (username, role).</summary>
        public event Action<string, string> LoginSucceeded;

        /// <summary>Raised when the authenticated user must change their password before continuing.</summary>
        public event Action<ChangePasswordViewModel> FirstLoginPasswordChangeRequired;

        /// <summary>Raised when the authenticated user's password is 90+ days old and must
        /// be changed before continuing.</summary>
        public event Action<ChangePasswordViewModel> PasswordExpiredPasswordChangeRequired;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        // internal (not private): lets tests await this deterministically instead of
        // relying on the constructor's fire-and-forget call having already completed.
        internal async Task LoadUsernamesAsync()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                foreach (var user in users.Where(u => u.IsActive))
                {
                    Usernames.Add(user.AccountName);
                }

                if (Usernames.Count > 0)
                {
                    SelectedUsername = Usernames[0];
                }
            }
            catch (Exception)
            {
                // TODO: Log the exception once a logger is introduced into the solution.
                ErrorMessage = string.Format(Strings.LoginViewModel_FailedToLoadUsers, ErrorCode.Unexpected.ToDisplayCode());
            }
        }

        // internal (not private): lets tests await the command's underlying operation
        // directly instead of racing RelayCommand's async-void Execute.
        internal async Task SignInAsync()
        {
            ErrorMessage = string.Empty;
            IsLoading = true;

            try
            {
                var result = await _authenticationService.AuthenticateAsync(SelectedUsername, Password);

                if (result.Success)
                {
                    var role = UserRoleParser.Parse(result.User.ProfileId);
                    _currentUserContext.Set(result.User, role);

                    if (result.User.FirstLogin == 1)
                    {
                        var changePasswordViewModel = new ChangePasswordViewModel(
                            _userService,
                            _passwordHasher,
                            _currentUserContext,
                            result.User
                        );
                        FirstLoginPasswordChangeRequired?.Invoke(changePasswordViewModel);
                    }
                    else if (result.IsPasswordExpired)
                    {
                        var changePasswordViewModel = new ChangePasswordViewModel(
                            _userService,
                            _passwordHasher,
                            _currentUserContext,
                            result.User,
                            isPasswordExpiredReset: true
                        );
                        PasswordExpiredPasswordChangeRequired?.Invoke(changePasswordViewModel);
                    }
                    else
                    {
                        LoginSucceeded?.Invoke(result.User.AccountName, role.ToString());
                    }

                    Password = string.Empty;
                }
                else
                {
                    // Order matters: the Password setter itself clears ErrorMessage
                    // (so a fresh attempt starts without a stale error), so it must run
                    // before ErrorMessage is set here — not after, or the message set on
                    // this line would be wiped out immediately.
                    Password = string.Empty;
                    ErrorMessage = result.ErrorMessage;
                }
            }
            catch (Exception)
            {
                // TODO: Log the exception once a logger is introduced into the solution.
                ErrorMessage = string.Format(Strings.LoginViewModel_UnexpectedError, ErrorCode.Unexpected.ToDisplayCode());
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
