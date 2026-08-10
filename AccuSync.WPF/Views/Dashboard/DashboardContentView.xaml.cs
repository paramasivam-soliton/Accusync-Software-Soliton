// --------------------------------------------------------------------------------
// <copyright file="DashboardContentView.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AccuSync.Models;
using AccuSync.WPF.Resources;
using AccuSync.WPF.Views.Dashboard.Dialogs;
using AccuSync.WPF.Controls;

namespace AccuSync.WPF.Views.Dashboard
{
    public partial class DashboardContentView : UserControl
    {
        public event Action<string> NavigateRequested;

        private string _username = "Admin";

        // Hover brushes shared by all stat and action cards
        private static readonly SolidColorBrush _cardDefaultBg = Brushes.White;
        private static readonly SolidColorBrush _cardDefaultBorder = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0"));
        private static readonly SolidColorBrush _cardHoverBg = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f0f7ff"));
        private static readonly SolidColorBrush _cardHoverBorder = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#428FEC"));

        public DashboardContentView()
        {
            InitializeComponent();
            PopulateCards();
            Loaded += (s, e) => UpdateGreeting();
        }

        // Overview stat cards and quick-action cards are generated from these lists
        // via ItemsControl templates (see DashboardContentView.xaml). The hardcoded
        // values remain placeholders until wired to real queries.
        private void PopulateCards()
        {
            StatCardsItems.ItemsSource = new List<DashboardCardInfo>
            {
                new DashboardCardInfo { Tag = "Referred", IconBackground = "#428FEC", Label = "REFERRED (7D)", Value = "4", CardMargin = new Thickness(0, 0, 6, 12),
                    IconData = "M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm0 18c-4.41 0-8-3.59-8-8s3.59-8 8-8 8 3.59 8 8-3.59 8-8 8zm-1-13h2v6h-2zm0 8h2v2h-2z" },
                new DashboardCardInfo { Tag = "Pass", IconBackground = "#00AAA7", Label = "PASS (7D)", Value = "4", CardMargin = new Thickness(6, 0, 6, 12),
                    IconData = "M9 16.17L4.83 12l-1.42 1.41L9 19 21 7l-1.41-1.41z" },
                new DashboardCardInfo { Tag = "Incomplete", IconBackground = "#005FBE", Label = "INCOMPLETE (7D)", Value = "3", CardMargin = new Thickness(6, 0, 0, 12),
                    IconData = "M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-2h2v2zm0-4h-2V7h2v6z" },
                new DashboardCardInfo { Tag = "ScreeningsToday", IconBackground = "#002856", Label = "SCREENINGS TODAY", Value = "12", CardMargin = new Thickness(0, 0, 6, 0),
                    IconData = "M19 3h-4.18C14.4 1.84 13.3 1 12 1c-1.3 0-2.4.84-2.82 2H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm-7 0c.55 0 1 .45 1 1s-.45 1-1 1-1-.45-1-1 .45-1 1-1zm2 14H7v-2h7v2zm3-4H7v-2h10v2zm0-4H7V7h10v2z" },
                new DashboardCardInfo { Tag = "NotExported", IconBackground = "#9EA2AC", Label = "NOT EXPORTED", Value = "3", CardMargin = new Thickness(6, 0, 6, 0),
                    IconData = "M9 16h6v-6h4l-7-7-7 7h4zm-4 2h14v2H5z" },
                new DashboardCardInfo { Tag = "DevicesUpdate", IconBackground = "#008B96", Label = "DEVICES UPDATE", Value = "2", CardMargin = new Thickness(6, 0, 0, 0),
                    IconData = "M17 1.01L7 1c-1.1 0-2 .9-2 2v18c0 1.1.9 2 2 2h10c1.1 0 2-.9 2-2V3c0-1.1-.9-1.99-2-1.99zM17 19H7V5h10v14z" },
            };

            QuickActionItems.ItemsSource = new List<DashboardCardInfo>
            {
                new DashboardCardInfo { Tag = "AddPatient", IconBackground = "#00AAA7", Label = "Add Patient", CardMargin = new Thickness(0, 0, 6, 12),
                    IconData = "M19 13h-6v6h-2v-6H5v-2h6V5h2v6h6v2z" },
                new DashboardCardInfo { Tag = "Import", IconBackground = "#428FEC", Label = "Import", CardMargin = new Thickness(6, 0, 6, 12),
                    IconData = "M19 9h-4V3H9v6H5l7 7 7-7zM5 18v2h14v-2H5z" },
                new DashboardCardInfo { Tag = "Export", IconBackground = "#9EA2AC", Label = "Export", CardMargin = new Thickness(6, 0, 0, 12),
                    IconData = "M9 16h6v-6h4l-7-7-7 7h4zm-4 2h14v2H5z" },
                new DashboardCardInfo { Tag = "Reports", IconBackground = "#005FBE", Label = "Reports", CardMargin = new Thickness(0, 0, 6, 0),
                    IconData = "M14 2H6c-1.1 0-1.99.9-1.99 2L4 20c0 1.1.89 2 1.99 2H18c1.1 0 2-.9 2-2V8l-6-6zm2 16H8v-2h8v2zm0-4H8v-2h8v2zm-3-5V3.5L18.5 9H13z" },
                new DashboardCardInfo { Tag = "Upload", IconBackground = "#008B96", Label = "Upload", CardMargin = new Thickness(6, 0, 6, 0),
                    IconData = "M17 1.01L7 1c-1.1 0-2 .9-2 2v18c0 1.1.9 2 2 2h10c1.1 0 2-.9 2-2V3c0-1.1-.9-1.99-2-1.99zM17 19H7V5h10v14zm-5.5-8l-3 3h2V17h2v-3h2z" },
            };
        }

        // Public API

        /// <summary>
        /// Sets the displayed username in the greeting bar.
        /// Call from SidebarNavigation.SetCurrentUser().
        /// </summary>
        public void SetUsername(string username)
        {
            _username = username ?? "Admin";
            UpdateGreeting();
        }

        // Greeting Logic

        private void UpdateGreeting()
        {
            var hour = DateTime.Now.Hour;
            string emoji, greeting;

            if (hour < 12)
            {
                emoji = "☀️";
                greeting = Strings.DashboardContentView_GreetingMorning;
            }
            else if (hour < 18)
            {
                emoji = "🌤️";
                greeting = Strings.DashboardContentView_GreetingAfternoon;
            }
            else
            {
                emoji = "🌙";
                greeting = Strings.DashboardContentView_GreetingEvening;
            }

            GreetingText.Text = $"{emoji} {greeting}, {_username}";
            DateText.Text = DateTime.Now.ToString("MMM d, yyyy");
        }

        // Stat Card Clicks

        private void StatCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is FrameworkElement fe) || !(fe.Tag is string tag))
                return;

            switch (tag)
            {
                case "Referred":
                case "Pass":
                case "Incomplete":
                    PatientListDialog.PatientListType listType;
                    if (tag == "Referred") listType = PatientListDialog.PatientListType.Referred;
                    else if (tag == "Pass") listType = PatientListDialog.PatientListType.Pass;
                    else listType = PatientListDialog.PatientListType.Incomplete;

                    var patientDialog = new PatientListDialog(listType);
                    patientDialog.Owner = Window.GetWindow(this);
                    patientDialog.ShowDialog();
                    break;

                case "ScreeningsToday":
                    var screenerDialog = new ScreenerListDialog();
                    screenerDialog.Owner = Window.GetWindow(this);
                    screenerDialog.ShowDialog();
                    break;

                case "NotExported":
                    var neDialog = new NotExportedDialog();
                    neDialog.Owner = Window.GetWindow(this);
                    neDialog.ShowDialog();
                    break;

                case "DevicesUpdate":
                    var duDialog = new DevicesUpdateDialog();
                    duDialog.Owner = Window.GetWindow(this);
                    duDialog.ShowDialog();
                    break;
            }
        }

        // Quick Action Card Clicks

        // TODO: AddPatient, Import, and Export all navigate to "Patients" with no differentiation.
        //       The parent needs to know which action to trigger after navigation.
        private void QuickAction_Click(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is FrameworkElement fe) || !(fe.Tag is string action))
                return;

            switch (action)
            {
                case "AddPatient":
                    NavigateRequested?.Invoke("Patients");
                    break;
                case "Import":
                    NavigateRequested?.Invoke("Patients");
                    break;
                case "Export":
                    NavigateRequested?.Invoke("Patients");
                    break;
                case "Reports":
                    AppDialog.Show(Strings.DashboardContentView_ReportsNotImplemented,
                        Strings.DashboardContentView_CaptionReports, MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                case "Upload":
                    AppDialog.Show(Strings.DashboardContentView_UploadNotImplemented,
                        Strings.DashboardContentView_CaptionUpload, MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
            }
        }

        // Card Hover Effects

        // NOTE: Imperative brush swap — same pattern as ExportDialog/PrintDialog.
        // Works fine here since all cards share the same hover colors.
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
    }
}