// --------------------------------------------------------------------------------
// <copyright file="ChangePasswordSettingsView.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AccuSync.Core.Abstractions.Services;
using AccuSync.Presentation.ViewModels;
using AccuSync.WPF.Helpers;
using AccuSync.WPF.Resources;

namespace AccuSync.WPF.Views.Settings
{
    /// <summary>
    /// Lets the currently signed-in user change their own password from the Settings
    /// page. Unlike the login-time <see cref="Login.ChangePasswordWindow"/>, there is
    /// no "current password" field — the active session is the proof of identity —
    /// so the underlying <see cref="ChangePasswordViewModel"/> is constructed with
    /// <c>requireCurrentPassword: false</c>.
    /// </summary>
    public partial class ChangePasswordSettingsView : UserControl
    {
        private ChangePasswordViewModel _viewModel;
        private bool _isNewPasswordVisible;
        private bool _isConfirmPasswordVisible;

        public ChangePasswordSettingsView()
        {
            InitializeComponent();
            Loaded += async (s, e) => await InitializeAsync();
        }

        /// <summary>
        /// Submits the password change if the user has entered one. This card has no
        /// Save button of its own — every host drives Save through its own UI (the
        /// Admin Settings page's ribbon toolbar) and calls this instead. Does nothing
        /// (and returns success) if both password fields are still empty — a generic
        /// page-level Save shouldn't fail just because this card was untouched.
        /// </summary>
        /// <returns>
        /// <c>true</c> if there was nothing to submit or the change succeeded;
        /// <c>false</c> if a change was attempted but rejected — the reason is left
        /// visible via the bound <c>ErrorMessage</c> on this card.
        /// </returns>
        public async Task<bool> SubmitIfRequestedAsync()
        {
            if (_viewModel == null)
            {
                return true;
            }

            if (string.IsNullOrEmpty(_viewModel.NewPassword) && string.IsNullOrEmpty(_viewModel.ConfirmPassword))
            {
                return true;
            }

            if (!_viewModel.CanSave)
            {
                _viewModel.ErrorMessage = Strings.SettingsContentView_PasswordRequirementsNotMet;
                return false;
            }

            await _viewModel.SavePasswordAsync();
            return string.IsNullOrEmpty(_viewModel.ErrorMessage);
        }

        private async Task InitializeAsync()
        {
            if (_viewModel != null)
            {
                return;
            }

            var userService = App.GetService<IUserService>();
            var passwordHasher = App.GetService<IPasswordHasher>();
            var currentUserContext = App.GetService<ICurrentUserContext>();

            var currentUser = await userService.GetUserByAccountNameAsync(currentUserContext.AccountName);

            _viewModel = new ChangePasswordViewModel(
                userService, passwordHasher, currentUserContext, currentUser,
                isPasswordExpiredReset: false, requireCurrentPassword: false);
            _viewModel.PasswordChangeSucceeded += OnPasswordChangeSucceeded;

            DataContext = _viewModel;
        }

        // No success dialog here — the only host (SettingsContentView) already shows one
        // generic "Settings successfully saved" dialog once its own SaveState completes,
        // covering this along with everything else on the page.
        private void OnPasswordChangeSucceeded(string accountName, string role)
        {
            ClearFields();
        }

        private void ClearFields()
        {
            NewPasswordMasked.Password = string.Empty;
            NewPasswordPlain.Text = string.Empty;
            ConfirmPasswordMasked.Password = string.Empty;
            ConfirmPasswordPlain.Text = string.Empty;
        }

        private void NewPasswordMasked_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                PasswordFieldHelper.SyncToProperty(sender, value => _viewModel.NewPassword = value);
            }
        }

        private void ConfirmPasswordMasked_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                PasswordFieldHelper.SyncToProperty(sender, value => _viewModel.ConfirmPassword = value);
            }
        }

        private void ToggleNewPasswordVisibility_Click(object sender, RoutedEventArgs e) =>
            PasswordFieldHelper.ToggleVisibility(ref _isNewPasswordVisible, NewPasswordMasked, NewPasswordPlain,
                NewPasswordEyeIcon, NewPasswordToggle,
                Strings.SettingsContentView_HidePassword, Strings.SettingsContentView_ShowPassword);

        private void ToggleConfirmPasswordVisibility_Click(object sender, RoutedEventArgs e) =>
            PasswordFieldHelper.ToggleVisibility(ref _isConfirmPasswordVisible, ConfirmPasswordMasked, ConfirmPasswordPlain,
                ConfirmPasswordEyeIcon, ConfirmPasswordToggle,
                Strings.SettingsContentView_HidePassword, Strings.SettingsContentView_ShowPassword);
    }
}
