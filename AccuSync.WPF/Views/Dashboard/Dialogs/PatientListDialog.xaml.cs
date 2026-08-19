// --------------------------------------------------------------------------------
// <copyright file="PatientListDialog.xaml.cs" company="Natus Sensory">
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
using AccuSync.Presentation.Models;
using AccuSync.WPF.Resources;

namespace AccuSync.WPF.Views.Dashboard.Dialogs
{
    /// <summary>
    /// Dialog listing patients for a specific dashboard stat category
    /// (referred, pass, or incomplete).
    /// </summary>
    public partial class PatientListDialog : Window
    {
        /// <summary>
        /// Identifies which patient category a <see cref="PatientListDialog"/> displays.
        /// </summary>
        public enum PatientListType
        {
            /// <summary>Patients referred within the last 7 days.</summary>
            Referred,

            /// <summary>Patients who passed screening within the last 7 days.</summary>
            Pass,

            /// <summary>Patients with an incomplete screening within the last 7 days.</summary>
            Incomplete
        }

        // ── State ──
        private PatientListType _listType;
        private List<PatientInfo> _allPatients;
        private List<PatientInfo> _filteredPatients;

        // ── Accent colors per type ──
        private static readonly Color _referredColor = (Color)ColorConverter.ConvertFromString("#428FEC");   // sky
        private static readonly Color _passColor = (Color)ColorConverter.ConvertFromString("#00AAA7");     // teal
        private static readonly Color _incompleteColor = (Color)ColorConverter.ConvertFromString("#005FBE"); // bright-blue

        // ── Card hover brushes ──
        private static readonly SolidColorBrush _cardHoverBorder = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#428FEC"));
        private static readonly SolidColorBrush _cardHoverBg = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f0f7ff"));
        private static readonly SolidColorBrush _cardDefaultBorder = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E5E7EB"));

        /// <summary>
        /// Initializes the dialog and loads patients for the given category.
        /// </summary>
        /// <param name="listType">The patient category to display.</param>
        public PatientListDialog(PatientListType listType)
        {
            InitializeComponent();
            _listType = listType;
            LoadPatientData(listType);
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
            if (e.Key == Key.Escape)
                Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
            => Close();

        // ═══════════════════════════════════
        //  Data loading
        // ═══════════════════════════════════

        private void LoadPatientData(PatientListType listType)
        {
            Color badgeColor;

            switch (listType)
            {
                case PatientListType.Referred:
                    DialogTitle.Text = Strings.PatientListDialog_TitleReferred;
                    _allPatients = GetReferredPatients();
                    badgeColor = _referredColor;
                    break;
                case PatientListType.Pass:
                    DialogTitle.Text = Strings.PatientListDialog_TitlePass;
                    _allPatients = GetPassPatients();
                    badgeColor = _passColor;
                    break;
                case PatientListType.Incomplete:
                default:
                    DialogTitle.Text = Strings.PatientListDialog_TitleIncomplete;
                    _allPatients = GetIncompletePatients();
                    badgeColor = _incompleteColor;
                    break;
            }

            CountBadge.Background = new SolidColorBrush(badgeColor);
            _filteredPatients = new List<PatientInfo>(_allPatients);
            UpdatePatientList();
        }

        private void UpdatePatientList()
        {
            PatientListItems.ItemsSource = null;
            PatientListItems.ItemsSource = _filteredPatients;
            CountText.Text = string.Format(
                _filteredPatients.Count == 1 ? Strings.PatientListDialog_CountSingular : Strings.PatientListDialog_CountPlural,
                _filteredPatients.Count);

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
            if (SearchBox.Text == Strings.PatientListDialog_SearchPlaceholder)
                SearchBox.Text = "";
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
                SearchBox.Text = Strings.PatientListDialog_SearchPlaceholder;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchBox.Text == Strings.PatientListDialog_SearchPlaceholder || _allPatients == null)
                return;

            var q = SearchBox.Text.ToLower();

            _filteredPatients = string.IsNullOrWhiteSpace(q)
                ? new List<PatientInfo>(_allPatients)
                : _allPatients.Where(p =>
                    p.MRN.ToLower().Contains(q) ||
                    p.FirstName.ToLower().Contains(q) ||
                    p.LastName.ToLower().Contains(q)).ToList();

            UpdatePatientList();
        }

        // ═══════════════════════════════════
        //  Card hover effects
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

        private T FindVisualChild<T>(DependencyObject parent, string name) where T : FrameworkElement
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T el && (string.IsNullOrEmpty(name) || el.Name == name))
                    return el;
                var result = FindVisualChild<T>(child, name);
                if (result != null) return result;
            }
            return null;
        }

        // ═══════════════════════════════════
        //  Sample data
        // ═══════════════════════════════════

        private List<PatientInfo> GetReferredPatients()
        {
            const string accent = "#428FEC";
            return new List<PatientInfo>
            {
                new PatientInfo
                {
                    MRN = "P-1001", FirstName = "Ava", LastName = "Johnson",
                    DOB = new DateTime(2024, 10, 15), LastScreenDate = new DateTime(2025, 10, 20),
                    AccentColor = accent,
                    RiskFactors = new List<RiskFactor>
                    {
                        new RiskFactor { Text = "Family hx", Background = "#DDD6FE", Foreground = "#6B21A8" },
                        new RiskFactor { Text = "NICU >5d", Background = "#DBEAFE", Foreground = "#1E40AF" }
                    }
                },
                new PatientInfo
                {
                    MRN = "P-1007", FirstName = "Liam", LastName = "Brown",
                    DOB = new DateTime(2024, 10, 12), LastScreenDate = new DateTime(2025, 10, 19),
                    AccentColor = accent,
                    RiskFactors = new List<RiskFactor>
                    {
                        new RiskFactor { Text = "Low birth weight", Background = "#FED7AA", Foreground = "#9A3412" }
                    }
                },
                new PatientInfo
                {
                    MRN = "P-1013", FirstName = "Noah", LastName = "Davis",
                    DOB = new DateTime(2024, 10, 10), LastScreenDate = new DateTime(2025, 10, 18),
                    AccentColor = accent,
                    RiskFactors = new List<RiskFactor>
                    {
                        new RiskFactor { Text = "Hyperbilirubinemia", Background = "#FED7AA", Foreground = "#9A3412" }
                    }
                },
                new PatientInfo
                {
                    MRN = "P-1044", FirstName = "Emma", LastName = "Wilson",
                    DOB = new DateTime(2024, 10, 8), LastScreenDate = new DateTime(2025, 10, 17),
                    AccentColor = accent,
                    RiskFactors = new List<RiskFactor>
                    {
                        new RiskFactor { Text = "Ototoxic meds", Background = "#FECACA", Foreground = "#991B1B" }
                    }
                }
            };
        }

        private List<PatientInfo> GetPassPatients()
        {
            const string accent = "#00AAA7";
            return new List<PatientInfo>
            {
                new PatientInfo
                {
                    MRN = "P-1201", FirstName = "Harper", LastName = "Clark",
                    DOB = new DateTime(2024, 10, 14), LastScreenDate = new DateTime(2025, 10, 21),
                    AccentColor = accent, RiskFactors = new List<RiskFactor>()
                },
                new PatientInfo
                {
                    MRN = "P-1203", FirstName = "Evelyn", LastName = "Lewis",
                    DOB = new DateTime(2024, 10, 13), LastScreenDate = new DateTime(2025, 10, 20),
                    AccentColor = accent, RiskFactors = new List<RiskFactor>()
                },
                new PatientInfo
                {
                    MRN = "P-1209", FirstName = "Henry", LastName = "Walker",
                    DOB = new DateTime(2024, 10, 11), LastScreenDate = new DateTime(2025, 10, 19),
                    AccentColor = accent, RiskFactors = new List<RiskFactor>()
                },
                new PatientInfo
                {
                    MRN = "P-1211", FirstName = "Luna", LastName = "Young",
                    DOB = new DateTime(2024, 10, 9), LastScreenDate = new DateTime(2025, 10, 18),
                    AccentColor = accent, RiskFactors = new List<RiskFactor>()
                }
            };
        }

        private List<PatientInfo> GetIncompletePatients()
        {
            const string accent = "#005FBE";
            return new List<PatientInfo>
            {
                new PatientInfo
                {
                    MRN = "P-1301", FirstName = "Levi", LastName = "Allen",
                    DOB = new DateTime(2024, 10, 16), LastScreenDate = new DateTime(2025, 10, 21),
                    AccentColor = accent, RiskFactors = new List<RiskFactor>()
                },
                new PatientInfo
                {
                    MRN = "P-1303", FirstName = "Sofia", LastName = "Scott",
                    DOB = new DateTime(2024, 10, 14), LastScreenDate = new DateTime(2025, 10, 20),
                    AccentColor = accent, RiskFactors = new List<RiskFactor>()
                },
                new PatientInfo
                {
                    MRN = "P-1307", FirstName = "Benjamin", LastName = "Adams",
                    DOB = new DateTime(2024, 10, 12), LastScreenDate = new DateTime(2025, 10, 19),
                    AccentColor = accent, RiskFactors = new List<RiskFactor>()
                }
            };
        }
    }
}
