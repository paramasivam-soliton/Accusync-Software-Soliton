// --------------------------------------------------------------------------------
// <copyright file="ExportConfigView.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AccuSync.Models;
using AccuSync.WPF.Resources;
using AccuSync.WPF.Controls;

namespace AccuSync.WPF.Views.SystemConfiguration
{
    // NOTE: Unlike other config views (which use ListView.ItemsSource), this view builds
    // list cards imperatively and manages selection with index tracking. Consider standardizing.
    public partial class ExportConfigView : UserControl
    {
        private bool _isInitialized = false;
        private bool _isSuppressingUndo = false;
        private bool _isLoadingItem = false;

        private List<ExportEntry> _entries = new();
        private int _selectedIndex = -1;

        private static readonly SolidColorBrush _selectedBg =
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D4E9F9"));
        private static readonly SolidColorBrush _selectedBorder =
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#005FBE"));
        private static readonly SolidColorBrush _normalBorder =
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e5e7eb"));

        // NOTE: TakeSnapshot deep-clones the entire entry list — correct since entries
        // can be added/removed (unlike single-item snapshots in ABR/DPOAE views).
        private record ExportSnapshot(
            List<ExportEntry> Entries,
            int SelectedIndex
        );

        private ExportSnapshot _savedState;
        private readonly Stack<ExportSnapshot> _undoStack = new();

        public ExportConfigView()
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

        private void InitializeDefaults()
        {
            _entries = new List<ExportEntry>
            {
                new ExportEntry
                {
                    Name = "Export JSON",
                    ExportFormatIndex = 0,  // AccuSync JSON
                    ExportDataIndex = 0     // All Patients
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

        private void LoadEntryToForm(ExportEntry entry)
        {
            _isLoadingItem = true;

            NameBox.Text = entry.Name;
            DescriptionBox.Text = entry.Description;
            ExportFormatCombo.SelectedIndex = entry.ExportFormatIndex;
            ExportDataCombo.SelectedIndex = entry.ExportDataIndex;
            ExportFolderBox.Text = entry.ExportFolder;

            _isLoadingItem = false;
        }

        private void SaveCurrentToEntry()
        {
            if (_selectedIndex < 0 || _selectedIndex >= _entries.Count) return;
            var entry = _entries[_selectedIndex];
            entry.Name = NameBox.Text ?? "";
            entry.Description = DescriptionBox.Text ?? "";
            entry.ExportFormatIndex = ExportFormatCombo.SelectedIndex;
            entry.ExportDataIndex = ExportDataCombo.SelectedIndex;
            entry.ExportFolder = ExportFolderBox.Text ?? "";
        }

        // Change Handlers

        private void Field_Changed(object sender, TextChangedEventArgs e)
        {
            if (!_isInitialized || _isSuppressingUndo || _isLoadingItem) return;
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

        // Browse Folder

        private void BrowseFolder_Click(object sender, RoutedEventArgs e)
        {
            using var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select Export Folder",
                ShowNewFolderButton = true
            };
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                PushUndo();
                ExportFolderBox.Text = dialog.SelectedPath;
            }
        }

        // Snapshot and Undo

        private ExportSnapshot TakeSnapshot()
        {
            SaveCurrentToEntry();
            var cloned = _entries.Select(e => new ExportEntry
            {
                Name = e.Name,
                Description = e.Description,
                ExportFormatIndex = e.ExportFormatIndex,
                ExportDataIndex = e.ExportDataIndex,
                ExportFolder = e.ExportFolder
            }).ToList();
            return new ExportSnapshot(cloned, _selectedIndex);
        }

        private void RestoreSnapshot(ExportSnapshot snap)
        {
            _isSuppressingUndo = true;

            _entries = snap.Entries.Select(e => new ExportEntry
            {
                Name = e.Name,
                Description = e.Description,
                ExportFormatIndex = e.ExportFormatIndex,
                ExportDataIndex = e.ExportDataIndex,
                ExportFolder = e.ExportFolder
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

        public void HandleAdd()
        {
            PushUndo();
            var entry = new ExportEntry
            {
                Name = $"New Export {_entries.Count + 1}",
                ExportFormatIndex = 0,
                ExportDataIndex = 0
            };
            _entries.Add(entry);
            BuildList();
            SelectItem(_entries.Count - 1);
        }

        public void HandleDelete()
        {
            if (_selectedIndex < 0 || _selectedIndex >= _entries.Count) return;

            var result = AppDialog.Show(
                string.Format(Strings.ExportConfigView_ConfirmDeleteEntry, _entries[_selectedIndex].Name),
                Strings.ExportConfigView_ConfirmDelete, MessageBoxButton.YesNo, MessageBoxImage.Warning);
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

        // TODO: HandleSave has no actual persistence — same pattern as all other config views.
        public void HandleSave()
        {
            SaveCurrentToEntry();
            _savedState = TakeSnapshot();
            _undoStack.Clear();
            AppDialog.Show(Strings.ExportConfigView_ConfigurationsSaved, Strings.ExportConfigView_Save,
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void HandleRevert()
        {
            if (_savedState == null) return;
            PushUndo();
            RestoreSnapshot(_savedState);
        }

        public void HandleUndo()
        {
            if (_undoStack.Count == 0) return;
            var prev = _undoStack.Pop();
            RestoreSnapshot(prev);
        }
    }
}