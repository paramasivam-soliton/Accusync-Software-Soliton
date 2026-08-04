// --------------------------------------------------------------------------------
// <copyright file="AssignedPatientsDialog.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AccuSync.Application.Models;
using AccuSync.WPF.Resources;
using AccuSync.WPF.Controls;

namespace AccuSync.WPF.Views.Dashboard.Dialogs
{
    public partial class AssignedPatientsDialog : CardListDialogBase
    {
        private List<AssignedPatient> _allPatients;
        private List<AssignedPatient> _filteredPatients;

        protected override UIElement LoadingElement => LoadingView;
        protected override UIElement ListElement => PatientListView;
        protected override UIElement EmptyElement => EmptyView;
        protected override TextBox SearchInput => SearchBox;
        protected override string SearchPlaceholder => Strings.AssignedPatientsDialog_SearchPlaceholder;

        public AssignedPatientsDialog()
        {
            InitializeComponent();
            ShowLoadingState();

            Dispatcher.InvokeAsync(async () =>
            {
                await System.Threading.Tasks.Task.Delay(800);
                LoadPatientData();
            });
        }

        private void LoadPatientData()
        {
            // TODO: Replace with actual database query
            // _allPatients = _patientService.GetAssignedPatientsForToday(screenerId);
            _allPatients = GetAssignedPatients();
            _filteredPatients = new List<AssignedPatient>(_allPatients);
            UpdatePatientList();
        }

        private void UpdatePatientList()
        {
            PatientListItems.ItemsSource = _filteredPatients;
            PatientCountText.Text = string.Format(
                _filteredPatients.Count == 1 ? Strings.AssignedPatientsDialog_CountSingular : Strings.AssignedPatientsDialog_CountPlural,
                _filteredPatients.Count);

            if (_filteredPatients.Count == 0)
                ShowEmptyState();
            else
                ShowList();
        }

        // TODO: All hardcoded test data. Replace with real patient service call.
        private List<AssignedPatient> GetAssignedPatients()
        {
            const string skyBlue = "#428FEC";
            const string orange = "#F59E0B";

            return new List<AssignedPatient>
            {
                new AssignedPatient
                {
                    MRN = "PT-2025-001",
                    FirstName = "Emma",
                    LastName = "Johnson",
                    DateOfBirth = new DateTime(2025, 3, 15),
                    AssignedTime = new DateTime(2025, 10, 29, 8, 30, 0),
                    IsScreened = false,
                    AccentColor = skyBlue,
                    Badges = new List<Badge>()
                },
                new AssignedPatient
                {
                    MRN = "PT-2025-002",
                    FirstName = "Liam",
                    LastName = "Smith",
                    DateOfBirth = new DateTime(2025, 5, 22),
                    AssignedTime = new DateTime(2025, 10, 29, 9, 15, 0),
                    IsScreened = false,
                    AccentColor = skyBlue,
                    Badges = new List<Badge>()
                },
                new AssignedPatient
                {
                    MRN = "PT-2025-003",
                    FirstName = "Olivia",
                    LastName = "Williams",
                    DateOfBirth = new DateTime(2025, 2, 8),
                    AssignedTime = new DateTime(2025, 10, 29, 7, 0, 0),
                    LastScreenDate = new DateTime(2025, 10, 29),
                    IsScreened = true,
                    ScreeningResult = "Pass",
                    AccentColor = skyBlue,
                    Badges = new List<Badge>
                    {
                        new Badge { Text = "Pass", Background = "#D1FAE5", Foreground = "#065F46" }
                    }
                },
                new AssignedPatient
                {
                    MRN = "PT-2025-004",
                    FirstName = "Noah",
                    LastName = "Brown",
                    DateOfBirth = new DateTime(2025, 7, 12),
                    AssignedTime = new DateTime(2025, 10, 29, 6, 30, 0),
                    LastScreenDate = new DateTime(2025, 10, 29),
                    IsScreened = true,
                    ScreeningResult = "Refer",
                    AccentColor = orange,
                    Badges = new List<Badge>
                    {
                        new Badge { Text = "Refer", Background = "#FEE2E2", Foreground = "#991B1B" },
                        new Badge { Text = "👨‍👩‍👧 Family hx", Background = "#DDD6FE", Foreground = "#6B21A8" },
                        new Badge { Text = "🏥 NICU >5d", Background = "#DBEAFE", Foreground = "#1E40AF" }
                    }
                },
                new AssignedPatient
                {
                    MRN = "PT-2025-005",
                    FirstName = "Ava",
                    LastName = "Davis",
                    DateOfBirth = new DateTime(2025, 4, 28),
                    AssignedTime = new DateTime(2025, 10, 29, 13, 0, 0),
                    IsScreened = false,
                    AccentColor = skyBlue,
                    Badges = new List<Badge>()
                }
            };
        }

        // Search
        // GotFocus/LostFocus placeholder handling lives in CardListDialogBase.

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchBox.Text == SearchPlaceholder || _allPatients == null)
                return;

            var searchText = SearchBox.Text.ToLower();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                _filteredPatients = new List<AssignedPatient>(_allPatients);
            }
            else
            {
                _filteredPatients = _allPatients.Where(p =>
                    p.MRN.ToLower().Contains(searchText) ||
                    p.FirstName.ToLower().Contains(searchText) ||
                    p.LastName.ToLower().Contains(searchText)
                ).ToList();
            }

            UpdatePatientList();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = SearchPlaceholder;
            ShowLoadingState();

            Dispatcher.InvokeAsync(async () =>
            {
                await System.Threading.Tasks.Task.Delay(800);
                LoadPatientData();
            });
        }

        // Card Hover Animations — FindVisualChild / AnimateChildOpacity live in CardListDialogBase.

        private void PatientCard_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border card)
            {
                AnimateChildOpacity(card, "AccentStrip", 1.0);
                AnimateChildOpacity(card, "QuickActions", 1.0);
            }
        }

        private void PatientCard_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border card)
            {
                AnimateChildOpacity(card, "AccentStrip", 0.0);
                AnimateChildOpacity(card, "QuickActions", 0.0);
            }
        }

        // Patient Card and Button Clicks

        private void PatientCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border card && card.Tag is AssignedPatient patient)
            {
                // TODO: Implement card click behavior
                // Screened patients: open screening details view
                // Unscreened patients: open screening form
            }
        }

        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            if (sender is Button button && button.Tag is AssignedPatient patient)
            {
                AppDialog.Show($"View details for {patient.FirstName} {patient.LastName}\n\n" +
                              $"MRN: {patient.MRN}\n" +
                              $"DOB: {patient.DateOfBirth:d}\n" +
                              $"Status: {(patient.IsScreened ? $"Screened - {patient.ScreeningResult}" : "Not Screened")}",
                              Strings.AssignedPatientsDialog_PatientDetailsCaption,
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);

                // TODO: Open patient details window
            }
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            if (sender is Button button && button.Tag is AssignedPatient patient)
            {
                AppDialog.Show($"Print report for {patient.FirstName} {patient.LastName}\n\n" +
                              $"MRN: {patient.MRN}",
                              Strings.AssignedPatientsDialog_PrintReportTooltip,
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);

                // TODO: Implement print functionality
            }
        }
    }
}