// --------------------------------------------------------------------------------
// <copyright file="ScreenerDashboardWindow.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AccuSync.WPF.Resources;
using AccuSync.WPF.Views.Dashboard.Dialogs;
using AccuSync.WPF.Controls;

namespace AccuSync.WPF.Views.Dashboard
{
    public partial class ScreenerDashboardWindow : Window
    {
        private Border? _activeNavItem;
        private string _username = "Screener";

        // Card hover brushes — matches DashboardContentView pattern
        private static readonly SolidColorBrush _cardDefaultBg =
            Brushes.White;
        private static readonly SolidColorBrush _cardDefaultBorder =
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0"));
        private static readonly SolidColorBrush _cardHoverBg =
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f0f7ff"));
        private static readonly SolidColorBrush _cardHoverBorder =
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#428FEC"));

        public ScreenerDashboardWindow()
        {
            InitializeComponent();
            _activeNavItem = NavDashboard;
            //ShowView("Dashboard");
            ShowView("Patients");
            UpdateGreeting();
            this.Loaded += (s, e) =>
            {
                HighlightCurrentDay();
                ShowRecentScreeningCounts();
            };
        }

        /// <summary>
        /// Sets the displayed username in the greeting bar.
        /// Call from login window after authentication.
        /// </summary>
        public void SetUsername(string username)
        {
            _username = username ?? "Screener";
            UpdateGreeting();
        }

        // ══════════════════════════════════════════
        //  GREETING + DATE
        // ══════════════════════════════════════════

        private void UpdateGreeting()
        {
            var hour = DateTime.Now.Hour;
            string emoji, greeting;

            if (hour < 12) { emoji = "☀️"; greeting = Strings.ScreenerDashboardWindow_GreetingMorning; }
            else if (hour < 18) { emoji = "🌤️"; greeting = Strings.ScreenerDashboardWindow_GreetingAfternoon; }
            else { emoji = "🌙"; greeting = Strings.ScreenerDashboardWindow_GreetingEvening; }

            GreetingText.Text = $"{emoji} {greeting}, {_username}";
            DateText.Text = DateTime.Now.ToString("MMM d, yyyy");
            CalMonthLabel.Text = DateTime.Now.ToString("MMMM yyyy");
        }

        // ══════════════════════════════════════════
        //  CALENDAR
        // ══════════════════════════════════════════

        private void HighlightCurrentDay()
        {
            var today = DateTime.Now.Day;
            var dayBorder = this.FindName($"Day{today}") as Border;
            if (dayBorder == null) return;

            // Design-system colours: light-blue bg, bright-blue border, midnight text
            dayBorder.Background = new SolidColorBrush(Color.FromRgb(0xD4, 0xE9, 0xF9)); // #D4E9F9
            dayBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(0x00, 0x5F, 0xBE)); // #005FBE
            dayBorder.BorderThickness = new Thickness(2);

            void StyleDay(TextBlock tb)
            {
                tb.FontWeight = FontWeights.Bold;
                tb.Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0x28, 0x56)); // #002856
            }

            if (dayBorder.Child is TextBlock tb1)
            {
                StyleDay(tb1);
            }
            else if (dayBorder.Child is StackPanel sp)
            {
                foreach (var child in sp.Children)
                    if (child is TextBlock tb2 && tb2.FontSize >= 13) { StyleDay(tb2); break; }
            }
        }

        private void ShowRecentScreeningCounts()
        {
            var today = DateTime.Now.Day;

            // TODO: replace with real DB query
            var counts = new Dictionary<int, int>
            {
                { today,     5 },
                { today - 1, 4 },
                { today - 2, 3 },
                { today - 3, 5 },
                { today - 4, 1 },
                { today - 5, 6 },
                { today - 6, 4 }
            };

            // Update KPI cards
            AssignedTodayValue.Text = "5";
            CompletedValue.Text = "3";
            PendingValue.Text = "2";
            NotExportedValue.Text = "1";
            ReferredValue.Text = "4";
            PassValue.Text = "9";

            var countColor = Color.FromRgb(0x42, 0x8F, 0xEC); // #428FEC (sky)

            foreach (var kvp in counts)
            {
                var day = kvp.Key;
                var count = kvp.Value;
                if (day < 1) continue;

                var dayBorder = this.FindName($"Day{day}") as Border;
                if (dayBorder == null) continue;

                // Only inject count badge if child is still a plain TextBlock
                if (dayBorder.Child is TextBlock)
                {
                    var sp = new StackPanel { VerticalAlignment = VerticalAlignment.Center };

                    var numTb = new TextBlock
                    {
                        Text = day.ToString(),
                        FontSize = 13,
                        Foreground = new SolidColorBrush(Color.FromRgb(0x37, 0x41, 0x51)),
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    var cntTb = new TextBlock
                    {
                        Text = count.ToString(),
                        FontSize = 10,
                        Foreground = new SolidColorBrush(countColor),
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    sp.Children.Add(numTb);
                    sp.Children.Add(cntTb);
                    dayBorder.Child = sp;

                    // Re-apply today's bold/colour on top of the injected StackPanel
                    if (day == today)
                    {
                        numTb.FontWeight = FontWeights.Bold;
                        numTb.Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0x28, 0x56));
                    }
                }
            }
        }

        private void PrevMonth_Click(object sender, MouseButtonEventArgs e)
        {
            // TODO: implement month navigation
        }

        private void NextMonth_Click(object sender, MouseButtonEventArgs e)
        {
            // TODO: implement month navigation
        }

        // ══════════════════════════════════════════
        //  NAVIGATION
        // ══════════════════════════════════════════

        private void NavDashboard_Click(object sender, MouseButtonEventArgs e)
        {
            SetActiveNavItem(NavDashboard);
            ShowView("Dashboard");
        }

        private void NavPatients_Click(object sender, MouseButtonEventArgs e)
        {
            SetActiveNavItem(NavPatients);
            ShowView("Patients");
        }

        private void NavAbout_Click(object sender, MouseButtonEventArgs e)
        {
            SetActiveNavItem(NavAbout);
            ShowView("About");
        }

        private void Logout_Click(object sender, MouseButtonEventArgs e)
        {
            var result = AppDialog.Show(
                Strings.ScreenerDashboardWindow_ConfirmLogoutMessage,
                Strings.ScreenerDashboardWindow_ConfirmLogoutCaption,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                AppDialog.Show(Strings.ScreenerDashboardWindow_LogoutSuccessMessage, Strings.ScreenerDashboardWindow_NavLogout,
                    MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
        }

        private void SetActiveNavItem(Border navItem)
        {
            if (_activeNavItem != null)
                _activeNavItem.Background =
                    new SolidColorBrush(Color.FromArgb(0x1A, 0xFF, 0xFF, 0xFF)); // 10% white
            _activeNavItem = navItem;
            _activeNavItem.Background =
                new SolidColorBrush(Color.FromArgb(0x40, 0xFF, 0xFF, 0xFF));     // 25% white
        }

        private void ShowView(string viewName)
        {
            DashboardView.Visibility = Visibility.Collapsed;
            PatientsView.Visibility = Visibility.Collapsed;
            AboutView.Visibility = Visibility.Collapsed;

            switch (viewName)
            {
                case "Dashboard":
                    DashboardView.Visibility = Visibility.Visible;
                    HeaderTitle.Text = Strings.ScreenerDashboardWindow_HeaderScreenerDashboard;
                    break;
                case "Patients":
                    PatientsView.Visibility = Visibility.Visible;
                    HeaderTitle.Text = Strings.ScreenerDashboardWindow_NavPatients;
                    break;
                case "About":
                    AboutView.Visibility = Visibility.Visible;
                    HeaderTitle.Text = Strings.ScreenerDashboardWindow_HeaderAboutAccuSync;
                    break;
            }
        }

        // ══════════════════════════════════════════
        //  HOVER EFFECTS
        // ══════════════════════════════════════════

        private void NavItem_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border b && b != _activeNavItem)
                b.Background = new SolidColorBrush(Color.FromArgb(0x26, 0xFF, 0xFF, 0xFF)); // 15%
        }

        private void NavItem_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border b && b != _activeNavItem)
                b.Background = new SolidColorBrush(Color.FromArgb(0x1A, 0xFF, 0xFF, 0xFF)); // 10%
        }

        private void ActionCard_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border b)
            {
                b.Background = _cardHoverBg;
                b.BorderBrush = _cardHoverBorder;
            }
        }

        private void ActionCard_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border b)
            {
                b.Background = _cardDefaultBg;
                b.BorderBrush = _cardDefaultBorder;
            }
        }

        // ══════════════════════════════════════════
        //  STAT CARD CLICKS
        // ══════════════════════════════════════════

        private void StatCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is FrameworkElement fe) || !(fe.Tag is string tag)) return;

            switch (tag)
            {
                case "AssignedToday":
                    new AssignedPatientsDialog { Owner = this }.ShowDialog(); break;
                case "Completed":
                    new CompletedScreeningsDialog { Owner = this }.ShowDialog(); break;
                case "Pending":
                    new PendingScreeningsDialog { Owner = this }.ShowDialog(); break;
                case "NotExported":
                    new ScreenerNotExportedDialog { Owner = this }.ShowDialog(); break;
                case "Referred":
                    // TODO: add ReferredPatientsDialog
                    AppDialog.Show(Strings.ScreenerDashboardWindow_ReferredNotImplemented, Strings.ScreenerDashboardWindow_CaptionReferred,
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                case "Pass":
                    // TODO: add PassResultsDialog
                    AppDialog.Show(Strings.ScreenerDashboardWindow_PassNotImplemented, Strings.ScreenerDashboardWindow_CaptionPass,
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
            }
        }

        // ══════════════════════════════════════════
        //  QUICK ACTION CLICKS
        // ══════════════════════════════════════════

        private void QuickAction_Click(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is FrameworkElement fe) || !(fe.Tag is string action)) return;

            switch (action)
            {
                case "AddPatient":
                case "MyPatients":
                    SetActiveNavItem(NavPatients);
                    ShowView("Patients");
                    break;
                case "Import":
                    AppDialog.Show(Strings.ScreenerDashboardWindow_ImportNotImplemented, Strings.ScreenerDashboardWindow_ActionImport,
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                case "Export":
                    AppDialog.Show(Strings.ScreenerDashboardWindow_ExportNotImplemented, Strings.ScreenerDashboardWindow_ActionExport,
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                case "Reports":
                    AppDialog.Show(Strings.ScreenerDashboardWindow_ReportsNotImplemented, Strings.ScreenerDashboardWindow_ActionReports,
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
            }
        }
    }
}