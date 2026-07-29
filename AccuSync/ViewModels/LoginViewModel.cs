// --------------------------------------------------------------------------------
// <copyright file="LoginViewModel.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Helpers;
using AccuSync.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using AccuSync.Resources;

namespace AccuSync.ViewModels
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
        private readonly IDatabaseService _databaseService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IEncryptionService _encryptionService;

        private string _selectedUsername;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading;
        private bool _isPasswordVisible;

        public ObservableCollection<string> Usernames { get; set; }

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

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set
            {
                _isPasswordVisible = value;
                OnPropertyChanged();
            }
        }

        public ICommand SignInCommand { get; }

        /// <summary>Raised after a successful, non-first-login sign-in. Carries (username, role).</summary>
        public event Action<string, string> LoginSucceeded;

        /// <summary>Raised when the authenticated user must change their password before continuing.</summary>
        public event Action<ChangePasswordViewModel> FirstLoginPasswordChangeRequired;

        public LoginViewModel(IDatabaseService databaseService, IAuthenticationService authenticationService, IEncryptionService encryptionService)
        {
            _databaseService = databaseService;
            _authenticationService = authenticationService;
            _encryptionService = encryptionService;

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
                var users = await _databaseService.GetAllUsersAsync();
                foreach (var user in users)
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
                    if (result.User.FirstLogin == 1)
                    {
                        var changePasswordViewModel = new ChangePasswordViewModel(
                            _databaseService,
                            _encryptionService,
                            result.User
                        );
                        FirstLoginPasswordChangeRequired?.Invoke(changePasswordViewModel);
                    }
                    else
                    {
                        // TODO: Use an actual Role field from the User model.
                        string role = string.Equals(result.User.AccountName, "Admin", StringComparison.OrdinalIgnoreCase)
                            ? "Admin"
                            : "Screener";

                        LoginSucceeded?.Invoke(result.User.AccountName, role);
                    }

                    Password = string.Empty;
                }
                else
                {
                    ErrorMessage = result.ErrorMessage;
                    Password = string.Empty;
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}