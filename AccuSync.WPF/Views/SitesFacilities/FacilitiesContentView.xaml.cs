// --------------------------------------------------------------------------------
// <copyright file="FacilitiesContentView.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using AccuSync.Application.Models;
using AccuSync.WPF.Resources;
using AccuSync.WPF.Controls;

namespace AccuSync.WPF.Views.SitesFacilities
{
    /// <summary>
    /// Immutable snapshot of a facility's editable form state, used for undo/revert.
    /// </summary>
    /// <param name="Name">The facility name.</param>
    /// <param name="Description">The facility description.</param>
    /// <param name="Code">The facility code.</param>
    /// <param name="SiteIndex">The selected index in the site combo box.</param>
    /// <param name="LocationTypeIndex">The selected index in the location type combo box.</param>
    public record FacilitySnapshot(
        string Name, string Description, string Code,
        int SiteIndex, int LocationTypeIndex);

    /// <summary>
    /// Facilities screen content: facility list/detail form with undo/revert support.
    /// </summary>
    public partial class FacilitiesContentView : UserControl
    {
        private ObservableCollection<FacilityEntry> _facilities;
        private bool _isInitialized;
        private bool _isSuppressingUndo;
        private bool _isLoadingFacility;

        private FacilitySnapshot _savedState;
        private Stack<FacilitySnapshot> _undoStack = new();

        /// <summary>
        /// Initializes the control and populates the facility list once loaded.
        /// </summary>
        public FacilitiesContentView()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                _isInitialized = true;
                InitializeFacilities();
            };
        }

        // Initialization

        private void InitializeFacilities()
        {
            _facilities = new ObservableCollection<FacilityEntry>();
            FacilitiesListView.ItemsSource = _facilities;
            UpdateEmptyListState();
        }

        private void UpdateEmptyListState()
        {
            EmptyListPanel.Visibility = _facilities.Count == 0
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        // Site Dropdown Population

        /// <summary>
        /// Called by SitesContentView when entering facilities mode to sync available sites.
        /// </summary>
        public void UpdateSiteList(IEnumerable<string> siteNames)
        {
            var currentSelection = (SiteCombo.SelectedItem as ComboBoxItem)?.Content?.ToString();

            _isSuppressingUndo = true;
            SiteCombo.Items.Clear();
            foreach (var name in siteNames)
            {
                SiteCombo.Items.Add(new ComboBoxItem { Content = name });
            }

            if (!string.IsNullOrEmpty(currentSelection))
            {
                for (int i = 0; i < SiteCombo.Items.Count; i++)
                {
                    if ((SiteCombo.Items[i] as ComboBoxItem)?.Content?.ToString() == currentSelection)
                    {
                        SiteCombo.SelectedIndex = i;
                        break;
                    }
                }
            }
            _isSuppressingUndo = false;
        }

        // List Selection

        private void FacilitiesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var facility = FacilitiesListView.SelectedItem as FacilityEntry;
            if (facility == null)
            {
                DetailFormPanel.Visibility = Visibility.Collapsed;
                EmptyDetailPanel.Visibility = Visibility.Visible;
                DetailHeaderText.Text = Strings.FacilitiesContentView_FacilityDetailsHeader;
                return;
            }

            EmptyDetailPanel.Visibility = Visibility.Collapsed;
            DetailFormPanel.Visibility = Visibility.Visible;
            LoadFacilityIntoForm(facility);
        }

        private void LoadFacilityIntoForm(FacilityEntry facility)
        {
            _isLoadingFacility = true;

            DetailHeaderText.Text = facility.Name.ToUpperInvariant();
            NameBox.Text = facility.Name;
            CodeBox.Text = facility.Code;
            DescriptionBox.Text = facility.Description;

            SiteCombo.SelectedIndex = -1;
            for (int i = 0; i < SiteCombo.Items.Count; i++)
            {
                if ((SiteCombo.Items[i] as ComboBoxItem)?.Content?.ToString() == facility.Site)
                { SiteCombo.SelectedIndex = i; break; }
            }

            LocationTypeCombo.SelectedIndex = -1;
            for (int i = 0; i < LocationTypeCombo.Items.Count; i++)
            {
                if ((LocationTypeCombo.Items[i] as ComboBoxItem)?.Content?.ToString() == facility.LocationType)
                { LocationTypeCombo.SelectedIndex = i; break; }
            }

            _savedState = CaptureSnapshot();
            _undoStack.Clear();

            _isLoadingFacility = false;
        }

        // Snapshot and Undo

        private FacilitySnapshot CaptureSnapshot()
        {
            return new FacilitySnapshot(
                NameBox?.Text ?? "",
                DescriptionBox?.Text ?? "",
                CodeBox?.Text ?? "",
                SiteCombo?.SelectedIndex ?? -1,
                LocationTypeCombo?.SelectedIndex ?? -1);
        }

        private void ApplySnapshot(FacilitySnapshot s)
        {
            _isSuppressingUndo = true;
            _isLoadingFacility = true;

            NameBox.Text = s.Name;
            CodeBox.Text = s.Code;
            DescriptionBox.Text = s.Description;
            SiteCombo.SelectedIndex = s.SiteIndex;
            LocationTypeCombo.SelectedIndex = s.LocationTypeIndex;

            _isLoadingFacility = false;
            _isSuppressingUndo = false;
        }

        private void PushUndo()
        {
            if (!_isInitialized || _isSuppressingUndo || _isLoadingFacility) return;
            _undoStack.Push(CaptureSnapshot());
        }

        // Public Handlers (called by toolbar)

        // NOTE: This is the only config view that validates required fields in HandleSave.
        // Consider applying the same pattern to ABR, DPOAE, Comments, Devices, and Export views.
        // TODO: Still no actual persistence to database.
        /// <summary>
        /// Validates the selected facility's required fields and saves the current state
        /// as the new baseline.
        /// </summary>
        public void HandleSave()
        {
            var facility = FacilitiesListView.SelectedItem as FacilityEntry;
            if (facility == null)
            {
                AppDialog.Show(Strings.FacilitiesContentView_NoFacilitySelected, Strings.FacilitiesContentView_SaveCaption,
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                AppDialog.Show(Strings.FacilitiesContentView_NameRequired, Strings.FacilitiesContentView_ValidationCaption,
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                NameBox.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(CodeBox.Text))
            {
                AppDialog.Show(Strings.FacilitiesContentView_CodeRequired, Strings.FacilitiesContentView_ValidationCaption,
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                CodeBox.Focus();
                return;
            }
            if (SiteCombo.SelectedIndex < 0)
            {
                AppDialog.Show(Strings.FacilitiesContentView_SiteRequired, Strings.FacilitiesContentView_ValidationCaption,
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            facility.Name = NameBox.Text;
            facility.Code = CodeBox.Text;
            facility.Description = DescriptionBox.Text;
            facility.Site = (SiteCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
            facility.LocationType = (LocationTypeCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";

            DetailHeaderText.Text = facility.Name.ToUpperInvariant();

            _savedState = CaptureSnapshot();
            _undoStack.Clear();

            AppDialog.Show(Strings.FacilitiesContentView_FacilitySaved, Strings.FacilitiesContentView_SaveCaption,
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

        // Add and Delete

        /// <summary>
        /// Adds a new facility with default values and selects it.
        /// </summary>
        public void HandleAdd()
        {
            var newFacility = new FacilityEntry
            {
                Name = "New Facility",
                Description = "",
                Code = "",
                Site = "",
                LocationType = ""
            };
            _facilities.Add(newFacility);
            FacilitiesListView.SelectedItem = newFacility;
            UpdateEmptyListState();

            NameBox.Focus();
            NameBox.SelectAll();
        }

        /// <summary>
        /// Deletes the selected facility after user confirmation.
        /// </summary>
        public void HandleDelete()
        {
            var facility = FacilitiesListView.SelectedItem as FacilityEntry;
            if (facility == null)
            {
                AppDialog.Show(Strings.FacilitiesContentView_NoFacilitySelected, Strings.FacilitiesContentView_DeleteCaption,
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = AppDialog.Show(
                string.Format(Strings.FacilitiesContentView_DeleteConfirm, facility.Name),
                Strings.FacilitiesContentView_DeleteFacilityCaption, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                int idx = _facilities.IndexOf(facility);
                _facilities.Remove(facility);

                if (_facilities.Count > 0)
                    FacilitiesListView.SelectedIndex = Math.Min(idx, _facilities.Count - 1);

                UpdateEmptyListState();
            }
        }

        // Field Change Handlers

        private void Field_Changed(object sender, EventArgs e)
        {
            PushUndo();
        }

        private void Field_Changed(object sender, TextChangedEventArgs e)
        {
            PushUndo();
        }

        private void Field_Changed(object sender, SelectionChangedEventArgs e)
        {
            PushUndo();
        }
    }
}