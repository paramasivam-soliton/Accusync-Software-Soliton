// --------------------------------------------------------------------------------
// <copyright file="AdditionalInfoTab.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Application.Helpers;
using AccuSync.Application.Models;
using AccuSync.Presentation.Models;
using AccuSync.Presentation.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace AccuSync.WPF.Views.PatientsTests.Tabs
{
    /// <summary>
    /// Code-behind for the additional info tab (Tab 3). Handles phone number
    /// live formatting, SSN formatting, name capitalization, clear-field
    /// handlers, and exposes section borders for jump-to navigation.
    /// </summary>
    public partial class AdditionalInfoTab : UserControl
    {
        private bool _isUpdatingPhone = false;
        private List<string> _recentHospitalIds;
        private List<CountryDialCode> _countries;

        // Exposed so the shell can scroll to a specific section via jump-to nav

        /// <summary>The Mother section border, exposed for jump-to navigation.</summary>
        public FrameworkElement MotherSection => MotherSectionBorder;

        /// <summary>The Caregiver section border, exposed for jump-to navigation.</summary>
        public FrameworkElement CaregiverSection => CaregiverSectionBorder;

        /// <summary>The Referral section border, exposed for jump-to navigation.</summary>
        public FrameworkElement ReferralSection => ReferralSectionBorder;

        /// <summary>The Medical section border, exposed for jump-to navigation.</summary>
        public FrameworkElement MedicalSection => MedicalSectionBorder;

        /// <summary>
        /// Initializes the control.
        /// </summary>
        public AdditionalInfoTab()
        {
            InitializeComponent();
        }

        // Called by the shell on LoadPatient / NewPatient to populate
        // all five dial-code ComboBoxes with the country list.

        /// <summary>
        /// Populates the mother/caregiver/referral phone dial-code combo boxes with the country list.
        /// </summary>
        public void InitializeDialCodes()
        {
            // TODO: Hardcoded hospital IDs — this looks like test data that
            //       was never wired up or replaced with a real data source.
            _recentHospitalIds = new List<string>
            {
                "HSP-2024-001", "HSP-2024-015", "HSP-2023-892",
                "Memorial-456", "St-Mary-123"
            };

            _countries = CountryDialCode.GetCountries();

            MotherPhoneDialCodeComboBox.ItemsSource = _countries;
            MotherMobilePhoneDialCodeComboBox.ItemsSource = _countries;
            CaregiverPhoneDialCodeComboBox.ItemsSource = _countries;
            CaregiverMobilePhoneDialCodeComboBox.ItemsSource = _countries;
            ReferralPhoneDialCodeComboBox.ItemsSource = _countries;
        }

        // Phone formatting — shared handler for all five phone TextBoxes.
        // Uses _isUpdatingPhone to prevent re-entrant formatting when we
        // programmatically set textBox.Text.

        // TODO: The five if/else branches are identical except for the property
        //       names (MotherPhoneNumber, CaregiverPhoneNumber, etc.). This is
        //       the same duplication pattern flagged in PatientViewModel's phone
        //       properties. A PhoneFieldViewModel would eliminate both.
        private void PhoneTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingPhone) return;

            var textBox = sender as TextBox;
            if (textBox == null) return;

            var vm = DataContext as PatientViewModel;
            if (vm == null) return;

            _isUpdatingPhone = true;

            try
            {
                int cursorPosition = textBox.SelectionStart;
                string oldText = textBox.Text;
                string digits = PhoneNumberFormatter.ExtractDigits(textBox.Text);

                string dialCode = "+1";

                if (textBox.Name == "MotherPhoneTextBox")
                {
                    dialCode = vm.MotherPhoneDialCode;
                    int max = PhoneNumberFormatter.GetMaxDigits(dialCode);
                    if (digits.Length > max) digits = digits.Substring(0, max);
                    if (vm.MotherPhoneNumber != digits) vm.MotherPhoneNumber = digits;
                }
                else if (textBox.Name == "MotherMobilePhoneTextBox")
                {
                    dialCode = vm.MotherMobilePhoneDialCode;
                    int max = PhoneNumberFormatter.GetMaxDigits(dialCode);
                    if (digits.Length > max) digits = digits.Substring(0, max);
                    if (vm.MotherMobilePhoneNumber != digits) vm.MotherMobilePhoneNumber = digits;
                }
                else if (textBox.Name == "CaregiverPhoneTextBox")
                {
                    dialCode = vm.CaregiverPhoneDialCode;
                    int max = PhoneNumberFormatter.GetMaxDigits(dialCode);
                    if (digits.Length > max) digits = digits.Substring(0, max);
                    if (vm.CaregiverPhoneNumber != digits) vm.CaregiverPhoneNumber = digits;
                }
                else if (textBox.Name == "CaregiverMobilePhoneTextBox")
                {
                    dialCode = vm.CaregiverMobilePhoneDialCode;
                    int max = PhoneNumberFormatter.GetMaxDigits(dialCode);
                    if (digits.Length > max) digits = digits.Substring(0, max);
                    if (vm.CaregiverMobilePhoneNumber != digits) vm.CaregiverMobilePhoneNumber = digits;
                }
                else if (textBox.Name == "ReferralPhoneTextBox")
                {
                    dialCode = vm.ReferralPhoneDialCode;
                    int max = PhoneNumberFormatter.GetMaxDigits(dialCode);
                    if (digits.Length > max) digits = digits.Substring(0, max);
                    if (vm.ReferralPhoneNumber != digits) vm.ReferralPhoneNumber = digits;
                }
                else { return; }

                string formatted = PhoneNumberFormatter.Format(digits, dialCode);

                if (textBox.Text != formatted)
                {
                    textBox.Text = formatted;
                    int newPos = cursorPosition + (formatted.Length - oldText.Length);
                    textBox.SelectionStart = Math.Max(0, Math.Min(newPos, formatted.Length));
                }
            }
            finally
            {
                _isUpdatingPhone = false;
            }
        }

        // SSN formatting — same unsubscribe/resubscribe pattern as NameTextBox

        private void SSNTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            int cursorPosition = textBox.SelectionStart;
            string oldText = textBox.Text;

            string digits = SSNFormatter.ExtractDigits(textBox.Text);
            if (digits.Length > 9) digits = digits.Substring(0, 9);
            string formatted = SSNFormatter.Format(digits);

            if (textBox.Text != formatted)
            {
                textBox.TextChanged -= SSNTextBox_TextChanged;
                textBox.Text = formatted;
                textBox.TextChanged += SSNTextBox_TextChanged;

                int newPos = cursorPosition + (formatted.Length - oldText.Length);
                textBox.SelectionStart = Math.Max(0, Math.Min(newPos, formatted.Length));
            }
        }

        // Name capitalization — same handler as PatientDetailsTab

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

        // Clear field handlers — Mother

        private void ClearMotherTitle_Click(object sender, RoutedEventArgs e)
        { if (DataContext is PatientViewModel vm) vm.MotherTitle = string.Empty; }

        private void ClearMotherSSN_Click(object sender, RoutedEventArgs e)
        { if (DataContext is PatientViewModel vm) vm.MotherSSN = string.Empty; }

        private void ClearMotherId_Click(object sender, RoutedEventArgs e)
        { if (DataContext is PatientViewModel vm) vm.MotherId = string.Empty; }

        private void ClearMotherFirstName_Click(object sender, RoutedEventArgs e)
        { if (DataContext is PatientViewModel vm) vm.MotherFirstName = string.Empty; }

        private void ClearMotherLastName_Click(object sender, RoutedEventArgs e)
        { if (DataContext is PatientViewModel vm) vm.MotherLastName = string.Empty; }

        private void ClearMotherAddress1_Click(object sender, RoutedEventArgs e)
        { if (DataContext is PatientViewModel vm) vm.MotherAddress1 = string.Empty; }

        private void ClearMotherAddress2_Click(object sender, RoutedEventArgs e)
        { if (DataContext is PatientViewModel vm) vm.MotherAddress2 = string.Empty; }

        private void ClearMotherCity_Click(object sender, RoutedEventArgs e)
        { if (DataContext is PatientViewModel vm) vm.MotherCity = string.Empty; }

        private void ClearMotherState_Click(object sender, RoutedEventArgs e)
        { if (DataContext is PatientViewModel vm) vm.MotherState = string.Empty; }

        private void ClearMotherZipCode_Click(object sender, RoutedEventArgs e)
        { if (DataContext is PatientViewModel vm) vm.MotherZipCode = string.Empty; }

        // Phone clear handlers must set the guard flag to prevent
        // PhoneTextBox_TextChanged from re-formatting during the clear.
        private void ClearMotherPhone_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is PatientViewModel vm)
            {
                _isUpdatingPhone = true;
                vm.MotherPhoneNumber = string.Empty;
                MotherPhoneTextBox.Text = string.Empty;
                _isUpdatingPhone = false;
            }
        }

        private void ClearMotherMobilePhone_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is PatientViewModel vm)
            {
                _isUpdatingPhone = true;
                vm.MotherMobilePhoneNumber = string.Empty;
                MotherMobilePhoneTextBox.Text = string.Empty;
                _isUpdatingPhone = false;
            }
        }

        private void ClearMotherFax_Click(object sender, RoutedEventArgs e)
        { if (DataContext is PatientViewModel vm) vm.MotherFax = string.Empty; }

        private void ClearMotherEmail_Click(object sender, RoutedEventArgs e)
        { if (DataContext is PatientViewModel vm) vm.MotherEmail = string.Empty; }

        // Clear field handlers — Caregiver

        private void ClearCaregiverTitle_Click(object sender, RoutedEventArgs e)
        { if (DataContext is PatientViewModel vm) vm.CaregiverTitle = string.Empty; }

        private void ClearCaregiverSSN_Click(object sender, RoutedEventArgs e)
        { if (DataContext is PatientViewModel vm) vm.CaregiverSSN = string.Empty; }

        private void ClearCaregiverFirstName_Click(object sender, RoutedEventArgs e)
        { if (DataContext is PatientViewModel vm) vm.CaregiverFirstName = string.Empty; }

        private void ClearCaregiverLastName_Click(object sender, RoutedEventArgs e)
        { if (DataContext is PatientViewModel vm) vm.CaregiverLastName = string.Empty; }

        private void ClearCaregiverAddress1_Click(object sender, RoutedEventArgs e)
        { if (DataContext is PatientViewModel vm) vm.CaregiverAddress1 = string.Empty; }

        private void ClearCaregiverAddress2_Click(object sender, RoutedEventArgs e)
        { if (DataContext is PatientViewModel vm) vm.CaregiverAddress2 = string.Empty; }

        private void ClearCaregiverCity_Click(object sender, RoutedEventArgs e)
        { if (DataContext is PatientViewModel vm) vm.CaregiverCity = string.Empty; }

        private void ClearCaregiverState_Click(object sender, RoutedEventArgs e)
        { if (DataContext is PatientViewModel vm) vm.CaregiverState = string.Empty; }

        private void ClearCaregiverZipCode_Click(object sender, RoutedEventArgs e)
        { if (DataContext is PatientViewModel vm) vm.CaregiverZipCode = string.Empty; }

        private void ClearCaregiverPhone_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is PatientViewModel vm)
            {
                _isUpdatingPhone = true;
                vm.CaregiverPhoneNumber = string.Empty;
                CaregiverPhoneTextBox.Text = string.Empty;
                _isUpdatingPhone = false;
            }
        }

        private void ClearCaregiverMobilePhone_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is PatientViewModel vm)
            {
                _isUpdatingPhone = true;
                vm.CaregiverMobilePhoneNumber = string.Empty;
                CaregiverMobilePhoneTextBox.Text = string.Empty;
                _isUpdatingPhone = false;
            }
        }

        private void ClearCaregiverFax_Click(object sender, RoutedEventArgs e)
        { if (DataContext is PatientViewModel vm) vm.CaregiverFax = string.Empty; }

        private void ClearCaregiverEmail_Click(object sender, RoutedEventArgs e)
        { if (DataContext is PatientViewModel vm) vm.CaregiverEmail = string.Empty; }

        // Clear field handlers — Referral

        private void ClearAudiologyReferral_Click(object sender, RoutedEventArgs e)
        { if (DataContext is PatientViewModel vm) vm.AudiologyReferral = string.Empty; }

        private void ClearReferralTo_Click(object sender, RoutedEventArgs e)
        { if (DataContext is PatientViewModel vm) vm.ReferralTo = string.Empty; }

        private void ClearReferralFrom_Click(object sender, RoutedEventArgs e)
        { if (DataContext is PatientViewModel vm) vm.ReferralFrom = string.Empty; }

        private void ClearReferralPhone_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is PatientViewModel vm)
            {
                _isUpdatingPhone = true;
                vm.ReferralPhoneNumber = string.Empty;
                ReferralPhoneTextBox.Text = string.Empty;
                _isUpdatingPhone = false;
            }
        }

        // Clear field handlers — Medical

        private void ClearMedication_Click(object sender, RoutedEventArgs e)
        { if (DataContext is PatientViewModel vm) vm.Medication = string.Empty; }

        private void ClearPhysician_Click(object sender, RoutedEventArgs e)
        { if (DataContext is PatientViewModel vm) vm.Physician = string.Empty; }

        private void ClearAudiologist_Click(object sender, RoutedEventArgs e)
        { if (DataContext is PatientViewModel vm) vm.Audiologist = string.Empty; }

        // Today button

        private void SetTodayReferralDate_Click(object sender, RoutedEventArgs e)
        { if (DataContext is PatientViewModel vm) vm.ReferralDate = DateTime.Today; }
    }
}
