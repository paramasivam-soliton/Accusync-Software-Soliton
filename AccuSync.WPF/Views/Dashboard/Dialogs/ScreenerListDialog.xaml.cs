// --------------------------------------------------------------------------------
// <copyright file="ScreenerListDialog.xaml.cs" company="Natus Sensory">
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
    /// Dialog listing screeners active today with their screening counts.
    /// </summary>
    public partial class ScreenerListDialog : Window
    {
        // ── State ──
        private List<ScreenerInfo> _allScreeners;
        private List<ScreenerInfo> _filteredScreeners;

        // ── Card hover brushes ──
        private static readonly SolidColorBrush _cardHoverBorder = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#428FEC"));
        private static readonly SolidColorBrush _cardHoverBg = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f0f7ff"));
        private static readonly SolidColorBrush _cardDefaultBorder = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E5E7EB"));

        /// <summary>
        /// Initializes the dialog and loads today's screener list.
        /// </summary>
        public ScreenerListDialog()
        {
            InitializeComponent();
            DateSubtitle.Text = DateTime.Now.ToString("MMM d, yyyy");
            LoadScreenerData();
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

        private void LoadScreenerData()
        {
            _allScreeners = GetScreenersToday();
            _filteredScreeners = new List<ScreenerInfo>(_allScreeners);
            UpdateScreenerList();
        }

        private void UpdateScreenerList()
        {
            ScreenerListItems.ItemsSource = null;
            ScreenerListItems.ItemsSource = _filteredScreeners;
            CountText.Text = $"{_filteredScreeners.Count} screener{(_filteredScreeners.Count != 1 ? "s" : "")}";

            if (_filteredScreeners.Count == 0)
            {
                EmptyView.Visibility = Visibility.Visible;
                ScreenerListView.Visibility = Visibility.Collapsed;
            }
            else
            {
                EmptyView.Visibility = Visibility.Collapsed;
                ScreenerListView.Visibility = Visibility.Visible;
            }
        }

        // ═══════════════════════════════════
        //  Search
        // ═══════════════════════════════════

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == Strings.ScreenerListDialog_SearchPlaceholder)
                SearchBox.Text = "";
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
                SearchBox.Text = Strings.ScreenerListDialog_SearchPlaceholder;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchBox.Text == Strings.ScreenerListDialog_SearchPlaceholder || _allScreeners == null)
                return;

            var q = SearchBox.Text.ToLower();

            _filteredScreeners = string.IsNullOrWhiteSpace(q)
                ? new List<ScreenerInfo>(_allScreeners)
                : _allScreeners.Where(s =>
                    s.Name.ToLower().Contains(q)).ToList();

            UpdateScreenerList();
        }

        // ═══════════════════════════════════
        //  Card hover effects
        // ═══════════════════════════════════

        private void ScreenerCard_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border card)
            {
                card.BorderBrush = _cardHoverBorder;
                card.Background = _cardHoverBg;
            }
        }

        private void ScreenerCard_MouseLeave(object sender, MouseEventArgs e)
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
        //  Sample data — project palette colors
        // ═══════════════════════════════════

        private List<ScreenerInfo> GetScreenersToday()
        {
            return new List<ScreenerInfo>
            {
                new ScreenerInfo
                {
                    Name = "Screener A",
                    ScreeningCount = 5,
                    LastActivity = "Last screening 25 min ago",
                    AvatarColor = "#428FEC"   // sky
                },
                new ScreenerInfo
                {
                    Name = "Screener B",
                    ScreeningCount = 3,
                    LastActivity = "Last screening 1 hr ago",
                    AvatarColor = "#005FBE"   // bright-blue
                },
                new ScreenerInfo
                {
                    Name = "Screener C",
                    ScreeningCount = 4,
                    LastActivity = "Last screening 45 min ago",
                    AvatarColor = "#00AAA7"   // teal
                },
                new ScreenerInfo
                {
                    Name = "Screener D",
                    ScreeningCount = 2,
                    LastActivity = "Last screening 2 hrs ago",
                    AvatarColor = "#008B96"   // dark-teal
                },
                new ScreenerInfo
                {
                    Name = "Screener E",
                    ScreeningCount = 6,
                    LastActivity = "Last screening 10 min ago",
                    AvatarColor = "#473089"   // purple
                }
            };
        }
    }
}
