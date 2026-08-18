// --------------------------------------------------------------------------------
// <copyright file="TestResultsView.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Application.Models;
using AccuSync.Presentation.Models;
using AccuSync.Core.Entities;
using AccuSync.WPF.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AccuSync.WPF.Controls;

namespace AccuSync.WPF.Views.PatientsTests
{
    /// <summary>
    /// Displays a selected patient's test results in a grid with a detail panel
    /// (waveform placeholder, EEG stats, comments, and device info tabs).
    /// </summary>
    public partial class TestResultsView : UserControl
    {
        private List<TestResultRow> _testRows;
        private int _currentTabIndex = 0;

        /// <summary>
        /// Initializes the control.
        /// </summary>
        public TestResultsView()
        {
            InitializeComponent();
        }

        // Public API — called by PatientsView

        /// <summary>
        /// Load tests for the selected patient. Pass null or empty to show empty state.
        /// </summary>
        public void LoadTests(List<TestRecord> tests)
        {
            if (tests == null || tests.Count == 0)
            {
                ShowNoTests(tests == null);
                return;
            }

            _testRows = tests
                .OrderByDescending(t => t.TestDate)
                .Select(t => new TestResultRow(t))
                .ToList();

            TestsDataGrid.ItemsSource = _testRows;
            ShowContent();

            if (_testRows.Count > 0)
                TestsDataGrid.SelectedIndex = 0;
        }

        /// <summary>
        /// Clear panel back to empty state.
        /// </summary>
        public void Clear()
        {
            TestsDataGrid.ItemsSource = null;
            _testRows = null;
            ShowEmptyState();
        }

        // Visibility State

        private void ShowEmptyState()
        {
            EmptyState.Visibility = Visibility.Visible;
            NoTestsState.Visibility = Visibility.Collapsed;
            ContentGrid.Visibility = Visibility.Collapsed;
        }

        private void ShowNoTests(bool isNull)
        {
            // null = no patient selected ("select a patient"), not-null = patient has no tests
            EmptyState.Visibility = isNull ? Visibility.Visible : Visibility.Collapsed;
            NoTestsState.Visibility = isNull ? Visibility.Collapsed : Visibility.Visible;
            ContentGrid.Visibility = Visibility.Collapsed;
        }

        private void ShowContent()
        {
            EmptyState.Visibility = Visibility.Collapsed;
            NoTestsState.Visibility = Visibility.Collapsed;
            ContentGrid.Visibility = Visibility.Visible;
        }

        // Test Selection

        private void TestsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TestsDataGrid.SelectedItem is TestResultRow row)
            {
                UpdateDetail(row.Source);
                SwitchTab(0);
            }
        }

        private void UpdateDetail(TestRecord test)
        {
            if (test == null) return;

            // DP Details tab — visible only for DPOAE tests
            if (test.IsDPOAE)
            {
                DpDetailsTab.Visibility = Visibility.Visible;
                DpDetailsColumn.Width = new GridLength(1, GridUnitType.Star);
            }
            else
            {
                DpDetailsTab.Visibility = Visibility.Collapsed;
                DpDetailsColumn.Width = new GridLength(0);
            }

            WaveformLabel.Text = string.Format(Strings.TestResultsView_GraphPlaceholderFormat, test.TestType);

            // EEG section — ABR tests only
            if (test.IsABR)
            {
                EegSection.Visibility = Visibility.Visible;
                ImpWhiteText.Text = string.Format(Strings.TestResultsView_ImpedanceFormat, test.ImpedanceWhite ?? 0);
                ImpRedText.Text = string.Format(Strings.TestResultsView_ImpedanceFormat, test.ImpedanceRed ?? 0);

                int eeg = Math.Clamp(test.EegNoisePercent ?? 0, 0, 100);
                EegBlackCol.Width = new GridLength(100 - eeg, GridUnitType.Star);
                EegLightCol.Width = new GridLength(eeg > 0 ? eeg : 0.001, GridUnitType.Star);
            }
            else
            {
                EegSection.Visibility = Visibility.Collapsed;
            }

            UpdateResultIcon(test.TestResult);

            ResultLabel.Text = test.TestResult;

            if (test.TestResult == "Incomplete")
            {
                ResultSublabel.Text = Strings.TestResultsView_ProbeError;
                ResultSublabel.Visibility = Visibility.Visible;
            }
            else
            {
                ResultSublabel.Visibility = Visibility.Collapsed;
            }

            ResultDate.Text = test.TestDateFormatted;

            StatDuration.Text = test.DurationFormatted;
            StatEar.Text = test.Ear;

            DevSerialText.Text = test.DeviceSerial ?? Strings.TestResultsView_NotAvailableDash;
            DevTypeText.Text = test.FirmwareBuild ?? Strings.TestResultsView_NotAvailableDash;
            TestFacilityText.Text = test.TestFacility ?? Strings.TestResultsView_NotAvailableDash;
            TestLocationText.Text = test.TestLocation ?? Strings.TestResultsView_NotAvailableDash;
            ProbeSerialText.Text = test.ProbeSerial ?? Strings.TestResultsView_NotAvailableDash;
            ProbeCalText.Text = test.ProbeCalibrationFormatted;
        }

        private void UpdateResultIcon(string result)
        {
            string data;
            Color color;

            switch (result)
            {
                case "Pass":
                    data = "M5 14.5L11 20.5L23 7.5";
                    color = Color.FromRgb(0x42, 0x9C, 0x10);
                    ResultIcon.Fill = Brushes.Transparent;
                    ResultIcon.Stroke = new SolidColorBrush(color);
                    ResultIcon.StrokeThickness = 3.5;
                    ResultIcon.StrokeStartLineCap = PenLineCap.Round;
                    ResultIcon.StrokeEndLineCap = PenLineCap.Round;
                    ResultIcon.StrokeLineJoin = PenLineJoin.Round;
                    break;
                case "Refer":
                    data = "M7 7L21 21M21 7L7 21";
                    color = Color.FromRgb(0xdc, 0x35, 0x45);
                    ResultIcon.Fill = Brushes.Transparent;
                    ResultIcon.Stroke = new SolidColorBrush(color);
                    ResultIcon.StrokeThickness = 3.5;
                    ResultIcon.StrokeStartLineCap = PenLineCap.Round;
                    ResultIcon.StrokeEndLineCap = PenLineCap.Round;
                    ResultIcon.StrokeLineJoin = PenLineJoin.Round;
                    break;
                default:
                    data = "M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-2h2v2zm0-4h-2V7h2v6z";
                    color = Color.FromRgb(0x9E, 0xA2, 0xAC);
                    ResultIcon.Fill = new SolidColorBrush(color);
                    ResultIcon.Stroke = null;
                    ResultIcon.StrokeThickness = 0;
                    break;
            }

            ResultIcon.Data = Geometry.Parse(data);
        }

        // Tab Switching

        private void TestResultTab_Click(object sender, RoutedEventArgs e) => SwitchTab(0);
        private void DpDetailsTab_Click(object sender, RoutedEventArgs e) => SwitchTab(1);
        private void TestCommentsTab_Click(object sender, RoutedEventArgs e) => SwitchTab(2);
        private void DeviceInfoTab_Click(object sender, RoutedEventArgs e) => SwitchTab(3);

        private void SwitchTab(int tabIndex)
        {
            _currentTabIndex = tabIndex;

            TestResultTab.Style = (Style)FindResource(tabIndex == 0 ? "DetailTabActiveStyle" : "DetailTabStyle");
            DpDetailsTab.Style = (Style)FindResource(tabIndex == 1 ? "DetailTabActiveStyle" : "DetailTabStyle");
            TestCommentsTab.Style = (Style)FindResource(tabIndex == 2 ? "DetailTabActiveStyle" : "DetailTabStyle");
            DeviceInfoTab.Style = (Style)FindResource(tabIndex == 3 ? "DetailTabActiveStyle" : "DetailTabStyle");

            TestResultContent.Visibility = tabIndex == 0 ? Visibility.Visible : Visibility.Collapsed;
            DpDetailsContent.Visibility = tabIndex == 1 ? Visibility.Visible : Visibility.Collapsed;
            TestCommentsContent.Visibility = tabIndex == 2 ? Visibility.Visible : Visibility.Collapsed;
            DeviceInfoContent.Visibility = tabIndex == 3 ? Visibility.Visible : Visibility.Collapsed;
        }

        // Comments

        private void AddComment_Click(object sender, RoutedEventArgs e)
        {
            if (CommentDropdown.SelectedIndex <= 0)
            {
                AppDialog.Show(Strings.TestResultsView_SelectCommentToAdd, Strings.TestResultsView_AddCommentCaption,
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var selected = (CommentDropdown.SelectedItem as ComboBoxItem)?.Content?.ToString();
            if (!string.IsNullOrEmpty(selected))
            {
                // NOTE: No persistence — shows confirmation but doesn't save to database
                AppDialog.Show(string.Format(Strings.TestResultsView_CommentAddedMessage, selected),
                    Strings.TestResultsView_CommentAddedCaption, MessageBoxButton.OK, MessageBoxImage.Information);
                CommentDropdown.SelectedIndex = 0;
            }
        }
    }
}
