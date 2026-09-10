// --------------------------------------------------------------------------------
// <copyright file="LoginWindow.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AccuSync.WPF.Helpers;
using AccuSync.WPF.Resources;
using AccuSync.Presentation.ViewModels;

namespace AccuSync.WPF.Views.Login
{
    public partial class LoginWindow : Window
    {
        private readonly LoginViewModel _viewModel;
        private bool _isPasswordVisible = false;

        public LoginWindow(LoginViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;

            _viewModel.LoginSucceeded += OnLoginSucceeded;
            _viewModel.FirstLoginPasswordChangeRequired += OnPasswordChangeRequired;
            _viewModel.PasswordExpiredPasswordChangeRequired += OnPasswordChangeRequired;
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        /// <summary>
        /// PasswordBox.Password can't be data-bound (WPF security restriction), so it's
        /// synced manually via PasswordChanged below — but that only covers box → ViewModel.
        /// This covers the other direction: when the ViewModel clears Password itself (e.g.
        /// after a failed login attempt), push that back down to the visible control so it
        /// doesn't keep showing stale text the ViewModel no longer has.
        /// </summary>
        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(LoginViewModel.Password))
            {
                return;
            }

            if (PasswordBox.Password == _viewModel.Password)
            {
                return;
            }

            PasswordBox.Password = _viewModel.Password;
        }

        private void OnLoginSucceeded(string username, string role)
        {
            App.NavigateAfterLogin(this, username, role);
        }

        // Shared by FirstLoginPasswordChangeRequired and PasswordExpiredPasswordChangeRequired —
        // both are "redirect to the change-password screen before granting access" flows that
        // differ only in the ChangePasswordViewModel the LoginViewModel constructs.
        private void OnPasswordChangeRequired(ChangePasswordViewModel changePasswordViewModel)
        {
            var changePasswordWindow = new ChangePasswordWindow(changePasswordViewModel);
            changePasswordWindow.Show();
            Close();
        }

        // The window has no native title bar (WindowStyle="None"), so the card
        // background itself doubles as the drag handle. Controls that handle their
        // own mouse-down (Button, ComboBox, TextBox, PasswordBox) mark it Handled,
        // so this never fires when the user is actually interacting with them.
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e) =>
            PasswordFieldHelper.SyncToProperty(sender, value => _viewModel.Password = value);

        private void TogglePasswordVisibility_Click(object sender, RoutedEventArgs e)
        {
            PasswordFieldHelper.ToggleVisibility(ref _isPasswordVisible, PasswordBox, PasswordTextBox,
                EyeIcon, TogglePasswordButton,
                Strings.LoginWindow_HidePassword, Strings.LoginWindow_ShowPassword);

            _viewModel.IsPasswordVisible = _isPasswordVisible;
        }
    }
}