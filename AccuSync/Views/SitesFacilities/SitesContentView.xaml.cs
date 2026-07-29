// --------------------------------------------------------------------------------
// <copyright file="SitesContentView.xaml.cs" company="Natus Sensory">
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
using AccuSync.Models;
using AccuSync.Resources;
using AccuSync.Controls;

namespace AccuSync.Views.SitesFacilities
{
    public record SiteSnapshot(string Name, string Description, string Code);

    public partial class SitesContentView : UserControl
    {
        private ObservableCollection<SiteEntry> _sites;
        private bool _isInitialized;
        private bool _isSuppressingUndo;
        private bool _isLoadingSite;

        private SiteSnapshot _savedState;
        private Stack<SiteSnapshot> _undoStack = new();

        public FacilitiesContentView FacilitiesView => FacilitiesContent;
        public LocationsContentView LocationsView => LocationsContent;

        public SitesContentView()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                _isInitialized = true;
                InitializeSites();
            };
        }

        // Initialization

        // TODO: Replace with real data from DatabaseService
        private void InitializeSites()
        {
            _sites = new ObservableCollection<SiteEntry>();
            SitesListView.ItemsSource = _sites;
            UpdateEmptyListState();
        }

        private void UpdateEmptyListState()
        {
            EmptyListPanel.Visibility = _sites.Count == 0
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        // Card Selection

        private void SitesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var site = SitesListView.SelectedItem as SiteEntry;
            if (site == null)
            {
                DetailFormPanel.Visibility = Visibility.Collapsed;
                EmptyDetailPanel.Visibility = Visibility.Visible;
                DetailHeaderText.Text = Strings.SitesContentView_SiteDetailsHeader;
                return;
            }

            EmptyDetailPanel.Visibility = Visibility.Collapsed;
            DetailFormPanel.Visibility = Visibility.Visible;
            LoadSiteIntoForm(site);
        }

        private void LoadSiteIntoForm(SiteEntry site)
        {
            _isLoadingSite = true;

            DetailHeaderText.Text = site.Name.ToUpperInvariant();
            NameBox.Text = site.Name;
            CodeBox.Text = site.Code;
            DescriptionBox.Text = site.Description;

            _savedState = CaptureSnapshot();
            _undoStack.Clear();

            _isLoadingSite = false;
        }

        // Snapshot Helpers

        private SiteSnapshot CaptureSnapshot()
        {
            return new SiteSnapshot(
                NameBox?.Text ?? "",
                DescriptionBox?.Text ?? "",
                CodeBox?.Text ?? "");
        }

        private void ApplySnapshot(SiteSnapshot s)
        {
            _isSuppressingUndo = true;
            _isLoadingSite = true;

            NameBox.Text = s.Name;
            CodeBox.Text = s.Code;
            DescriptionBox.Text = s.Description;

            _isLoadingSite = false;
            _isSuppressingUndo = false;
        }

        private void PushUndo()
        {
            if (!_isInitialized || _isSuppressingUndo || _isLoadingSite) return;
            _undoStack.Push(CaptureSnapshot());
        }

        // Save / Revert / Undo — called from toolbar

        // NOTE: HandleSave updates the in-memory model but doesn't persist to database
        public void HandleSave()
        {
            var site = SitesListView.SelectedItem as SiteEntry;
            if (site == null)
            {
                AppDialog.Show(Strings.SitesContentView_NoSiteSelected, Strings.SitesContentView_SaveCaption,
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                AppDialog.Show(Strings.SitesContentView_NameRequired, Strings.SitesContentView_ValidationCaption,
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                NameBox.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(CodeBox.Text))
            {
                AppDialog.Show(Strings.SitesContentView_CodeRequired, Strings.SitesContentView_ValidationCaption,
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                CodeBox.Focus();
                return;
            }

            site.Name = NameBox.Text;
            site.Code = CodeBox.Text;
            site.Description = DescriptionBox.Text;

            DetailHeaderText.Text = site.Name.ToUpperInvariant();

            _savedState = CaptureSnapshot();
            _undoStack.Clear();

            AppDialog.Show(Strings.SitesContentView_SiteSaved, Strings.SitesContentView_SaveCaption,
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

        // Add / Delete — called from toolbar

        public void HandleAdd()
        {
            var newSite = new SiteEntry
            {
                Name = "New Site",
                Description = "",
                Code = ""
            };
            _sites.Add(newSite);
            SitesListView.SelectedItem = newSite;
            UpdateEmptyListState();

            NameBox.Focus();
            NameBox.SelectAll();
        }

        public void HandleDelete()
        {
            var site = SitesListView.SelectedItem as SiteEntry;
            if (site == null)
            {
                AppDialog.Show(Strings.SitesContentView_NoSiteSelected, Strings.SitesContentView_DeleteCaption,
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = AppDialog.Show(
                string.Format(Strings.SitesContentView_DeleteConfirm, site.Name),
                Strings.SitesContentView_DeleteSiteCaption, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                int idx = _sites.IndexOf(site);
                _sites.Remove(site);

                if (_sites.Count > 0)
                    SitesListView.SelectedIndex = Math.Min(idx, _sites.Count - 1);

                UpdateEmptyListState();
            }
        }

        // Facilities Takeover

        public void ShowFacilities()
        {
            // Sync current site names into the facilities site dropdown
            FacilitiesContent.UpdateSiteList(
                _sites.Select(s => s.Name));

            NormalSitesGrid.Visibility = Visibility.Collapsed;
            FacilitiesContent.Visibility = Visibility.Visible;
        }

        public void HideFacilities()
        {
            FacilitiesContent.Visibility = Visibility.Collapsed;
            NormalSitesGrid.Visibility = Visibility.Visible;
        }

        // Locations Takeover

        public void ShowLocations()
        {
            NormalSitesGrid.Visibility = Visibility.Collapsed;
            LocationsContent.Visibility = Visibility.Visible;
        }

        public void HideLocations()
        {
            LocationsContent.Visibility = Visibility.Collapsed;
            NormalSitesGrid.Visibility = Visibility.Visible;
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