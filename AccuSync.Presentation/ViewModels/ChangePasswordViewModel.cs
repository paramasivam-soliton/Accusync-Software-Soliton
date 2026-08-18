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
using AccuSync.Core.Abstractions.Repositories;
using AccuSync.Core.Abstractions.Services;
using AccuSync.Core.Entities;
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
        private readonly IUserRepository _userRepository;
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

        /// <summary>The user's current password, as entered for verification.</summary>
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

        /// <summary>The password the user wants to set. Re-validates policy rules on every change.</summary>
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

        /// <summary>Repeat entry of <see cref="NewPassword"/>, used to confirm the user typed it correctly.</summary>
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

        /// <summary>Error text shown to the user, or empty when there is no error.</summary>
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        /// <summary>True while a save operation is in progress.</summary>
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        /// <summary>True when <see cref="NewPassword"/> meets the minimum length policy.</summary>
        public bool HasMinimumLength
        {
            get => _hasMinimumLength;
            set { _hasMinimumLength = value; OnPropertyChanged(); }
        }

        /// <summary>True when <see cref="NewPassword"/> contains an uppercase letter.</summary>
        public bool HasUpperCase
        {
            get => _hasUpperCase;
            set { _hasUpperCase = value; OnPropertyChanged(); }
        }

        /// <summary>True when <see cref="NewPassword"/> contains a lowercase letter.</summary>
        public bool HasLowerCase
        {
            get => _hasLowerCase;
            set { _hasLowerCase = value; OnPropertyChanged(); }
        }

        /// <summary>True when <see cref="NewPassword"/> contains a digit.</summary>
        public bool HasNumber
        {
            get => _hasNumber;
            set { _hasNumber = value; OnPropertyChanged(); }
        }

        /// <summary>True when <see cref="NewPassword"/> contains a non-alphanumeric character.</summary>
        public bool HasSpecialChar
        {
            get => _hasSpecialChar;
            set { _hasSpecialChar = value; OnPropertyChanged(); }
        }

        /// <summary>True when <see cref="NewPassword"/> and <see cref="ConfirmPassword"/> are identical.</summary>
        public bool PasswordsMatch
        {
            get => _passwordsMatch;
            set { _passwordsMatch = value; OnPropertyChanged(); }
        }

        /// <summary>True when <see cref="NewPassword"/> differs from <see cref="OldPassword"/> as typed.</summary>
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

        /// <summary>Command that validates and persists the new password.</summary>
        public ICommand SaveCommand { get; }

        /// <summary>Raised after a successful password change. Carries (username, role).</summary>
        public event Action<string, string> PasswordChangeSucceeded;

        /// <summary>
        /// Creates the view model for the given user's password change flow.
        /// </summary>
        /// <param name="userRepository">Service used to persist the updated user record.</param>
        /// <param name="encryptionService">Service used to encrypt/decrypt password values.</param>
        /// <param name="currentUser">The user whose password is being changed.</param>
        public ChangePasswordViewModel(IUserRepository userRepository, IEncryptionService encryptionService, User currentUser)
        {
            _userRepository = userRepository;
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

                bool success = await _userRepository.UpdateUserAsync(_currentUser);

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

        /// <summary>Raised whenever a bound property's value changes.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}