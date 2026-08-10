// --------------------------------------------------------------------------------
// <copyright file="ExportDialog.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.WPF.Resources;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using AccuSync.WPF.Controls;

namespace AccuSync.WPF.Views.PatientsTests.Dialogs
{
    public enum ExportScope
    {
        NewResults,
        AllPatients,
        DateRange,
        SelectedPatients
    }

    /// <summary>
    /// Code-behind for the export dialog. The user selects a format, scope
    /// (new results, all, date range, or selected patients), and optional
    /// de-identification. Results are read via public properties after
    /// <c>DialogResult = true</c>.
    /// </summary>
    public partial class ExportDialog : Window
    {
        private ExportScope _selectedScope = ExportScope.NewResults;
        private Border _selectedCard;

        // Card visual state brushes — static to avoid repeated allocations
        private static readonly SolidColorBrush _activeBorderBrush = new(Color.FromRgb(0, 95, 190));
        private static readonly SolidColorBrush _lightBlueBrush = new(Color.FromRgb(212, 233, 249));
        private static readonly SolidColorBrush _skyBrush = new(Color.FromRgb(66, 143, 236));
        private static readonly SolidColorBrush _defaultBorderBrush = new(Color.FromRgb(229, 231, 235));
        private static readonly SolidColorBrush _hoverBgBrush = new(Color.FromRgb(240, 247, 255));

        // Read by the caller after dialog closes
        public string SelectedFormat =>
            (FormatComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";

        public ExportScope SelectedScope => _selectedScope;
        public DateTime? DateFrom => FromDatePicker.SelectedDate;
        public DateTime? DateTo => ToDatePicker.SelectedDate;
        public bool IsDeidentified => DeidentifyCheckBox.IsChecked == true;

        public ExportDialog()
        {
            InitializeComponent();
            SelectScope(ExportScope.NewResults);
        }

        // Window chrome

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                DialogResult = false;
        }

        // Footer buttons

        private void CloseButton_Click(object sender, RoutedEventArgs e)
            => DialogResult = false;

        private void CancelButton_Click(object sender, RoutedEventArgs e)
            => DialogResult = false;

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedScope == ExportScope.DateRange)
            {
                if (FromDatePicker.SelectedDate == null || ToDatePicker.SelectedDate == null)
                {
                    AppDialog.Show(Strings.ExportDialog_SelectBothDates,
                        Strings.ExportDialog_DateRangeRequired, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (FromDatePicker.SelectedDate > ToDatePicker.SelectedDate)
                {
                    AppDialog.Show(Strings.ExportDialog_FromBeforeTo,
                        Strings.ExportDialog_InvalidDateRange, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            DialogResult = true;
        }

        private void DeidentifyLabel_Click(object sender, MouseButtonEventArgs e)
        {
            DeidentifyCheckBox.IsChecked = !DeidentifyCheckBox.IsChecked;
        }

        // Scope card selection
        // Each card is a Border element; clicking one highlights it and
        // deselects the others. Only DateRange shows the date picker panel.

        private void NewResultsCard_Click(object sender, MouseButtonEventArgs e)
            => SelectScope(ExportScope.NewResults);

        private void AllPatientsCard_Click(object sender, MouseButtonEventArgs e)
            => SelectScope(ExportScope.AllPatients);

        private void DateRangeCard_Click(object sender, MouseButtonEventArgs e)
            => SelectScope(ExportScope.DateRange);

        private void SelectedPatientsCard_Click(object sender, MouseButtonEventArgs e)
            => SelectScope(ExportScope.SelectedPatients);

        // TODO: The card highlight/reset logic manually maps each ExportScope to
        //       a (card, icon) pair. If a fifth scope is added, the switch must
        //       be updated. Consider storing card/icon pairs in a dictionary
        //       keyed by ExportScope.
        private void SelectScope(ExportScope scope)
        {
            _selectedScope = scope;

            ResetCard(NewResultsCard, NewResultsIconBorder);
            ResetCard(AllPatientsCard, AllPatientsIconBorder);
            ResetCard(DateRangeCard, DateRangeIconBorder);
            ResetCard(SelectedPatientsCard, SelectedPatientsIconBorder);

            Border card;
            Border icon;

            switch (scope)
            {
                case ExportScope.NewResults:
                    card = NewResultsCard; icon = NewResultsIconBorder; break;
                case ExportScope.AllPatients:
                    card = AllPatientsCard; icon = AllPatientsIconBorder; break;
                case ExportScope.DateRange:
                    card = DateRangeCard; icon = DateRangeIconBorder; break;
                case ExportScope.SelectedPatients:
                    card = SelectedPatientsCard; icon = SelectedPatientsIconBorder; break;
                default:
                    card = NewResultsCard; icon = NewResultsIconBorder; break;
            }

            HighlightCard(card, icon);
            _selectedCard = card;

            DateRangePanel.Visibility = scope == ExportScope.DateRange
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void ResetCard(Border card, Border iconBorder)
        {
            card.BorderBrush = _defaultBorderBrush;
            card.Background = Brushes.White;
            card.Effect = null;
            iconBorder.Background = _lightBlueBrush;
        }

        private void HighlightCard(Border card, Border iconBorder)
        {
            card.BorderBrush = _activeBorderBrush;
            card.Background = _lightBlueBrush;
            card.Effect = new DropShadowEffect
            {
                BlurRadius = 8,
                Opacity = 0.12,
                ShadowDepth = 0,
                Color = Color.FromRgb(0, 95, 190)
            };
            iconBorder.Background = Brushes.White;
        }

        // Hover effects — skipped for the currently selected card

        private void ScopeCard_MouseEnter(object sender, MouseEventArgs e)
        {
            var card = sender as Border;
            if (card == null || card == _selectedCard) return;

            card.BorderBrush = _skyBrush;
            card.Background = _hoverBgBrush;
        }

        private void ScopeCard_MouseLeave(object sender, MouseEventArgs e)
        {
            var card = sender as Border;
            if (card == null || card == _selectedCard) return;

            card.BorderBrush = _defaultBorderBrush;
            card.Background = Brushes.White;
        }
    }
}