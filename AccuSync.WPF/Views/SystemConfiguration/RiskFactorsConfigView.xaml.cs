// --------------------------------------------------------------------------------
// <copyright file="RiskFactorsConfigView.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using AccuSync.Models;
using AccuSync.WPF.Resources;
using AccuSync.WPF.Controls;

namespace AccuSync.WPF.Views.SystemConfiguration
{
    public partial class RiskFactorsConfigView : UserControl
    {
        private ObservableCollection<RiskFactorEntry> _riskFactors = new();
        private bool _isInitialized;
        private bool _isSuppressingUndo;
        private bool _isLoadingItem;

        private record RiskFactorSnapshot(
            string Name, string Description, int ActiveIndex,
            int TranslationLanguageIndex,
            string TranslationName, string TranslationDescription
        );

        private RiskFactorSnapshot _savedState;
        private readonly Stack<RiskFactorSnapshot> _undoStack = new();

        public RiskFactorsConfigView()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                _isInitialized = true;
                InitializeDefaults();
            };
        }

        // Initialization

        // TODO: Replace hardcoded risk factors with data from DatabaseService
        private void InitializeDefaults()
        {
            _isSuppressingUndo = true;

            _riskFactors.Add(new RiskFactorEntry { Name = "Family History of Permanent Childhood Hearing Loss", Description = "", Active = true, ActiveIndex = 0, InUse = false });
            _riskFactors.Add(new RiskFactorEntry { Name = "Bacterial Meningitis", Description = "", Active = true, ActiveIndex = 0, InUse = false });
            _riskFactors.Add(new RiskFactorEntry { Name = "Craniofacial Anomalies-Microtia/Atresia, Clefting", Description = "", Active = true, ActiveIndex = 0, InUse = false });
            _riskFactors.Add(new RiskFactorEntry { Name = "Hyperbilirubinemia with Exchange Transfusion", Description = "", Active = true, ActiveIndex = 0, InUse = false });
            _riskFactors.Add(new RiskFactorEntry { Name = "Low Birth Weight", Description = "", Active = true, ActiveIndex = 0, InUse = false });
            _riskFactors.Add(new RiskFactorEntry { Name = "Ototoxic Medications", Description = "", Active = true, ActiveIndex = 0, InUse = false });
            _riskFactors.Add(new RiskFactorEntry { Name = "Perinatal or Postnatal Infection", Description = "", Active = true, ActiveIndex = 0, InUse = false });
            _riskFactors.Add(new RiskFactorEntry { Name = "Prolonged Ventilation", Description = "", Active = true, ActiveIndex = 0, InUse = false });
            _riskFactors.Add(new RiskFactorEntry { Name = "Asphyxia- Hypoxic Ischemic Encephalopathy (HIE)", Description = "", Active = true, ActiveIndex = 0, InUse = false });
            _riskFactors.Add(new RiskFactorEntry { Name = "Syndromes & Genetic Disorders of Hearing Loss", Description = "", Active = true, ActiveIndex = 0, InUse = false });
            _riskFactors.Add(new RiskFactorEntry { Name = "Aminoglycosides for >5 Days", Description = "", Active = true, ActiveIndex = 0, InUse = false });
            _riskFactors.Add(new RiskFactorEntry { Name = "In Utero Infections such as CMV & Zika", Description = "", Active = true, ActiveIndex = 0, InUse = false });
            _riskFactors.Add(new RiskFactorEntry { Name = "Caregiver Concern", Description = "", Active = true, ActiveIndex = 0, InUse = false });
            _riskFactors.Add(new RiskFactorEntry { Name = "NICU >5 Days", Description = "", Active = true, ActiveIndex = 0, InUse = false });
            _riskFactors.Add(new RiskFactorEntry { Name = "Exposure to Head Trauma or Chemotherapy", Description = "", Active = true, ActiveIndex = 0, InUse = false });
            _riskFactors.Add(new RiskFactorEntry { Name = "ECMO-Extracorporeal Membrane Oxygenation", Description = "", Active = true, ActiveIndex = 0, InUse = false });

            RiskFactorsListView.ItemsSource = _riskFactors;
            EmptyListPanel.Visibility = _riskFactors.Count > 0 ? Visibility.Collapsed : Visibility.Visible;

            if (_riskFactors.Count > 0)
                RiskFactorsListView.SelectedIndex = 0;

            _isSuppressingUndo = false;
        }

        // List Selection

        private void RiskFactorsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RiskFactorsListView.SelectedItem is RiskFactorEntry rf)
            {
                _isLoadingItem = true;
                _isSuppressingUndo = true;

                EmptyDetailPanel.Visibility = Visibility.Collapsed;
                DetailFormPanel.Visibility = Visibility.Visible;
                DetailHeaderText.Text = rf.Name.ToUpper();

                NameBox.Text = rf.Name;
                DescriptionBox.Text = rf.Description;
                ActiveCombo.SelectedIndex = rf.ActiveIndex;

                TranslationLanguageCombo.SelectedIndex = 0;
                LoadTranslationFields(rf, 0);

                SystemInfoBox.Text = rf.InUse
                    ? Strings.RiskFactorsConfigView_InUseInfo
                    : Strings.RiskFactorsConfigView_NotInUseInfo;

                _undoStack.Clear();
                _savedState = CaptureSnapshot();

                _isLoadingItem = false;
                _isSuppressingUndo = false;
            }
            else
            {
                EmptyDetailPanel.Visibility = Visibility.Visible;
                DetailFormPanel.Visibility = Visibility.Collapsed;
                DetailHeaderText.Text = Strings.RiskFactorsConfigView_RiskFactorDetailsHeader;
            }
        }

        // Falls back to the risk factor's own name/description for the default language (index 0)
        private void LoadTranslationFields(RiskFactorEntry rf, int langIndex)
        {
            if (rf.TranslationNames.TryGetValue(langIndex, out var tName))
                TranslationNameBox.Text = tName;
            else
                TranslationNameBox.Text = langIndex == 0 ? rf.Name : "";

            if (rf.TranslationDescriptions.TryGetValue(langIndex, out var tDesc))
                TranslationDescriptionBox.Text = tDesc;
            else
                TranslationDescriptionBox.Text = langIndex == 0 ? rf.Description : "";
        }

        // Field Change Tracking

        private void Field_TextChanged(object sender, TextChangedEventArgs e)
        {
            PushUndo();
            SyncToSelectedRiskFactor();
        }

        private void Field_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitialized || _isLoadingItem || _isSuppressingUndo) return;

            // Language switch: load translation fields for the newly selected language.
            // The previous language's edits are already persisted on every
            // SyncToSelectedRiskFactor call, so no explicit save is needed here.
            if (sender == TranslationLanguageCombo)
            {
                if (RiskFactorsListView.SelectedItem is RiskFactorEntry rf)
                {
                    _isLoadingItem = true;
                    LoadTranslationFields(rf, TranslationLanguageCombo.SelectedIndex);
                    _isLoadingItem = false;
                }
            }

            PushUndo();
            SyncToSelectedRiskFactor();
        }

        private void SyncToSelectedRiskFactor()
        {
            if (_isLoadingItem || !_isInitialized) return;
            if (RiskFactorsListView.SelectedItem is not RiskFactorEntry rf) return;

            rf.Name = NameBox.Text;
            rf.Description = DescriptionBox.Text;
            rf.ActiveIndex = ActiveCombo.SelectedIndex;
            rf.Active = ActiveCombo.SelectedIndex == 0;

            int langIdx = TranslationLanguageCombo.SelectedIndex;
            rf.TranslationNames[langIdx] = TranslationNameBox.Text;
            rf.TranslationDescriptions[langIdx] = TranslationDescriptionBox.Text;

            DetailHeaderText.Text = rf.Name.ToUpper();
            RiskFactorsListView.Items.Refresh();
        }

        // Snapshot Helpers

        private RiskFactorSnapshot CaptureSnapshot()
        {
            return new RiskFactorSnapshot(
                NameBox.Text,
                DescriptionBox.Text,
                ActiveCombo.SelectedIndex,
                TranslationLanguageCombo.SelectedIndex,
                TranslationNameBox.Text,
                TranslationDescriptionBox.Text
            );
        }

        private void RestoreSnapshot(RiskFactorSnapshot snap)
        {
            _isLoadingItem = true;
            _isSuppressingUndo = true;

            NameBox.Text = snap.Name;
            DescriptionBox.Text = snap.Description;
            ActiveCombo.SelectedIndex = snap.ActiveIndex;
            TranslationLanguageCombo.SelectedIndex = snap.TranslationLanguageIndex;
            TranslationNameBox.Text = snap.TranslationName;
            TranslationDescriptionBox.Text = snap.TranslationDescription;

            SyncToSelectedRiskFactor();

            _isLoadingItem = false;
            _isSuppressingUndo = false;
        }

        private void PushUndo()
        {
            if (!_isInitialized || _isSuppressingUndo || _isLoadingItem) return;
            _undoStack.Push(CaptureSnapshot());
        }

        // Save / Revert / Undo / Add / Delete — called from toolbar

        // NOTE: HandleSave captures snapshot only — doesn't persist to database
        public void HandleAdd()
        {
            var entry = new RiskFactorEntry
            {
                Name = "New Risk Factor",
                Description = "",
                Active = true,
                ActiveIndex = 0,
                InUse = false
            };

            _riskFactors.Add(entry);
            EmptyListPanel.Visibility = Visibility.Collapsed;
            RiskFactorsListView.SelectedItem = entry;
        }

        public void HandleDelete()
        {
            if (RiskFactorsListView.SelectedItem is not RiskFactorEntry rf) return;

            if (rf.InUse)
            {
                AppDialog.Show(
                    Strings.RiskFactorsConfigView_CannotDeleteAssigned,
                    Strings.RiskFactorsConfigView_CannotDelete, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = AppDialog.Show(
                string.Format(Strings.RiskFactorsConfigView_ConfirmDeleteRiskFactor, rf.Name),
                Strings.RiskFactorsConfigView_ConfirmDelete, MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            int idx = RiskFactorsListView.SelectedIndex;
            _riskFactors.Remove(rf);
            RiskFactorsListView.Items.Refresh();

            if (_riskFactors.Count == 0)
            {
                EmptyListPanel.Visibility = Visibility.Visible;
                EmptyDetailPanel.Visibility = Visibility.Visible;
                DetailFormPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                RiskFactorsListView.SelectedIndex = Math.Min(idx, _riskFactors.Count - 1);
            }
        }

        public void HandleSave()
        {
            _savedState = CaptureSnapshot();
            _undoStack.Clear();
            AppDialog.Show(Strings.RiskFactorsConfigView_RiskFactorSaved, Strings.RiskFactorsConfigView_Save, MessageBoxButton.OK, MessageBoxImage.Information);
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