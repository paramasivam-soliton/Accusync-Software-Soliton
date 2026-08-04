// --------------------------------------------------------------------------------
// <copyright file="ScreenerDetailsDialog.xaml.cs" company="Natus Sensory">
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

namespace AccuSync.WPF.Views.Dashboard.Dialogs
{
    public partial class ScreenerDetailsDialog : Window
    {
        private string _screenerName;

        public ScreenerDetailsDialog(string screenerName, SolidColorBrush screenerColor)
        {
            InitializeComponent();
            _screenerName = screenerName;

            // Set screener info
            ScreenerName.Text = screenerName;
            DataContext = new { ScreenerColor = screenerColor, ScreenerInitials = GetInitials(screenerName) };

            LoadPatientData();
        }

        private string GetInitials(string name)
        {
            var parts = name.Split(' ');
            if (parts.Length >= 2)
            {
                return $"{parts[0][0]}{parts[1][0]}";
            }
            return parts.Length > 0 && parts[0].Length > 0 ? parts[0].Substring(0, Math.Min(2, parts[0].Length)) : "?";
        }

        private void LoadPatientData()
        {
            var patients = GetAssignedPatients();
            PatientListItems.ItemsSource = patients;

            // Update counts
            AssignedCountText.Text = $"{patients.Count} screening{(patients.Count != 1 ? "s" : "")} assigned today";

            var passCount = patients.Count(p => p.ResultText == "Pass");
            var referCount = patients.Count(p => p.ResultText == "Refer");

            PassCountText.Text = string.Format(Strings.ScreenerDetailsDialog_PassCountFormat, passCount);
            ReferCountText.Text = string.Format(Strings.ScreenerDetailsDialog_ReferCountFormat, referCount);
        }

        private List<AssignedPatientInfo> GetAssignedPatients()
        {
            // Dummy data - in production, query by screener name
            return new List<AssignedPatientInfo>
            {
                new AssignedPatientInfo
                {
                    MRN = "P-1201",
                    FirstName = "Harper",
                    LastName = "Clark",
                    DOB = new DateTime(2024, 10, 14),
                    ResultText = "Pass",
                    ResultBackground = "#D1FAE5",
                    ResultForeground = "#065F46",
                    AccentColor = "#009B77",
                    CompletionTime = "08:45 AM"
                },
                new AssignedPatientInfo
                {
                    MRN = "P-1203",
                    FirstName = "Evelyn",
                    LastName = "Lewis",
                    DOB = new DateTime(2024, 10, 13),
                    ResultText = "Refer",
                    ResultBackground = "#FED7AA",
                    ResultForeground = "#9A3412",
                    AccentColor = "#F59E0B",
                    CompletionTime = "09:20 AM"
                },
                new AssignedPatientInfo
                {
                    MRN = "P-1209",
                    FirstName = "Henry",
                    LastName = "Walker",
                    DOB = new DateTime(2024, 10, 11),
                    ResultText = "Pass",
                    ResultBackground = "#D1FAE5",
                    ResultForeground = "#065F46",
                    AccentColor = "#009B77",
                    CompletionTime = "10:15 AM"
                },
                new AssignedPatientInfo
                {
                    MRN = "P-1211",
                    FirstName = "Luna",
                    LastName = "Young",
                    DOB = new DateTime(2024, 10, 9),
                    ResultText = "Refer",
                    ResultBackground = "#FED7AA",
                    ResultForeground = "#9A3412",
                    AccentColor = "#F59E0B",
                    CompletionTime = "11:05 AM"
                },
                new AssignedPatientInfo
                {
                    MRN = "P-1307",
                    FirstName = "Benjamin",
                    LastName = "Adams",
                    DOB = new DateTime(2024, 10, 12),
                    ResultText = "Incomplete",
                    ResultBackground = "#DBEAFE",
                    ResultForeground = "#0369A1",
                    AccentColor = "#0066A1",
                    CompletionTime = "11:45 AM"
                }
            };
        }

        #region Card Hover Animations

        private void PatientCard_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border card)
            {
                // Show accent strip
                var accentStrip = FindVisualChild<Rectangle>(card, "AccentStrip");
                if (accentStrip != null)
                {
                    var fadeIn = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(200));
                    accentStrip.BeginAnimation(UIElement.OpacityProperty, fadeIn);
                }
            }
        }

        private void PatientCard_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border card)
            {
                // Hide accent strip
                var accentStrip = FindVisualChild<Rectangle>(card, "AccentStrip");
                if (accentStrip != null)
                {
                    var fadeOut = new DoubleAnimation(0.0, TimeSpan.FromMilliseconds(200));
                    accentStrip.BeginAnimation(UIElement.OpacityProperty, fadeOut);
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

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}