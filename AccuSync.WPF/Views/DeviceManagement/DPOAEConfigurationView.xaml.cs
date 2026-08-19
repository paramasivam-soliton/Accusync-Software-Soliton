// --------------------------------------------------------------------------------
// <copyright file="DPOAEConfigurationView.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using AccuSync.Application.Models;
using AccuSync.Presentation.Models;
using AccuSync.WPF.Resources;
using AccuSync.WPF.Controls;

namespace AccuSync.WPF.Views.DeviceManagement
{
    // NOTE: This is the third config view with near-identical undo/snapshot/restore infrastructure
    // (ABRConfigurationView, CommentsConfigView, this). Extract a generic ConfigViewBase.
    /// <summary>
    /// Toolbar takeover view for configuring DPOAE (Distortion Product Otoacoustic Emissions) protocols.
    /// </summary>
    public partial class DPOAEConfigurationView : UserControl
    {
        private ObservableCollection<DPOAEProtocolEntry> _protocols = new();
        private bool _isInitialized;
        private bool _isSuppressingUndo;
        private bool _isLoadingItem;

        private ToggleButton[] _freqToggles;

        // Undo Infrastructure
        private record DPOAESnapshot(
            string Name, string Description,
            int CategoryIndex, int StatusIndex,
            int L2Index, int L1Index,
            bool[] Frequencies,
            int RetestFreqIndex, int PassCriterionIndex,
            int AutoStopIndex, int MinLevelIndex, int SNRIndex
        );

        private DPOAESnapshot _savedState;
        private readonly Stack<DPOAESnapshot> _undoStack = new();

        // Initialization

        /// <summary>
        /// Initializes the control and populates the default DPOAE protocol once loaded.
        /// </summary>
        public DPOAEConfigurationView()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                _freqToggles = new[] { Freq1000, Freq1500, Freq2000, Freq3000, Freq4000, Freq5000, Freq6000 };
                _isInitialized = true;
                InitializeDefaults();
            };
        }

        private void InitializeDefaults()
        {
            _isSuppressingUndo = true;

            _protocols.Add(new DPOAEProtocolEntry
            {
                Name = "DP Full",
                Description = "",
                Category = "Basic",
                Status = "Active",
                CategoryIndex = 0,
                StatusIndex = 0,
                L2Index = 1,              // 55 dB
                L1Index = 0,              // Auto
                Frequencies = new[] { false, true, true, true, true, true, true },
                RetestFreqIndex = 1,      // No
                PassCriterionIndex = 3,   // 4 frequencies of 6
                AutoStopIndex = 0,        // Yes
                MinLevelIndex = 2,        // -5 dB
                SNRIndex = 1              // 9 dB
            });

            ProtocolsListView.ItemsSource = _protocols;
            EmptyListPanel.Visibility = _protocols.Count > 0 ? Visibility.Collapsed : Visibility.Visible;

            if (_protocols.Count > 0)
                ProtocolsListView.SelectedIndex = 0;

            _isSuppressingUndo = false;
        }

        // List Selection

        private void ProtocolsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProtocolsListView.SelectedItem is DPOAEProtocolEntry protocol)
            {
                _isLoadingItem = true;
                _isSuppressingUndo = true;

                EmptyDetailPanel.Visibility = Visibility.Collapsed;
                DetailFormPanel.Visibility = Visibility.Visible;
                DetailHeaderText.Text = protocol.Name;

                NameBox.Text = protocol.Name;
                DescriptionBox.Text = protocol.Description;
                CategoryCombo.SelectedIndex = protocol.CategoryIndex;
                StatusCombo.SelectedIndex = protocol.StatusIndex;

                L2Combo.SelectedIndex = protocol.L2Index;
                L1Combo.SelectedIndex = protocol.L1Index;

                for (int i = 0; i < _freqToggles.Length && i < protocol.Frequencies.Length; i++)
                    _freqToggles[i].IsChecked = protocol.Frequencies[i];

                RetestFreqCombo.SelectedIndex = protocol.RetestFreqIndex;
                PassCriterionCombo.SelectedIndex = protocol.PassCriterionIndex;
                AutoStopCombo.SelectedIndex = protocol.AutoStopIndex;
                MinLevelCombo.SelectedIndex = protocol.MinLevelIndex;
                SNRCombo.SelectedIndex = protocol.SNRIndex;

                _undoStack.Clear();
                _savedState = CaptureSnapshot();

                _isLoadingItem = false;
                _isSuppressingUndo = false;
            }
            else
            {
                EmptyDetailPanel.Visibility = Visibility.Visible;
                DetailFormPanel.Visibility = Visibility.Collapsed;
                DetailHeaderText.Text = Strings.DPOAEConfigurationView_ProtocolDetails;
            }
        }

        // Field Change Tracking

        private void Field_Changed(object sender, RoutedEventArgs e)
        {
            PushUndo();
            SyncToSelectedProtocol();
        }

        private void Field_Changed(object sender, TextChangedEventArgs e)
        {
            PushUndo();
            SyncToSelectedProtocol();
        }

        private void Freq_Changed(object sender, RoutedEventArgs e)
        {
            PushUndo();
            SyncToSelectedProtocol();
        }

        private void SyncToSelectedProtocol()
        {
            if (_isLoadingItem || !_isInitialized) return;
            if (ProtocolsListView.SelectedItem is not DPOAEProtocolEntry protocol) return;

            protocol.Name = NameBox.Text;
            protocol.Description = DescriptionBox.Text;
            protocol.CategoryIndex = CategoryCombo.SelectedIndex;
            protocol.StatusIndex = StatusCombo.SelectedIndex;
            protocol.L2Index = L2Combo.SelectedIndex;
            protocol.L1Index = L1Combo.SelectedIndex;
            protocol.Frequencies = _freqToggles.Select(t => t.IsChecked == true).ToArray();
            protocol.RetestFreqIndex = RetestFreqCombo.SelectedIndex;
            protocol.PassCriterionIndex = PassCriterionCombo.SelectedIndex;
            protocol.AutoStopIndex = AutoStopCombo.SelectedIndex;
            protocol.MinLevelIndex = MinLevelCombo.SelectedIndex;
            protocol.SNRIndex = SNRCombo.SelectedIndex;

            protocol.Category = (CategoryCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Basic";
            protocol.Status = (StatusCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Active";

            DetailHeaderText.Text = protocol.Name;
            ProtocolsListView.Items.Refresh();
        }

        // Snapshot and Undo

        private bool[] CaptureFrequencies()
        {
            return _freqToggles.Select(t => t.IsChecked == true).ToArray();
        }

        private DPOAESnapshot CaptureSnapshot()
        {
            return new DPOAESnapshot(
                NameBox.Text,
                DescriptionBox.Text,
                CategoryCombo.SelectedIndex,
                StatusCombo.SelectedIndex,
                L2Combo.SelectedIndex,
                L1Combo.SelectedIndex,
                CaptureFrequencies(),
                RetestFreqCombo.SelectedIndex,
                PassCriterionCombo.SelectedIndex,
                AutoStopCombo.SelectedIndex,
                MinLevelCombo.SelectedIndex,
                SNRCombo.SelectedIndex
            );
        }

        private void RestoreSnapshot(DPOAESnapshot snap)
        {
            _isLoadingItem = true;
            _isSuppressingUndo = true;

            NameBox.Text = snap.Name;
            DescriptionBox.Text = snap.Description;
            CategoryCombo.SelectedIndex = snap.CategoryIndex;
            StatusCombo.SelectedIndex = snap.StatusIndex;
            L2Combo.SelectedIndex = snap.L2Index;
            L1Combo.SelectedIndex = snap.L1Index;

            for (int i = 0; i < _freqToggles.Length && i < snap.Frequencies.Length; i++)
                _freqToggles[i].IsChecked = snap.Frequencies[i];

            RetestFreqCombo.SelectedIndex = snap.RetestFreqIndex;
            PassCriterionCombo.SelectedIndex = snap.PassCriterionIndex;
            AutoStopCombo.SelectedIndex = snap.AutoStopIndex;
            MinLevelCombo.SelectedIndex = snap.MinLevelIndex;
            SNRCombo.SelectedIndex = snap.SNRIndex;

            SyncToSelectedProtocol();

            _isLoadingItem = false;
            _isSuppressingUndo = false;
        }

        private void PushUndo()
        {
            if (!_isInitialized || _isSuppressingUndo || _isLoadingItem) return;
            _undoStack.Push(CaptureSnapshot());
        }

        // Public Handlers (called by SidebarNavigation)

        /// <summary>
        /// Adds a new DPOAE protocol with default values and selects it.
        /// </summary>
        public void HandleAdd()
        {
            var entry = new DPOAEProtocolEntry
            {
                Name = "New Protocol",
                Description = "",
                Category = "Basic",
                Status = "Active",
                CategoryIndex = 0,
                StatusIndex = 0,
                L2Index = 1,
                L1Index = 0,
                Frequencies = new[] { false, true, true, true, true, true, true },
                RetestFreqIndex = 1,
                PassCriterionIndex = 3,
                AutoStopIndex = 0,
                MinLevelIndex = 2,
                SNRIndex = 1
            };

            _protocols.Add(entry);
            EmptyListPanel.Visibility = Visibility.Collapsed;
            ProtocolsListView.SelectedItem = entry;
        }

        /// <summary>
        /// Deletes the selected DPOAE protocol after user confirmation.
        /// </summary>
        public void HandleDelete()
        {
            if (ProtocolsListView.SelectedItem is not DPOAEProtocolEntry protocol) return;

            var result = AppDialog.Show(
                string.Format(Strings.DPOAEConfigurationView_ConfirmDeleteProtocol, protocol.Name),
                Strings.DPOAEConfigurationView_ConfirmDelete, MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            int idx = ProtocolsListView.SelectedIndex;
            _protocols.Remove(protocol);
            ProtocolsListView.Items.Refresh();

            if (_protocols.Count == 0)
            {
                EmptyListPanel.Visibility = Visibility.Visible;
                EmptyDetailPanel.Visibility = Visibility.Visible;
                DetailFormPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                ProtocolsListView.SelectedIndex = Math.Min(idx, _protocols.Count - 1);
            }
        }

        // TODO: HandleSave has no actual persistence — same pattern as all other config views.
        /// <summary>
        /// Saves the current state as the new baseline and clears the undo stack.
        /// </summary>
        public void HandleSave()
        {
            _savedState = CaptureSnapshot();
            _undoStack.Clear();
            AppDialog.Show(Strings.DPOAEConfigurationView_ProtocolSaved, Strings.DPOAEConfigurationView_Save, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Reverts all changes back to the last saved state.
        /// </summary>
        public void HandleRevert()
        {
            if (_savedState == null) return;
            RestoreSnapshot(_savedState);
            _undoStack.Clear();
        }

        /// <summary>
        /// Restores the previous state from the undo stack.
        /// </summary>
        public void HandleUndo()
        {
            if (_undoStack.Count == 0) return;
            var snap = _undoStack.Pop();
            RestoreSnapshot(snap);
        }
    }
}
