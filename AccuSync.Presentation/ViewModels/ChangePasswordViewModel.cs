// --------------------------------------------------------------------------------
// <copyright file="ChangePasswordViewModel.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using AccuSync.Presentation.Helpers;
using AccuSync.Application.Abstractions.Services;
using AccuSync.Application.Models;
using AccuSync.Application.Resources;

namespace AccuSync.Presentation.ViewModels
{
    /// <summary>
    /// Drives the change-password screen. Validates against password policy
    /// rules in real time and checks the last three passwords on save.
    /// Also handles the first-login forced password change flow. Raises
    /// <see cref="PasswordChangeSucceeded"/> so the hosting View decides which
    /// dashboard Window to open — this VM has no dependency on any concrete Window type.
    /// </summary>
    public class ChangePasswordViewModel : INotifyPropertyChanged
    {
        private readonly IDatabaseService _databaseService;
        private readonly IEncryptionService _encryptionService;
        private readonly User _currentUser;

        private string _oldPassword = string.Empty;
        private string _newPassword = string.Empty;
        private string _confirmPassword = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading;

        // Policy validation flags — bound to checkmark/X indicators in the view
        private bool _hasMinimumLength;
        private bool _hasUpperCase;
        private bool _hasLowerCase;
        private bool _hasNumber;
        private bool _hasSpecialChar;
        private bool _passwordsMatch;
        private bool _notSameAsOld;

        public string OldPassword
        {
            get => _oldPassword;
            set
            {
                _oldPassword = value;
                OnPropertyChanged();
                ValidatePassword();
            }
        }

        public string NewPassword
        {
            get => _newPassword;
            set
            {
                _newPassword = value;
                OnPropertyChanged();
                ValidatePassword();
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                _confirmPassword = value;
                OnPropertyChanged();
                ValidatePassword();
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

        public bool HasMinimumLength
        {
            get => _hasMinimumLength;
            set { _hasMinimumLength = value; OnPropertyChanged(); }
        }

        public bool HasUpperCase
        {
            get => _hasUpperCase;
            set { _hasUpperCase = value; OnPropertyChanged(); }
        }

        public bool HasLowerCase
        {
            get => _hasLowerCase;
            set { _hasLowerCase = value; OnPropertyChanged(); }
        }

        public bool HasNumber
        {
            get => _hasNumber;
            set { _hasNumber = value; OnPropertyChanged(); }
        }

        public bool HasSpecialChar
        {
            get => _hasSpecialChar;
            set { _hasSpecialChar = value; OnPropertyChanged(); }
        }

        public bool PasswordsMatch
        {
            get => _passwordsMatch;
            set { _passwordsMatch = value; OnPropertyChanged(); }
        }

        public bool NotSameAsOld
        {
            get => _notSameAsOld;
            set { _notSameAsOld = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// All policy rules must pass before the Save button is enabled.
        /// </summary>
        public bool CanSave => HasMinimumLength && HasUpperCase && HasLowerCase &&
                               HasNumber && HasSpecialChar && PasswordsMatch && NotSameAsOld;

        public ICommand SaveCommand { get; }

        /// <summary>Raised after a successful password change. Carries (username, role).</summary>
        public event Action<string, string> PasswordChangeSucceeded;

        public ChangePasswordViewModel(IDatabaseService databaseService, IEncryptionService encryptionService, User currentUser)
        {
            _databaseService = databaseService;
            _encryptionService = encryptionService;
            _currentUser = currentUser;

            SaveCommand = new RelayCommand(async () => await SavePasswordAsync(), () => !IsLoading);
        }

        /// <summary>
        /// Runs on every keystroke in any of the three password fields.
        /// Updates the individual policy flags so the view shows real-time feedback.
        /// </summary>
        private void ValidatePassword()
        {
            ErrorMessage = string.Empty;

            HasMinimumLength = !string.IsNullOrEmpty(NewPassword) && NewPassword.Length >= 8;
            HasUpperCase = !string.IsNullOrEmpty(NewPassword) && NewPassword.Any(char.IsUpper);
            HasLowerCase = !string.IsNullOrEmpty(NewPassword) && NewPassword.Any(char.IsLower);
            HasNumber = !string.IsNullOrEmpty(NewPassword) && NewPassword.Any(char.IsDigit);
            HasSpecialChar = !string.IsNullOrEmpty(NewPassword) &&
                           NewPassword.Any(c => !char.IsLetterOrDigit(c));

            PasswordsMatch = !string.IsNullOrEmpty(NewPassword) &&
                           !string.IsNullOrEmpty(ConfirmPassword) &&
                           NewPassword == ConfirmPassword;

            // Only compares against what the user typed in the Old Password field,
            // not the stored password — the stored check happens on save.
            NotSameAsOld = !string.IsNullOrEmpty(NewPassword) &&
                         !string.IsNullOrEmpty(OldPassword) &&
                         NewPassword != OldPassword;

            OnPropertyChanged(nameof(CanSave));
        }

        private async Task SavePasswordAsync()
        {
            ErrorMessage = string.Empty;
            IsLoading = true;

            try
            {
                // Verify old password against the database
                string decryptedPassword = _encryptionService.Decrypt(_currentUser.ProfilePassword);
                if (OldPassword != decryptedPassword)
                {
                    ErrorMessage = Strings.ChangePasswordViewModel_CurrentPasswordIncorrect;
                    IsLoading = false;
                    return;
                }

                // LastThreePasswords is a pipe-delimited string of encrypted passwords.
                // See User.cs TODO about documenting this format.
                if (!string.IsNullOrEmpty(_currentUser.LastThreePasswords))
                {
                    var lastPasswords = _currentUser.LastThreePasswords.Split('|');
                    foreach (var oldPass in lastPasswords)
                    {
                        if (!string.IsNullOrEmpty(oldPass) && _encryptionService.Decrypt(oldPass) == NewPassword)
                        {
                            ErrorMessage = Strings.ChangePasswordViewModel_PasswordReused;
                            IsLoading = false;
                            return;
                        }
                    }
                }

                string encryptedNewPassword = _encryptionService.Encrypt(NewPassword);

                // Prepend the current password to the history and keep only three.
                var passwordList = string.IsNullOrEmpty(_currentUser.LastThreePasswords)
                    ? new string[0]
                    : _currentUser.LastThreePasswords.Split('|');

                var updatedPasswordList = new[] { _currentUser.ProfilePassword }
                    .Concat(passwordList)
                    .Take(3)
                    .ToArray();

                _currentUser.ProfilePassword = encryptedNewPassword;
                _currentUser.LastThreePasswords = string.Join("|", updatedPasswordList);
                _currentUser.PasswordModificationDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                _currentUser.FirstLogin = 0;
                _currentUser.ModificationDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                bool success = await _databaseService.UpdateUserAsync(_currentUser);

                if (success)
                {
                    // TODO: Use an actual Role field from the User model.
                    string role = string.Equals(_currentUser.AccountName, "Admin", StringComparison.OrdinalIgnoreCase)
                        ? "Admin"
                        : "Screener";

                    PasswordChangeSucceeded?.Invoke(_currentUser.AccountName, role);
                }
                else
                {
                    ErrorMessage = Strings.ChangePasswordViewModel_UpdateFailed;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = string.Format(Strings.ChangePasswordViewModel_UnexpectedError, ex.Message);
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