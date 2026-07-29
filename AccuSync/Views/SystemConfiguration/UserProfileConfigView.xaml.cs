// --------------------------------------------------------------------------------
// <copyright file="UserProfileConfigView.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using AccuSync.Resources;
using AccuSync.Controls;

namespace AccuSync.Views.SystemConfiguration
{
    public partial class UserProfileConfigView : UserControl
    {
        private bool _isInitialized = false;
        private bool _isSuppressingUndo = false;

        private int _lockingTimeValue = 15;
        private const int LockingTimeMin = 1;
        private const int LockingTimeMax = 60;

        private static readonly Dictionary<int, string> _passwordDescriptions = new()
        {
            { 0, Strings.UserProfileConfigView_PasswordDescNone },
            { 1, Strings.UserProfileConfigView_PasswordDescSimple },
            { 2, Strings.UserProfileConfigView_PasswordDescComplex }
        };

        private record UserProfileSnapshot(
            int LockingTimeValue,
            int PasswordRuleIndex
        );

        private UserProfileSnapshot _savedState;
        private readonly Stack<UserProfileSnapshot> _undoStack = new();

        public UserProfileConfigView()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                _isInitialized = true;
                UpdateLockingTimeDisplay();
                UpdatePasswordDescription();
                _savedState = TakeSnapshot();
            };
        }

        // Snapshot Helpers

        private UserProfileSnapshot TakeSnapshot()
        {
            return new UserProfileSnapshot(
                _lockingTimeValue,
                PasswordRuleCombo.SelectedIndex
            );
        }

        private void RestoreSnapshot(UserProfileSnapshot snap)
        {
            _isSuppressingUndo = true;

            _lockingTimeValue = snap.LockingTimeValue;
            UpdateLockingTimeDisplay();
            PasswordRuleCombo.SelectedIndex = snap.PasswordRuleIndex;
            UpdatePasswordDescription();

            _isSuppressingUndo = false;
        }

        private void PushUndo()
        {
            if (!_isInitialized || _isSuppressingUndo) return;
            _undoStack.Push(TakeSnapshot());
        }

        // Save / Revert / Undo — called from toolbar

        // NOTE: HandleSave captures snapshot only — doesn't persist to database
        public void HandleSave()
        {
            _savedState = TakeSnapshot();
            _undoStack.Clear();
            AppDialog.Show(Strings.UserProfileConfigView_ConfigurationSaved, Strings.UserProfileConfigView_Save, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void HandleRevert()
        {
            if (_savedState == null) return;
            PushUndo();
            RestoreSnapshot(_savedState);
        }

        public void HandleUndo()
        {
            if (_undoStack.Count == 0) return;
            var prev = _undoStack.Pop();
            RestoreSnapshot(prev);
        }

        // Locking Time Spinner

        private void LockingTime_Up(object sender, RoutedEventArgs e)
        {
            if (_lockingTimeValue < LockingTimeMax)
            {
                PushUndo();
                _lockingTimeValue++;
                UpdateLockingTimeDisplay();
            }
        }

        private void LockingTime_Down(object sender, RoutedEventArgs e)
        {
            if (_lockingTimeValue > LockingTimeMin)
            {
                PushUndo();
                _lockingTimeValue--;
                UpdateLockingTimeDisplay();
            }
        }

        private void UpdateLockingTimeDisplay()
        {
            LockingTimeText.Text = string.Format(Strings.UserProfileConfigView_LockingTimeMinutes, _lockingTimeValue);
        }

        // Password Security Rule

        private void PasswordRuleCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitialized) return;
            PushUndo();
            UpdatePasswordDescription();
        }

        private void UpdatePasswordDescription()
        {
            if (PasswordDescriptionBox == null) return;
            int idx = PasswordRuleCombo.SelectedIndex;
            PasswordDescriptionBox.Text = _passwordDescriptions.ContainsKey(idx)
                ? _passwordDescriptions[idx]
                : "";
        }
    }
}