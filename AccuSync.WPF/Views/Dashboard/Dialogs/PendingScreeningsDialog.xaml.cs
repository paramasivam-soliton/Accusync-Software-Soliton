// --------------------------------------------------------------------------------
// <copyright file="PendingScreeningsDialog.xaml.cs" company="Natus Sensory">
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
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using AccuSync.Application.Models;
using AccuSync.WPF.Resources;
using AccuSync.WPF.Controls;

namespace AccuSync.WPF.Views.Dashboard.Dialogs
{
    public partial class PendingScreeningsDialog : Window
    {
        private List<PendingScreening> _allScreenings;
        private List<PendingScreening> _filteredScreenings;

        public PendingScreeningsDialog()
        {
            InitializeComponent();

            // Show loading state initially
            ShowLoadingState();

            // Simulate async data loading
            Dispatcher.InvokeAsync(async () =>
            {
                await System.Threading.Tasks.Task.Delay(800); // Simulate loading
                LoadScreeningData();
            });
        }

        private void ShowLoadingState()
        {
            LoadingView.Visibility = Visibility.Visible;
            ScreeningListView.Visibility = Visibility.Collapsed;
            EmptyView.Visibility = Visibility.Collapsed;
        }

        private void ShowEmptyState()
        {
            LoadingView.Visibility = Visibility.Collapsed;
            ScreeningListView.Visibility = Visibility.Collapsed;
            EmptyView.Visibility = Visibility.Visible;
        }

        private void ShowScreeningList()
        {
            LoadingView.Visibility = Visibility.Collapsed;
            ScreeningListView.Visibility = Visibility.Visible;
            EmptyView.Visibility = Visibility.Collapsed;
        }

        private void LoadScreeningData()
        {
            // TODO: Replace with actual database query
            // _allScreenings = _screeningService.GetPendingScreenings(screenerId);

            _allScreenings = GetPendingScreenings();
            _filteredScreenings = new List<PendingScreening>(_allScreenings);
            UpdateScreeningList();
        }

        private void UpdateScreeningList()
        {
            ScreeningListItems.ItemsSource = _filteredScreenings;
            ScreeningCountText.Text = string.Format(Strings.PendingScreeningsDialog_CountFormat, _filteredScreenings.Count);

            if (_filteredScreenings.Count == 0)
            {
                ShowEmptyState();
            }
            else
            {
                ShowScreeningList();
            }
        }

        private List<PendingScreening> GetPendingScreenings()
        {
            // Sample data for demonstration
            return new List<PendingScreening>
            {
                new PendingScreening
                {
                    PatientId = "PT-2025-001",
                    PatientName = "Emma Johnson",
                    DateOfBirth = new DateTime(2025, 3, 15),
                    AssignedTime = new DateTime(2025, 10, 29, 8, 30, 0),
                    WaitingTime = "6h 15m",
                    RiskFactors = new List<PendingRiskFactor>
                    {
                        new PendingRiskFactor { Text = "👨‍👩‍👧 Family hx", Background = "#DDD6FE", Foreground = "#6B21A8" }
                    }
                },
                new PendingScreening
                {
                    PatientId = "PT-2025-004",
                    PatientName = "Noah Brown",
                    DateOfBirth = new DateTime(2025, 7, 12),
                    AssignedTime = new DateTime(2025, 10, 29, 11, 30, 0),
                    WaitingTime = "3h 15m",
                    RiskFactors = new List<PendingRiskFactor>()
                }
            };
        }

        #region Search Functionality

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == Strings.PendingScreeningsDialog_SearchPlaceholder)
            {
                SearchBox.Text = "";
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchBox.Text = Strings.PendingScreeningsDialog_SearchPlaceholder;
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchBox.Text == Strings.PendingScreeningsDialog_SearchPlaceholder || _allScreenings == null)
                return;

            var searchText = SearchBox.Text.ToLower();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                _filteredScreenings = new List<PendingScreening>(_allScreenings);
            }
            else
            {
                _filteredScreenings = _allScreenings.Where(s =>
                    s.PatientId.ToLower().Contains(searchText) ||
                    s.PatientName.ToLower().Contains(searchText)
                ).ToList();
            }

            UpdateScreeningList();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = Strings.PendingScreeningsDialog_SearchPlaceholder;
            ShowLoadingState();

            Dispatcher.InvokeAsync(async () =>
            {
                await System.Threading.Tasks.Task.Delay(800);
                LoadScreeningData();
            });
        }

        #endregion

        #region Card Hover Animations

        private void ScreeningCard_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border card)
            {
                // Show accent strip
                var accentStrip = FindVisualChild<Rectangle>(card, "AccentStrip");
                if (accentStrip != null)
                {
                    var fadeIn = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(200));
                    accentStrip.BeginAnimation(OpacityProperty, fadeIn);
                }

                // Show quick actions
                var quickActions = FindVisualChild<StackPanel>(card, "QuickActions");
                if (quickActions != null)
                {
                    var fadeIn = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(200));
                    quickActions.BeginAnimation(OpacityProperty, fadeIn);
                }
            }
        }

        private void ScreeningCard_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border card)
            {
                // Hide accent strip
                var accentStrip = FindVisualChild<Rectangle>(card, "AccentStrip");
                if (accentStrip != null)
                {
                    var fadeOut = new DoubleAnimation(0.0, TimeSpan.FromMilliseconds(200));
                    accentStrip.BeginAnimation(OpacityProperty, fadeOut);
                }

                // Hide quick actions
                var quickActions = FindVisualChild<StackPanel>(card, "QuickActions");
                if (quickActions != null)
                {
                    var fadeOut = new DoubleAnimation(0.0, TimeSpan.FromMilliseconds(200));
                    quickActions.BeginAnimation(OpacityProperty, fadeOut);
                }
            }
        }

        private T FindVisualChild<T>(DependencyObject parent, string name) where T : FrameworkElement
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T element && (string.IsNullOrEmpty(name) || element.Name == name))
                {
                    return element;
                }

                var result = FindVisualChild<T>(child, name);
                if (result != null)
                    return result;
            }
            return null;
        }

        #endregion

        #region Screening Card and Button Clicks

        private void ScreeningCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border card && card.Tag is PendingScreening screening)
            {
                // TODO: Implement card click behavior
                // Open screening form for pending patient

                // var screeningWindow = new ScreeningWindow(screening.PatientId);
                // screeningWindow.ShowDialog();
                // this.Close();
            }
        }

        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true; // Prevent card click event

            if (sender is Button button && button.Tag is PendingScreening screening)
            {
                AppDialog.Show($"View details for {screening.PatientName}\n\n" +
                              $"Patient ID: {screening.PatientId}\n" +
                              $"DOB: {screening.DateOfBirth:MM/dd/yyyy}\n" +
                              $"Assigned: {screening.AssignedTime:HH:mm}\n" +
                              $"Waiting Time: {screening.WaitingTime}",
                              Strings.PendingScreeningsDialog_PatientDetailsCaption,
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);

                // TODO: Open patient details window
                // var detailsWindow = new PatientDetailsWindow(screening.PatientId);
                // detailsWindow.ShowDialog();
            }
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true; // Prevent card click event

            if (sender is Button button && button.Tag is PendingScreening screening)
            {
                AppDialog.Show(string.Format(Strings.PendingScreeningsDialog_PrintReportMessage, screening.PatientName, screening.PatientId),
                              Strings.PendingScreeningsDialog_PrintReportToolTip,
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);

                // TODO: Implement print functionality
                // _reportService.PrintPatientReport(screening.PatientId);
            }
        }

        #endregion

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}