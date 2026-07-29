// --------------------------------------------------------------------------------
// <copyright file="SiteFacilityConfigView.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using AccuSync.WPF.Resources;
using AccuSync.WPF.Controls;

namespace AccuSync.WPF.Views.SystemConfiguration
{
    public partial class SiteFacilityConfigView : UserControl
    {
        private bool _isInitialized = false;
        private bool _isSuppressingUndo = false;

        // ═══════════════════════════════════
        // State snapshots for Revert & Undo
        // ═══════════════════════════════════

        private record SiteFacilitySnapshot(
            bool TransferToDevice
        );

        private SiteFacilitySnapshot _savedState;
        private readonly Stack<SiteFacilitySnapshot> _undoStack = new();

        public SiteFacilityConfigView()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                _isInitialized = true;
                _savedState = TakeSnapshot();
            };
        }

        // ═══════════════════════════════════
        // Snapshot helpers
        // ═══════════════════════════════════

        private SiteFacilitySnapshot TakeSnapshot()
        {
            return new SiteFacilitySnapshot(
                TransferCheckBox.IsChecked == true
            );
        }

        private void RestoreSnapshot(SiteFacilitySnapshot snap)
        {
            _isSuppressingUndo = true;
            TransferCheckBox.IsChecked = snap.TransferToDevice;
            _isSuppressingUndo = false;
        }

        private void PushUndo()
        {
            if (!_isInitialized || _isSuppressingUndo) return;
            _undoStack.Push(TakeSnapshot());
        }

        // ═══════════════════════════════════
        // CheckBox change handler
        // ═══════════════════════════════════

        private void CheckBox_Changed(object sender, RoutedEventArgs e)
        {
            PushUndo();
        }

        // ═══════════════════════════════════
        // Public API — Save / Revert / Undo
        // ═══════════════════════════════════

        public void HandleSave()
        {
            _savedState = TakeSnapshot();
            _undoStack.Clear();
            AppDialog.Show(Strings.SiteFacilityConfigView_ConfigurationSaved, Strings.SiteFacilityConfigView_Save, MessageBoxButton.OK, MessageBoxImage.Information);
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
    }
}