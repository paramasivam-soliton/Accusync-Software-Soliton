// --------------------------------------------------------------------------------
// <copyright file="LoginViewModel.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Presentation.Helpers;
using AccuSync.Application.Helpers;
using AccuSync.Core.Abstractions.Repositories;
using AccuSync.Core.Abstractions.Services;
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
    /// Raises <see cref="LoginSucceeded"/> or <see cref="FirstLoginPasswordChangeRequired"/>
    /// so the hosting View decides how to navigate — this VM has no dependency on any
    /// concrete Window type, so it works whether the View lives in this project or another.
    /// </summary>
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthenticationService _authenticationService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ICurrentUserContext _currentUserContext;

        private string _selectedUsername;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading;
        private bool _isPasswordVisible;

        /// <summary>Account names for the dropdown, loaded once on construction.</summary>
        public ObservableCollection<string> Usernames { get; set; }

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

        /// <summary>Creates the view model and kicks off loading the username dropdown.</summary>
        public LoginViewModel(IUserRepository userRepository, IAuthenticationService authenticationService, IPasswordHasher passwordHasher, ICurrentUserContext currentUserContext)
        {
            _userRepository = userRepository;
            _authenticationService = authenticationService;
            _passwordHasher = passwordHasher;
            _currentUserContext = currentUserContext;

            Usernames = new ObservableCollection<string>();
            _isPasswordVisible = false;
            SignInCommand = new RelayCommand(async () => await SignInAsync(), () => !IsLoading);

            // BUG: async void fire-and-forget — if this throws after the constructor
            //      returns, the exception is unobserved and the dropdown stays empty
            //      with no error shown. Consider an async initialization pattern.
            LoadUsernamesAsync();
        }

        private async void LoadUsernamesAsync()
        {
            try
            {
                var users = await _userRepository.GetAllUsersAsync();
                foreach (var user in users.Where(u => u.Status))
                {
                    Usernames.Add(user.AccountName);
                }

                if (Usernames.Count > 0)
                {
                    SelectedUsername = Usernames[0];
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = string.Format(Strings.LoginViewModel_FailedToLoadUsers, ex.Message);
            }
        }

        private async Task SignInAsync()
        {
            ErrorMessage = string.Empty;
            IsLoading = true;

            try
            {
                var result = await _authenticationService.AuthenticateAsync(SelectedUsername, Password);

                if (result.Success)
                {
                    var role = UserRoleParser.Parse(result.User.ProfileId);
                    _currentUserContext.SignIn(result.User, role);

                    if (result.User.FirstLogin == 1)
                    {
                        var changePasswordViewModel = new ChangePasswordViewModel(
                            _userRepository,
                            _passwordHasher,
                            _currentUserContext,
                            result.User
                        );
                        FirstLoginPasswordChangeRequired?.Invoke(changePasswordViewModel);
                    }
                    else
                    {
                        LoginSucceeded?.Invoke(result.User.AccountName, role.ToString());
                    }

                    Password = string.Empty;
                }
                else
                {
                    Password = string.Empty;
                    ErrorMessage = result.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = string.Format(Strings.LoginViewModel_UnexpectedError, ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}