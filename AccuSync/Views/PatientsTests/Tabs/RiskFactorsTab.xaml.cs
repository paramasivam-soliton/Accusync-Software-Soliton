// --------------------------------------------------------------------------------
// <copyright file="RiskFactorsTab.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Resources;
using AccuSync.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AccuSync.Views.PatientsTests.Tabs
{
    /// <summary>
    /// Code-behind for the risk factors tab. Manages the Yes/No/Unknown
    /// tri-state button groups for each of the 16 risk factors, the summary
    /// badge, and the "set all" bulk actions.
    /// </summary>
    // AllRiskFactorNames is the single source of truth for the risk factor set.
    // InitializeRiskButtons, SetValueForAllRisks, and SetRiskFactor_Click all drive
    // off it via reflection, so adding a 17th risk factor is a one-line change here.
    public partial class RiskFactorsTab : UserControl
    {
        private static readonly string[] AllRiskFactorNames = new[]
        {
            "FamilyHistory", "LowBirthWeight", "Hyperbilirubinemia", "Asphyxia",
            "CraniofacialAnomalies", "Syndromes", "InUteroInfections",
            "BacterialMeningitis", "PerinatalInfection", "OtotoxicMedications",
            "Aminoglycosides", "ProlongedVentilation", "ECMO", "NICUStay", "HeadTrauma",
            "CaregiverConcernRisk"
        };

        public RiskFactorsTab()
        {
            InitializeComponent();
        }

        // Called by the shell on LoadPatient, Revert, and Undo to sync
        // button visuals with the ViewModel's current values.

        public void InitializeRiskButtons()
        {
            var vm = DataContext as PatientViewModel;
            if (vm == null) return;

            foreach (var riskName in AllRiskFactorNames)
            {
                var value = typeof(PatientViewModel).GetProperty(riskName)?.GetValue(vm) as string;
                UpdateRiskFactorButtons(riskName, value);
            }

            UpdateRiskSummary();
        }

        // Called by the shell when PropertyChanged fires on any risk factor
        // property to refresh the summary badge and count text.

        public void UpdateRiskSummary()
        {
            var vm = DataContext as PatientViewModel;
            if (vm == null) return;

            int yesCount = vm.PerinatalYesCount + vm.PostnatalYesCount + vm.OtherYesCount;
            int totalAnswered = vm.GetPerinatalAnsweredCount()
                              + vm.GetPostnatalAnsweredCount()
                              + vm.GetOtherAnsweredCount();

            var summaryText = FindName("RiskSummaryText") as TextBlock;
            if (summaryText != null)
            {
                if (yesCount > 0)
                {
                    summaryText.Text = $"{yesCount} risk factor{(yesCount != 1 ? "s" : "")} present · {totalAnswered}/16 answered";
                    summaryText.Foreground = new SolidColorBrush(Color.FromRgb(236, 182, 35));
                }
                else
                {
                    summaryText.Text = string.Format(Strings.RiskFactorsTab_AnsweredCount, totalAnswered);
                    summaryText.Foreground = (SolidColorBrush)FindResource("GrayBrush");
                }
            }

            var yesBadge = FindName("RiskYesBadge") as Border;
            var yesBadgeText = FindName("RiskYesBadgeText") as TextBlock;
            if (yesBadge != null && yesBadgeText != null)
            {
                if (yesCount > 0)
                {
                    yesBadge.Visibility = Visibility.Visible;
                    yesBadgeText.Text = yesCount.ToString();
                }
                else
                {
                    yesBadge.Visibility = Visibility.Collapsed;
                }
            }
        }

        // Individual risk factor click — Tag format is "PropertyName|Value"
        // (e.g., "FamilyHistory|Yes"). Uses reflection to set the ViewModel
        // property, which is why AllRiskFactorNames isn't needed here.

        private void SetRiskFactor_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag == null) return;

            var vm = DataContext as PatientViewModel;
            if (vm == null) return;

            var parts = button.Tag.ToString().Split('|');
            if (parts.Length != 2) return;

            string propertyName = parts[0];
            string value = parts[1];

            var property = typeof(PatientViewModel).GetProperty(propertyName);
            if (property != null)
            {
                property.SetValue(vm, value);
                UpdateRiskFactorButtons(propertyName, value);
                UpdateRiskSummary();
            }
        }

        // Button styling — finds the three buttons by naming convention
        // (e.g., FamilyHistoryYes, FamilyHistoryNo, FamilyHistoryUnknown)
        // and highlights the selected one.

        private void UpdateRiskFactorButtons(string riskName, string selectedValue)
        {
            var yesBtn = FindName($"{riskName}Yes") as Button;
            var noBtn = FindName($"{riskName}No") as Button;
            var unknownBtn = FindName($"{riskName}Unknown") as Button;

            if (yesBtn == null || noBtn == null || unknownBtn == null) return;

            SetRiskButtonStyle(yesBtn, false, "Yes");
            SetRiskButtonStyle(noBtn, false, "No");
            SetRiskButtonStyle(unknownBtn, false, "Unknown");

            if (selectedValue == "Yes") SetRiskButtonStyle(yesBtn, true, "Yes");
            else if (selectedValue == "No") SetRiskButtonStyle(noBtn, true, "No");
            else if (selectedValue == "Unknown") SetRiskButtonStyle(unknownBtn, true, "Unknown");
        }

        // TODO: Colors are hardcoded — should reference shared brush resources.
        //       Yellow (#ECB623) = present, Green (#429C10) = absent,
        //       Gray (#6C757D) = unknown.
        private void SetRiskButtonStyle(Button button, bool isSelected, string buttonType)
        {
            if (button == null) return;

            if (isSelected)
            {
                switch (buttonType)
                {
                    case "Yes":
                        button.Background = new SolidColorBrush(Color.FromRgb(236, 182, 35));
                        button.Foreground = Brushes.White;
                        button.BorderBrush = new SolidColorBrush(Color.FromRgb(236, 182, 35));
                        break;
                    case "No":
                        button.Background = new SolidColorBrush(Color.FromRgb(66, 156, 16));
                        button.Foreground = Brushes.White;
                        button.BorderBrush = new SolidColorBrush(Color.FromRgb(66, 156, 16));
                        break;
                    case "Unknown":
                        button.Background = new SolidColorBrush(Color.FromRgb(108, 117, 125));
                        button.Foreground = Brushes.White;
                        button.BorderBrush = new SolidColorBrush(Color.FromRgb(108, 117, 125));
                        break;
                }
            }
            else
            {
                button.Background = Brushes.White;
                button.Foreground = new SolidColorBrush(Color.FromRgb(73, 80, 87));
                button.BorderBrush = new SolidColorBrush(Color.FromRgb(206, 212, 218));
            }
            button.BorderThickness = new Thickness(1);
        }

        // Bulk actions — set all 16 risk factors to the same value at once

        private void SetValueForAllRisks(PatientViewModel vm, string value)
        {
            foreach (var riskName in AllRiskFactorNames)
            {
                typeof(PatientViewModel).GetProperty(riskName)?.SetValue(vm, value);
                UpdateRiskFactorButtons(riskName, value);
            }

            UpdateRiskSummary();
        }

        private void AllRisksYes_Click(object sender, RoutedEventArgs e)
        { if (DataContext is PatientViewModel vm) SetValueForAllRisks(vm, "Yes"); }

        private void AllRisksNo_Click(object sender, RoutedEventArgs e)
        { if (DataContext is PatientViewModel vm) SetValueForAllRisks(vm, "No"); }

        private void AllRisksUnknown_Click(object sender, RoutedEventArgs e)
        { if (DataContext is PatientViewModel vm) SetValueForAllRisks(vm, "Unknown"); }
    }
}