// --------------------------------------------------------------------------------
// <copyright file="CommentsConfigView.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using AccuSync.Models;
using AccuSync.Resources;
using AccuSync.Controls;

namespace AccuSync.Views.SystemConfiguration
{
    // NOTE: Undo/snapshot/restore infrastructure is nearly identical to ABRConfigurationView.
    // If a third config view appears, extract a generic ConfigViewBase with shared undo logic.
    public partial class CommentsConfigView : UserControl
    {
        private ObservableCollection<CommentEntry> _comments = new();
        private bool _isInitialized;
        private bool _isSuppressingUndo;
        private bool _isLoadingItem;

        // Undo Infrastructure
        private record CommentSnapshot(
            string Comment, int ActiveIndex,
            int TranslationLanguageIndex,
            string Translation
        );

        private CommentSnapshot _savedState;
        private readonly Stack<CommentSnapshot> _undoStack = new();

        // Initialization

        public CommentsConfigView()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                _isInitialized = true;
                InitializeDefaults();
            };
        }

        private void InitializeDefaults()
        {
            _isSuppressingUndo = true;

            _comments.Add(new CommentEntry { Comment = "Patient restless" });
            _comments.Add(new CommentEntry { Comment = "Noisy test conditions" });
            _comments.Add(new CommentEntry { Comment = "Wax in ear canal" });
            _comments.Add(new CommentEntry { Comment = "Impedance is high" });
            _comments.Add(new CommentEntry { Comment = "No ear canal" });
            _comments.Add(new CommentEntry { Comment = "Patient discharged" });
            _comments.Add(new CommentEntry { Comment = "Parent refused test" });
            _comments.Add(new CommentEntry { Comment = "Diagnostic referrals" });
            _comments.Add(new CommentEntry { Comment = "Medical referral" });
            _comments.Add(new CommentEntry { Comment = "Retest recommended" });
            _comments.Add(new CommentEntry { Comment = "Unable to test" });
            _comments.Add(new CommentEntry { Comment = "Electrode is faulty" });
            _comments.Add(new CommentEntry { Comment = "Poor probe fit" });
            _comments.Add(new CommentEntry { Comment = "Equipment error" });

            CommentsListView.ItemsSource = _comments;
            EmptyListPanel.Visibility = _comments.Count > 0 ? Visibility.Collapsed : Visibility.Visible;

            if (_comments.Count > 0)
                CommentsListView.SelectedIndex = 0;

            _isSuppressingUndo = false;
        }

        // List Selection

        private void CommentsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CommentsListView.SelectedItem is CommentEntry entry)
            {
                _isLoadingItem = true;
                _isSuppressingUndo = true;

                EmptyDetailPanel.Visibility = Visibility.Collapsed;
                DetailFormPanel.Visibility = Visibility.Visible;
                DetailHeaderText.Text = entry.Comment.ToUpper();

                CommentBox.Text = entry.Comment;
                ActiveCombo.SelectedIndex = entry.ActiveIndex;

                TranslationLanguageCombo.SelectedIndex = 0;
                LoadTranslationField(entry, 0);

                _undoStack.Clear();
                _savedState = CaptureSnapshot();

                _isLoadingItem = false;
                _isSuppressingUndo = false;
            }
            else
            {
                EmptyDetailPanel.Visibility = Visibility.Visible;
                DetailFormPanel.Visibility = Visibility.Collapsed;
                DetailHeaderText.Text = Strings.CommentsConfigView_CommentDetails;
            }
        }

        private void LoadTranslationField(CommentEntry entry, int langIndex)
        {
            if (entry.Translations.TryGetValue(langIndex, out var t))
                TranslationBox.Text = t;
            else
                TranslationBox.Text = langIndex == 0 ? entry.Comment : "";
        }

        // Field Change Tracking

        private void Field_TextChanged(object sender, TextChangedEventArgs e)
        {
            PushUndo();
            SyncToSelectedComment();
        }

        private void Field_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitialized || _isLoadingItem || _isSuppressingUndo) return;

            // Language combo change: load the translation for the newly selected language
            // TODO: SaveCurrentTranslation was empty and removed. Verify the current translation
            //       is persisted before switching — SyncToSelectedComment below handles it.
            if (sender == TranslationLanguageCombo)
            {
                if (CommentsListView.SelectedItem is CommentEntry entry)
                {
                    _isLoadingItem = true;
                    LoadTranslationField(entry, TranslationLanguageCombo.SelectedIndex);
                    _isLoadingItem = false;
                }
            }

            PushUndo();
            SyncToSelectedComment();
        }

        private void SyncToSelectedComment()
        {
            if (_isLoadingItem || !_isInitialized) return;
            if (CommentsListView.SelectedItem is not CommentEntry entry) return;

            entry.Comment = CommentBox.Text;
            entry.ActiveIndex = ActiveCombo.SelectedIndex;
            entry.Active = ActiveCombo.SelectedIndex == 0;

            int langIdx = TranslationLanguageCombo.SelectedIndex;
            entry.Translations[langIdx] = TranslationBox.Text;

            DetailHeaderText.Text = entry.Comment.ToUpper();
            CommentsListView.Items.Refresh();
        }

        // Snapshot and Undo

        private CommentSnapshot CaptureSnapshot()
        {
            return new CommentSnapshot(
                CommentBox.Text,
                ActiveCombo.SelectedIndex,
                TranslationLanguageCombo.SelectedIndex,
                TranslationBox.Text
            );
        }

        private void RestoreSnapshot(CommentSnapshot snap)
        {
            _isLoadingItem = true;
            _isSuppressingUndo = true;

            CommentBox.Text = snap.Comment;
            ActiveCombo.SelectedIndex = snap.ActiveIndex;
            TranslationLanguageCombo.SelectedIndex = snap.TranslationLanguageIndex;
            TranslationBox.Text = snap.Translation;

            SyncToSelectedComment();

            _isLoadingItem = false;
            _isSuppressingUndo = false;
        }

        private void PushUndo()
        {
            if (!_isInitialized || _isSuppressingUndo || _isLoadingItem) return;
            _undoStack.Push(CaptureSnapshot());
        }

        // Public Handlers (called by SidebarNavigation)

        public void HandleAdd()
        {
            var entry = new CommentEntry
            {
                Comment = "New Comment",
                Active = true,
                ActiveIndex = 0,
                InUse = false
            };

            _comments.Add(entry);
            EmptyListPanel.Visibility = Visibility.Collapsed;
            CommentsListView.SelectedItem = entry;
        }

        public void HandleDelete()
        {
            if (CommentsListView.SelectedItem is not CommentEntry entry) return;

            if (entry.InUse)
            {
                AppDialog.Show(
                    Strings.CommentsConfigView_CannotDeleteAssigned,
                    Strings.CommentsConfigView_CannotDelete, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = AppDialog.Show(
                string.Format(Strings.CommentsConfigView_ConfirmDeleteComment, entry.Comment),
                Strings.CommentsConfigView_ConfirmDelete, MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            int idx = CommentsListView.SelectedIndex;
            _comments.Remove(entry);
            CommentsListView.Items.Refresh();

            if (_comments.Count == 0)
            {
                EmptyListPanel.Visibility = Visibility.Visible;
                EmptyDetailPanel.Visibility = Visibility.Visible;
                DetailFormPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                CommentsListView.SelectedIndex = Math.Min(idx, _comments.Count - 1);
            }
        }

        // TODO: HandleSave has no actual persistence — same issue as ABRConfigurationView.
        public void HandleSave()
        {
            _savedState = CaptureSnapshot();
            _undoStack.Clear();
            AppDialog.Show(Strings.CommentsConfigView_CommentSaved, Strings.CommentsConfigView_Save, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void HandleRevert()
        {
            if (_savedState == null) return;
            RestoreSnapshot(_savedState);
            _undoStack.Clear();
        }

        public void HandleUndo()
        {
            if (_undoStack.Count == 0) return;
            var snap = _undoStack.Pop();
            RestoreSnapshot(snap);
        }
    }
}