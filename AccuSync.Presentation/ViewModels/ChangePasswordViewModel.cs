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
using AccuSync.Core.Abstractions.Services;
using AccuSync.Core.Entities;
using AccuSync.Application.Resources;

namespace AccuSync.Presentation.ViewModels
{
    /// <summary>
    /// Drives the change-password screen. Validates against password policy
    /// rules in real time and checks the last three passwords on save.
    /// Also handles the first-login and 90-day-expiry forced password change
    /// flows. Raises <see cref="PasswordChangeSucceeded"/> so the hosting View
    /// decides which dashboard Window to open — this VM has no dependency on
    /// any concrete Window type.
    /// </summary>
    public class ChangePasswordViewModel : INotifyPropertyChanged
    {
        private readonly IUserService _userService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ICurrentUserContext _currentUserContext;
        private readonly User _currentUser;
        private readonly bool _isPasswordExpiredReset;
        private readonly bool _requireCurrentPassword;

        /// <summary>
        /// Creates the view model for the given user's password change flow.
        /// </summary>
        /// <param name="userService">Service used to persist the updated user record.</param>
        /// <param name="passwordHasher">Service used to hash and verify password values.</param>
        /// <param name="currentUserContext">Provides the signed-in user's role for <see cref="PasswordChangeSucceeded"/>.</param>
        /// <param name="currentUser">The user whose password is being changed.</param>
        /// <param name="isPasswordExpiredReset">Whether this change was forced by 90-day password expiration rather than first-login or a voluntary change. Drives the initial notice message.</param>
        /// <param name="requireCurrentPassword">Whether the caller collects and must verify the user's current password before allowing a change. False for the Settings-page voluntary change flow, which has no "current password" field — <see cref="SavePasswordAsync"/> still rejects a new password identical to the current one via a direct hash comparison instead.</param>
        public ChangePasswordViewModel(IUserService userService, IPasswordHasher passwordHasher, ICurrentUserContext currentUserContext, User currentUser, bool isPasswordExpiredReset = false, bool requireCurrentPassword = true)
        {
            _userService = userService;
            _passwordHasher = passwordHasher;
            _currentUserContext = currentUserContext;
            _currentUser = currentUser;
            _isPasswordExpiredReset = isPasswordExpiredReset;
            _requireCurrentPassword = requireCurrentPassword;

            if (_isPasswordExpiredReset)
            {
                ErrorMessage = Strings.ChangePasswordViewModel_PasswordExpiredNotice;
            }

            SaveCommand = new RelayCommand(async () => await SavePasswordAsync(), () => !IsLoading);
        }

        private string _oldPassword = string.Empty;

        /// <summary>The user's current password, as typed for verification.</summary>
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

        private string _newPassword = string.Empty;

        /// <summary>The password being set.</summary>
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

        private string _confirmPassword = string.Empty;

        /// <summary>Re-entry of <see cref="NewPassword"/>, used to confirm it was typed correctly.</summary>
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

        private string _errorMessage = string.Empty;

        /// <summary>Message shown to the user when validation or save fails.</summary>
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

        /// <summary>Whether a save is in progress.</summary>
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        // Policy validation flags — bound to checkmark/X indicators in the view
        private bool _hasMinimumLength;

        /// <summary>Whether <see cref="NewPassword"/> meets the minimum length rule.</summary>
        public bool HasMinimumLength
        {
            get => _hasMinimumLength;
            set { _hasMinimumLength = value; OnPropertyChanged(); }
        }

        private bool _hasUpperCase;

        /// <summary>Whether <see cref="NewPassword"/> contains an uppercase letter.</summary>
        public bool HasUpperCase
        {
            get => _hasUpperCase;
            set { _hasUpperCase = value; OnPropertyChanged(); }
        }

        private bool _hasLowerCase;

        /// <summary>Whether <see cref="NewPassword"/> contains a lowercase letter.</summary>
        public bool HasLowerCase
        {
            get => _hasLowerCase;
            set { _hasLowerCase = value; OnPropertyChanged(); }
        }

        private bool _hasNumber;

        /// <summary>Whether <see cref="NewPassword"/> contains a digit.</summary>
        public bool HasNumber
        {
            get => _hasNumber;
            set { _hasNumber = value; OnPropertyChanged(); }
        }

        private bool _hasSpecialChar;

        /// <summary>Whether <see cref="NewPassword"/> contains a non-alphanumeric character.</summary>
        public bool HasSpecialChar
        {
            get => _hasSpecialChar;
            set { _hasSpecialChar = value; OnPropertyChanged(); }
        }

        private bool _passwordsMatch;

        /// <summary>Whether <see cref="NewPassword"/> and <see cref="ConfirmPassword"/> match.</summary>
        public bool PasswordsMatch
        {
            get => _passwordsMatch;
            set { _passwordsMatch = value; OnPropertyChanged(); }
        }

        private bool _notSameAsOld;

        /// <summary>Whether <see cref="NewPassword"/> differs from <see cref="OldPassword"/>.</summary>
        public bool NotSameAsOld
        {
            get => _notSameAsOld;
            set { _notSameAsOld = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// All policy rules must pass before the Save button is enabled. When
        /// <see cref="_requireCurrentPassword"/> is <c>false</c> there is no typed
        /// old password to compare <see cref="NewPassword"/> against, so
        /// <see cref="NotSameAsOld"/> is excluded here — <see cref="SavePasswordAsync"/>
        /// still rejects a no-op change by comparing against the stored hash directly.
        /// </summary>
        public bool CanSave => HasMinimumLength && HasUpperCase && HasLowerCase &&
                               HasNumber && HasSpecialChar && PasswordsMatch &&
                               (!_requireCurrentPassword || NotSameAsOld);

        /// <summary>Command bound to the Save button; validates and persists the new password.</summary>
        public ICommand SaveCommand { get; }

        /// <summary>Raised after a successful password change. Carries (username, role).</summary>
        public event Action<string, string> PasswordChangeSucceeded;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

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

            // Compares against the typed Old Password, not the stored one — checked on save.
            NotSameAsOld = !string.IsNullOrEmpty(NewPassword) &&
                         !string.IsNullOrEmpty(OldPassword) &&
                         NewPassword != OldPassword;

            OnPropertyChanged(nameof(CanSave));
        }

        /// <summary>
        /// Validates and persists <see cref="NewPassword"/>. Public (not just reachable
        /// via <see cref="SaveCommand"/>) so a host that drives Save from its own
        /// generic action — e.g. a page-level Save button — can await the result
        /// directly, rather than racing <see cref="ICommand.Execute"/>'s fire-and-forget
        /// <c>async void</c>.
        /// </summary>
        public async Task SavePasswordAsync()
        {
            ErrorMessage = string.Empty;
            IsLoading = true;

            try
            {
                if (!VerifyCurrentPassword())
                {
                    return;
                }

                if (IsNewPasswordSameAsCurrent())
                {
                    return;
                }

                var previousHashes = GetPreviousPasswordHashes();
                if (IsPasswordReused(previousHashes))
                {
                    return;
                }

                ApplyNewPassword(previousHashes);
                await PersistPasswordChangeAsync();
            }
            catch (Exception)
            {
                // TODO: Log the exception once a logger is introduced into the solution.
                ErrorMessage = string.Format(Strings.ChangePasswordViewModel_UnexpectedError, ErrorCode.Unexpected.ToDisplayCode());
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Verifies the typed old password against the currently stored hash — skipped
        // for the Settings-page flow, which has no "current password" field and instead
        // trusts the already-authenticated session.
        private bool VerifyCurrentPassword()
        {
            if (!_requireCurrentPassword || _passwordHasher.Verify(OldPassword, _currentUser.ProfilePassword))
            {
                return true;
            }

            ErrorMessage = Strings.ChangePasswordViewModel_CurrentPasswordIncorrect;
            return false;
        }

        // A no-op "change" back to the same password is rejected regardless of flow —
        // for the current-password flows this duplicates NotSameAsOld (already gating
        // CanSave), but is the only guard against it at all when there's no typed old
        // password to compare against.
        private bool IsNewPasswordSameAsCurrent()
        {
            if (!_passwordHasher.Verify(NewPassword, _currentUser.ProfilePassword))
            {
                return false;
            }

            ErrorMessage = Strings.ChangePasswordViewModel_PasswordSameAsCurrent;
            return true;
        }

        // LastThreePasswords is a pipe-delimited string of password hashes. The
        // delimiter can't collide with a stored hash — Base64 (the hash's own
        // encoding) never produces '|' — but entries are still filtered for
        // empty/malformed values in case the stored string was ever hand-edited.
        private string[] GetPreviousPasswordHashes()
        {
            return string.IsNullOrEmpty(_currentUser.LastThreePasswords)
                ? []
                : _currentUser.LastThreePasswords.Split('|').Where(h => !string.IsNullOrEmpty(h)).ToArray();
        }

        private bool IsPasswordReused(string[] previousHashes)
        {
            if (!previousHashes.Any(oldHash => _passwordHasher.Verify(NewPassword, oldHash)))
            {
                return false;
            }

            ErrorMessage = Strings.ChangePasswordViewModel_PasswordReused;
            return true;
        }

        // Prepends the current password to the history and keeps only three.
        private void ApplyNewPassword(string[] previousHashes)
        {
            var updatedPasswordList = new[] { _currentUser.ProfilePassword }
                .Concat(previousHashes)
                .Where(h => !string.IsNullOrEmpty(h))
                .Take(3)
                .ToArray();

            _currentUser.ProfilePassword = _passwordHasher.Hash(NewPassword);
            _currentUser.LastThreePasswords = string.Join("|", updatedPasswordList);
            _currentUser.PasswordModificationDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            _currentUser.FirstLogin = 0;
            _currentUser.ModificationDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        private async Task PersistPasswordChangeAsync()
        {
            bool success = await _userService.UpdateUserAsync(_currentUser);

            if (success)
            {
                PasswordChangeSucceeded?.Invoke(_currentUser.AccountName, _currentUserContext.Role.ToString());
            }
            else
            {
                ErrorMessage = Strings.ChangePasswordViewModel_UpdateFailed;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
