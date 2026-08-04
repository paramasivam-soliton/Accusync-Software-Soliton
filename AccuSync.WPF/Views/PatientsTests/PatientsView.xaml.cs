// --------------------------------------------------------------------------------
// <copyright file="PatientsView.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Presentation.ViewModels;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AccuSync.WPF.Controls;
using AccuSync.WPF.Views.PatientsTests.Dialogs;
using AccuSync.Application.Models;
using AccuSync.Core.Entities;
using AccuSync.Application.Services;
using AccuSync.WPF.Resources;

namespace AccuSync.WPF.Views.PatientsTests
{
    public partial class PatientsView : UserControl, INotifyPropertyChanged
    {
        private ObservableCollection<Patient> _allPatients;
        private ObservableCollection<Patient> _filteredPatients;
        private ObservableCollection<Patient> _pagedPatients;
        private HashSet<string> _selectedFilters = new HashSet<string>();
        private readonly ImportService _importService = new();
        private string _importFormat;   // track format for Back navigation
        private string _importFilePath; // track file path for Back navigation
        private bool _isCompactView = false;
        private const string CompactIconData = "M2,3 L16,3 L16,4.8 L2,4.8 Z M2,7 L16,7 L16,8.8 L2,8.8 Z M2,11 L16,11 L16,12.8 L2,12.8 Z M2,15 L16,15 L16,16.8 L2,16.8 Z";
        private const string DetailedIconData = "M2,2 L16,2 L16,5.5 L2,5.5 Z M2,7.5 L16,7.5 L16,11 L2,11 Z M2,13 L16,13 L16,16.5 L2,16.5 Z";

        private int _currentPage = 1;
        private int _itemsPerPage = 10;
        private int _totalPages = 1;

        private string _currentSortField = "LastName";
        private bool _sortAscending = true;

        private bool _isSelectionModeEnabled = false;
        private Visibility _checkboxVisibility = Visibility.Collapsed;
        private bool _isUpdatingSelectAll = false;

        public Visibility CheckboxVisibility
        {
            get => _checkboxVisibility;
            set
            {
                _checkboxVisibility = value;
                OnPropertyChanged(nameof(CheckboxVisibility));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public PatientsView()
        {
            InitializeComponent();
            DataContext = this;
            LoadDummyPatients();

            FilterPopup.Visibility = Visibility.Collapsed;
            PatientsListView.SelectionChanged += PatientsListView_SelectionChanged;

            // Manual placeholder pattern — see cross-cutting issue on watermark adorner
            SearchBox.TextChanged += SearchBox_TextChanged;
            SearchBox.GotFocus += (s, e) => {
                if (SearchBox.Text == Strings.PatientsView_SearchPlaceholder)
                    SearchBox.Text = "";
            };
            SearchBox.LostFocus += (s, e) => {
                if (string.IsNullOrWhiteSpace(SearchBox.Text))
                {
                    SearchBox.Text = Strings.PatientsView_SearchPlaceholder;
                    ClearSearchButton.Visibility = Visibility.Collapsed;
                }
            };
        }

        /// <summary>
        /// Called by AdminDashboardWindow when toolbar "Add" is clicked.
        /// Deselects any active patient and puts the info panel into Add mode.
        /// </summary>
        public void StartAddPatient()
        {
            PatientsListView.SelectedItem = null;
            PatientInfoPanel.NewPatient();
        }

        // TODO: Replace with real data from DatabaseService
        private void LoadDummyPatients()
        {
            _allPatients = new ObservableCollection<Patient>
            {
                new Patient {
                    FirstName = "John", LastName = "Smith", BirthDate = new DateTime(2026, 1, 10),
                    PatientId = "1234567890", Gender = "Male", LeftEarResult = "Pass", RightEarResult = "Pass",
                    DateOfScreen = new DateTime(2026, 1, 12), HospitalId = "HOSP-001",
                    BirthLocation = "Memorial Hospital", GestationalAge = "40 weeks"
                },
                new Patient { FirstName = "Sarah", LastName = "Johnson", BirthDate = new DateTime(2026, 1, 15),
                    PatientId = "2345678901", Gender = "Female", LeftEarResult = "Pass", RightEarResult = "Refer",
                    DateOfScreen = new DateTime(2026, 1, 17) },
                new Patient { FirstName = "Michael", LastName = "Williams", BirthDate = new DateTime(2026, 2, 3),
                    PatientId = "3456789012", Gender = "Male", LeftEarResult = "Incomplete", RightEarResult = "Incomplete",
                    DateOfScreen = null },
                new Patient { FirstName = "Emily", LastName = "Brown", BirthDate = new DateTime(2026, 2, 8),
                    PatientId = "4567890123", Gender = "Female", LeftEarResult = "Pass", RightEarResult = "Pass",
                    DateOfScreen = new DateTime(2026, 2, 10) },
                new Patient { FirstName = "David", LastName = "Jones", BirthDate = new DateTime(2026, 2, 14),
                    PatientId = "5678901234", Gender = "Male", LeftEarResult = "Refer", RightEarResult = "Refer",
                    DateOfScreen = new DateTime(2026, 2, 16) },
                new Patient { FirstName = "Jessica", LastName = "Garcia", BirthDate = new DateTime(2026, 2, 20),
                    PatientId = "6789012345", Gender = "Female", LeftEarResult = "Pass", RightEarResult = "Pass",
                    DateOfScreen = new DateTime(2026, 2, 23) },
                new Patient { FirstName = "Daniel", LastName = "Miller", BirthDate = new DateTime(2026, 3, 1),
                    PatientId = "7890123456", Gender = "Male", LeftEarResult = "Refer", RightEarResult = "Pass",
                    DateOfScreen = new DateTime(2026, 3, 3) },
                new Patient { FirstName = "Ashley", LastName = "Davis", BirthDate = new DateTime(2026, 3, 5),
                    PatientId = "8901234567", Gender = "Female", LeftEarResult = "Pass", RightEarResult = "Refer",
                    DateOfScreen = new DateTime(2026, 3, 7) },
                new Patient { FirstName = "Matthew", LastName = "Rodriguez", BirthDate = new DateTime(2026, 3, 12),
                    PatientId = "9012345678", Gender = "Male", LeftEarResult = "Pass", RightEarResult = "Pass",
                    DateOfScreen = new DateTime(2026, 3, 14) },
                new Patient { FirstName = "Amanda", LastName = "Martinez", BirthDate = new DateTime(2026, 3, 18),
                    PatientId = "0123456789", Gender = "Female", LeftEarResult = "Incomplete", RightEarResult = "Pass",
                    DateOfScreen = null }
            };

            _filteredPatients = new ObservableCollection<Patient>(_allPatients);
            ApplyCurrentSort();
            UpdatePagination();
            AddDummyTestData();
        }

        private void AddDummyTestData()
        {
            var john = _allPatients.FirstOrDefault(p => p.PatientId == "1234567890");
            if (john != null)
            {
                john.Tests = new List<TestRecord>
                {
                    new TestRecord {
                        Id = "t1", TestType = "TEOAE", Ear = "Right Ear", TestResult = "Pass",
                        TestDate = new DateTime(2026, 1, 12, 9, 15, 0), DurationMs = 12000,
                        DeviceSerial = "3077519", DeviceName = "AccuScreen",
                        ProbeSerial = "1008821", ProbeType = "DP (Newborn)",
                        ProbeCalibrationDate = new DateTime(2025, 11, 3),
                        ProbeNextCalibrationDate = new DateTime(2026, 6, 16),
                        TestFacility = "Main Hospital", TestLocation = "Nursery", Examiner = "J. Smith"
                    },
                    new TestRecord {
                        Id = "t2", TestType = "TEOAE", Ear = "Left Ear", TestResult = "Pass",
                        TestDate = new DateTime(2026, 1, 12, 9, 15, 0), DurationMs = 8000,
                        DeviceSerial = "3077519", DeviceName = "AccuScreen",
                        ProbeSerial = "1008821", ProbeType = "DP (Newborn)",
                        ProbeCalibrationDate = new DateTime(2025, 11, 3),
                        ProbeNextCalibrationDate = new DateTime(2026, 6, 16),
                        TestFacility = "Main Hospital", TestLocation = "Nursery", Examiner = "J. Smith"
                    },
                    new TestRecord {
                        Id = "t3", TestType = "ABR", Ear = "Right Ear", TestResult = "Pass",
                        TestDate = new DateTime(2026, 1, 12, 14, 30, 0), DurationMs = 250000,
                        EegNoisePercent = 38, ImpedanceWhite = 0, ImpedanceRed = 0,
                        DeviceSerial = "3077519", DeviceName = "AccuScreen",
                        ProbeSerial = "37623", ProbeType = "Bast2",
                        ProbeCalibrationDate = new DateTime(2025, 8, 6),
                        ProbeNextCalibrationDate = new DateTime(2026, 8, 6),
                        TestFacility = "Main Hospital", TestLocation = "NICU", Examiner = "M. Lee"
                    },
                    new TestRecord {
                        Id = "t4", TestType = "ABR", Ear = "Left Ear", TestResult = "Refer",
                        TestDate = new DateTime(2026, 1, 12, 13, 16, 0), DurationMs = 268000,
                        EegNoisePercent = 38, ImpedanceWhite = 0, ImpedanceRed = 0,
                        DeviceSerial = "3077519", DeviceName = "AccuScreen",
                        ProbeSerial = "37623", ProbeType = "Bast2",
                        ProbeCalibrationDate = new DateTime(2025, 8, 6),
                        ProbeNextCalibrationDate = new DateTime(2026, 8, 6),
                        TestFacility = "Main Hospital", TestLocation = "NICU", Examiner = "M. Lee"
                    },
                    new TestRecord {
                        Id = "t5", TestType = "DPOAE", Ear = "Right Ear", TestResult = "Pass",
                        TestDate = new DateTime(2026, 1, 12, 12, 44, 0), DurationMs = 12000,
                        DeviceSerial = "3077519", DeviceName = "AccuScreen",
                        ProbeSerial = "1008821", ProbeType = "DP (Newborn)",
                        ProbeCalibrationDate = new DateTime(2025, 11, 3),
                        ProbeNextCalibrationDate = new DateTime(2026, 6, 16),
                        TestFacility = "Main Hospital", TestLocation = "Nursery", Examiner = "J. Smith"
                    },
                    new TestRecord {
                        Id = "t6", TestType = "DPOAE", Ear = "Left Ear", TestResult = "Pass",
                        TestDate = new DateTime(2026, 1, 12, 12, 49, 0), DurationMs = 35000,
                        DeviceSerial = "3077519", DeviceName = "AccuScreen",
                        ProbeSerial = "1008821", ProbeType = "DP (Newborn)",
                        ProbeCalibrationDate = new DateTime(2025, 11, 3),
                        ProbeNextCalibrationDate = new DateTime(2026, 6, 16),
                        TestFacility = "Main Hospital", TestLocation = "Nursery", Examiner = "J. Smith"
                    }
                };
            }

            var sarah = _allPatients.FirstOrDefault(p => p.PatientId == "2345678901");
            if (sarah != null)
            {
                sarah.Tests = new List<TestRecord>
                {
                    new TestRecord {
                        Id = "t7", TestType = "TEOAE", Ear = "Right Ear", TestResult = "Refer",
                        TestDate = new DateTime(2026, 1, 17, 10, 30, 0), DurationMs = 22000,
                        DeviceSerial = "3077519", DeviceName = "AccuScreen",
                        ProbeSerial = "1008821", ProbeType = "DP (Newborn)",
                        ProbeCalibrationDate = new DateTime(2025, 11, 3),
                        ProbeNextCalibrationDate = new DateTime(2026, 6, 16),
                        Examiner = "J. Smith"
                    },
                    new TestRecord {
                        Id = "t8", TestType = "TEOAE", Ear = "Left Ear", TestResult = "Pass",
                        TestDate = new DateTime(2026, 1, 17, 10, 33, 0), DurationMs = 57000,
                        DeviceSerial = "3077519", DeviceName = "AccuScreen",
                        ProbeSerial = "1008821", ProbeType = "DP (Newborn)",
                        ProbeCalibrationDate = new DateTime(2025, 11, 3),
                        ProbeNextCalibrationDate = new DateTime(2026, 6, 16),
                        Examiner = "J. Smith"
                    }
                };
            }

            var david = _allPatients.FirstOrDefault(p => p.PatientId == "5678901234");
            if (david != null)
            {
                david.Tests = new List<TestRecord>
                {
                    new TestRecord {
                        Id = "t9", TestType = "ABR", Ear = "Right Ear", TestResult = "Refer",
                        TestDate = new DateTime(2026, 2, 16, 12, 48, 0), DurationMs = 265000,
                        EegNoisePercent = 39, ImpedanceWhite = 0, ImpedanceRed = 0,
                        DeviceSerial = "3077519", DeviceName = "AccuScreen",
                        ProbeSerial = "37623", ProbeType = "Bast2",
                        ProbeCalibrationDate = new DateTime(2025, 8, 6),
                        ProbeNextCalibrationDate = new DateTime(2026, 8, 6),
                        Examiner = "M. Lee"
                    },
                    new TestRecord {
                        Id = "t10", TestType = "ABR", Ear = "Left Ear", TestResult = "Refer",
                        TestDate = new DateTime(2026, 2, 16, 12, 52, 0), DurationMs = 248000,
                        EegNoisePercent = 38, ImpedanceWhite = 0, ImpedanceRed = 0,
                        DeviceSerial = "3077519", DeviceName = "AccuScreen",
                        ProbeSerial = "37623", ProbeType = "Bast2",
                        ProbeCalibrationDate = new DateTime(2025, 8, 6),
                        ProbeNextCalibrationDate = new DateTime(2026, 8, 6),
                        Examiner = "M. Lee"
                    }
                };
            }
        }
        // Sorting

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SortComboBox.SelectedItem is ComboBoxItem item && _filteredPatients != null)
            {
                string selection = item.Content.ToString();

                if (selection.Contains("Last Name")) { _currentSortField = "LastName"; _sortAscending = true; }
                else if (selection.Contains("First Name")) { _currentSortField = "FirstName"; _sortAscending = true; }
                else if (selection.Contains("Date of Birth (Newest)")) { _currentSortField = "BirthDate"; _sortAscending = false; }
                else if (selection.Contains("Date of Birth (Oldest)")) { _currentSortField = "BirthDate"; _sortAscending = true; }
                else if (selection.Contains("Date of Screen")) { _currentSortField = "DateOfScreen"; _sortAscending = false; }
                else if (selection.Contains("Patient ID")) { _currentSortField = "PatientId"; _sortAscending = true; }

                ApplyCurrentSort();
                _currentPage = 1;
                UpdatePagination();
            }
        }

        private void ApplyCurrentSort()
        {
            if (_filteredPatients == null || _filteredPatients.Count == 0)
                return;

            var sorted = _currentSortField switch
            {
                "FirstName" => _sortAscending
                    ? _filteredPatients.OrderBy(p => p.FirstName)
                    : _filteredPatients.OrderByDescending(p => p.FirstName),
                "LastName" => _sortAscending
                    ? _filteredPatients.OrderBy(p => p.LastName)
                    : _filteredPatients.OrderByDescending(p => p.LastName),
                "BirthDate" => _sortAscending
                    ? _filteredPatients.OrderBy(p => p.BirthDate)
                    : _filteredPatients.OrderByDescending(p => p.BirthDate),
                "DateOfScreen" => _sortAscending
                    ? _filteredPatients.OrderBy(p => p.DateOfScreen ?? DateTime.MinValue)
                    : _filteredPatients.OrderByDescending(p => p.DateOfScreen ?? DateTime.MinValue),
                "PatientId" => _sortAscending
                    ? _filteredPatients.OrderBy(p => p.PatientId)
                    : _filteredPatients.OrderByDescending(p => p.PatientId),
                _ => _filteredPatients.OrderBy(p => p.LastName)
            };

            _filteredPatients = new ObservableCollection<Patient>(sorted);
        }

        // Pagination

        private void UpdatePagination()
        {
            if (_filteredPatients == null)
                return;

            int totalItems = _filteredPatients.Count;
            _totalPages = (int)Math.Ceiling((double)totalItems / _itemsPerPage);

            if (_totalPages == 0) _totalPages = 1;
            if (_currentPage > _totalPages) _currentPage = _totalPages;

            int skip = (_currentPage - 1) * _itemsPerPage;
            _pagedPatients = new ObservableCollection<Patient>(_filteredPatients.Skip(skip).Take(_itemsPerPage));

            PatientsListView.ItemsSource = _pagedPatients;
            UpdatePatientCount();
            UpdatePaginationUI();
        }

        private void UpdatePaginationUI()
        {
            if (_filteredPatients == null)
                return;

            int startItem = _filteredPatients.Count == 0 ? 0 : ((_currentPage - 1) * _itemsPerPage) + 1;
            int endItem = Math.Min(_currentPage * _itemsPerPage, _filteredPatients.Count);

            PaginationInfoText.Text = string.Format(Strings.PatientsView_PaginationInfoFormat, startItem, endItem, _filteredPatients.Count);

            PrevPageButton.IsEnabled = _currentPage > 1;
            NextPageButton.IsEnabled = _currentPage < _totalPages;

            PageNumbersPanel.Children.Clear();

            int maxPagesToShow = 5;
            int startPage = Math.Max(1, _currentPage - 2);
            int endPage = Math.Min(_totalPages, startPage + maxPagesToShow - 1);

            if (endPage - startPage < maxPagesToShow - 1)
                startPage = Math.Max(1, endPage - maxPagesToShow + 1);

            for (int i = startPage; i <= endPage; i++)
            {
                int pageNum = i;
                var pageBtn = new Button
                {
                    Content = pageNum.ToString(),
                    Padding = new Thickness(10, 4, 10, 4),
                    Margin = new Thickness(2),
                    FontSize = 12,
                    Cursor = Cursors.Hand,
                    Background = pageNum == _currentPage ? new SolidColorBrush(Color.FromRgb(0x3A, 0x3A, 0x3C)) : Brushes.White,
                    Foreground = pageNum == _currentPage ? Brushes.White : new SolidColorBrush(Color.FromRgb(51, 51, 51)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(221, 221, 221)),
                    BorderThickness = new Thickness(1)
                };

                pageBtn.Click += (s, e) => { _currentPage = pageNum; UpdatePagination(); };

                var style = new Style(typeof(Button));
                var template = new ControlTemplate(typeof(Button));
                var border = new FrameworkElementFactory(typeof(Border));
                border.SetBinding(Border.BackgroundProperty, new System.Windows.Data.Binding("Background") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
                border.SetBinding(Border.BorderBrushProperty, new System.Windows.Data.Binding("BorderBrush") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
                border.SetBinding(Border.BorderThicknessProperty, new System.Windows.Data.Binding("BorderThickness") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
                border.SetValue(Border.CornerRadiusProperty, new CornerRadius(4));
                border.SetBinding(Border.PaddingProperty, new System.Windows.Data.Binding("Padding") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });

                var contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
                contentPresenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                contentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
                border.AppendChild(contentPresenter);

                template.VisualTree = border;
                style.Setters.Add(new Setter(Button.TemplateProperty, template));
                pageBtn.Style = style;

                PageNumbersPanel.Children.Add(pageBtn);
            }
        }

        private void PrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1) { _currentPage--; UpdatePagination(); }
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < _totalPages) { _currentPage++; UpdatePagination(); }
        }

        private void ItemsPerPage_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (ItemsPerPageComboBox.SelectedItem is ComboBoxItem item && _filteredPatients != null)
            {
                _itemsPerPage = int.Parse(item.Content.ToString());
                _currentPage = 1;
                UpdatePagination();
            }
        }

        // Selection Mode

        private void SelectionModeToggle_Click(object sender, RoutedEventArgs e)
        {
            _isSelectionModeEnabled = !_isSelectionModeEnabled;

            if (_isSelectionModeEnabled)
            {
                SelectionModeToggle.Background = new SolidColorBrush(Color.FromArgb(0x4D, 0xFF, 0xFF, 0xFF));
                SelectionModeToggle.ToolTip = Strings.PatientsView_ExitSelectionMode;
                CheckboxVisibility = Visibility.Visible;
                SelectAllCheckBox.Visibility = Visibility.Visible;

                // Detach single-select handler so clicks toggle checkboxes instead
                PatientsListView.SelectionChanged -= PatientsListView_SelectionChanged;
            }
            else
            {
                SelectionModeToggle.Background = new SolidColorBrush(Color.FromArgb(0x26, 0xFF, 0xFF, 0xFF));
                SelectionModeToggle.ToolTip = Strings.PatientsView_SelectPatients;
                CheckboxVisibility = Visibility.Collapsed;
                SelectAllCheckBox.Visibility = Visibility.Collapsed;

                foreach (var patient in _filteredPatients)
                    patient.IsSelected = false;

                PatientCountText.Text = string.Format(Strings.PatientsView_PatientCountFormat, _filteredPatients.Count);
                BulkActionsBar.Visibility = Visibility.Collapsed;
                UpdateSelectAllState();

                // Restore single-select handler
                PatientsListView.SelectionChanged += PatientsListView_SelectionChanged;
            }
        }

        private void ListViewItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isSelectionModeEnabled)
                return;

            var listViewItem = sender as ListViewItem;
            if (listViewItem?.DataContext is Patient patient)
            {
                e.Handled = true;
                patient.IsSelected = !patient.IsSelected;
                UpdateBulkActionsBarFast();
                UpdateSelectAllState();
            }
        }

        // Select All

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingSelectAll || _filteredPatients == null)
                return;

            bool selectAll = SelectAllCheckBox.IsChecked == true;

            foreach (var patient in _filteredPatients)
                patient.IsSelected = selectAll;

            UpdateBulkActionsBarFast();
        }

        /// <summary>
        /// Updates the Select All checkbox tri-state based on current selection.
        /// Unchecked = none, Indeterminate = some, Checked = all.
        /// </summary>
        private void UpdateSelectAllState()
        {
            if (_filteredPatients == null || !_isSelectionModeEnabled)
                return;

            _isUpdatingSelectAll = true;

            int total = _filteredPatients.Count;
            int selected = 0;
            foreach (var p in _filteredPatients)
            {
                if (p.IsSelected) selected++;
            }

            if (selected == 0)
                SelectAllCheckBox.IsChecked = false;
            else if (selected == total)
                SelectAllCheckBox.IsChecked = true;
            else
                SelectAllCheckBox.IsChecked = null;

            _isUpdatingSelectAll = false;
        }

        // Bulk Actions

        private void UpdateBulkActionsBarFast()
        {
            if (_filteredPatients == null)
                return;

            int selectedCount = 0;
            foreach (var patient in _filteredPatients)
                if (patient.IsSelected) selectedCount++;

            int total = _filteredPatients.Count;
            PatientCountText.Text = selectedCount > 0
                ? string.Format(Strings.PatientsView_SelectedOfTotal, selectedCount, total)
                : string.Format(Strings.PatientsView_PatientCountFormat, total);

            if (selectedCount > 0 && BulkActionsBar.Visibility != Visibility.Visible)
                BulkActionsBar.Visibility = Visibility.Visible;
            else if (selectedCount == 0 && BulkActionsBar.Visibility != Visibility.Collapsed)
                BulkActionsBar.Visibility = Visibility.Collapsed;
        }

        private void ClearSelection_Click(object sender, RoutedEventArgs e)
        {
            foreach (var patient in _filteredPatients)
                patient.IsSelected = false;

            UpdateBulkActionsBarFast();
            UpdateSelectAllState();
        }

        private void BulkDelete_Click(object sender, RoutedEventArgs e)
        {
            var selectedPatients = _filteredPatients.Where(p => p.IsSelected).ToList();
            if (selectedPatients.Count == 0) return;

            var result = AppDialog.Show(
                string.Format(Strings.PatientsView_BulkDeleteConfirm, selectedPatients.Count),
                Strings.PatientsView_ConfirmDeletionCaption,
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                // NOTE: Removes from in-memory collections only — no database delete
                foreach (var patient in selectedPatients)
                {
                    _allPatients.Remove(patient);
                    _filteredPatients.Remove(patient);
                }
                UpdatePagination();
                UpdateBulkActionsBarFast();
                UpdateSelectAllState();
                AppDialog.Show(string.Format(Strings.PatientsView_BulkDeleteSuccess, selectedPatients.Count), Strings.PatientsView_DeleteCompleteCaption, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Export

        private void CloseExportModal_Click(object sender, RoutedEventArgs e)
        {
            ExportModalOverlay.Visibility = Visibility.Collapsed;
        }

        private void ExportModalOverlay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source == ExportModalOverlay)
                ExportModalOverlay.Visibility = Visibility.Collapsed;
        }

        private void ExportCSV_Click(object sender, RoutedEventArgs e)
        {
            ExportToCSV();
            ExportModalOverlay.Visibility = Visibility.Collapsed;
        }

        private void ExportExcel_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement Excel export using EPPlus or ClosedXML NuGet package
            AppDialog.Show(Strings.PatientsView_ExcelExportMessage,
                Strings.PatientsView_ExportToExcelCaption, MessageBoxButton.OK, MessageBoxImage.Information);
            ExportModalOverlay.Visibility = Visibility.Collapsed;
        }

        private void ExportPDF_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement PDF export using iTextSharp or PdfSharp NuGet package
            AppDialog.Show(Strings.PatientsView_PdfExportMessage,
                Strings.PatientsView_ExportToPdfCaption, MessageBoxButton.OK, MessageBoxImage.Information);
            ExportModalOverlay.Visibility = Visibility.Collapsed;
        }

        private void ExportToCSV()
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                FileName = $"PatientList_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    var csv = new StringBuilder();
                    csv.AppendLine(string.Join(",", new[]
                    {
                        "Last Name", "First Name", "Date of Birth", "Patient ID",
                        "Gender", "Date of Screen", "Left Ear", "Right Ear"
                    }.Select(EscapeCsvField)));

                    // Exports selected patients if any are checked, otherwise all filtered
                    var patientsToExport = _filteredPatients.Any(p => p.IsSelected)
                        ? _filteredPatients.Where(p => p.IsSelected)
                        : _filteredPatients;

                    foreach (var patient in patientsToExport)
                    {
                        csv.AppendLine(string.Join(",", new[]
                        {
                            patient.LastName,
                            patient.FirstName,
                            patient.BirthDate.ToString("MM/dd/yyyy"),
                            patient.PatientId,
                            patient.Gender,
                            patient.DateOfScreen?.ToString("MM/dd/yyyy") ?? "N/A",
                            patient.LeftEarResult,
                            patient.RightEarResult
                        }.Select(EscapeCsvField)));
                    }

                    File.WriteAllText(saveDialog.FileName, csv.ToString());
                    AppDialog.Show(string.Format(Strings.PatientsView_ExportSuccess, saveDialog.FileName),
                        Strings.PatientsView_ExportCompleteCaption, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    AppDialog.Show(string.Format(Strings.PatientsView_ExportError, ex.Message), Strings.PatientsView_ExportErrorCaption,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Escapes a single CSV field per RFC 4180: values containing a comma, quote,
        // or line break are wrapped in double quotes, and embedded quotes are doubled.
        private static string EscapeCsvField(string value)
        {
            value = value ?? string.Empty;

            if (value.IndexOfAny(new[] { ',', '"', '\r', '\n' }) < 0)
                return value;

            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        // Search and Filter

        private bool _isUpdatingSearchBoxProgrammatically = false;

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingSearchBoxProgrammatically)
                return;

            if (SearchBox.Text == "Type here to search")
            {
                ClearSearchButton.Visibility = Visibility.Collapsed;
                return;
            }

            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                ClearSearchButton.Visibility = Visibility.Collapsed;

                if (FilterFieldsContainer.Children.Count > 0)
                    ApplyFilters();
                else
                {
                    _filteredPatients = new ObservableCollection<Patient>(_allPatients);
                    ApplyCurrentSort();
                    _currentPage = 1;
                    UpdatePagination();
                }
            }
            else
            {
                ClearSearchButton.Visibility = Visibility.Visible;
                PerformGeneralSearch(SearchBox.Text);
            }
        }

        private void PerformGeneralSearch(string searchText)
        {
            // When filter fields are active, search within already-filtered set
            var searchBase = FilterFieldsContainer.Children.Count > 0 ? _filteredPatients : _allPatients;

            var filtered = searchBase.Where(p =>
                p.FirstName.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                p.LastName.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                p.PatientId.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                p.Gender.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                p.BirthDate.ToString("MM/dd/yyyy").Contains(searchText)
            );

            _filteredPatients = new ObservableCollection<Patient>(filtered);
            ApplyCurrentSort();
            _currentPage = 1;
            UpdatePagination();
        }

        private void FilterToggle_Click(object sender, RoutedEventArgs e)
        {
            FilterPopup.IsOpen = !FilterPopup.IsOpen;
        }

        private void FilterApply_Click(object sender, RoutedEventArgs e)
        {
            FilterFieldsContainer.Children.Clear();
            _selectedFilters.Clear();

            if (ChkFirstName.IsChecked == true)
            {
                _selectedFilters.Add("FirstName");
                AddFilterField(Strings.PatientsView_FilterFirstName, "FirstNameFilter", FilterFieldType.TextBox);
            }
            if (ChkLastName.IsChecked == true)
            {
                _selectedFilters.Add("LastName");
                AddFilterField(Strings.PatientsView_FilterLastName, "LastNameFilter", FilterFieldType.TextBox);
            }
            if (ChkPatientId.IsChecked == true)
            {
                _selectedFilters.Add("PatientId");
                AddFilterField(Strings.PatientsView_FilterPatientId, "PatientIdFilter", FilterFieldType.TextBox);
            }
            if (ChkBirthDate.IsChecked == true)
            {
                _selectedFilters.Add("BirthDate");
                AddFilterField(Strings.PatientsView_FilterBirthDate, "BirthDateFilter", FilterFieldType.DateRange);
            }
            if (ChkGender.IsChecked == true)
            {
                _selectedFilters.Add("Gender");
                AddFilterField(Strings.PatientsView_FilterGender, "GenderFilter", FilterFieldType.ComboBox);
            }

            UpdateFilterFieldsVisibility();
            FilterPopup.IsOpen = false;
        }

        private void FilterCancel_Click(object sender, RoutedEventArgs e)
        {
            FilterPopup.IsOpen = false;
        }

        private void ClearSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "";
            _selectedFilters.Clear();
            ChkFirstName.IsChecked = false;
            ChkLastName.IsChecked = false;
            ChkBirthDate.IsChecked = false;
            ChkPatientId.IsChecked = false;
            ChkGender.IsChecked = false;
            FilterFieldsContainer.Children.Clear();
            FilterFieldsPanel.Visibility = Visibility.Collapsed;
            ClearSearchButton.Visibility = Visibility.Collapsed;

            _filteredPatients = new ObservableCollection<Patient>(_allPatients);
            ApplyCurrentSort();
            _currentPage = 1;
            UpdatePagination();
            SearchBox.Focus();
        }

        private void UpdateFilterFieldsVisibility()
        {
            FilterFieldsPanel.Visibility = _selectedFilters.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private enum FilterFieldType { TextBox, DateRange, ComboBox }

        private void AddFilterField(string label, string name, FilterFieldType type)
        {
            var grid = new Grid { Margin = new Thickness(0, 0, 0, 10), Tag = GetFilterTagFromName(name) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30, GridUnitType.Pixel) });

            var sp = new StackPanel();
            sp.Children.Add(new TextBlock { Text = label, FontSize = 11, Foreground = Brushes.Gray, Margin = new Thickness(0, 0, 0, 4) });

            if (type == FilterFieldType.TextBox)
            {
                var tb = new TextBox
                {
                    Name = name,
                    Background = Brushes.White,
                    BorderBrush = new SolidColorBrush(Color.FromRgb(209, 213, 219)),
                    BorderThickness = new Thickness(1),
                    Padding = new Thickness(8, 6, 8, 6),
                    FontSize = 12
                };
                tb.TextChanged += FilterField_TextChanged;
                sp.Children.Add(tb);
            }
            else if (type == FilterFieldType.DateRange)
            {
                var dateGrid = new Grid();
                dateGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                dateGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10, GridUnitType.Pixel) });
                dateGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var dpFrom = new DatePicker { Name = name + "From", FontSize = 11, Padding = new Thickness(4), DisplayDate = DateTime.Today };
                dpFrom.SelectedDateChanged += FilterField_TextChanged;
                Grid.SetColumn(dpFrom, 0);

                var dpTo = new DatePicker { Name = name + "To", FontSize = 11, Padding = new Thickness(4), DisplayDate = DateTime.Today };
                dpTo.SelectedDateChanged += FilterField_TextChanged;
                Grid.SetColumn(dpTo, 2);

                dateGrid.Children.Add(dpFrom);
                dateGrid.Children.Add(dpTo);
                sp.Children.Add(dateGrid);
            }
            else if (type == FilterFieldType.ComboBox)
            {
                var cb = new ComboBox
                {
                    Name = name,
                    Background = Brushes.White,
                    BorderBrush = new SolidColorBrush(Color.FromRgb(209, 213, 219)),
                    BorderThickness = new Thickness(1),
                    Padding = new Thickness(8, 6, 8, 6),
                    FontSize = 12
                };
                cb.Items.Add("Male");
                cb.Items.Add("Female");
                cb.Items.Add("Unknown");
                cb.SelectedIndex = -1;
                cb.SelectionChanged += FilterField_TextChanged;
                sp.Children.Add(cb);
            }

            Grid.SetColumn(sp, 0);
            grid.Children.Add(sp);

            var removeBtn = new Button
            {
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(5, 20, 0, 0),
                Tag = label,
                ToolTip = Strings.PatientsView_RemoveFilter
            };

            var path = new System.Windows.Shapes.Path
            {
                Width = 14,
                Height = 14,
                Stretch = Stretch.Uniform,
                Fill = new SolidColorBrush(Color.FromRgb(107, 114, 128)),
                Data = Geometry.Parse("M19 6.41L17.59 5 12 10.59 6.41 5 5 6.41 10.59 12 5 17.59 6.41 19 12 13.41 17.59 19 19 17.59 13.41 12z")
            };
            removeBtn.Content = path;
            removeBtn.Click += RemoveFilter_Click;
            Grid.SetColumn(removeBtn, 1);
            grid.Children.Add(removeBtn);

            FilterFieldsContainer.Children.Add(grid);
        }

        private string GetFilterTagFromName(string name) => name.Replace("Filter", "");

        private void FilterField_TextChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void RemoveFilter_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string label)
            {
                string filterName =
                    label == Strings.PatientsView_FilterFirstName ? "FirstName" :
                    label == Strings.PatientsView_FilterLastName ? "LastName" :
                    label == Strings.PatientsView_FilterPatientId ? "PatientId" :
                    label == Strings.PatientsView_FilterBirthDate ? "BirthDate" :
                    label == Strings.PatientsView_FilterGender ? "Gender" :
                    null;

                if (filterName != null)
                {
                    switch (filterName)
                    {
                        case "FirstName": ChkFirstName.IsChecked = false; break;
                        case "LastName": ChkLastName.IsChecked = false; break;
                        case "PatientId": ChkPatientId.IsChecked = false; break;
                        case "BirthDate": ChkBirthDate.IsChecked = false; break;
                        case "Gender": ChkGender.IsChecked = false; break;
                    }
                    _selectedFilters.Remove(filterName);
                    RemoveFilterField(filterName);
                }
            }
        }

        private void RemoveFilterField(string filterName)
        {
            UIElement toRemove = null;
            foreach (var child in FilterFieldsContainer.Children)
            {
                if (child is Grid grid && grid.Tag is string tag && tag == filterName)
                {
                    toRemove = grid;
                    break;
                }
            }
            if (toRemove != null)
            {
                FilterFieldsContainer.Children.Remove(toRemove);
                UpdateFilterFieldsVisibility();
                ApplyFilters();
            }
        }

        // Reads filter values from dynamically-created UI controls by walking the visual tree.
        // Fragile — depends on the exact nesting structure from AddFilterField.
        private void ApplyFilters()
        {
            string firstNameFilter = "";
            string lastNameFilter = "";
            string patientIdFilter = "";
            string genderFilter = "";
            DateTime? birthDateFrom = null;
            DateTime? birthDateTo = null;

            foreach (var child in FilterFieldsContainer.Children)
            {
                if (child is Grid grid)
                {
                    foreach (var gridChild in grid.Children)
                    {
                        if (gridChild is StackPanel sp)
                        {
                            foreach (var control in sp.Children)
                            {
                                if (control is TextBox tb)
                                {
                                    if (tb.Name == "FirstNameFilter") firstNameFilter = tb.Text;
                                    if (tb.Name == "LastNameFilter") lastNameFilter = tb.Text;
                                    if (tb.Name == "PatientIdFilter") patientIdFilter = tb.Text;
                                }
                                else if (control is ComboBox cb && cb.Name == "GenderFilter")
                                {
                                    if (cb.SelectedIndex >= 0)
                                        genderFilter = cb.SelectedItem?.ToString() ?? "";
                                }
                                else if (control is Grid dateGrid)
                                {
                                    foreach (var dateGridChild in dateGrid.Children)
                                    {
                                        if (dateGridChild is DatePicker dp)
                                        {
                                            if (dp.Name.Contains("From")) birthDateFrom = dp.SelectedDate;
                                            if (dp.Name.Contains("To")) birthDateTo = dp.SelectedDate;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Show active filter summary in the search box
            var filterParts = new List<string>();

            if (!string.IsNullOrWhiteSpace(firstNameFilter))
                filterParts.Add($"FirstName=\"{firstNameFilter}\"");
            if (!string.IsNullOrWhiteSpace(lastNameFilter))
                filterParts.Add($"LastName=\"{lastNameFilter}\"");
            if (!string.IsNullOrWhiteSpace(patientIdFilter))
                filterParts.Add($"PatientID=\"{patientIdFilter}\"");
            if (!string.IsNullOrWhiteSpace(genderFilter))
                filterParts.Add($"Gender=\"{genderFilter}\"");
            if (birthDateFrom.HasValue)
                filterParts.Add($"DOB>={birthDateFrom.Value:MM/dd/yyyy}");
            if (birthDateTo.HasValue)
                filterParts.Add($"DOB<={birthDateTo.Value:MM/dd/yyyy}");

            _isUpdatingSearchBoxProgrammatically = true;
            if (filterParts.Count > 0)
            {
                SearchBox.Text = string.Join(" ", filterParts);
                ClearSearchButton.Visibility = Visibility.Visible;
            }
            else
            {
                SearchBox.Text = "";
                ClearSearchButton.Visibility = Visibility.Collapsed;
            }
            _isUpdatingSearchBoxProgrammatically = false;

            var filtered = _allPatients.Where(p =>
            {
                bool match = true;

                if (!string.IsNullOrWhiteSpace(firstNameFilter))
                    match &= p.FirstName.Contains(firstNameFilter, StringComparison.OrdinalIgnoreCase);
                if (!string.IsNullOrWhiteSpace(lastNameFilter))
                    match &= p.LastName.Contains(lastNameFilter, StringComparison.OrdinalIgnoreCase);
                if (!string.IsNullOrWhiteSpace(patientIdFilter))
                    match &= p.PatientId.Contains(patientIdFilter, StringComparison.OrdinalIgnoreCase);
                if (!string.IsNullOrWhiteSpace(genderFilter))
                    match &= p.Gender.Equals(genderFilter, StringComparison.OrdinalIgnoreCase);
                if (birthDateFrom.HasValue)
                    match &= p.BirthDate.Date >= birthDateFrom.Value.Date;
                if (birthDateTo.HasValue)
                    match &= p.BirthDate.Date <= birthDateTo.Value.Date;

                return match;
            });

            _filteredPatients = new ObservableCollection<Patient>(filtered);
            ApplyCurrentSort();
            _currentPage = 1;
            UpdatePagination();
        }

        // Misc

        private void RefreshPatients_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Replace with real data reload when DB is implemented
            LoadDummyPatients();
            AppDialog.Show(Strings.PatientsView_RefreshSuccess, Strings.PatientsView_Refresh, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ViewToggle_Click(object sender, RoutedEventArgs e)
        {
            _isCompactView = !_isCompactView;

            if (_isCompactView)
            {
                PatientsListView.ItemTemplate = (DataTemplate)FindResource("CompactCardTemplate");
                ViewToggleIcon.Data = Geometry.Parse(DetailedIconData);
                ViewToggleButton.ToolTip = Strings.PatientsView_SwitchToDetailedView;
            }
            else
            {
                PatientsListView.ItemTemplate = (DataTemplate)FindResource("DetailedCardTemplate");
                ViewToggleIcon.Data = Geometry.Parse(CompactIconData);
                ViewToggleButton.ToolTip = Strings.PatientsView_SwitchToCompactView;
            }
        }

        private void PatientsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isSelectionModeEnabled)
                return;

            if (PatientsListView.SelectedItem is Patient selectedPatient)
            {
                var viewModel = new PatientViewModel
                {
                    PatientId = selectedPatient.PatientId,
                    HospitalId = selectedPatient.HospitalId ?? "HOSP-001",
                    FirstName = selectedPatient.FirstName,
                    LastName = selectedPatient.LastName,
                    DateOfBirth = selectedPatient.BirthDate,
                    Gender = selectedPatient.Gender,
                    LeftEarResult = selectedPatient.LeftEarResult ?? "Incomplete",
                    RightEarResult = selectedPatient.RightEarResult ?? "Incomplete",
                    BirthLocation = selectedPatient.BirthLocation ?? "Labor & Delivery",
                    GestationalAge = selectedPatient.GestationalAge ?? "Unknown"
                };
                PatientInfoPanel.LoadPatient(viewModel);
                TestResultsPanel.LoadTests(selectedPatient.Tests);
            }
            else
            {
                PatientInfoPanel.LoadPatient(null);
                TestResultsPanel.Clear();
            }
        }

        private void UpdatePatientCount()
        {
            if (_filteredPatients == null)
            {
                PatientCountText.Text = Strings.PatientsView_PatientCountZero;
                return;
            }
            PatientCountText.Text = string.Format(Strings.PatientsView_PatientCountFormat, _filteredPatients.Count);
        }

        // Import Workflow

        /// <summary>
        /// Called by AdminDashboardWindow when toolbar "Import" is clicked.
        /// Handles the full import flow: file dialog, parse, then takeover.
        /// </summary>
        public void StartImport()
        {
            ShowImportFileDialog();
        }

        private void ShowImportFileDialog()
        {
            var dialog = new ImportFileDialog();
            dialog.Owner = Window.GetWindow(this);

            if (dialog.ShowDialog() != true)
                return;

            string filePath = dialog.SelectedFilePath;
            string format = dialog.SelectedFormat;

            _importFormat = format;
            _importFilePath = filePath;

            var result = _importService.ParseFile(filePath, format);

            if (result.ParseErrors.Any())
            {
                string errors = string.Join("\n", result.ParseErrors);
                AppDialog.Show(errors, Strings.PatientsView_ImportErrorCaption, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!result.Patients.Any())
            {
                AppDialog.Show(Strings.PatientsView_NoPatientRecords, Strings.PatientsView_ImportCaption, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Facesheet formats yield a single patient for editable review;
            // batch formats (XML/JSON) go to multi-patient preview table
            if (ImportService.IsFacesheetFormat(format))
                EnterFacesheetReviewMode(result.Patients.First().Patient, filePath, format);
            else
                EnterImportMode(result.Patients);
        }

        /// <summary>
        /// Wraps the legacy ImportService function (List of PatientData to results)
        /// into the ImportRequest-aware delegate that preserves action badges.
        /// </summary>
        private Func<List<ImportRequest>, List<ImportResultItem>> WrapImportFunction()
        {
            var legacyFn = _importService.GetImportFunction(
                existsInDatabase: null,              // TODO: Connect real DB lookup
                saveToDatabase: null     // TODO: Connect real DB save
            );

            return requests =>
            {
                var results = legacyFn(requests.Select(r => r.Patient).ToList());

                // Map request actions onto result badges
                var actionMap = requests.ToDictionary(
                    r => r.Patient.PatientId ?? "",
                    r => r.Action);

                foreach (var item in results)
                {
                    if (item.Badge != null) continue;
                    if (actionMap.TryGetValue(item.PatientId ?? "", out var action))
                    {
                        item.Badge = action switch
                        {
                            ImportAction.Replace => "REPLACED",
                            ImportAction.Create => "NEW RECORD",
                            ImportAction.AddTests => "TESTS ADDED",
                            _ => null
                        };
                    }
                }

                return results;
            };
        }

        private void EnterImportMode(List<ImportPatientData> importData)
        {
            ImportReview.BackToFileSelect += ImportWorkspace_BackToFileSelect;
            ImportReview.ImportCancelled += ImportWorkspace_Cancelled;
            ImportReview.ImportDone += ImportWorkspace_Done;
            ImportReview.ViewPatientRequested += ImportWorkspace_ViewPatient;

            ImportReview.ImportFunction = WrapImportFunction();
            ImportReview.LoadPreviewData(importData);

            NormalPanelsGrid.Visibility = Visibility.Collapsed;
            FacesheetReview.Visibility = Visibility.Collapsed;
            ImportReview.Visibility = Visibility.Visible;

            // TODO: Set Import toolbar button to active/purple state
        }

        private void ExitImportMode()
        {
            ImportReview.BackToFileSelect -= ImportWorkspace_BackToFileSelect;
            ImportReview.ImportCancelled -= ImportWorkspace_Cancelled;
            ImportReview.ImportDone -= ImportWorkspace_Done;
            ImportReview.ViewPatientRequested -= ImportWorkspace_ViewPatient;

            ImportReview.Visibility = Visibility.Collapsed;
            FacesheetReview.Visibility = Visibility.Collapsed;
            NormalPanelsGrid.Visibility = Visibility.Visible;

            // TODO: Reset Import toolbar button from active state
        }

        // Import workspace event handlers

        private void ImportWorkspace_BackToFileSelect(object sender, EventArgs e)
        {
            ImportReview.BackToFileSelect -= ImportWorkspace_BackToFileSelect;
            ImportReview.ImportCancelled -= ImportWorkspace_Cancelled;
            ImportReview.ImportDone -= ImportWorkspace_Done;
            ImportReview.ViewPatientRequested -= ImportWorkspace_ViewPatient;

            ImportReview.Visibility = Visibility.Collapsed;

            ShowImportFileDialog();

            // If dialog was cancelled, restore normal view
            if (ImportReview.Visibility == Visibility.Collapsed
                && FacesheetReview.Visibility == Visibility.Collapsed)
            {
                NormalPanelsGrid.Visibility = Visibility.Visible;
            }
        }

        private void ImportWorkspace_Cancelled(object sender, EventArgs e)
        {
            ExitImportMode();
        }

        private void ImportWorkspace_Done(object sender, EventArgs e)
        {
            ExitImportMode();

            // TODO: Refresh patient list to show newly imported patients
        }

        private void ImportWorkspace_ViewPatient(object sender, PatientData patient)
        {
            ExitImportMode();

            // TODO: Select the matching patient in the list and load their info.
            // Requires DB lookup to find the patient by ID.
        }

        // Facesheet Review Flow

        /// <summary>
        /// Shows FacesheetReview for single-patient editable review.
        /// Hides normal panels and ImportWorkspace.
        /// </summary>
        private void EnterFacesheetReviewMode(PatientData patient, string filePath, string format)
        {
            NormalPanelsGrid.Visibility = Visibility.Collapsed;
            ImportReview.Visibility = Visibility.Collapsed;
            FacesheetReview.Visibility = Visibility.Visible;

            FacesheetReview.BackRequested += FacesheetReview_BackRequested;
            FacesheetReview.CancelRequested += FacesheetReview_CancelRequested;
            FacesheetReview.ImportRequested += FacesheetReview_ImportRequested;

            FacesheetReview.LoadReview(patient, filePath, format);
        }

        private void ExitFacesheetReviewMode()
        {
            FacesheetReview.BackRequested -= FacesheetReview_BackRequested;
            FacesheetReview.CancelRequested -= FacesheetReview_CancelRequested;
            FacesheetReview.ImportRequested -= FacesheetReview_ImportRequested;

            FacesheetReview.Visibility = Visibility.Collapsed;
            ImportReview.Visibility = Visibility.Collapsed;
            NormalPanelsGrid.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Back button — disconnect events, reopen file dialog so user can pick a different file.
        /// </summary>
        private void FacesheetReview_BackRequested(object sender, EventArgs e)
        {
            FacesheetReview.BackRequested -= FacesheetReview_BackRequested;
            FacesheetReview.CancelRequested -= FacesheetReview_CancelRequested;
            FacesheetReview.ImportRequested -= FacesheetReview_ImportRequested;

            FacesheetReview.Visibility = Visibility.Collapsed;

            ShowImportFileDialog();

            if (FacesheetReview.Visibility == Visibility.Collapsed
                && ImportReview.Visibility == Visibility.Collapsed)
            {
                NormalPanelsGrid.Visibility = Visibility.Visible;
            }
        }

        private void FacesheetReview_CancelRequested(object sender, EventArgs e)
        {
            ExitFacesheetReviewMode();
        }

        /// <summary>
        /// Import Patient button — runs import on the edited patient, then
        /// transitions from FacesheetReview to ImportReview Step 3 (results).
        /// </summary>
        private void FacesheetReview_ImportRequested(object sender, PatientData editedPatient)
        {
            FacesheetReview.BackRequested -= FacesheetReview_BackRequested;
            FacesheetReview.CancelRequested -= FacesheetReview_CancelRequested;
            FacesheetReview.ImportRequested -= FacesheetReview_ImportRequested;

            var importFn = WrapImportFunction();
            var results = importFn(new List<ImportRequest>
            {
                new ImportRequest { Patient = editedPatient, Action = ImportAction.Import }
            });

            FacesheetReview.Visibility = Visibility.Collapsed;
            ImportReview.Visibility = Visibility.Visible;

            ImportReview.ImportDone += ImportWorkspace_Done;
            ImportReview.ViewPatientRequested += ImportWorkspace_ViewPatient;

            ImportReview.ShowResultsDirectly(results);
        }
    }
}