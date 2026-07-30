// --------------------------------------------------------------------------------
// <copyright file="PatientDetailsTab.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Application.Helpers;
using AccuSync.WPF.Resources;
using AccuSync.Presentation.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using AccuSync.WPF.Controls;

namespace AccuSync.WPF.Views.PatientsTests.Tabs
{
    /// <summary>
    /// Code-behind for the patient details tab. Handles view-layer behaviors
    /// that don't belong in the ViewModel: forced validation on save,
    /// live name capitalization, character counting, and clipboard operations.
    /// </summary>
    public partial class PatientDetailsTab : UserControl
    {
        public PatientDetailsTab()
        {
            InitializeComponent();
        }

        // Validation — called by the shell before saving to force WPF to
        // evaluate bindings on required fields the user may not have touched.

        /// <summary>
        /// Triggers binding validation on required fields that are still empty.
        /// WPF only validates a field when its binding updates, so untouched
        /// fields won't show errors until we force <c>UpdateSource</c>.
        /// </summary>
        public void ForceRequiredFieldValidation(PatientViewModel viewModel)
        {
            BindingExpression be;

            if (string.IsNullOrWhiteSpace(viewModel.PatientId))
            {
                be = PatientIdTextBox.GetBindingExpression(TextBox.TextProperty);
                be?.UpdateSource();
            }

            if (string.IsNullOrWhiteSpace(viewModel.HospitalId))
            {
                be = HospitalIdTextBox.GetBindingExpression(TextBox.TextProperty);
                be?.UpdateSource();
            }

            if (string.IsNullOrWhiteSpace(viewModel.LastName))
            {
                be = LastNameTextBox.GetBindingExpression(TextBox.TextProperty);
                be?.UpdateSource();
            }
        }

        // Name capitalization — applied on every keystroke via TextChanged.
        // Temporarily unsubscribes to avoid re-triggering itself when setting
        // the capitalized text.

        private void NameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            int cursorPosition = textBox.SelectionStart;
            string capitalized = NameFormatter.Capitalize(textBox.Text);

            if (textBox.Text != capitalized)
            {
                textBox.TextChanged -= NameTextBox_TextChanged;
                textBox.Text = capitalized;
                textBox.TextChanged += NameTextBox_TextChanged;
                textBox.SelectionStart = cursorPosition;
            }
        }

        // Character counter — color shifts to warn as the user approaches MaxLength

        private void CommentsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && CommentsCharCounter != null)
            {
                int currentLength = textBox.Text?.Length ?? 0;
                int maxLength = textBox.MaxLength;
                CommentsCharCounter.Text = string.Format(Strings.PatientDetailsTab_CharCounterFormat, currentLength, maxLength);

                if (currentLength > maxLength * 0.9)
                    CommentsCharCounter.Foreground = new SolidColorBrush(Colors.Red);
                else if (currentLength > maxLength * 0.75)
                    CommentsCharCounter.Foreground = new SolidColorBrush(Colors.Orange);
                else
                    CommentsCharCounter.Foreground = (SolidColorBrush)FindResource("GrayBrush");
            }
        }

        // Clipboard — try/catch because Clipboard.SetText can throw if
        // another process has the clipboard locked.

        private void CopyPatientId_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as PatientViewModel;
            if (vm != null && !string.IsNullOrWhiteSpace(vm.PatientId))
            {
                try { Clipboard.SetText(vm.PatientId); }
                catch (Exception ex) { AppDialog.Show(string.Format(Strings.PatientDetailsTab_ErrorCopying, ex.Message)); }
            }
        }

        private void CopyHospitalId_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as PatientViewModel;
            if (vm != null && !string.IsNullOrWhiteSpace(vm.HospitalId))
            {
                try { Clipboard.SetText(vm.HospitalId); }
                catch (Exception ex) { AppDialog.Show(string.Format(Strings.PatientDetailsTab_ErrorCopying, ex.Message)); }
            }
        }

        // Clear field handlers

        private void ClearPatientId_Click(object sender, RoutedEventArgs e)
        { if (DataContext is PatientViewModel vm) vm.PatientId = string.Empty; }

        private void ClearHospitalId_Click(object sender, RoutedEventArgs e)
        { if (DataContext is PatientViewModel vm) vm.HospitalId = string.Empty; }

        private void ClearFirstName_Click(object sender, RoutedEventArgs e)
        { if (DataContext is PatientViewModel vm) vm.FirstName = string.Empty; }

        private void ClearLastName_Click(object sender, RoutedEventArgs e)
        { if (DataContext is PatientViewModel vm) vm.LastName = string.Empty; }

        private void ClearWeight_Click(object sender, RoutedEventArgs e)
        { if (DataContext is PatientViewModel vm) vm.Weight = string.Empty; }

        private void ClearHeight_Click(object sender, RoutedEventArgs e)
        { if (DataContext is PatientViewModel vm) vm.Height = string.Empty; }

        // Today button handlers

        private void SetTodayDateOfBirth_Click(object sender, RoutedEventArgs e)
        { if (DataContext is PatientViewModel vm) vm.DateOfBirth = DateTime.Today; }

        private void SetTodayDischarged_Click(object sender, RoutedEventArgs e)
        { if (DataContext is PatientViewModel vm) vm.Discharged = DateTime.Today; }

        private void SetTodayDeceased_Click(object sender, RoutedEventArgs e)
        { if (DataContext is PatientViewModel vm) vm.Deceased = DateTime.Today; }
    }
}