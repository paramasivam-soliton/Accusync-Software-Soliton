// --------------------------------------------------------------------------------
// <copyright file="PasswordFieldHelper.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using AccuSync.WPF.Resources.Constants;

namespace AccuSync.WPF.Helpers
{
    /// <summary>
    /// Shared logic for the masked/plain-text password field pairs used by
    /// ChangePasswordWindow (login-time flow) and ChangePasswordSettingsView
    /// (Settings-page flow). Both host a visually different form around the same
    /// PasswordBox+TextBox+eye-icon pattern — this covers only the mechanical parts
    /// that were byte-for-byte identical between them, not the visual layout.
    /// </summary>
    public static class PasswordFieldHelper
    {
        /// <summary>
        /// Reads <paramref name="sender"/>'s current password and passes it to
        /// <paramref name="setProperty"/>. <c>PasswordBox.Password</c> can't be data-bound
        /// directly (WPF security restriction), so every masked password field needs a
        /// <c>PasswordChanged</c> handler to bridge it to the view model.
        /// </summary>
        /// <param name="sender">The event sender, expected to be the <see cref="PasswordBox"/> that changed.</param>
        /// <param name="setProperty">Assigns the new password to the target view model property.</param>
        public static void SyncToProperty(object sender, Action<string> setProperty)
        {
            if (sender is PasswordBox passwordBox)
            {
                setProperty(passwordBox.Password);
            }
        }

        /// <summary>
        /// Toggles a password field between its masked <see cref="PasswordBox"/> and
        /// plain-text <see cref="TextBox"/> display, carrying the current value across,
        /// focusing the newly-visible control, and updating the eye icon and tooltip
        /// to match the new state.
        /// </summary>
        /// <param name="isVisible">The field's own visibility flag; flipped and used to pick the branch.</param>
        /// <param name="maskedBox">The masked password field.</param>
        /// <param name="plainBox">The plain-text field shown in its place while visible.</param>
        /// <param name="eyeIcon">The eye icon whose <see cref="Path.Data"/> reflects the current state.</param>
        /// <param name="toggleButton">The button hosting <paramref name="eyeIcon"/>, whose tooltip is updated.</param>
        /// <param name="hideTooltip">Tooltip to show once the field becomes visible (offers to hide it).</param>
        /// <param name="showTooltip">Tooltip to show once the field is masked again (offers to show it).</param>
        public static void ToggleVisibility(
            ref bool isVisible,
            PasswordBox maskedBox,
            TextBox plainBox,
            Path eyeIcon,
            FrameworkElement toggleButton,
            string hideTooltip,
            string showTooltip)
        {
            isVisible = !isVisible;

            if (isVisible)
            {
                plainBox.Text = maskedBox.Password;
                maskedBox.Visibility = Visibility.Collapsed;
                plainBox.Visibility = Visibility.Visible;
                plainBox.Focus();
                plainBox.CaretIndex = plainBox.Text.Length;
                eyeIcon.Data = Geometry.Parse(PasswordVisibilityIcons.EyeOff);
                toggleButton.ToolTip = hideTooltip;
            }
            else
            {
                maskedBox.Password = plainBox.Text;
                plainBox.Visibility = Visibility.Collapsed;
                maskedBox.Visibility = Visibility.Visible;
                maskedBox.Focus();
                eyeIcon.Data = Geometry.Parse(PasswordVisibilityIcons.Eye);
                toggleButton.ToolTip = showTooltip;
            }
        }
    }
}
