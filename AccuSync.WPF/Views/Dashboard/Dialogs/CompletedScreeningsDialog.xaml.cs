// --------------------------------------------------------------------------------
// <copyright file="CompletedScreeningsDialog.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AccuSync.Models;
using AccuSync.WPF.Resources;
using AccuSync.WPF.Controls;

namespace AccuSync.WPF.Views.Dashboard.Dialogs
{
    // TODO: This code-behind is near-identical to AssignedPatientsDialog.xaml.cs (three-state visibility,
    //       placeholder search, card hover animations, View/Print stubs). Extract a shared base or helper.
    public partial class CompletedScreeningsDialog : CardListDialogBase
    {
        private List<CompletedScreening> _allScreenings;
        private List<CompletedScreening> _filteredScreenings;

        protected override UIElement LoadingElement => LoadingView;
        protected override UIElement ListElement => ScreeningListView;
        protected override UIElement EmptyElement => EmptyView;
        protected override TextBox SearchInput => SearchBox;
        protected override string SearchPlaceholder => Strings.CompletedScreeningsDialog_SearchPlaceholder;

        public CompletedScreeningsDialog()
        {
            InitializeComponent();
            ShowLoadingState();

            Dispatcher.InvokeAsync(async () =>
            {
                await System.Threading.Tasks.Task.Delay(800);
                LoadScreeningData();
            });
        }

        private void LoadScreeningData()
        {
            // TODO: Replace with actual database query
            _allScreenings = GetCompletedScreenings();
            _filteredScreenings = new List<CompletedScreening>(_allScreenings);
            UpdateScreeningList();
        }

        private void UpdateScreeningList()
        {
            ScreeningListItems.ItemsSource = _filteredScreenings;
            ScreeningCountText.Text = string.Format(
                _filteredScreenings.Count == 1 ? Strings.CompletedScreeningsDialog_CountSingular : Strings.CompletedScreeningsDialog_CountPlural,
                _filteredScreenings.Count);

            if (_filteredScreenings.Count == 0)
                ShowEmptyState();
            else
                ShowList();
        }

        // TODO: All hardcoded test data. Replace with actual service call.
        private List<CompletedScreening> GetCompletedScreenings()
        {
            return new List<CompletedScreening>
            {
                new CompletedScreening
                {
                    PatientId = "PT-2025-003",
                    PatientName = "Olivia Williams",
                    DateOfBirth = new DateTime(2025, 2, 8),
                    LastScreenDate = new DateTime(2025, 10, 29),
                    CompletedTime = new DateTime(2025, 10, 29, 10, 45, 0),
                    Result = "Pass",
                    Duration = "15 min",
                    Badges = new List<ScreeningBadge>
                    {
                        new ScreeningBadge { Text = "Pass", Background = "#D1FAE5", Foreground = "#065F46" }
                    }
                },
                new CompletedScreening
                {
                    PatientId = "PT-2025-007",
                    PatientName = "Sophia Martinez",
                    DateOfBirth = new DateTime(2025, 6, 14),
                    LastScreenDate = new DateTime(2025, 10, 29),
                    CompletedTime = new DateTime(2025, 10, 29, 12, 20, 0),
                    Result = "Refer",
                    Duration = "18 min",
                    Badges = new List<ScreeningBadge>
                    {
                        new ScreeningBadge { Text = "Refer", Background = "#FEE2E2", Foreground = "#991B1B" },
                        new ScreeningBadge { Text = "👨‍👩‍👧 Family hx", Background = "#DDD6FE", Foreground = "#6B21A8" },
                        new ScreeningBadge { Text = "🏥 NICU >5d", Background = "#DBEAFE", Foreground = "#1E40AF" }
                    }
                },
                new CompletedScreening
                {
                    PatientId = "PT-2025-009",
                    PatientName = "Jackson Lee",
                    DateOfBirth = new DateTime(2025, 1, 30),
                    LastScreenDate = new DateTime(2025, 10, 29),
                    CompletedTime = new DateTime(2025, 10, 29, 14, 10, 0),
                    Result = "Pass",
                    Duration = "12 min",
                    Badges = new List<ScreeningBadge>
                    {
                        new ScreeningBadge { Text = "Pass", Background = "#D1FAE5", Foreground = "#065F46" }
                    }
                }
            };
        }

        // Search
        // GotFocus/LostFocus placeholder handling lives in CardListDialogBase.

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchBox.Text == SearchPlaceholder || _allScreenings == null)
                return;

            var searchText = SearchBox.Text.ToLower();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                _filteredScreenings = new List<CompletedScreening>(_allScreenings);
            }
            else
            {
                _filteredScreenings = _allScreenings.Where(s =>
                    s.PatientId.ToLower().Contains(searchText) ||
                    s.PatientName.ToLower().Contains(searchText) ||
                    s.Result.ToLower().Contains(searchText)
                ).ToList();
            }

            UpdateScreeningList();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = SearchPlaceholder;
            ShowLoadingState();

            Dispatcher.InvokeAsync(async () =>
            {
                await System.Threading.Tasks.Task.Delay(800);
                LoadScreeningData();
            });
        }

        // Card Hover Animations

        // Finds children by x:Name via visual tree walk — same pattern as AssignedPatientsDialog.
        // AnimateChildOpacity logs a warning if a target can't be found, so a renamed
        // DataTemplate element surfaces during development instead of silently disabling the animation.
        private void ScreeningCard_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border card)
            {
                AnimateChildOpacity(card, "AccentStrip", 1.0);
                AnimateChildOpacity(card, "QuickActions", 1.0);
            }
        }

        private void ScreeningCard_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border card)
            {
                AnimateChildOpacity(card, "AccentStrip", 0.0);
                AnimateChildOpacity(card, "QuickActions", 0.0);
            }
        }

        // Screening Card and Button Clicks

        private void ScreeningCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border card && card.Tag is CompletedScreening screening)
            {
                // TODO: Implement card click — open screening details view
            }
        }

        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            if (sender is Button button && button.Tag is CompletedScreening screening)
            {
                AppDialog.Show($"View details for {screening.PatientName}\n\n" +
                              $"Patient ID: {screening.PatientId}\n" +
                              $"Result: {screening.Result}\n" +
                              $"Completed: {screening.CompletedTime:yyyy-MM-dd HH:mm}\n" +
                              $"Duration: {screening.Duration}",
                              Strings.CompletedScreeningsDialog_ScreeningDetailsCaption,
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);

                // TODO: Open screening details window
            }
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            if (sender is Button button && button.Tag is CompletedScreening screening)
            {
                AppDialog.Show(string.Format(Strings.CompletedScreeningsDialog_PrintReportMessage, screening.PatientName, screening.PatientId),
                              Strings.CompletedScreeningsDialog_PrintReport,
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);

                // TODO: Implement print functionality
            }
        }
    }
}