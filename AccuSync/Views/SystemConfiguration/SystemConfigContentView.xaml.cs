// --------------------------------------------------------------------------------
// <copyright file="SystemConfigContentView.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using AccuSync.Resources;
using AccuSync.Controls;

namespace AccuSync.Views.SystemConfiguration
{
    public partial class SystemConfigContentView : UserControl
    {
        private bool _isInitialized = false;
        private bool _isSuppressingUndo = false;

        private string _logoFilePath = null;
        private string _logoFileName = null;

        private static readonly SolidColorBrush _defaultBorder =
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D1D5DB"));
        private static readonly SolidColorBrush _filledBorder =
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#28a745"));

        private record SystemConfigSnapshot(
            int LanguageIndex,
            int ConfirmSaveIndex,
            int ConfirmDeleteIndex,
            int DataModWarningIndex,
            string LogoFilePath,
            string LogoFileName,
            int PaperFormatIndex
        );

        private SystemConfigSnapshot _savedState;
        private readonly Stack<SystemConfigSnapshot> _undoStack = new();

        public SystemConfigContentView()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                _isInitialized = true;
                _savedState = TakeSnapshot();
            };
        }

        // Snapshot Helpers

        private SystemConfigSnapshot TakeSnapshot()
        {
            return new SystemConfigSnapshot(
                SystemLanguageCombo.SelectedIndex,
                ConfirmSaveCombo.SelectedIndex,
                ConfirmDeleteCombo.SelectedIndex,
                DataModWarningCombo.SelectedIndex,
                _logoFilePath,
                _logoFileName,
                PaperFormatCombo.SelectedIndex
            );
        }

        private void RestoreSnapshot(SystemConfigSnapshot snap)
        {
            _isSuppressingUndo = true;

            SystemLanguageCombo.SelectedIndex = snap.LanguageIndex;
            ConfirmSaveCombo.SelectedIndex = snap.ConfirmSaveIndex;
            ConfirmDeleteCombo.SelectedIndex = snap.ConfirmDeleteIndex;
            DataModWarningCombo.SelectedIndex = snap.DataModWarningIndex;
            PaperFormatCombo.SelectedIndex = snap.PaperFormatIndex;

            _logoFilePath = snap.LogoFilePath;
            _logoFileName = snap.LogoFileName;
            UpdateLogoDisplay();

            _isSuppressingUndo = false;
        }

        private void PushUndo()
        {
            if (!_isInitialized || _isSuppressingUndo) return;
            _undoStack.Push(TakeSnapshot());
        }

        // Save / Revert / Undo

        // NOTE: SaveState captures snapshot only — doesn't persist to database
        public void SaveState()
        {
            _savedState = TakeSnapshot();
            _undoStack.Clear();
            AppDialog.Show(Strings.SystemConfigContentView_ConfigurationSaved, Strings.SystemConfigContentView_Save, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void Revert()
        {
            if (_savedState == null) return;
            PushUndo();
            RestoreSnapshot(_savedState);
        }

        public void Undo()
        {
            if (_undoStack.Count == 0) return;
            var prev = _undoStack.Pop();
            RestoreSnapshot(prev);
        }

        public bool CanUndo => _undoStack.Count > 0;

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PushUndo();
        }

        // Hospital Logo

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = Strings.SystemConfigContentView_SelectHospitalLogo,
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All Files|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                PushUndo();
                _logoFilePath = dialog.FileName;
                _logoFileName = System.IO.Path.GetFileName(dialog.FileName);
                UpdateLogoDisplay();
            }
        }

        private void RestoreDefault_Click(object sender, RoutedEventArgs e)
        {
            PushUndo();
            _logoFilePath = null;
            _logoFileName = null;
            UpdateLogoDisplay();
        }

        private static readonly string[] _imageExtensions = { ".png", ".jpg", ".jpeg", ".bmp", ".gif" };

        private void Logo_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length == 1 && IsImageFile(files[0]))
                {
                    e.Effects = DragDropEffects.Copy;
                    if (sender is Border border)
                        border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#005FBE"));
                    e.Handled = true;
                    return;
                }
            }
            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        private void Logo_DragLeave(object sender, DragEventArgs e)
        {
            if (sender == LogoPreviewBorder)
                LogoPreviewBorder.BorderBrush = _defaultBorder;
            else if (sender == LogoSelectedState)
                LogoSelectedState.BorderBrush = _filledBorder;
        }

        private void Logo_Drop(object sender, DragEventArgs e)
        {
            Logo_DragLeave(sender, e);

            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length != 1 || !IsImageFile(files[0])) return;

            PushUndo();
            _logoFilePath = files[0];
            _logoFileName = System.IO.Path.GetFileName(files[0]);
            UpdateLogoDisplay();
        }

        private bool IsImageFile(string path)
        {
            var ext = System.IO.Path.GetExtension(path)?.ToLowerInvariant();
            return ext != null && System.Array.Exists(_imageExtensions, e => e == ext);
        }

        private void UpdateLogoDisplay()
        {
            if (_logoFilePath != null)
            {
                LogoEmptyState.Visibility = Visibility.Collapsed;
                LogoPreviewBorder.Visibility = Visibility.Collapsed;
                LogoSelectedState.Visibility = Visibility.Visible;
                RestoreDefaultButton.IsEnabled = true;

                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new System.Uri(_logoFilePath, System.UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    LogoImage.Source = bitmap;
                }
                catch
                {
                    LogoImage.Source = null;
                }

                string sizeText = "";
                try
                {
                    var info = new System.IO.FileInfo(_logoFilePath);
                    if (info.Exists)
                    {
                        double kb = info.Length / 1024.0;
                        sizeText = kb < 1024
                            ? $"{kb:F1} KB"
                            : $"{kb / 1024:F1} MB";
                    }
                }
                catch { /* ignore */ }

                LogoFileInfo.Text = string.Format(Strings.SystemConfigContentView_LogoFileInfo, _logoFileName, sizeText);
            }
            else
            {
                LogoSelectedState.Visibility = Visibility.Collapsed;
                LogoPreviewBorder.Visibility = Visibility.Visible;
                LogoEmptyState.Visibility = Visibility.Visible;
                LogoImage.Source = null;
                RestoreDefaultButton.IsEnabled = false;
            }
        }

        // Public Getters

        public string GetSelectedLanguage()
        {
            var content = (SystemLanguageCombo.SelectedItem as ComboBoxItem)?.Content?.ToString();
            return content ?? "English";
        }

        public bool GetConfirmSave() => ConfirmSaveCombo.SelectedIndex == 0;
        public bool GetConfirmDelete() => ConfirmDeleteCombo.SelectedIndex == 0;
        public bool GetDataModWarning() => DataModWarningCombo.SelectedIndex == 0;
        public string GetLogoFilePath() => _logoFilePath;
        public string GetPaperFormat()
        {
            var content = (PaperFormatCombo.SelectedItem as ComboBoxItem)?.Content?.ToString();
            return content ?? "A4";
        }

        // Content Takeover
        // Each Show/Hide pair swaps between the normal config grid and a sub-config view.
        // NOTE: Consider extracting into a generic ShowChild/HideChild helper to reduce repetition.

        public void ShowRiskFactors()
        {
            NormalConfigGrid.Visibility = Visibility.Collapsed;
            RiskFactorsView.Visibility = Visibility.Visible;
        }

        public void HideRiskFactors()
        {
            RiskFactorsView.Visibility = Visibility.Collapsed;
            NormalConfigGrid.Visibility = Visibility.Visible;
        }

        public void ShowComments()
        {
            NormalConfigGrid.Visibility = Visibility.Collapsed;
            CommentsView.Visibility = Visibility.Visible;
        }

        public void HideComments()
        {
            CommentsView.Visibility = Visibility.Collapsed;
            NormalConfigGrid.Visibility = Visibility.Visible;
        }

        public void ShowFieldSetup()
        {
            NormalConfigGrid.Visibility = Visibility.Collapsed;
            FieldSetupView.Visibility = Visibility.Visible;
        }

        public void HideFieldSetup()
        {
            FieldSetupView.Visibility = Visibility.Collapsed;
            NormalConfigGrid.Visibility = Visibility.Visible;
        }

        public void ShowUserProfile()
        {
            NormalConfigGrid.Visibility = Visibility.Collapsed;
            UserProfileConfigView.Visibility = Visibility.Visible;
        }

        public void HideUserProfile()
        {
            UserProfileConfigView.Visibility = Visibility.Collapsed;
            NormalConfigGrid.Visibility = Visibility.Visible;
        }

        public void ShowSiteFacility()
        {
            NormalConfigGrid.Visibility = Visibility.Collapsed;
            SiteFacilityConfigView.Visibility = Visibility.Visible;
        }

        public void HideSiteFacility()
        {
            SiteFacilityConfigView.Visibility = Visibility.Collapsed;
            NormalConfigGrid.Visibility = Visibility.Visible;
        }

        public void ShowImportConfig()
        {
            NormalConfigGrid.Visibility = Visibility.Collapsed;
            ImportConfigView.Visibility = Visibility.Visible;
        }

        public void HideImportConfig()
        {
            ImportConfigView.Visibility = Visibility.Collapsed;
            NormalConfigGrid.Visibility = Visibility.Visible;
        }

        public void ShowExportConfig()
        {
            NormalConfigGrid.Visibility = Visibility.Collapsed;
            ExportConfigView.Visibility = Visibility.Visible;
        }

        public void HideExportConfig()
        {
            ExportConfigView.Visibility = Visibility.Collapsed;
            NormalConfigGrid.Visibility = Visibility.Visible;
        }
    }
}