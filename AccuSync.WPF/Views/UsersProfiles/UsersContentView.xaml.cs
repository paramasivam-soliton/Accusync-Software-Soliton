// --------------------------------------------------------------------------------
// <copyright file="UsersContentView.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AccuSync.Application.Models;
using AccuSync.WPF.Resources;
using AccuSync.WPF.Controls;

namespace AccuSync.WPF.Views.UsersProfiles
{
    /// <summary>
    /// Immutable snapshot of a user's editable form state, used for undo/revert.
    /// </summary>
    /// <param name="LoginName">The user's login name.</param>
    /// <param name="ProfileIndex">The selected index in the profile combo box.</param>
    /// <param name="FirstName">The user's first name.</param>
    /// <param name="LastName">The user's last name.</param>
    /// <param name="Password">The user's password.</param>
    /// <param name="Verify">The password verification value.</param>
    /// <param name="LanguageIndex">The selected index in the language combo box.</param>
    /// <param name="StatusIndex">The selected index in the status combo box.</param>
    public record UserSnapshot(
        string LoginName, int ProfileIndex, string FirstName, string LastName,
        string Password, string Verify, int LanguageIndex, int StatusIndex);

    /// <summary>
    /// Users screen content: user list/detail form plus the Profiles takeover view.
    /// </summary>
    public partial class UsersContentView : UserControl
    {
        private ObservableCollection<UserEntry> _users;
        private bool _isInitialized;
        private bool _isSuppressingUndo;
        private bool _isSyncingPassword;
        private bool _isLoadingUser;
        private bool _passwordVisible;
        private bool _verifyVisible;

        private UserSnapshot _savedState;
        private Stack<UserSnapshot> _undoStack = new();

        /// <summary>
        /// Initializes the control and populates the default user list once loaded.
        /// </summary>
        public UsersContentView()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                _isInitialized = true;
                InitializeDefaultUsers();
            };
        }

        // Initialization

        // TODO: Replace hardcoded users with data from DatabaseService
        // BUG: Hardcoded "1234" default passwords — see cross-cutting security issue
        private void InitializeDefaultUsers()
        {
            _users = new ObservableCollection<UserEntry>
            {
                new UserEntry
                {
                    LoginName = "Admin", FirstName = "", LastName = "Administrator",
                    Profile = "Administrator", Status = "Active", IsLocked = false,
                    Password = "1234", Verify = "1234", LanguageIndex = 0
                },
                new UserEntry
                {
                    LoginName = "Screener", FirstName = "", LastName = "Screener",
                    Profile = "Screener", Status = "Active", IsLocked = false,
                    Password = "1234", Verify = "1234", LanguageIndex = 0
                }
            };

            UsersListView.ItemsSource = _users;

            if (_users.Count > 0)
                UsersListView.SelectedIndex = 0;
        }

        // Card Selection

        private void UsersListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var user = UsersListView.SelectedItem as UserEntry;
            if (user == null)
            {
                EmptyStatePanel.Visibility = Visibility.Visible;
                return;
            }

            EmptyStatePanel.Visibility = Visibility.Collapsed;
            LoadUserIntoForm(user);
        }

        private void LoadUserIntoForm(UserEntry user)
        {
            _isLoadingUser = true;

            DetailHeaderText.Text = user.LoginName.ToUpperInvariant();
            LoginNameBox.Text = user.LoginName;
            FirstNameBox.Text = user.FirstName;
            LastNameBox.Text = user.LastName;

            for (int i = 0; i < ProfileCombo.Items.Count; i++)
            {
                if ((ProfileCombo.Items[i] as ComboBoxItem)?.Content?.ToString() == user.Profile)
                { ProfileCombo.SelectedIndex = i; break; }
            }

            // Reset password visibility to hidden on user switch
            _passwordVisible = false;
            _verifyVisible = false;
            PasswordBox.Visibility = Visibility.Visible;
            PasswordTextBox.Visibility = Visibility.Collapsed;
            VerifyBox.Visibility = Visibility.Visible;
            VerifyTextBox.Visibility = Visibility.Collapsed;

            _isSyncingPassword = true;
            PasswordBox.Password = user.Password;
            PasswordTextBox.Text = user.Password;
            VerifyBox.Password = user.Verify;
            VerifyTextBox.Text = user.Verify;
            _isSyncingPassword = false;

            LanguageCombo.SelectedIndex = user.LanguageIndex;

            for (int i = 0; i < StatusCombo.Items.Count; i++)
            {
                if ((StatusCombo.Items[i] as ComboBoxItem)?.Content?.ToString() == user.Status)
                { StatusCombo.SelectedIndex = i; break; }
            }

            LockedDisplay.Text = user.LockedDisplay;

            UpdateMatchIndicator();

            _savedState = CaptureSnapshot();
            _undoStack.Clear();

            _isLoadingUser = false;
        }

        // Snapshot Helpers

        private UserSnapshot CaptureSnapshot()
        {
            string pwd = _passwordVisible ? PasswordTextBox.Text : PasswordBox.Password;
            string ver = _verifyVisible ? VerifyTextBox.Text : VerifyBox.Password;

            return new UserSnapshot(
                LoginNameBox?.Text ?? "",
                ProfileCombo?.SelectedIndex ?? 0,
                FirstNameBox?.Text ?? "",
                LastNameBox?.Text ?? "",
                pwd ?? "",
                ver ?? "",
                LanguageCombo?.SelectedIndex ?? 0,
                StatusCombo?.SelectedIndex ?? 0);
        }

        private void ApplySnapshot(UserSnapshot s)
        {
            _isSuppressingUndo = true;
            _isLoadingUser = true;

            LoginNameBox.Text = s.LoginName;
            ProfileCombo.SelectedIndex = s.ProfileIndex;
            FirstNameBox.Text = s.FirstName;
            LastNameBox.Text = s.LastName;

            _isSyncingPassword = true;
            if (_passwordVisible)
                PasswordTextBox.Text = s.Password;
            else
                PasswordBox.Password = s.Password;
            PasswordTextBox.Text = s.Password;
            PasswordBox.Password = s.Password;

            if (_verifyVisible)
                VerifyTextBox.Text = s.Verify;
            else
                VerifyBox.Password = s.Verify;
            VerifyTextBox.Text = s.Verify;
            VerifyBox.Password = s.Verify;
            _isSyncingPassword = false;

            LanguageCombo.SelectedIndex = s.LanguageIndex;
            StatusCombo.SelectedIndex = s.StatusIndex;

            UpdateMatchIndicator();

            _isLoadingUser = false;
            _isSuppressingUndo = false;
        }

        private void PushUndo()
        {
            if (!_isInitialized || _isSuppressingUndo || _isLoadingUser) return;
            _undoStack.Push(CaptureSnapshot());
        }

        // Save / Revert / Undo — called from toolbar

        // NOTE: HandleSave updates the in-memory model but doesn't persist to database
        /// <summary>
        /// Validates the selected user's required fields and password match, then saves
        /// the current state as the new baseline.
        /// </summary>
        public void HandleSave()
        {
            var user = UsersListView.SelectedItem as UserEntry;
            if (user == null) return;

            if (string.IsNullOrWhiteSpace(LoginNameBox.Text))
            {
                AppDialog.Show(Strings.UsersContentView_LoginNameRequired, Strings.UsersContentView_Validation,
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                LoginNameBox.Focus();
                return;
            }
            if (ProfileCombo.SelectedIndex < 0)
            {
                AppDialog.Show(Strings.UsersContentView_ProfileRequired, Strings.UsersContentView_Validation,
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string pwd = _passwordVisible ? PasswordTextBox.Text : PasswordBox.Password;
            string ver = _verifyVisible ? VerifyTextBox.Text : VerifyBox.Password;
            if (!string.IsNullOrEmpty(pwd) && pwd != ver)
            {
                AppDialog.Show(Strings.UsersContentView_PasswordsDoNotMatch, Strings.UsersContentView_Validation,
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            user.LoginName = LoginNameBox.Text;
            user.Profile = (ProfileCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
            user.FirstName = FirstNameBox.Text;
            user.LastName = LastNameBox.Text;
            user.Password = pwd;
            user.Verify = ver;
            user.LanguageIndex = LanguageCombo.SelectedIndex;
            user.Status = (StatusCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Active";

            DetailHeaderText.Text = user.LoginName.ToUpperInvariant();

            _savedState = CaptureSnapshot();
            _undoStack.Clear();

            AppDialog.Show(Strings.UsersContentView_UserSaved, Strings.UsersContentView_Save,
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Reverts all changes back to the last saved state.
        /// </summary>
        public void HandleRevert()
        {
            if (_savedState == null) return;
            PushUndo();
            ApplySnapshot(_savedState);
        }

        /// <summary>
        /// Restores the previous state from the undo stack.
        /// </summary>
        public void HandleUndo()
        {
            if (_undoStack.Count == 0) return;
            var prev = _undoStack.Pop();
            ApplySnapshot(prev);
        }

        // Add / Delete / Unlock — called from toolbar

        /// <summary>
        /// Adds a new user with default values and selects it.
        /// </summary>
        public void HandleAdd()
        {
            var newUser = new UserEntry
            {
                LoginName = "NewUser",
                FirstName = "",
                LastName = "",
                Profile = "Screener",
                Status = "Active",
                IsLocked = false,
                Password = "",
                Verify = "",
                LanguageIndex = 0
            };
            _users.Add(newUser);
            UsersListView.SelectedItem = newUser;
        }

        /// <summary>
        /// Deletes the selected user after user confirmation.
        /// </summary>
        public void HandleDelete()
        {
            var user = UsersListView.SelectedItem as UserEntry;
            if (user == null) return;

            var result = AppDialog.Show(
                string.Format(Strings.UsersContentView_DeleteConfirm, user.LoginName),
                Strings.UsersContentView_DeleteUser, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                int idx = _users.IndexOf(user);
                _users.Remove(user);
                if (_users.Count > 0)
                    UsersListView.SelectedIndex = Math.Min(idx, _users.Count - 1);
            }
        }

        /// <summary>
        /// Unlocks the selected user account if it is currently locked.
        /// </summary>
        public void HandleUnlock()
        {
            var user = UsersListView.SelectedItem as UserEntry;
            if (user == null) return;

            if (user.IsLocked)
            {
                user.IsLocked = false;
                LockedDisplay.Text = user.LockedDisplay;
                AppDialog.Show(string.Format(Strings.UsersContentView_UserUnlocked, user.LoginName),
                    Strings.UsersContentView_Unlock, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                AppDialog.Show(string.Format(Strings.UsersContentView_UserNotLocked, user.LoginName),
                    Strings.UsersContentView_Unlock, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Password Visibility Toggle
        // Swaps between PasswordBox (masked) and TextBox (visible), keeping values in sync

        private void TogglePasswordVisibility_Click(object sender, RoutedEventArgs e)
        {
            _isSyncingPassword = true;
            _passwordVisible = !_passwordVisible;

            if (_passwordVisible)
            {
                PasswordTextBox.Text = PasswordBox.Password;
                PasswordBox.Visibility = Visibility.Collapsed;
                PasswordTextBox.Visibility = Visibility.Visible;
                PasswordTextBox.Focus();
            }
            else
            {
                PasswordBox.Password = PasswordTextBox.Text;
                PasswordTextBox.Visibility = Visibility.Collapsed;
                PasswordBox.Visibility = Visibility.Visible;
                PasswordBox.Focus();
            }
            _isSyncingPassword = false;
        }

        private void ToggleVerifyVisibility_Click(object sender, RoutedEventArgs e)
        {
            _isSyncingPassword = true;
            _verifyVisible = !_verifyVisible;

            if (_verifyVisible)
            {
                VerifyTextBox.Text = VerifyBox.Password;
                VerifyBox.Visibility = Visibility.Collapsed;
                VerifyTextBox.Visibility = Visibility.Visible;
                VerifyTextBox.Focus();
            }
            else
            {
                VerifyBox.Password = VerifyTextBox.Text;
                VerifyTextBox.Visibility = Visibility.Collapsed;
                VerifyBox.Visibility = Visibility.Visible;
                VerifyBox.Focus();
            }
            _isSyncingPassword = false;
        }

        // Password Sync and Match Indicator

        private void Password_Changed(object sender, RoutedEventArgs e)
        {
            if (_isSyncingPassword) return;
            _isSyncingPassword = true;
            PasswordTextBox.Text = PasswordBox.Password;
            _isSyncingPassword = false;
            PushUndo();
            UpdateMatchIndicator();
        }

        private void PasswordText_Changed(object sender, TextChangedEventArgs e)
        {
            if (_isSyncingPassword) return;
            _isSyncingPassword = true;
            PasswordBox.Password = PasswordTextBox.Text;
            _isSyncingPassword = false;
            PushUndo();
            UpdateMatchIndicator();
        }

        private void Verify_Changed(object sender, RoutedEventArgs e)
        {
            if (_isSyncingPassword) return;
            _isSyncingPassword = true;
            VerifyTextBox.Text = VerifyBox.Password;
            _isSyncingPassword = false;
            PushUndo();
            UpdateMatchIndicator();
        }

        private void VerifyText_Changed(object sender, TextChangedEventArgs e)
        {
            if (_isSyncingPassword) return;
            _isSyncingPassword = true;
            VerifyBox.Password = VerifyTextBox.Text;
            _isSyncingPassword = false;
            PushUndo();
            UpdateMatchIndicator();
        }

        private void UpdateMatchIndicator()
        {
            string pwd = _passwordVisible ? PasswordTextBox.Text : PasswordBox.Password;
            string ver = _verifyVisible ? VerifyTextBox.Text : VerifyBox.Password;

            if (string.IsNullOrEmpty(pwd) && string.IsNullOrEmpty(ver))
            {
                MatchIndicator.Text = "";
                return;
            }

            if (pwd == ver)
            {
                MatchIndicator.Text = Strings.UsersContentView_PasswordsMatch;
                MatchIndicator.Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#429C10"));
            }
            else
            {
                MatchIndicator.Text = Strings.UsersContentView_PasswordsDoNotMatchIndicator;
                MatchIndicator.Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#dc3545"));
            }
        }

        // Field Change Handlers

        private void Field_Changed(object sender, EventArgs e)
        {
            PushUndo();
        }

        private void Field_Changed(object sender, SelectionChangedEventArgs e)
        {
            PushUndo();
        }

        // Profiles Takeover — switches between Users and Profiles views

        /// <summary>Provides access to the embedded profiles view for the toolbar's takeover mode.</summary>
        public ProfilesContentView ProfilesView => ProfilesContent;

        /// <summary>Switches from the normal user list/detail view into the profiles takeover.</summary>
        public void ShowProfiles()
        {
            NormalUsersGrid.Visibility = Visibility.Collapsed;
            ProfilesContent.Visibility = Visibility.Visible;
        }

        /// <summary>Switches back from the profiles takeover to the normal user view.</summary>
        public void HideProfiles()
        {
            ProfilesContent.Visibility = Visibility.Collapsed;
            NormalUsersGrid.Visibility = Visibility.Visible;
        }
    }
}