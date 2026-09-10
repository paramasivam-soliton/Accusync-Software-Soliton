// --------------------------------------------------------------------------------
// <copyright file="SettingsContentView.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AccuSync.WPF.Resources;
using AccuSync.WPF.Controls;

namespace AccuSync.WPF.Views.Settings
{
    public partial class SettingsContentView : UserControl
    {
        private bool _isSuppressingUndo = false;

        private static readonly Dictionary<string, string> _languageCodes = new()
        {
            { "English", "en" },
            { "French", "fr" },
            { "Italian", "it" },
            { "German", "de" },
            { "Spanish", "es" }
        };

        // Snapshot for Revert and Undo — Language only. Password changes are handled
        // by ChangePasswordSettingsView, submitted through this page's own Save.
        private record SettingsSnapshot(int LanguageIndex);

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
            return new SettingsSnapshot(LanguageComboBox.SelectedIndex);
        }

        private void RestoreSnapshot(SettingsSnapshot snap)
        {
            _isSuppressingUndo = true;
            LanguageComboBox.SelectedIndex = snap.LanguageIndex;
            _isSuppressingUndo = false;
        }

        private void PushUndo()
        {
            if (_isSuppressingUndo) return;
            _undoStack.Push(TakeSnapshot());
        }

        // Save / Revert / Undo — called from toolbar

        /// <summary>
        /// Submits a pending password change (if any) via <see cref="ChangePasswordCard"/>,
        /// then saves the Language selection as the baseline for Revert and clears the
        /// undo stack. If a password change was attempted but rejected, the reason is
        /// shown inline on the card and this stops short of the "Settings saved" dialog.
        /// </summary>
        public async Task SaveState()
        {
            bool passwordOk = await ChangePasswordCard.SubmitIfRequestedAsync();
            if (!passwordOk)
            {
                return;
            }

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

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PushUndo();
        }

        // Public API

        public string GetSelectedLanguage()
        {
            var content = (LanguageComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
            if (content != null && _languageCodes.TryGetValue(content, out var code))
                return code;
            return "en";
        }
    }
}