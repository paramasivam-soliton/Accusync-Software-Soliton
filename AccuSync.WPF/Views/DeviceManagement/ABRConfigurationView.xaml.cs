// --------------------------------------------------------------------------------
// <copyright file="ABRConfigurationView.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AccuSync.Application.Models;
using AccuSync.WPF.Resources;
using AccuSync.WPF.Controls;

namespace AccuSync.WPF.Views.DeviceManagement
{
    /// <summary>
    /// Toolbar takeover view for configuring ABR (Auditory Brainstem Response) protocols.
    /// </summary>
    public partial class ABRConfigurationView : UserControl
    {
        private ObservableCollection<ABRProtocolEntry> _protocols = new();
        private bool _isInitialized;
        private bool _isSuppressingUndo;
        private bool _isLoadingItem;

        // Undo Infrastructure
        private record ABRSnapshot(
            string Name, string Description,
            int CategoryIndex, int StatusIndex,
            int ABRLevelIndex, int NotchFilterIndex, int StimulusDuringPauseIndex
        );

        private ABRSnapshot _savedState;
        private readonly Stack<ABRSnapshot> _undoStack = new();

        // Initialization

        /// <summary>
        /// Initializes the control and populates the default ABR protocol once loaded.
        /// </summary>
        public ABRConfigurationView()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                _isInitialized = true;
                InitializeDefaults();
            };
        }

        private void InitializeDefaults()
        {
            _isSuppressingUndo = true;

            _protocols.Add(new ABRProtocolEntry
            {
                Name = "ABR35",
                Description = "",
                Category = "Basic",
                Status = "Active",
                CategoryIndex = 0,
                StatusIndex = 0,
                ABRLevelIndex = 1,       // 35 dB
                NotchFilterIndex = 0,    // 50 Hz
                StimulusDuringPauseIndex = 0  // Yes
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
            if (ProtocolsListView.SelectedItem is ABRProtocolEntry protocol)
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
                ABRLevelCombo.SelectedIndex = protocol.ABRLevelIndex;
                NotchFilterCombo.SelectedIndex = protocol.NotchFilterIndex;
                StimulusDuringPauseCombo.SelectedIndex = protocol.StimulusDuringPauseIndex;

                _undoStack.Clear();
                _savedState = CaptureSnapshot();

                _isLoadingItem = false;
                _isSuppressingUndo = false;
            }
            else
            {
                EmptyDetailPanel.Visibility = Visibility.Visible;
                DetailFormPanel.Visibility = Visibility.Collapsed;
                DetailHeaderText.Text = Strings.ABRConfigurationView_ProtocolDetails;
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

        private void SyncToSelectedProtocol()
        {
            if (_isLoadingItem || !_isInitialized) return;
            if (ProtocolsListView.SelectedItem is not ABRProtocolEntry protocol) return;

            protocol.Name = NameBox.Text;
            protocol.Description = DescriptionBox.Text;
            protocol.CategoryIndex = CategoryCombo.SelectedIndex;
            protocol.StatusIndex = StatusCombo.SelectedIndex;
            protocol.ABRLevelIndex = ABRLevelCombo.SelectedIndex;
            protocol.NotchFilterIndex = NotchFilterCombo.SelectedIndex;
            protocol.StimulusDuringPauseIndex = StimulusDuringPauseCombo.SelectedIndex;

            protocol.Category = (CategoryCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Basic";
            protocol.Status = (StatusCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Active";

            DetailHeaderText.Text = protocol.Name;
            ProtocolsListView.Items.Refresh();
        }

        // Snapshot and Undo

        private ABRSnapshot CaptureSnapshot()
        {
            return new ABRSnapshot(
                NameBox.Text,
                DescriptionBox.Text,
                CategoryCombo.SelectedIndex,
                StatusCombo.SelectedIndex,
                ABRLevelCombo.SelectedIndex,
                NotchFilterCombo.SelectedIndex,
                StimulusDuringPauseCombo.SelectedIndex
            );
        }

        private void RestoreSnapshot(ABRSnapshot snap)
        {
            _isLoadingItem = true;
            _isSuppressingUndo = true;

            NameBox.Text = snap.Name;
            DescriptionBox.Text = snap.Description;
            CategoryCombo.SelectedIndex = snap.CategoryIndex;
            StatusCombo.SelectedIndex = snap.StatusIndex;
            ABRLevelCombo.SelectedIndex = snap.ABRLevelIndex;
            NotchFilterCombo.SelectedIndex = snap.NotchFilterIndex;
            StimulusDuringPauseCombo.SelectedIndex = snap.StimulusDuringPauseIndex;

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
        /// Adds a new ABR protocol with default values and selects it.
        /// </summary>
        public void HandleAdd()
        {
            var entry = new ABRProtocolEntry
            {
                Name = "New Protocol",
                Description = "",
                Category = "Basic",
                Status = "Active",
                CategoryIndex = 0,
                StatusIndex = 0,
                ABRLevelIndex = 1,
                NotchFilterIndex = 0,
                StimulusDuringPauseIndex = 0
            };

            _protocols.Add(entry);
            EmptyListPanel.Visibility = Visibility.Collapsed;
            ProtocolsListView.SelectedItem = entry;
        }

        /// <summary>
        /// Deletes the selected ABR protocol after user confirmation.
        /// </summary>
        public void HandleDelete()
        {
            if (ProtocolsListView.SelectedItem is not ABRProtocolEntry protocol) return;

            var result = AppDialog.Show(
                string.Format(Strings.ABRConfigurationView_ConfirmDeleteProtocol, protocol.Name),
                Strings.ABRConfigurationView_ConfirmDelete, MessageBoxButton.YesNo, MessageBoxImage.Warning);

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

        // TODO: HandleSave only captures a snapshot and shows a MessageBox — no actual
        //       persistence. Wire to DatabaseService or file storage.
        /// <summary>
        /// Saves the current state as the new baseline and clears the undo stack.
        /// </summary>
        public void HandleSave()
        {
            _savedState = CaptureSnapshot();
            _undoStack.Clear();
            AppDialog.Show(Strings.ABRConfigurationView_ProtocolSaved, Strings.ABRConfigurationView_Save, MessageBoxButton.OK, MessageBoxImage.Information);
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