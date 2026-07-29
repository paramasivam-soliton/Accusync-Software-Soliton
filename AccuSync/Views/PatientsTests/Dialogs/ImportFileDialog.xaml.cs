// --------------------------------------------------------------------------------
// <copyright file="ImportFileDialog.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Resources;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AccuSync.Views.PatientsTests.Dialogs
{
    /// <summary>
    /// Code-behind for the import file dialog. The user selects a format from
    /// a dropdown, then picks a file via browse or drag-and-drop. On success,
    /// <see cref="SelectedFormat"/> and <see cref="SelectedFilePath"/> are
    /// populated and <c>DialogResult</c> is set to <c>true</c>.
    /// </summary>
    public partial class ImportFileDialog : Window
    {
        private string _selectedFormat;   // ComboBoxItem.Tag: "accusync-xml", "algo5-xml", etc.
        private string _selectedFilePath;

        public string SelectedFormat => _selectedFormat;
        public string SelectedFilePath => _selectedFilePath;

        public ImportFileDialog()
        {
            InitializeComponent();
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

        private void CloseButton_Click(object sender, RoutedEventArgs e)
            => DialogResult = false;

        private void CancelButton_Click(object sender, RoutedEventArgs e)
            => DialogResult = false;

        // Format selection

        private void FormatComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FormatComboBox.SelectedItem is ComboBoxItem item)
                _selectedFormat = item.Tag?.ToString();

            ClearValidation();
        }

        // Dropzone — click to browse

        private void DropZone_Click(object sender, MouseButtonEventArgs e)
        {
            if (!ValidateFormatSelected()) return;
            BrowseForFile();
        }

        private void BrowseForFile()
        {
            var dlg = new OpenFileDialog { Filter = GetFileFilter() };

            if (dlg.ShowDialog() == true)
                AcceptFile(dlg.FileName);
        }

        // Dropzone — drag and drop

        private void DropZone_Drop(object sender, DragEventArgs e)
        {
            ResetDropZoneVisual();

            if (!ValidateFormatSelected()) return;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    if (!ValidateFileExtension(files[0]))
                    {
                        ShowValidation(Strings.ImportFileDialog_FileTypeMismatch);
                        return;
                    }
                    AcceptFile(files[0]);
                }
            }
        }

        private void DropZone_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) && !string.IsNullOrEmpty(_selectedFormat))
            {
                e.Effects = DragDropEffects.Copy;
                DropZone.BorderBrush = (SolidColorBrush)FindResource("DropzoneHoverBorderBrush");
                DropZone.Background = (SolidColorBrush)FindResource("DropzoneHoverBgBrush");
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void DropZone_DragLeave(object sender, DragEventArgs e)
        {
            ResetDropZoneVisual();
        }

        private void ResetDropZoneVisual()
        {
            DropZone.BorderBrush = (SolidColorBrush)FindResource("DropzoneBorderBrush");
            DropZone.Background = Brushes.Transparent;
        }

        // File acceptance + removal

        private void AcceptFile(string filePath)
        {
            _selectedFilePath = filePath;
            ClearValidation();

            var fi = new FileInfo(filePath);
            FileNameText.Text = fi.Name;
            FileMetaText.Text = string.Format(Strings.ImportFileDialog_FileMetaFormat, GetFormatLabel(), FormatFileSize(fi.Length));

            // Swap the dropzone for the file-selected indicator
            DropZone.Visibility = Visibility.Collapsed;
            FileSelectedPanel.Visibility = Visibility.Visible;
        }

        private void RemoveFile_Click(object sender, RoutedEventArgs e)
        {
            _selectedFilePath = null;

            FileSelectedPanel.Visibility = Visibility.Collapsed;
            DropZone.Visibility = Visibility.Visible;
        }

        // Next button

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateFormatSelected()) return;

            if (string.IsNullOrEmpty(_selectedFilePath))
            {
                ShowValidation(Strings.ImportFileDialog_SelectFileToImport);
                return;
            }

            if (!File.Exists(_selectedFilePath))
            {
                ShowValidation(Strings.ImportFileDialog_FileNoLongerExists);
                return;
            }

            DialogResult = true;
        }

        // Validation

        private bool ValidateFormatSelected()
        {
            if (string.IsNullOrEmpty(_selectedFormat))
            {
                ShowValidation(Strings.ImportFileDialog_SelectFormatFirst);
                return false;
            }
            return true;
        }

        // NOTE: Extension validation on drop only — the OpenFileDialog filter
        //       handles it for the browse path, so this is the drag-and-drop guard.
        private bool ValidateFileExtension(string filePath)
        {
            var ext = Path.GetExtension(filePath).ToLowerInvariant();

            return _selectedFormat switch
            {
                "accusync-xml" or "algo5-xml" or "acculink-xml" => ext == ".xml",
                "accusync-json" or "algopro-json" => ext == ".json",
                "facesheet-image" => ext == ".png" || ext == ".jpg" || ext == ".jpeg",
                "facesheet-pdf" => ext == ".pdf",
                "facesheet-docx" => ext == ".docx",
                _ => false
            };
        }

        private void ShowValidation(string message)
        {
            ValidationMessage.Text = message;
            ValidationMessage.Visibility = Visibility.Visible;
        }

        private void ClearValidation()
        {
            ValidationMessage.Visibility = Visibility.Collapsed;
        }

        // Utility — format tags must match ImportService.ParseFile switch arms

        private string GetFileFilter()
        {
            return _selectedFormat switch
            {
                "accusync-xml" or "algo5-xml" or "acculink-xml" => "XML Files (*.xml)|*.xml",
                "accusync-json" or "algopro-json" => "JSON Files (*.json)|*.json",
                "facesheet-image" => "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
                "facesheet-pdf" => "PDF Files (*.pdf)|*.pdf",
                "facesheet-docx" => "Word Documents (*.docx)|*.docx",
                _ => "All Files (*.*)|*.*"
            };
        }

        private string GetFormatLabel()
        {
            return _selectedFormat switch
            {
                "accusync-xml" => Strings.ImportFileDialog_FormatAccuSyncXml,
                "accusync-json" => Strings.ImportFileDialog_FormatAccuSyncJson,
                "algo5-xml" => Strings.ImportFileDialog_FormatAlgo5Xml,
                "algopro-json" => Strings.ImportFileDialog_FormatAlgoProJson,
                "acculink-xml" => Strings.ImportFileDialog_FormatAccuLinkXml,
                "facesheet-image" => Strings.ImportFileDialog_LabelImage,
                "facesheet-pdf" => Strings.ImportFileDialog_LabelPdf,
                "facesheet-docx" => Strings.ImportFileDialog_LabelDocx,
                _ => Strings.ImportFileDialog_LabelFile
            };
        }

        private static string FormatFileSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            return $"{bytes / (1024.0 * 1024.0):F1} MB";
        }
    }
}