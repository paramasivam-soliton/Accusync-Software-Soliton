// --------------------------------------------------------------------------------
// <copyright file="FirmwareUpdateDialog.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using AccuSync.WPF.Resources;
using AccuSync.WPF.Controls;

namespace AccuSync.WPF.Views.DeviceManagement.Dialogs
{
    /// <summary>
    /// Code-behind for the firmware update dialog. Shows device info,
    /// lets the user browse for a firmware folder, and returns
    /// <see cref="SelectedFolderPath"/> and <see cref="FirmwareFileName"/>
    /// via <c>DialogResult = true</c>.
    /// </summary>
    public partial class FirmwareUpdateDialog : Window
    {
        // Set by the caller after construction, before ShowDialog()
        public string DeviceName { get; set; }
        public string CurrentFirmware { get; set; }
        public string HardwareVersion { get; set; }

        // Populated by BrowseButton_Click, read by the caller after dialog closes
        public string SelectedFolderPath { get; private set; }
        public string FirmwareFileName { get; private set; }
        public bool HasFolder => !string.IsNullOrEmpty(SelectedFolderPath);

        public FirmwareUpdateDialog()
        {
            InitializeComponent();
            Loaded += (s, e) => PopulateDeviceInfo();
        }

        private void PopulateDeviceInfo()
        {
            DeviceNameText.Text = DeviceName ?? "—";
            CurrentFirmwareText.Text = CurrentFirmware ?? "—";
            HardwareVersionText.Text = HardwareVersion ?? "—";
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

        // Browse

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = Strings.FirmwareUpdateDialog_SelectFolderDescription
            };

            if (dialog.ShowDialog() == true)
            {
                SelectedFolderPath = dialog.FolderName;
                FolderPathTextBox.Text = SelectedFolderPath;
                FolderPlaceholder.Visibility = Visibility.Collapsed;

                FolderPathTextBox.BorderBrush =
                    (System.Windows.Media.SolidColorBrush)FindResource("FilledBorderBrush");

                // DetectFirmwareFile drives UpdateButton.IsEnabled — the button stays
                // disabled if the chosen folder has no recognizable firmware file.
                DetectFirmwareFile(SelectedFolderPath);
            }
        }

        /// <summary>
        /// Scans the selected folder for the first file matching a known firmware
        /// extension. Updates the metadata panel and enables the Update button only
        /// when a firmware file is found.
        /// </summary>
        private void DetectFirmwareFile(string folderPath)
        {
            try
            {
                string[] fwExtensions = { ".afw", ".bin", ".pkg", ".hex", ".fw" };
                var dir = new DirectoryInfo(folderPath);
                var fwFile = dir.GetFiles()
                    .FirstOrDefault(f => fwExtensions.Contains(f.Extension.ToLowerInvariant()));

                if (fwFile != null)
                {
                    FirmwareFileName = fwFile.Name;
                    FileNameText.Text = fwFile.Name;
                    FileSizeText.Text = FormatFileSize(fwFile.Length);
                    FileMetaPanel.Visibility = Visibility.Visible;
                    NoFirmwareText.Visibility = Visibility.Collapsed;
                    UpdateButton.IsEnabled = true;
                    return;
                }

                FirmwareFileName = null;
                FileMetaPanel.Visibility = Visibility.Collapsed;
                NoFirmwareText.Visibility = Visibility.Visible;
                UpdateButton.IsEnabled = false;
            }
            catch
            {
                FirmwareFileName = null;
                FileMetaPanel.Visibility = Visibility.Collapsed;
                NoFirmwareText.Visibility = Visibility.Visible;
                UpdateButton.IsEnabled = false;
            }
        }

        private static string FormatFileSize(long bytes)
        {
            if (bytes >= 1_048_576)
                return $"{bytes / 1_048_576.0:F1} MB";
            if (bytes >= 1024)
                return $"{bytes / 1024.0:F1} KB";
            return $"{bytes} B";
        }

        // Footer buttons

        private void CloseButton_Click(object sender, RoutedEventArgs e)
            => DialogResult = false;

        private void CancelButton_Click(object sender, RoutedEventArgs e)
            => DialogResult = false;

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!HasFolder || string.IsNullOrEmpty(FirmwareFileName))
            {
                AppDialog.Show(Strings.FirmwareUpdateDialog_SelectFolderMessage,
                    Strings.FirmwareUpdateDialog_FirmwareFileRequired, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
        }
    }
}