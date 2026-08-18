// --------------------------------------------------------------------------------
// <copyright file="ImportReview.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Application.Models;
using AccuSync.WPF.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AccuSync.WPF.Controls;

namespace AccuSync.WPF.Views.PatientsTests
{
    /// <summary>
    /// Multi-step import workflow: previews parsed patients (Step 2), lets the
    /// user resolve duplicates and select records, then shows import results (Step 3).
    /// </summary>
    public partial class ImportReview : UserControl
    {
        // State
        private List<ImportRecord> _records = new();
        private ImportRecord _activeRecord;
        private int _currentStep = 2;
        private bool _isUpdatingSelection;
        private bool _isUpdatingDupAction;

        // Events for Parent Communication

        /// <summary>Raised when the user clicks Back to return to file selection.</summary>
        public event EventHandler BackToFileSelect;

        /// <summary>Raised when the user cancels the import.</summary>
        public event EventHandler ImportCancelled;

        /// <summary>Raised when the user finishes viewing the import results.</summary>
        public event EventHandler ImportDone;

        // Subscribed by PatientsView; will be raised once the import-preview
        // "view patient" action is implemented. Suppress "never used" until then.
#pragma warning disable CS0067
        /// <summary>Raised when the user requests to view a patient from the import results.</summary>
        public event EventHandler<PatientData> ViewPatientRequested;
#pragma warning restore CS0067

        /// <summary>
        /// Delegate for performing the actual import. Receives selected patients
        /// with their chosen actions, returns categorized results.
        /// When null, a mock import is used.
        /// </summary>
        public Func<List<ImportRequest>, List<ImportResultItem>> ImportFunction { get; set; }

        /// <summary>
        /// Initializes the control and shows Step 2 (preview) by default.
        /// </summary>
        public ImportReview()
        {
            InitializeComponent();
            ShowStep(2);
        }

        // Public API

        /// <summary>
        /// Loads parsed patients into the preview table (Step 2).
        /// </summary>
        public void LoadPreviewData(List<ImportPatientData> importData)
        {
            // Unsubscribe from old records to avoid leaked handlers
            foreach (var r in _records)
                r.PropertyChanged -= OnRecordPropertyChanged;

            _records = importData.Select(d => new ImportRecord
            {
                Name = FormatName(d.Patient),
                DateOfBirth = d.Patient.DateOfBirth ?? "",
                Gender = d.Patient.Gender ?? "",
                PatientId = d.Patient.PatientId ?? "",
                Status = d.Status,
                Patient = d.Patient,
                Tests = d.Tests ?? new(),
                DupInfo = d.DupInfo,
                IsSelected = true,
                // Only read for duplicates (see PerformImport); AddTests is the default choice.
                DupAction = ImportAction.AddTests
            }).ToList();

            foreach (var r in _records)
                r.PropertyChanged += OnRecordPropertyChanged;

            PatientListControl.ItemsSource = _records;
            HideDetailPanel();
            UpdateSelectionState();
            ShowStep(2);
        }

        /// <summary>
        /// Backward-compatible overload — wraps plain PatientData list
        /// as new patients with no test detail.
        /// </summary>
        public void LoadPreviewData(List<PatientData> patients)
        {
            var importData = patients.Select(p => new ImportPatientData
            {
                Patient = p,
                Status = ImportStatus.New,
                Tests = new(),
                DupInfo = null
            }).ToList();

            LoadPreviewData(importData);
        }

        /// <summary>
        /// Jumps directly to Step 3 (results) with pre-built result items.
        /// Used by facesheet import flow.
        /// </summary>
        public void ShowResultsDirectly(List<ImportResultItem> results)
        {
            _records = results.Select(r => new ImportRecord
            {
                Name = r.Name,
                PatientId = r.PatientId,
                Patient = r.Patient,
                Status = ImportStatus.New,
                IsSelected = true
            }).ToList();

            PopulateResults(results, new List<ImportResultItem>());
            ShowStep(3);
        }

        // Step Navigation

        private void ShowStep(int step)
        {
            _currentStep = step;

            Step2Panel.Visibility = step == 2 ? Visibility.Visible : Visibility.Collapsed;
            Step3Panel.Visibility = step == 3 ? Visibility.Visible : Visibility.Collapsed;
            Step2Footer.Visibility = step == 2 ? Visibility.Visible : Visibility.Collapsed;
            Step3Footer.Visibility = step == 3 ? Visibility.Visible : Visibility.Collapsed;

            if (step == 2)
                UpdateSelectionState();
        }

        // Selection Management

        private void OnRecordPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ImportRecord.IsSelected) && !_isUpdatingSelection)
                UpdateSelectionState();
        }

        private void SelectAllCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingSelection) return;

            _isUpdatingSelection = true;
            bool check = SelectAllCheckBox.IsChecked == true;

            foreach (var r in _records)
                r.IsSelected = check;

            _isUpdatingSelection = false;
            UpdateSelectionState();
        }

        private void SelectAllLabel_Click(object sender, MouseButtonEventArgs e)
        {
            SelectAllCheckBox.IsChecked = SelectAllCheckBox.IsChecked != true;
        }

        private void UpdateSelectionState()
        {
            int selected = _records.Count(r => r.IsSelected);
            int total = _records.Count;

            SelectionCountText.Text = string.Format(Strings.ImportReview_SelectionCount, selected, total);
            ImportButton.Content = selected == 1
                ? Strings.ImportReview_ImportOneRecord
                : string.Format(Strings.ImportReview_ImportNRecords, selected);

            _isUpdatingSelection = true;
            SelectAllCheckBox.IsChecked = selected == total ? true : selected == 0 ? false : null;
            _isUpdatingSelection = false;
        }

        // Detail Panel

        private void PatientRow_Click(object sender, MouseButtonEventArgs e)
        {
            if (IsCheckBoxClick(e.OriginalSource as DependencyObject))
                return;

            if (sender is FrameworkElement fe && fe.DataContext is ImportRecord record)
            {
                if (_activeRecord == record)
                    HideDetailPanel();
                else
                    ShowDetailPanel(record);
            }
        }

        private void ShowDetailPanel(ImportRecord record)
        {
            if (_activeRecord != null)
                _activeRecord.IsActive = false;

            _activeRecord = record;
            _activeRecord.IsActive = true;

            DetailPanelColumn.Width = new GridLength(400);
            DetailTestCountRun.Text = record.TestCount.ToString();
            TestItemsList.ItemsSource = record.Tests;

            bool hasNewTests = record.Tests.Any(t => !t.IsExisting);
            NewTestLegend.Visibility = hasNewTests ? Visibility.Visible : Visibility.Collapsed;

            if (record.IsDuplicate && record.DupInfo != null)
            {
                DupWarningPanel.Visibility = Visibility.Visible;

                int newTests = record.DupInfo.NewTests;
                DupAddTestsSubText.Text = newTests > 0
                    ? $"{newTests} new test{(newTests > 1 ? "s" : "")} found"
                    : Strings.ImportReview_NoNewTestsDetected;

                // Guard prevents radio Checked events from writing back while we restore state
                _isUpdatingDupAction = true;
                DupActionReplace.IsChecked = record.DupAction == ImportAction.Replace;
                DupActionCreate.IsChecked = record.DupAction == ImportAction.Create;
                DupActionAddTests.IsChecked = record.DupAction == ImportAction.AddTests;
                DupActionSkip.IsChecked = record.DupAction == ImportAction.Skip;
                _isUpdatingDupAction = false;
            }
            else
            {
                DupWarningPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void HideDetailPanel()
        {
            if (_activeRecord != null)
            {
                _activeRecord.IsActive = false;
                _activeRecord = null;
            }

            DetailPanelColumn.Width = new GridLength(0);
            TestItemsList.ItemsSource = null;
        }

        private void CloseDetailPanel_Click(object sender, RoutedEventArgs e)
        {
            HideDetailPanel();
        }

        // Duplicate Action Radio Buttons

        private void DupAction_Changed(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingDupAction || _activeRecord == null || !_activeRecord.IsDuplicate)
                return;

            if (DupActionReplace.IsChecked == true)
                _activeRecord.DupAction = ImportAction.Replace;
            else if (DupActionCreate.IsChecked == true)
                _activeRecord.DupAction = ImportAction.Create;
            else if (DupActionAddTests.IsChecked == true)
                _activeRecord.DupAction = ImportAction.AddTests;
            else if (DupActionSkip.IsChecked == true)
                _activeRecord.DupAction = ImportAction.Skip;
        }

        // Import Execution

        private void PerformImport()
        {
            var requests = _records
                .Where(r => r.IsSelected)
                .Select(r => new ImportRequest
                {
                    Patient = r.Patient,
                    Action = r.IsDuplicate ? r.DupAction : ImportAction.Import
                })
                .ToList();

            // Build skipped list: unchecked patients + duplicates set to Skip
            var skippedRecords = _records
                .Where(r => !r.IsSelected || (r.IsDuplicate && r.DupAction == ImportAction.Skip))
                .ToList();

            // Remove Skip entries so they don't reach ImportFunction
            requests.RemoveAll(r => r.Action == ImportAction.Skip);

            List<ImportResultItem> results;

            if (ImportFunction != null)
            {
                results = ImportFunction(requests);
            }
            else
            {
                results = requests.Select(req =>
                {
                    string badge = req.Action switch
                    {
                        ImportAction.Replace => "REPLACED",
                        ImportAction.Create => "NEW RECORD",
                        ImportAction.AddTests => "TESTS ADDED",
                        _ => null
                    };

                    return new ImportResultItem
                    {
                        Name = FormatName(req.Patient),
                        PatientId = req.Patient.PatientId ?? "",
                        Detail = "",
                        Badge = badge,
                        Patient = req.Patient
                    };
                }).ToList();
            }

            var skippedItems = skippedRecords.Select(r => new ImportResultItem
            {
                Name = r.Name,
                PatientId = r.PatientId,
                Detail = !r.IsSelected ? "Deselected by user" : "Skipped by user",
                Patient = r.Patient
            }).ToList();

            PopulateResults(results, skippedItems);
            ShowStep(3);
        }

        // Results Population

        private void PopulateResults(List<ImportResultItem> importResults, List<ImportResultItem> skippedItems)
        {
            var imported = importResults
                .Where(r => string.IsNullOrEmpty(r.Detail) || !r.Detail.StartsWith("Error:"))
                .ToList();

            var errors = importResults
                .Where(r => r.Detail != null && r.Detail.StartsWith("Error:"))
                .ToList();

            foreach (var err in errors)
                err.Detail = err.Detail.Substring("Error:".Length).Trim();

            int totalRecords = _records.Count;
            int importedCount = imported.Count;

            ResultSummaryText.Text = string.Format(Strings.ImportReview_ResultSummary, importedCount, totalRecords);

            ImportedSection.Visibility = imported.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            if (imported.Count > 0)
            {
                ImportedCountText.Text = imported.Count.ToString();
                ImportedList.ItemsSource = imported;
            }

            ErrorsSection.Visibility = errors.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            if (errors.Count > 0)
            {
                ErrorsCountText.Text = errors.Count.ToString();
                ErrorsList.ItemsSource = errors;
            }

            SkippedSection.Visibility = skippedItems.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            if (skippedItems.Count > 0)
            {
                SkippedCountText.Text = skippedItems.Count.ToString();
                SkippedList.ItemsSource = skippedItems;
            }
        }

        // Button Handlers

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            HideDetailPanel();
            BackToFileSelect?.Invoke(this, EventArgs.Empty);
        }

        private void CancelImport_Click(object sender, RoutedEventArgs e)
        {
            HideDetailPanel();
            ImportCancelled?.Invoke(this, EventArgs.Empty);
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            int selected = _records.Count(r => r.IsSelected);
            if (selected == 0)
            {
                AppDialog.Show(Strings.ImportReview_SelectAtLeastOnePatient,
                    Strings.ImportReview_NoPatientsSelectedTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            HideDetailPanel();
            PerformImport();
        }

        private void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            ImportDone?.Invoke(this, EventArgs.Empty);
        }

        // Utility

        private static string FormatName(PatientData p)
        {
            var last = p.LastName?.Trim() ?? "";
            var first = p.FirstName?.Trim() ?? "";
            if (!string.IsNullOrEmpty(last) && !string.IsNullOrEmpty(first))
                return $"{last}, {first}";
            return last + first;
        }

        /// <summary>
        /// Walks up the visual tree to check whether the click originated
        /// from a CheckBox, so row-click doesn't toggle the detail panel.
        /// </summary>
        private static bool IsCheckBoxClick(DependencyObject source)
        {
            while (source != null)
            {
                if (source is CheckBox) return true;
                source = VisualTreeHelper.GetParent(source);
            }
            return false;
        }
    }
}