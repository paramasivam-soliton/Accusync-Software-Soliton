// --------------------------------------------------------------------------------
// <copyright file="ChangePasswordWindow.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AccuSync.Resources;
using AccuSync.ViewModels;

namespace AccuSync.Views.Login
{
    // TODO: Three PasswordChanged handlers are identical except for the target property.
    //       Extract a single handler that reads a Tag to determine which ViewModel property to set.
    // TODO: Three toggle-visibility handlers are identical except for control name prefix.
    //       Extract a shared helper, e.g. TogglePasswordVisibility(PasswordBox, TextBox, Path, Button, ref bool).
    public partial class ChangePasswordWindow : Window
    {
        private readonly ChangePasswordViewModel _viewModel;
        private bool _isOldPasswordVisible = false;
        private bool _isNewPasswordVisible = false;
        private bool _isConfirmPasswordVisible = false;

        private const string EyeIconData = "M12 4.5C7 4.5 2.73 7.61 1 12c1.73 4.39 6 7.5 11 7.5s9.27-3.11 11-7.5c-1.73-4.39-6-7.5-11-7.5zM12 17c-2.76 0-5-2.24-5-5s2.24-5 5-5 5 2.24 5 5-2.24 5-5 5zm0-8c-1.66 0-3 1.34-3 3s1.34 3 3 3 3-1.34 3-3-1.34-3-3-3z";
        private const string EyeOffIconData = "M12 7c2.76 0 5 2.24 5 5 0 .65-.13 1.26-.36 1.83l2.92 2.92c1.51-1.26 2.7-2.89 3.43-4.75-1.73-4.39-6-7.5-11-7.5-1.4 0-2.74.25-3.98.7l2.16 2.16C10.74 7.13 11.35 7 12 7zM2 4.27l2.28 2.28.46.46C3.08 8.3 1.78 10.02 1 12c1.73 4.39 6 7.5 11 7.5 1.55 0 3.03-.3 4.38-.84l.42.42L19.73 22 21 20.73 3.27 3 2 4.27zM7.53 9.8l1.55 1.55c-.05.21-.08.43-.08.65 0 1.66 1.34 3 3 3 .22 0 .44-.03.65-.08l1.55 1.55c-.67.33-1.41.53-2.2.53-2.76 0-5-2.24-5-5 0-.79.2-1.53.53-2.2zm4.31-.78l3.15 3.15.02-.16c0-1.66-1.34-3-3-3l-.17.01z";

        public ChangePasswordWindow(ChangePasswordViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        private void OldPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
                _viewModel.OldPassword = passwordBox.Password;
        }

        private void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
                _viewModel.NewPassword = passwordBox.Password;
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
                _viewModel.ConfirmPassword = passwordBox.Password;
        }

        private void ToggleOldPasswordVisibility_Click(object sender, RoutedEventArgs e)
        {
            _isOldPasswordVisible = !_isOldPasswordVisible;

            if (_isOldPasswordVisible)
            {
                OldPasswordTextBox.Text = OldPasswordBox.Password;
                OldPasswordBox.Visibility = Visibility.Collapsed;
                OldPasswordTextBox.Visibility = Visibility.Visible;
                OldPasswordTextBox.Focus();
                OldPasswordTextBox.CaretIndex = OldPasswordTextBox.Text.Length;
                OldPasswordEyeIcon.Data = Geometry.Parse(EyeOffIconData);
                OldPasswordToggleButton.ToolTip = Strings.ChangePasswordWindow_HidePassword;
            }
            else
            {
                OldPasswordBox.Password = OldPasswordTextBox.Text;
                OldPasswordTextBox.Visibility = Visibility.Collapsed;
                OldPasswordBox.Visibility = Visibility.Visible;
                OldPasswordBox.Focus();
                OldPasswordEyeIcon.Data = Geometry.Parse(EyeIconData);
                OldPasswordToggleButton.ToolTip = Strings.ChangePasswordWindow_ShowPassword;
            }
        }

        private void ToggleNewPasswordVisibility_Click(object sender, RoutedEventArgs e)
        {
            _isNewPasswordVisible = !_isNewPasswordVisible;

            if (_isNewPasswordVisible)
            {
                NewPasswordTextBox.Text = NewPasswordBox.Password;
                NewPasswordBox.Visibility = Visibility.Collapsed;
                NewPasswordTextBox.Visibility = Visibility.Visible;
                NewPasswordTextBox.Focus();
                NewPasswordTextBox.CaretIndex = NewPasswordTextBox.Text.Length;
                NewPasswordEyeIcon.Data = Geometry.Parse(EyeOffIconData);
                NewPasswordToggleButton.ToolTip = Strings.ChangePasswordWindow_HidePassword;
            }
            else
            {
                NewPasswordBox.Password = NewPasswordTextBox.Text;
                NewPasswordTextBox.Visibility = Visibility.Collapsed;
                NewPasswordBox.Visibility = Visibility.Visible;
                NewPasswordBox.Focus();
                NewPasswordEyeIcon.Data = Geometry.Parse(EyeIconData);
                NewPasswordToggleButton.ToolTip = Strings.ChangePasswordWindow_ShowPassword;
            }
        }

        private void ToggleConfirmPasswordVisibility_Click(object sender, RoutedEventArgs e)
        {
            _isConfirmPasswordVisible = !_isConfirmPasswordVisible;

            if (_isConfirmPasswordVisible)
            {
                ConfirmPasswordTextBox.Text = ConfirmPasswordBox.Password;
                ConfirmPasswordBox.Visibility = Visibility.Collapsed;
                ConfirmPasswordTextBox.Visibility = Visibility.Visible;
                ConfirmPasswordTextBox.Focus();
                ConfirmPasswordTextBox.CaretIndex = ConfirmPasswordTextBox.Text.Length;
                ConfirmPasswordEyeIcon.Data = Geometry.Parse(EyeOffIconData);
                ConfirmPasswordToggleButton.ToolTip = Strings.ChangePasswordWindow_HidePassword;
            }
            else
            {
                ConfirmPasswordBox.Password = ConfirmPasswordTextBox.Text;
                ConfirmPasswordTextBox.Visibility = Visibility.Collapsed;
                ConfirmPasswordBox.Visibility = Visibility.Visible;
                ConfirmPasswordBox.Focus();
                ConfirmPasswordEyeIcon.Data = Geometry.Parse(EyeIconData);
                ConfirmPasswordToggleButton.ToolTip = Strings.ChangePasswordWindow_ShowPassword;
            }
        }
    }
}