// --------------------------------------------------------------------------------
// <copyright file="ChangePasswordWindow.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Windows;
using System.Windows.Input;
using AccuSync.WPF.Helpers;
using AccuSync.WPF.Resources;
using AccuSync.Presentation.ViewModels;
using AccuSync.WPF.Views.Dashboard;

namespace AccuSync.WPF.Views.Login
{
    public partial class ChangePasswordWindow : Window
    {
        private readonly ChangePasswordViewModel _viewModel;
        private bool _isOldPasswordVisible = false;
        private bool _isNewPasswordVisible = false;
        private bool _isConfirmPasswordVisible = false;

        public ChangePasswordWindow(ChangePasswordViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;

            _viewModel.PasswordChangeSucceeded += OnPasswordChangeSucceeded;
        }

        private void OnPasswordChangeSucceeded(string username, string role)
        {
            if (string.Equals(role, "Admin", System.StringComparison.OrdinalIgnoreCase))
            {
                var adminDashboard = App.GetService<AdminDashboardWindow>();
                adminDashboard.Show();
            }
            else
            {
                var screenerDashboard = App.GetService<ScreenerDashboardWindow>();
                screenerDashboard.Show();
            }

            Close();
        }

        // The window has no native title bar (WindowStyle="None"), so the card
        // background itself doubles as the drag handle. Controls that handle their
        // own mouse-down (Button, TextBox, PasswordBox) mark it Handled, so this
        // never fires when the user is actually interacting with them.
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

        // Ctrl+S is a shortcut for the same action as the Save button; IsDefault="True" on
        // that button already covers plain Enter, so only the Control modifier needs handling here.
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (_viewModel.SaveCommand.CanExecute(null))
                {
                    _viewModel.SaveCommand.Execute(null);
                }

                e.Handled = true;
            }
        }

        private void OldPasswordBox_PasswordChanged(object sender, RoutedEventArgs e) =>
            PasswordFieldHelper.SyncToProperty(sender, value => _viewModel.OldPassword = value);

        private void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e) =>
            PasswordFieldHelper.SyncToProperty(sender, value => _viewModel.NewPassword = value);

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e) =>
            PasswordFieldHelper.SyncToProperty(sender, value => _viewModel.ConfirmPassword = value);

        private void ToggleOldPasswordVisibility_Click(object sender, RoutedEventArgs e) =>
            PasswordFieldHelper.ToggleVisibility(ref _isOldPasswordVisible, OldPasswordBox, OldPasswordTextBox,
                OldPasswordEyeIcon, OldPasswordToggleButton,
                Strings.ChangePasswordWindow_HidePassword, Strings.ChangePasswordWindow_ShowPassword);

        private void ToggleNewPasswordVisibility_Click(object sender, RoutedEventArgs e) =>
            PasswordFieldHelper.ToggleVisibility(ref _isNewPasswordVisible, NewPasswordBox, NewPasswordTextBox,
                NewPasswordEyeIcon, NewPasswordToggleButton,
                Strings.ChangePasswordWindow_HidePassword, Strings.ChangePasswordWindow_ShowPassword);

        private void ToggleConfirmPasswordVisibility_Click(object sender, RoutedEventArgs e) =>
            PasswordFieldHelper.ToggleVisibility(ref _isConfirmPasswordVisible, ConfirmPasswordBox, ConfirmPasswordTextBox,
                ConfirmPasswordEyeIcon, ConfirmPasswordToggleButton,
                Strings.ChangePasswordWindow_HidePassword, Strings.ChangePasswordWindow_ShowPassword);
    }
}