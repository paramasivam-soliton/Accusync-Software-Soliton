// --------------------------------------------------------------------------------
// <copyright file="LoginViewModel.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Helpers;
using AccuSync.Services;
using AccuSync.Views.Login;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AccuSync.Resources;

namespace AccuSync.ViewModels
{
    /// <summary>
    /// Drives the login screen. Loads available usernames into a dropdown
    /// and delegates credential verification to <see cref="IAuthenticationService"/>.
    /// Routes to the change-password screen on first login, or to the
    /// appropriate dashboard on success.
    /// </summary>
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly IDatabaseService _databaseService;
        private readonly IAuthenticationService _authenticationService;

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

        public LoginViewModel(IDatabaseService databaseService, IAuthenticationService authenticationService)
        {
            _databaseService = databaseService;
            _authenticationService = authenticationService;

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
                        // TODO: Same MVVM concern as ChangePasswordViewModel — ViewModel
                        //       creates and shows a Window directly. Move to a navigation service.
                        var changePasswordWindow = new ChangePasswordWindow(
                            new ChangePasswordViewModel(
                                _databaseService,
                                App.GetService<IEncryptionService>(),
                                result.User
                            )
                        );
                        changePasswordWindow.Show();
                        CloseLoginWindow();
                    }
                    else
                    {
                        // TODO: Use an actual Role field from the User model.
                        string role = string.Equals(result.User.AccountName, "Admin", StringComparison.OrdinalIgnoreCase)
                            ? "Admin"
                            : "Screener";

                        var loginWindow = Application.Current.Windows
                            .OfType<LoginWindow>()
                            .FirstOrDefault();

                        if (loginWindow != null)
                        {
                            App.NavigateAfterLogin(loginWindow, result.User.AccountName, role);
                        }
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

        private void CloseLoginWindow()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var loginWindow = Application.Current.Windows
                    .OfType<LoginWindow>()
                    .FirstOrDefault();
                loginWindow?.Close();
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}