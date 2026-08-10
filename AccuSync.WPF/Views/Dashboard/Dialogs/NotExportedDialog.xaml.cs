// --------------------------------------------------------------------------------
// <copyright file="NotExportedDialog.xaml.cs" company="Natus Sensory">
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
using AccuSync.Models;
using AccuSync.WPF.Resources;
using AccuSync.WPF.Controls;

namespace AccuSync.WPF.Views.Dashboard.Dialogs
{
    public partial class NotExportedDialog : Window
    {
        private List<NotExportedPatientInfo> _allPatients;
        private List<NotExportedPatientInfo> _filteredPatients;

        private static readonly SolidColorBrush _cardHoverBorder = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#428FEC"));
        private static readonly SolidColorBrush _cardHoverBg = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f0f7ff"));
        private static readonly SolidColorBrush _cardDefaultBorder = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E5E7EB"));

        public NotExportedDialog()
        {
            InitializeComponent();
            LoadPatientData();
        }

        // ═══════════════════════════════════
        //  Window chrome
        // ═══════════════════════════════════

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        // ═══════════════════════════════════
        //  Data
        // ═══════════════════════════════════

        private void LoadPatientData()
        {
            _allPatients = GetNotExportedPatients();
            _filteredPatients = new List<NotExportedPatientInfo>(_allPatients);
            UpdatePatientList();
        }

        private void UpdatePatientList()
        {
            PatientListItems.ItemsSource = null;
            PatientListItems.ItemsSource = _filteredPatients;
            CountText.Text = $"{_filteredPatients.Count} patient{(_filteredPatients.Count != 1 ? "s" : "")}";

            if (_filteredPatients.Count == 0)
            {
                EmptyView.Visibility = Visibility.Visible;
                PatientListView.Visibility = Visibility.Collapsed;
            }
            else
            {
                EmptyView.Visibility = Visibility.Collapsed;
                PatientListView.Visibility = Visibility.Visible;
            }
        }

        // ═══════════════════════════════════
        //  Search
        // ═══════════════════════════════════

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == Strings.NotExportedDialog_SearchPlaceholder) SearchBox.Text = "";
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text)) SearchBox.Text = Strings.NotExportedDialog_SearchPlaceholder;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchBox.Text == Strings.NotExportedDialog_SearchPlaceholder || _allPatients == null) return;
            var q = SearchBox.Text.ToLower();

            _filteredPatients = string.IsNullOrWhiteSpace(q)
                ? new List<NotExportedPatientInfo>(_allPatients)
                : _allPatients.Where(p =>
                    p.MRN.ToLower().Contains(q) ||
                    p.FirstName.ToLower().Contains(q) ||
                    p.LastName.ToLower().Contains(q) ||
                    p.OwnerScreener.ToLower().Contains(q)).ToList();

            UpdatePatientList();
        }

        // ═══════════════════════════════════
        //  Card hover
        // ═══════════════════════════════════

        private void PatientCard_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border card)
            {
                card.BorderBrush = _cardHoverBorder;
                card.Background = _cardHoverBg;
            }
        }

        private void PatientCard_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border card)
            {
                card.BorderBrush = _cardDefaultBorder;
                card.Background = Brushes.White;
            }
        }

        private T FindChild<T>(DependencyObject parent, string name) where T : FrameworkElement
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T el && (string.IsNullOrEmpty(name) || el.Name == name)) return el;
                var r = FindChild<T>(child, name);
                if (r != null) return r;
            }
            return null;
        }

        // ═══════════════════════════════════
        //  Export action
        // ═══════════════════════════════════

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is NotExportedPatientInfo patient)
            {
                var result = AppDialog.Show(
                    string.Format(Strings.NotExportedDialog_ConfirmExportMessage, patient.FirstName, patient.LastName, patient.MRN),
                    Strings.NotExportedDialog_ConfirmExportCaption, MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    AppDialog.Show(
                        string.Format(Strings.NotExportedDialog_ExportCompleteMessage, patient.FirstName, patient.LastName),
                        Strings.NotExportedDialog_ExportCompleteCaption, MessageBoxButton.OK, MessageBoxImage.Information);

                    _allPatients.Remove(patient);
                    _filteredPatients.Remove(patient);
                    UpdatePatientList();
                }
            }
        }

        // ═══════════════════════════════════
        //  Sample data
        // ═══════════════════════════════════

        private List<NotExportedPatientInfo> GetNotExportedPatients()
        {
            return new List<NotExportedPatientInfo>
            {
                new NotExportedPatientInfo { MRN = "P-1002", FirstName = "Oliver", LastName = "Smith", OwnerScreener = "Screener B" },
                new NotExportedPatientInfo { MRN = "P-1011", FirstName = "Sophia", LastName = "Miller", OwnerScreener = "Screener C" },
                new NotExportedPatientInfo { MRN = "P-1015", FirstName = "Mia", LastName = "Martinez", OwnerScreener = "Screener A" },
                new NotExportedPatientInfo { MRN = "P-1022", FirstName = "Liam", LastName = "Anderson", OwnerScreener = "Screener D" },
                new NotExportedPatientInfo { MRN = "P-1033", FirstName = "Emma", LastName = "Taylor", OwnerScreener = "Screener B" }
            };
        }
    }
}