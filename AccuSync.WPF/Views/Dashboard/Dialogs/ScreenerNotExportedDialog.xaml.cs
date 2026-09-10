// --------------------------------------------------------------------------------
// <copyright file="ScreenerNotExportedDialog.xaml.cs" company="Natus Sensory">
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
using AccuSync.Presentation.Models;
using AccuSync.WPF.Resources;
using AccuSync.WPF.Controls;

namespace AccuSync.WPF.Views.Dashboard.Dialogs
{
    /// <summary>
    /// Dialog listing a screener's completed screenings that have not yet been exported.
    /// </summary>
    public partial class ScreenerNotExportedDialog : Window
    {
        private List<ScreenerNotExportedScreening> _allScreenings;
        private List<ScreenerNotExportedScreening> _filteredScreenings;

        /// <summary>
        /// Initializes the dialog and asynchronously loads not-exported screening data.
        /// </summary>
        public ScreenerNotExportedDialog()
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
            _allScreenings = GetNotExportedScreenings();
            _filteredScreenings = new List<ScreenerNotExportedScreening>(_allScreenings);
            UpdateScreeningList();
        }

        private void UpdateScreeningList()
        {
            ScreeningListItems.ItemsSource = _filteredScreenings;
            ScreeningCountText.Text = string.Format(
                _filteredScreenings.Count == 1 ? Strings.ScreenerNotExportedDialog_CountSingular : Strings.ScreenerNotExportedDialog_CountPlural,
                _filteredScreenings.Count);

            if (_filteredScreenings.Count == 0)
            {
                ShowEmptyState();
            }
            else
            {
                ShowScreeningList();
            }
        }

        private List<ScreenerNotExportedScreening> GetNotExportedScreenings()
        {
            // TODO: Replace with actual database query
            // var screenings = _screeningService.GetNotExportedScreeningsForScreener(screenerId);

            // Sample data - in production, query screenings pending export for this screener
            return new List<ScreenerNotExportedScreening>
            {
                new ScreenerNotExportedScreening
                {
                    PatientId = "PT-2025-007",
                    PatientName = "Sophia Martinez",
                    CompletedTime = new DateTime(2025, 10, 29, 12, 20, 0),
                    Result = "Refer"
                },
                new ScreenerNotExportedScreening
                {
                    PatientId = "PT-2025-012",
                    PatientName = "Jackson Lee",
                    CompletedTime = new DateTime(2025, 10, 28, 15, 45, 0),
                    Result = "Pass"
                },
                new ScreenerNotExportedScreening
                {
                    PatientId = "PT-2025-018",
                    PatientName = "Emma Wilson",
                    CompletedTime = new DateTime(2025, 10, 27, 9, 30, 0),
                    Result = "Pass"
                }
            };
        }

        #region Search Functionality

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == Strings.ScreenerNotExportedDialog_SearchPlaceholder)
            {
                SearchBox.Text = "";
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchBox.Text = Strings.ScreenerNotExportedDialog_SearchPlaceholder;
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchBox.Text == Strings.ScreenerNotExportedDialog_SearchPlaceholder || _allScreenings == null)
                return;

            var searchText = SearchBox.Text.ToLower();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                _filteredScreenings = new List<ScreenerNotExportedScreening>(_allScreenings);
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
            SearchBox.Text = Strings.ScreenerNotExportedDialog_SearchPlaceholder;
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

        #region Export Functionality

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ScreenerNotExportedScreening screening)
            {
                var result = AppDialog.Show(
                    $"Export screening results for {screening.PatientName} (ID: {screening.PatientId})?\n\n" +
                    $"Result: {screening.Result}\n" +
                    $"Completed: {screening.CompletedTime:yyyy-MM-dd HH:mm}",
                    Strings.ScreenerNotExportedDialog_ConfirmExportCaption,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // TODO: In production, perform actual export here
                    // _exportService.ExportScreening(screening.PatientId);

                    AppDialog.Show(
                        string.Format(Strings.ScreenerNotExportedDialog_ExportSuccessMessage, screening.PatientName),
                        Strings.ScreenerNotExportedDialog_ExportCompleteCaption,
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    // Remove from list
                    _allScreenings.Remove(screening);
                    _filteredScreenings.Remove(screening);
                    UpdateScreeningList();
                }
            }
        }

        #endregion

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
