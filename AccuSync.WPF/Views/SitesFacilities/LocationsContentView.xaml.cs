// --------------------------------------------------------------------------------
// <copyright file="LocationsContentView.xaml.cs" company="Natus Sensory">
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
    public record LocationSnapshot(string Name, string Description, string Code);

    public partial class LocationsContentView : UserControl
    {
        private ObservableCollection<LocationEntry> _locations;
        private bool _isInitialized;
        private bool _isSuppressingUndo;
        private bool _isLoadingLocation;

        private LocationSnapshot _savedState;
        private Stack<LocationSnapshot> _undoStack = new();

        public LocationsContentView()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                _isInitialized = true;
                InitializeLocations();
            };
        }

        // Initialization

        private void InitializeLocations()
        {
            _locations = new ObservableCollection<LocationEntry>
            {
                new LocationEntry { Name = "Inpatient", Description = "", Code = "IN" },
                new LocationEntry { Name = "Outpatient", Description = "", Code = "OUT" },
                new LocationEntry { Name = "Home Visit", Description = "", Code = "HOME" }
            };

            LocationsListView.ItemsSource = _locations;
            UpdateEmptyListState();
        }

        private void UpdateEmptyListState()
        {
            EmptyListPanel.Visibility = _locations.Count == 0
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        // List Selection

        private void LocationsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var location = LocationsListView.SelectedItem as LocationEntry;
            if (location == null)
            {
                DetailFormPanel.Visibility = Visibility.Collapsed;
                EmptyDetailPanel.Visibility = Visibility.Visible;
                DetailHeaderText.Text = Strings.LocationsContentView_LocationDetailsHeader;
                return;
            }

            EmptyDetailPanel.Visibility = Visibility.Collapsed;
            DetailFormPanel.Visibility = Visibility.Visible;
            LoadLocationIntoForm(location);
        }

        private void LoadLocationIntoForm(LocationEntry location)
        {
            _isLoadingLocation = true;

            DetailHeaderText.Text = location.Name.ToUpperInvariant();
            NameBox.Text = location.Name;
            CodeBox.Text = location.Code;
            DescriptionBox.Text = location.Description;

            _savedState = CaptureSnapshot();
            _undoStack.Clear();

            _isLoadingLocation = false;
        }

        // Snapshot and Undo

        private LocationSnapshot CaptureSnapshot()
        {
            return new LocationSnapshot(
                NameBox?.Text ?? "",
                DescriptionBox?.Text ?? "",
                CodeBox?.Text ?? "");
        }

        private void ApplySnapshot(LocationSnapshot s)
        {
            _isSuppressingUndo = true;
            _isLoadingLocation = true;

            NameBox.Text = s.Name;
            CodeBox.Text = s.Code;
            DescriptionBox.Text = s.Description;

            _isLoadingLocation = false;
            _isSuppressingUndo = false;
        }

        private void PushUndo()
        {
            if (!_isInitialized || _isSuppressingUndo || _isLoadingLocation) return;
            _undoStack.Push(CaptureSnapshot());
        }

        // Public Handlers (called by toolbar)

        // TODO: HandleSave validates fields (good!) but still has no actual persistence.
        public void HandleSave()
        {
            var location = LocationsListView.SelectedItem as LocationEntry;
            if (location == null)
            {
                AppDialog.Show(Strings.LocationsContentView_NoLocationSelected, Strings.LocationsContentView_SaveCaption,
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                AppDialog.Show(Strings.LocationsContentView_NameRequired, Strings.LocationsContentView_ValidationCaption,
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                NameBox.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(CodeBox.Text))
            {
                AppDialog.Show(Strings.LocationsContentView_CodeRequired, Strings.LocationsContentView_ValidationCaption,
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                CodeBox.Focus();
                return;
            }

            location.Name = NameBox.Text;
            location.Code = CodeBox.Text;
            location.Description = DescriptionBox.Text;

            DetailHeaderText.Text = location.Name.ToUpperInvariant();

            _savedState = CaptureSnapshot();
            _undoStack.Clear();

            AppDialog.Show(Strings.LocationsContentView_LocationSaved, Strings.LocationsContentView_SaveCaption,
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void HandleRevert()
        {
            if (_savedState == null) return;
            PushUndo();
            ApplySnapshot(_savedState);
        }

        public void HandleUndo()
        {
            if (_undoStack.Count == 0) return;
            var prev = _undoStack.Pop();
            ApplySnapshot(prev);
        }

        // Add and Delete

        public void HandleAdd()
        {
            var newLocation = new LocationEntry
            {
                Name = "New Location",
                Description = "",
                Code = ""
            };
            _locations.Add(newLocation);
            LocationsListView.SelectedItem = newLocation;
            UpdateEmptyListState();

            NameBox.Focus();
            NameBox.SelectAll();
        }

        public void HandleDelete()
        {
            var location = LocationsListView.SelectedItem as LocationEntry;
            if (location == null)
            {
                AppDialog.Show(Strings.LocationsContentView_NoLocationSelected, Strings.LocationsContentView_DeleteCaption,
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = AppDialog.Show(
                string.Format(Strings.LocationsContentView_DeleteConfirm, location.Name),
                Strings.LocationsContentView_DeleteLocationCaption, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                int idx = _locations.IndexOf(location);
                _locations.Remove(location);

                if (_locations.Count > 0)
                    LocationsListView.SelectedIndex = Math.Min(idx, _locations.Count - 1);

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
    }
}