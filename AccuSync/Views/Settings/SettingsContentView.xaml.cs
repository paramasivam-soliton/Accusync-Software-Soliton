// --------------------------------------------------------------------------------
// <copyright file="SettingsContentView.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AccuSync.Resources;
using AccuSync.Controls;

namespace AccuSync.Views.Settings
{
    public partial class SettingsContentView : UserControl
    {
        private bool _passwordVisible = false;
        private bool _verifyVisible = false;

        // Prevents circular updates when syncing PasswordBox ↔ TextBox
        private bool _isSyncingPassword = false;
        private bool _isSyncingVerify = false;

        private bool _isSuppressingUndo = false;

        private const string EyeOpen =
            "M12 4.5C7 4.5 2.73 7.61 1 12c1.73 4.39 6 7.5 11 7.5s9.27-3.11 11-7.5c-1.73-4.39-6-7.5-11-7.5zM12 17c-2.76 0-5-2.24-5-5s2.24-5 5-5 5 2.24 5 5-2.24 5-5 5zm0-8c-1.66 0-3 1.34-3 3s1.34 3 3 3 3-1.34 3-3-1.34-3-3-3z";
        private const string EyeClosed =
            "M12 7c2.76 0 5 2.24 5 5 0 .65-.13 1.26-.36 1.83l2.92 2.92c1.51-1.26 2.7-2.89 3.43-4.75-1.73-4.39-6-7.5-11-7.5-1.4 0-2.74.25-3.98.7l2.16 2.16C10.74 7.13 11.35 7 12 7zM2 4.27l2.28 2.28.46.46C3.08 8.3 1.78 10.02 1 12c1.73 4.39 6 7.5 11 7.5 1.55 0 3.03-.3 4.38-.84l.42.42L19.73 22 21 20.73 3.27 3 2 4.27zM7.53 9.8l1.55 1.55c-.05.21-.08.43-.08.65 0 1.66 1.34 3 3 3 .22 0 .44-.03.65-.08l1.55 1.55c-.67.33-1.41.53-2.2.53-2.76 0-5-2.24-5-5 0-.79.2-1.53.53-2.2zm4.31-.78l3.15 3.15.02-.16c0-1.66-1.34-3-3-3l-.17.01z";

        private static readonly Dictionary<string, string> _languageCodes = new()
        {
            { "English", "en" },
            { "French", "fr" },
            { "Italian", "it" },
            { "German", "de" },
            { "Spanish", "es" }
        };

        // Snapshot for Revert and Undo

        private record SettingsSnapshot(string Password, string Verify, int LanguageIndex);

        private SettingsSnapshot _savedState;
        private readonly Stack<SettingsSnapshot> _undoStack = new();

        public SettingsContentView()
        {
            InitializeComponent();
            _savedState = TakeSnapshot();
        }

        // Snapshot Helpers

        private SettingsSnapshot TakeSnapshot()
        {
            return new SettingsSnapshot(
                _passwordVisible ? PasswordPlain.Text : PasswordMasked.Password,
                _verifyVisible ? VerifyPlain.Text : VerifyMasked.Password,
                LanguageComboBox.SelectedIndex
            );
        }

        private void RestoreSnapshot(SettingsSnapshot snap)
        {
            _isSuppressingUndo = true;
            _isSyncingPassword = true;
            _isSyncingVerify = true;

            PasswordMasked.Password = snap.Password;
            PasswordPlain.Text = snap.Password;
            VerifyMasked.Password = snap.Verify;
            VerifyPlain.Text = snap.Verify;
            LanguageComboBox.SelectedIndex = snap.LanguageIndex;

            _isSyncingPassword = false;
            _isSyncingVerify = false;
            _isSuppressingUndo = false;
        }

        private void PushUndo()
        {
            if (_isSuppressingUndo) return;
            _undoStack.Push(TakeSnapshot());
        }

        // Save / Revert / Undo — called from toolbar

        /// <summary>
        /// Saves the current state as the baseline for Revert and clears the undo stack.
        /// </summary>
        // NOTE: Saves snapshot only — doesn't persist password change to database
        public void SaveState()
        {
            _savedState = TakeSnapshot();
            _undoStack.Clear();
            AppDialog.Show(Strings.SettingsContentView_SettingsSaved, Strings.SettingsContentView_SaveCaption, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Restores all fields to the last saved state.
        /// Pushes current state to undo stack first so the revert itself is undoable.
        /// </summary>
        public void Revert()
        {
            if (_savedState == null) return;
            PushUndo();
            RestoreSnapshot(_savedState);
        }

        /// <summary>
        /// Restores the previous field state from the undo stack.
        /// </summary>
        public void Undo()
        {
            if (_undoStack.Count == 0) return;
            var prev = _undoStack.Pop();
            RestoreSnapshot(prev);
        }

        public bool CanUndo => _undoStack.Count > 0;

        // Password Visibility Toggle

        private void TogglePassword_Click(object sender, RoutedEventArgs e)
        {
            _passwordVisible = !_passwordVisible;

            if (_passwordVisible)
            {
                _isSyncingPassword = true;
                PasswordPlain.Text = PasswordMasked.Password;
                _isSyncingPassword = false;

                PasswordMasked.Visibility = Visibility.Collapsed;
                PasswordPlain.Visibility = Visibility.Visible;
                PasswordPlain.Focus();
                PasswordPlain.CaretIndex = PasswordPlain.Text.Length;

                PasswordEyeIcon.Data = Geometry.Parse(EyeClosed);
                PasswordToggle.ToolTip = Strings.SettingsContentView_HidePassword;
            }
            else
            {
                _isSyncingPassword = true;
                PasswordMasked.Password = PasswordPlain.Text;
                _isSyncingPassword = false;

                PasswordPlain.Visibility = Visibility.Collapsed;
                PasswordMasked.Visibility = Visibility.Visible;
                PasswordMasked.Focus();

                PasswordEyeIcon.Data = Geometry.Parse(EyeOpen);
                PasswordToggle.ToolTip = Strings.SettingsContentView_ShowPassword;
            }
        }

        private void ToggleVerify_Click(object sender, RoutedEventArgs e)
        {
            _verifyVisible = !_verifyVisible;

            if (_verifyVisible)
            {
                _isSyncingVerify = true;
                VerifyPlain.Text = VerifyMasked.Password;
                _isSyncingVerify = false;

                VerifyMasked.Visibility = Visibility.Collapsed;
                VerifyPlain.Visibility = Visibility.Visible;
                VerifyPlain.Focus();
                VerifyPlain.CaretIndex = VerifyPlain.Text.Length;

                VerifyEyeIcon.Data = Geometry.Parse(EyeClosed);
                VerifyToggle.ToolTip = Strings.SettingsContentView_HidePassword;
            }
            else
            {
                _isSyncingVerify = true;
                VerifyMasked.Password = VerifyPlain.Text;
                _isSyncingVerify = false;

                VerifyPlain.Visibility = Visibility.Collapsed;
                VerifyMasked.Visibility = Visibility.Visible;
                VerifyMasked.Focus();

                VerifyEyeIcon.Data = Geometry.Parse(EyeOpen);
                VerifyToggle.ToolTip = Strings.SettingsContentView_ShowPassword;
            }
        }

        // Password Sync — keeps masked and plain values in lockstep

        private void Password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_isSyncingPassword) return;
            PushUndo();
            _isSyncingPassword = true;
            PasswordPlain.Text = PasswordMasked.Password;
            _isSyncingPassword = false;
        }

        private void PasswordPlain_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isSyncingPassword) return;
            PushUndo();
            _isSyncingPassword = true;
            PasswordMasked.Password = PasswordPlain.Text;
            _isSyncingPassword = false;
        }

        private void Verify_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_isSyncingVerify) return;
            PushUndo();
            _isSyncingVerify = true;
            VerifyPlain.Text = VerifyMasked.Password;
            _isSyncingVerify = false;
        }

        private void VerifyPlain_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isSyncingVerify) return;
            PushUndo();
            _isSyncingVerify = true;
            VerifyMasked.Password = VerifyPlain.Text;
            _isSyncingVerify = false;
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PushUndo();
        }

        // Public API

        public string GetPassword() =>
            _passwordVisible ? PasswordPlain.Text : PasswordMasked.Password;

        public string GetVerifyPassword() =>
            _verifyVisible ? VerifyPlain.Text : VerifyMasked.Password;

        public string GetSelectedLanguage()
        {
            var content = (LanguageComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
            if (content != null && _languageCodes.TryGetValue(content, out var code))
                return code;
            return "en";
        }

        public bool IsPasswordValid()
        {
            string pw = GetPassword();
            return !string.IsNullOrEmpty(pw) && pw == GetVerifyPassword();
        }

        public void ClearPasswords()
        {
            _isSuppressingUndo = true;
            _isSyncingPassword = true;
            _isSyncingVerify = true;
            PasswordMasked.Password = "";
            PasswordPlain.Text = "";
            VerifyMasked.Password = "";
            VerifyPlain.Text = "";
            _isSyncingPassword = false;
            _isSyncingVerify = false;
            _isSuppressingUndo = false;
        }
    }
}