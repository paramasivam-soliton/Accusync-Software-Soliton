// --------------------------------------------------------------------------------
// <copyright file="ImportConfigView.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using AccuSync.Application.Models;
using AccuSync.WPF.Resources;
using AccuSync.WPF.Controls;

namespace AccuSync.WPF.Views.SystemConfiguration
{
    // NOTE: List-building, selection, and snapshot/undo patterns are near-identical to
    // ExportConfigView. Extract a shared ConfigListDetailBase.
    /// <summary>
    /// Toolbar takeover view for configuring import entries (format, user profile,
    /// password, and source folder).
    /// </summary>
    public partial class ImportConfigView : UserControl
    {
        private bool _isInitialized = false;
        private bool _isSuppressingUndo = false;
        private bool _isLoadingItem = false;
        private bool _isSyncingPassword = false;

        private const string EyeOpen =
            "M12 4.5C7 4.5 2.73 7.61 1 12c1.73 4.39 6 7.5 11 7.5s9.27-3.11 11-7.5c-1.73-4.39-6-7.5-11-7.5z" +
            "M12 17c-2.76 0-5-2.24-5-5s2.24-5 5-5 5 2.24 5 5-2.24 5-5 5z" +
            "m0-8c-1.66 0-3 1.34-3 3s1.34 3 3 3 3-1.34 3-3-1.34-3-3-3z";
        private const string EyeClosed =
            "M12 7c2.76 0 5 2.24 5 5 0 .65-.13 1.26-.36 1.83l2.92 2.92c1.51-1.26 2.7-2.89 3.43-4.75-1.73-4.39-6-7.5-11-7.5" +
            "-1.4 0-2.74.25-3.98.7l2.16 2.16C10.74 7.13 11.35 7 12 7z" +
            "M2 4.27l2.28 2.28.46.46C3.08 8.3 1.78 10.02 1 12c1.73 4.39 6 7.5 11 7.5 1.55 0 3.03-.3 4.38-.84" +
            "l.42.42L19.73 22 21 20.73 3.27 3 2 4.27z" +
            "M7.53 9.8l1.55 1.55c-.05.21-.08.43-.08.65 0 1.66 1.34 3 3 3 .22 0 .44-.03.65-.08l1.55 1.55" +
            "c-.67.33-1.41.53-2.2.53-2.76 0-5-2.24-5-5 0-.79.2-1.53.53-2.2z" +
            "m4.31-.78l3.15 3.15.02-.16c0-1.66-1.34-3-3-3l-.17.01z";

        private bool _isPwVisible = false;
        private bool _isVerifyVisible = false;

        private List<ImportEntry> _entries = new();
        private int _selectedIndex = -1;

        private static readonly SolidColorBrush _selectedBg =
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D4E9F9"));
        private static readonly SolidColorBrush _selectedBorder =
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#005FBE"));
        private static readonly SolidColorBrush _normalBorder =
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e5e7eb"));

        private record ImportSnapshot(
            List<ImportEntry> Entries,
            int SelectedIndex
        );

        private ImportSnapshot _savedState;
        private readonly Stack<ImportSnapshot> _undoStack = new();

        /// <summary>
        /// Initializes the control and populates the default import entry once loaded.
        /// </summary>
        public ImportConfigView()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                _isInitialized = true;
                InitializeDefaults();
                BuildList();
                if (_entries.Count > 0) SelectItem(0);
                _savedState = TakeSnapshot();
            };
        }

        // TODO: Default password "1234" is hardcoded here and in HandleAdd.
        //       Same concern flagged in DatabaseService.
        private void InitializeDefaults()
        {
            _entries = new List<ImportEntry>
            {
                new ImportEntry
                {
                    Name = "Import XML",
                    ImportFormatIndex = 0,  // AccuSync XML
                    UserProfileIndex = 0,   // Administrator
                    Password = "1234",
                    PasswordVerify = "1234"
                }
            };
        }

        // List Building

        private void BuildList()
        {
            ListPanel.Children.Clear();
            EmptyState.Visibility = _entries.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

            for (int i = 0; i < _entries.Count; i++)
            {
                var entry = _entries[i];
                var idx = i;

                var card = new Border
                {
                    Padding = new Thickness(16, 14, 16, 14),
                    Cursor = Cursors.Hand,
                    Background = Brushes.White,
                    BorderThickness = new Thickness(0, 0, 0, 1),
                    BorderBrush = _normalBorder,
                    Tag = idx
                };

                var nameText = new TextBlock
                {
                    Text = entry.Name,
                    FontSize = 13,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5C5149"))
                };

                card.Child = nameText;
                card.MouseLeftButtonDown += (s, e) => SelectItem((int)((Border)s).Tag);
                ListPanel.Children.Add(card);
            }

            UpdateListSelection();
        }

        private void UpdateListSelection()
        {
            for (int i = 0; i < ListPanel.Children.Count; i++)
            {
                if (ListPanel.Children[i] is Border card)
                {
                    bool sel = i == _selectedIndex;
                    card.Background = sel ? _selectedBg : Brushes.White;
                    card.BorderBrush = sel ? _selectedBorder : _normalBorder;
                    card.BorderThickness = sel ? new Thickness(1.5) : new Thickness(0, 0, 0, 1);
                    card.CornerRadius = sel ? new CornerRadius(6) : new CornerRadius(0);
                    card.Margin = sel ? new Thickness(4, 2, 4, 2) : new Thickness(0);
                }
            }
        }

        private void SelectItem(int index)
        {
            if (index < 0 || index >= _entries.Count) return;

            if (_selectedIndex >= 0 && _selectedIndex < _entries.Count)
                SaveCurrentToEntry();

            _selectedIndex = index;
            UpdateListSelection();
            LoadEntryToForm(_entries[index]);

            DetailPanel.Visibility = Visibility.Visible;
            DetailEmpty.Visibility = Visibility.Collapsed;
            DetailHeader.Text = _entries[index].Name.ToUpperInvariant();
        }

        // Form Load and Save

        private void LoadEntryToForm(ImportEntry entry)
        {
            _isLoadingItem = true;
            _isSyncingPassword = true;

            NameBox.Text = entry.Name;
            DescriptionBox.Text = entry.Description;
            ImportFormatCombo.SelectedIndex = entry.ImportFormatIndex;
            UserProfileCombo.SelectedIndex = entry.UserProfileIndex;

            PasswordField.Password = entry.Password;
            PasswordVisible.Text = entry.Password;
            VerifyField.Password = entry.PasswordVerify;
            VerifyVisible.Text = entry.PasswordVerify;

            ImportFolderBox.Text = entry.ImportFolder;

            _isSyncingPassword = false;
            _isLoadingItem = false;
        }

        private void SaveCurrentToEntry()
        {
            if (_selectedIndex < 0 || _selectedIndex >= _entries.Count) return;
            var entry = _entries[_selectedIndex];
            entry.Name = NameBox.Text ?? "";
            entry.Description = DescriptionBox.Text ?? "";
            entry.ImportFormatIndex = ImportFormatCombo.SelectedIndex;
            entry.UserProfileIndex = UserProfileCombo.SelectedIndex;
            entry.Password = _isPwVisible ? PasswordVisible.Text : PasswordField.Password;
            entry.PasswordVerify = _isVerifyVisible ? VerifyVisible.Text : VerifyField.Password;
            entry.ImportFolder = ImportFolderBox.Text ?? "";
        }

        // Change Handlers

        private void Field_Changed(object sender, TextChangedEventArgs e)
        {
            if (!_isInitialized || _isSuppressingUndo || _isLoadingItem || _isSyncingPassword) return;
            PushUndo();

            if (sender == NameBox && _selectedIndex >= 0)
            {
                DetailHeader.Text = (NameBox.Text ?? "—").ToUpperInvariant();
                _entries[_selectedIndex].Name = NameBox.Text ?? "";
                if (_selectedIndex < ListPanel.Children.Count &&
                    ListPanel.Children[_selectedIndex] is Border card &&
                    card.Child is TextBlock tb)
                {
                    tb.Text = NameBox.Text;
                }
            }
        }

        private void ComboBox_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitialized || _isSuppressingUndo || _isLoadingItem) return;
            PushUndo();
        }

        private void Password_Changed(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized || _isSuppressingUndo || _isLoadingItem || _isSyncingPassword) return;
            PushUndo();
        }

        // Password Toggle

        // TODO: Toggle logic duplicated from ChangePasswordWindow. Extract shared PasswordField UserControl.
        private void TogglePasswordVisibility(object sender, MouseButtonEventArgs e)
        {
            _isSyncingPassword = true;
            _isPwVisible = !_isPwVisible;
            if (_isPwVisible)
            {
                PasswordVisible.Text = PasswordField.Password;
                PasswordField.Visibility = Visibility.Collapsed;
                PasswordVisible.Visibility = Visibility.Visible;
                PwEyeIcon.Data = Geometry.Parse(EyeClosed);
            }
            else
            {
                PasswordField.Password = PasswordVisible.Text;
                PasswordVisible.Visibility = Visibility.Collapsed;
                PasswordField.Visibility = Visibility.Visible;
                PwEyeIcon.Data = Geometry.Parse(EyeOpen);
            }
            _isSyncingPassword = false;
        }

        private void ToggleVerifyVisibility(object sender, MouseButtonEventArgs e)
        {
            _isSyncingPassword = true;
            _isVerifyVisible = !_isVerifyVisible;
            if (_isVerifyVisible)
            {
                VerifyVisible.Text = VerifyField.Password;
                VerifyField.Visibility = Visibility.Collapsed;
                VerifyVisible.Visibility = Visibility.Visible;
                VerifyEyeIcon.Data = Geometry.Parse(EyeClosed);
            }
            else
            {
                VerifyField.Password = VerifyVisible.Text;
                VerifyVisible.Visibility = Visibility.Collapsed;
                VerifyField.Visibility = Visibility.Visible;
                VerifyEyeIcon.Data = Geometry.Parse(EyeOpen);
            }
            _isSyncingPassword = false;
        }

        // Browse Folder

        private void BrowseFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Select Import Folder"
            };
            if (dialog.ShowDialog() == true)
            {
                PushUndo();
                ImportFolderBox.Text = dialog.FolderName;
            }
        }

        // Snapshot and Undo

        private ImportSnapshot TakeSnapshot()
        {
            SaveCurrentToEntry();
            var cloned = _entries.Select(e => new ImportEntry
            {
                Name = e.Name,
                Description = e.Description,
                ImportFormatIndex = e.ImportFormatIndex,
                UserProfileIndex = e.UserProfileIndex,
                Password = e.Password,
                PasswordVerify = e.PasswordVerify,
                ImportFolder = e.ImportFolder
            }).ToList();
            return new ImportSnapshot(cloned, _selectedIndex);
        }

        private void RestoreSnapshot(ImportSnapshot snap)
        {
            _isSuppressingUndo = true;

            _entries = snap.Entries.Select(e => new ImportEntry
            {
                Name = e.Name,
                Description = e.Description,
                ImportFormatIndex = e.ImportFormatIndex,
                UserProfileIndex = e.UserProfileIndex,
                Password = e.Password,
                PasswordVerify = e.PasswordVerify,
                ImportFolder = e.ImportFolder
            }).ToList();

            _selectedIndex = -1;
            BuildList();

            if (snap.SelectedIndex >= 0 && snap.SelectedIndex < _entries.Count)
                SelectItem(snap.SelectedIndex);
            else
            {
                DetailPanel.Visibility = Visibility.Collapsed;
                DetailEmpty.Visibility = Visibility.Visible;
                DetailHeader.Text = "—";
            }

            _isSuppressingUndo = false;
        }

        private void PushUndo()
        {
            if (!_isInitialized || _isSuppressingUndo || _isLoadingItem) return;
            _undoStack.Push(TakeSnapshot());
        }

        // Public API

        /// <summary>
        /// Adds a new import entry with default values and selects it.
        /// </summary>
        public void HandleAdd()
        {
            PushUndo();
            var entry = new ImportEntry
            {
                Name = $"New Import {_entries.Count + 1}",
                ImportFormatIndex = 0,
                UserProfileIndex = 0,
                Password = "1234",
                PasswordVerify = "1234"
            };
            _entries.Add(entry);
            BuildList();
            SelectItem(_entries.Count - 1);
        }

        /// <summary>
        /// Deletes the selected import entry after user confirmation.
        /// </summary>
        public void HandleDelete()
        {
            if (_selectedIndex < 0 || _selectedIndex >= _entries.Count) return;

            var result = AppDialog.Show(
                string.Format(Strings.ImportConfigView_ConfirmDeleteEntry, _entries[_selectedIndex].Name),
                Strings.ImportConfigView_ConfirmDelete, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            PushUndo();
            _entries.RemoveAt(_selectedIndex);

            if (_entries.Count == 0)
            {
                _selectedIndex = -1;
                DetailPanel.Visibility = Visibility.Collapsed;
                DetailEmpty.Visibility = Visibility.Visible;
                DetailHeader.Text = "—";
            }
            else
            {
                _selectedIndex = System.Math.Min(_selectedIndex, _entries.Count - 1);
            }

            BuildList();
            if (_selectedIndex >= 0) SelectItem(_selectedIndex);
        }

        // TODO: HandleSave has no actual persistence.
        /// <summary>
        /// Saves the current state as the new baseline and clears the undo stack.
        /// </summary>
        public void HandleSave()
        {
            SaveCurrentToEntry();
            _savedState = TakeSnapshot();
            _undoStack.Clear();
            AppDialog.Show(Strings.ImportConfigView_ConfigurationsSaved, Strings.ImportConfigView_Save,
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Reverts all changes back to the last saved state.
        /// </summary>
        public void HandleRevert()
        {
            if (_savedState == null) return;
            PushUndo();
            RestoreSnapshot(_savedState);
        }

        /// <summary>
        /// Restores the previous state from the undo stack.
        /// </summary>
        public void HandleUndo()
        {
            if (_undoStack.Count == 0) return;
            var prev = _undoStack.Pop();
            RestoreSnapshot(prev);
        }
    }
}