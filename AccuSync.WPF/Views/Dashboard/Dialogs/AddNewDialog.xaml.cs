// --------------------------------------------------------------------------------
// <copyright file="AddNewDialog.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Windows;
using System.Windows.Input;

namespace AccuSync.WPF.Views.Dashboard.Dialogs
{
    public partial class AddNewDialog : Window
    {
        public string? SelectedOption { get; private set; }

        public AddNewDialog()
        {
            InitializeComponent();
        }

        // TODO: These four handlers are identical except for the string value.
        //       A single handler reading a Tag property from the sender would replace all four.
        private void PatientOption_Click(object sender, MouseButtonEventArgs e)
        {
            SelectedOption = "Patient";
            DialogResult = true;
            Close();
        }

        private void UserOption_Click(object sender, MouseButtonEventArgs e)
        {
            SelectedOption = "User";
            DialogResult = true;
            Close();
        }

        private void SiteOption_Click(object sender, MouseButtonEventArgs e)
        {
            SelectedOption = "Site";
            DialogResult = true;
            Close();
        }

        private void DeviceOption_Click(object sender, MouseButtonEventArgs e)
        {
            SelectedOption = "Device";
            DialogResult = true;
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}