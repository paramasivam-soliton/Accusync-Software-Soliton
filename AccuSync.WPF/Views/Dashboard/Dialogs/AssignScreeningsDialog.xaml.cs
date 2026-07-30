// --------------------------------------------------------------------------------
// <copyright file="AssignScreeningsDialog.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AccuSync.Application.Models;
using AccuSync.WPF.Resources;
using AccuSync.WPF.Controls;

namespace AccuSync.WPF.Views.Dashboard.Dialogs
{
    public partial class AssignScreeningsDialog : Window
    {
        private string _screenerName;
        private List<UnassignedPatientInfo> _allPatients;
        private List<UnassignedPatientInfo> _filteredPatients;
        private bool _showHighPriorityOnly = true;

        public AssignScreeningsDialog(string screenerName)
        {
            InitializeComponent();
            _screenerName = screenerName;
            SubtitleText.Text = string.Format(Strings.AssignScreeningsDialog_SubtitleFormat, screenerName);

            LoadPatientData();
        }

        private void LoadPatientData()
        {
            _allPatients = GetUnassignedPatients();
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            var searchText = SearchBox.Text;
            bool isSearching = !string.IsNullOrWhiteSpace(searchText) && searchText != Strings.AssignScreeningsDialog_SearchPlaceholder;

            _filteredPatients = _allPatients.Where(p =>
            {
                if (_showHighPriorityOnly && !p.IsHighPriority)
                    return false;

                if (isSearching)
                {
                    return p.MRN.ToLower().Contains(searchText.ToLower()) ||
                           p.FirstName.ToLower().Contains(searchText.ToLower()) ||
                           p.LastName.ToLower().Contains(searchText.ToLower());
                }

                return true;
            }).ToList();

            UpdatePatientList();
        }

        private void UpdatePatientList()
        {
            PatientListItems.ItemsSource = null;
            PatientListItems.ItemsSource = _filteredPatients;

            var count = _filteredPatients.Count;
            PatientCountText.Text = string.Format(
                count == 1 ? Strings.AssignScreeningsDialog_UnassignedSingular : Strings.AssignScreeningsDialog_UnassignedPlural,
                count);

            UpdateSelectionCount();
        }

        private void UpdateSelectionCount()
        {
            var selectedCount = _filteredPatients.Count(p => p.IsSelected);
            SelectionCountText.Text = string.Format(Strings.AssignScreeningsDialog_SelectedFormat, selectedCount);

            AssignButton.Content = string.Format(
                selectedCount == 1 ? Strings.AssignScreeningsDialog_AssignSingular : Strings.AssignScreeningsDialog_AssignPlural,
                selectedCount);
            AssignButton.IsEnabled = selectedCount > 0;
        }

        // TODO: All hardcoded test data. Replace with actual patient service query.
        private List<UnassignedPatientInfo> GetUnassignedPatients()
        {
            return new List<UnassignedPatientInfo>
            {
                new UnassignedPatientInfo
                {
                    MRN = "P-2001",
                    FirstName = "Olivia",
                    LastName = "Brown",
                    DOB = new DateTime(2024, 10, 20),
                    WaitTime = "15 min",
                    IsHighPriority = true,
                    PriorityText = "⚠️ High Priority - NICU",
                },
                new UnassignedPatientInfo
                {
                    MRN = "P-2003",
                    FirstName = "Mason",
                    LastName = "White",
                    DOB = new DateTime(2024, 10, 19),
                    WaitTime = "8 min",
                    IsHighPriority = true,
                    PriorityText = "⚠️ High Priority - Family hx",
                },
                new UnassignedPatientInfo
                {
                    MRN = "P-2005",
                    FirstName = "Ava",
                    LastName = "Martinez",
                    DOB = new DateTime(2024, 10, 18),
                    WaitTime = "5 min",
                    IsHighPriority = false,
                },
                new UnassignedPatientInfo
                {
                    MRN = "P-2007",
                    FirstName = "Ethan",
                    LastName = "Johnson",
                    DOB = new DateTime(2024, 10, 17),
                    WaitTime = "3 min",
                    IsHighPriority = false,
                },
                new UnassignedPatientInfo
                {
                    MRN = "P-2009",
                    FirstName = "Sophia",
                    LastName = "Davis",
                    DOB = new DateTime(2024, 10, 16),
                    WaitTime = "2 min",
                    IsHighPriority = false,
                },
                new UnassignedPatientInfo
                {
                    MRN = "P-2011",
                    FirstName = "James",
                    LastName = "Wilson",
                    DOB = new DateTime(2024, 10, 15),
                    WaitTime = "45 min",
                    IsHighPriority = true,
                    PriorityText = "⚠️ High Priority - Low birth weight",
                },
                new UnassignedPatientInfo
                {
                    MRN = "P-2013",
                    FirstName = "Isabella",
                    LastName = "Moore",
                    DOB = new DateTime(2024, 10, 14),
                    WaitTime = "12 min",
                    IsHighPriority = false,
                },
                new UnassignedPatientInfo
                {
                    MRN = "P-2015",
                    FirstName = "William",
                    LastName = "Taylor",
                    DOB = new DateTime(2024, 10, 13),
                    WaitTime = "7 min",
                    IsHighPriority = false,
                }
            };
        }

        // Search and Filter

        // TODO: Same placeholder-text search pattern as AssignedPatientsDialog.
        //       Share or replace with watermark adorner.
        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == Strings.AssignScreeningsDialog_SearchPlaceholder)
                SearchBox.Text = "";
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
                SearchBox.Text = Strings.AssignScreeningsDialog_SearchPlaceholder;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_allPatients == null)
                return;

            ApplyFilters();
        }

        // TODO: Filter toggle swaps Background/Foreground imperatively. Use a bool + DataTrigger
        //       or converter to keep styling in XAML.
        private void HighPriorityFilter_Click(object sender, RoutedEventArgs e)
        {
            _showHighPriorityOnly = true;

            HighPriorityButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B4BA8"));
            HighPriorityButton.Foreground = Brushes.White;

            AllPatientsButton.Background = Brushes.White;
            AllPatientsButton.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7280"));

            ApplyFilters();
        }

        private void AllPatientsFilter_Click(object sender, RoutedEventArgs e)
        {
            _showHighPriorityOnly = false;

            AllPatientsButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B4BA8"));
            AllPatientsButton.Foreground = Brushes.White;

            HighPriorityButton.Background = Brushes.White;
            HighPriorityButton.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7280"));

            ApplyFilters();
        }

        // Patient Selection

        private void PatientCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is UnassignedPatientInfo patient)
            {
                patient.IsSelected = !patient.IsSelected;
                UpdatePatientList();
            }
        }

        // Button Handlers

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AssignButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedPatients = _allPatients.Where(p => p.IsSelected).ToList();

            if (selectedPatients.Count == 0)
                return;

            var result = AppDialog.Show(
                $"Assign {selectedPatients.Count} patient{(selectedPatients.Count != 1 ? "s" : "")} to {_screenerName}?",
                Strings.AssignScreeningsDialog_ConfirmAssignmentCaption,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // TODO: Persist assignment to database.
                AppDialog.Show(
                    $"Successfully assigned {selectedPatients.Count} patient{(selectedPatients.Count != 1 ? "s" : "")} to {_screenerName}!",
                    Strings.AssignScreeningsDialog_AssignmentCompleteCaption,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                this.DialogResult = true;
                this.Close();
            }
        }
    }
}