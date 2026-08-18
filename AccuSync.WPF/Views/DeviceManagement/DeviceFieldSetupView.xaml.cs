// --------------------------------------------------------------------------------
// <copyright file="DeviceFieldSetupView.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AccuSync.Application.Models;
using AccuSync.Application.Helpers;
using AccuSync.WPF.Resources;
using AccuSync.WPF.Controls;

namespace AccuSync.WPF.Views.DeviceManagement
{
    /// <summary>
    /// Toolbar takeover view for configuring which patient fields are
    /// active and mandatory on a specific device. Mirrors the system-level
    /// FieldSetupConfigView but omits the "Include QR" column.
    /// </summary>
    // NOTE: Field list, snapshot/undo, and table rendering are near-identical to
    // FieldSetupConfigView. Extract a shared base if a third variant appears.
    public partial class DeviceFieldSetupView : UserControl
    {
        private bool _isInitialized;
        private bool _isSuppressingUndo;
        private List<DeviceFieldEntry> _fields;


        private record FieldSetupSnapshot(
            int PatientIdRuleIndex,
            List<(string DeviceCustomLabel, bool Active, bool Mandatory)> FieldStates
        );

        private FieldSetupSnapshot _savedState;
        private readonly Stack<FieldSetupSnapshot> _undoStack = new();

        // Per-row UI references for custom label management
        private readonly Dictionary<int, TextBox> _customLabelBoxes = new();
        private readonly Dictionary<int, Button> _resetButtons = new();

        /// <summary>
        /// Initializes the control and builds the field table once loaded.
        /// </summary>
        public DeviceFieldSetupView()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                _isInitialized = true;
                InitializeFields();
                BuildFieldTable();
                UpdateSelectAllStates();
                UpdateCustomLabelCount();
                _savedState = TakeSnapshot();
            };
        }

        // Field Definitions

        // TODO: Field list is hardcoded. Load from the FieldSetup table when wired to DB.
        // SystemCustomLabel would be loaded from FieldSetup.CustomLabel via DAL.
        // Example overrides are set to demonstrate the inheritance placeholder.
        private void InitializeFields()
        {
            _fields = new List<DeviceFieldEntry>
            {
                // Patient
                new() { FieldName = "Patient ID" },
                new() { FieldName = "Hospital ID" },
                new() { FieldName = "Patient First Name" },
                new() { FieldName = "Patient Last Name" },
                new() { FieldName = "Patient Date of Birth" },
                new() { FieldName = "Patient Gender" },
                new() { FieldName = "Patient Gestational Age" },
                new() { FieldName = "Patient Weight" },
                new() { FieldName = "Patient Height" },
                new() { FieldName = "Patient Birth Location" },
                new() { FieldName = "Patient Nationality" },
                new() { FieldName = "Patient Screening Consent" },
                new() { FieldName = "Patient Consent State" },
                new() { FieldName = "Patient NICU" },
                new() { FieldName = "Patient Discharged" },
                new() { FieldName = "Patient Deceased" },
                new() { FieldName = "Patient Tracking Consent" },
                new() { FieldName = "Patient Comments" },
                // Mother
                new() { FieldName = "Mother Title" },
                new() { FieldName = "Mother SSN" },
                new() { FieldName = "Mother ID" },
                new() { FieldName = "Mother First Name" },
                new() { FieldName = "Mother Last Name" },
                new() { FieldName = "Mother Date of Birth" },
                new() { FieldName = "Mother Language" },
                new() { FieldName = "Mother Address 1" },
                new() { FieldName = "Mother Address 2" },
                new() { FieldName = "Mother City" },
                new() { FieldName = "Mother State" },
                new() { FieldName = "Mother Zip Code" },
                new() { FieldName = "Mother Country" },
                new() { FieldName = "Mother Phone" },
                new() { FieldName = "Mother Mobile Phone" },
                new() { FieldName = "Mother Fax" },
                new() { FieldName = "Mother Email" },
                // Caregiver
                new() { FieldName = "Caregiver Title" },
                new() { FieldName = "Caregiver SSN" },
                new() { FieldName = "Caregiver First Name" },
                new() { FieldName = "Caregiver Last Name" },
                new() { FieldName = "Caregiver Language" },
                new() { FieldName = "Caregiver Address 1" },
                new() { FieldName = "Caregiver Address 2" },
                new() { FieldName = "Caregiver City" },
                new() { FieldName = "Caregiver State" },
                new() { FieldName = "Caregiver Zip Code" },
                new() { FieldName = "Caregiver Country" },
                new() { FieldName = "Caregiver Phone" },
                new() { FieldName = "Caregiver Mobile Phone" },
                new() { FieldName = "Caregiver Fax" },
                new() { FieldName = "Caregiver Email" },
                // Referral
                new() { FieldName = "Audiology Referral" },
                new() { FieldName = "Referral Date" },
                new() { FieldName = "Referral To" },
                new() { FieldName = "Referral From" },
                new() { FieldName = "Referral Phone" },
                // Other
                new() { FieldName = "Medication" },
                new() { FieldName = "Physician" },
                new() { FieldName = "Audiologist" },
                // Custom fields — inactive by default until configured
                new() { FieldName = "Available Field #1", IsActive = false },
                new() { FieldName = "Available Field #2", IsActive = false },
                new() { FieldName = "Available Field #3", IsActive = false },
                new() { FieldName = "Available Field #4", IsActive = false },
            };
        }

        // Table Rendering

        private void BuildFieldTable()
        {
            FieldTableBody.Children.Clear();
            _customLabelBoxes.Clear();
            _resetButtons.Clear();

            for (int i = 0; i < _fields.Count; i++)
                FieldTableBody.Children.Add(CreateFieldRow(_fields[i], i));
        }

        private Border CreateFieldRow(DeviceFieldEntry field, int index)
        {
            var row = new Border
            {
                Background = index % 2 == 0 ? Brushes.White : FieldSetupTable.AltRowBg,
                BorderBrush = FieldSetupTable.BorderBrush,
                BorderThickness = new Thickness(0, 0, 0, index < _fields.Count - 1 ? 1 : 0),
                Padding = new Thickness(16, 6, 16, 6),
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.2, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });

            // Col 0: Field name (read-only)
            var nameText = new TextBlock
            {
                Text = field.FieldName,
                FontSize = 13,
                Foreground = FieldSetupTable.TextBrush,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = new FontFamily("Segoe UI"),
            };
            Grid.SetColumn(nameText, 0);
            grid.Children.Add(nameText);

            // Col 1: Custom label TextBox with inline reset button
            // Placeholder shows InheritedLabel (system custom label or default name)
            bool hasCustom = !string.IsNullOrEmpty(field.DeviceCustomLabel);
            var labelContainer = new Grid { Margin = new Thickness(0, 0, 8, 0) };

            var labelBox = new TextBox
            {
                Text = hasCustom ? field.DeviceCustomLabel : field.FieldName,
                Foreground = hasCustom ? FieldSetupTable.TextBrush : FieldSetupTable.PlaceholderBrush,
                FontSize = 13,
                FontFamily = new FontFamily("Segoe UI"),
                Padding = new Thickness(8, 6, 24, 6),
                VerticalAlignment = VerticalAlignment.Center,
                BorderBrush = hasCustom ? FieldSetupTable.FilledBorderBrush : FieldSetupTable.DefaultBorderBrush,
                BorderThickness = new Thickness(1),
                Background = Brushes.White,
                Tag = index,
            };
            labelBox.GotFocus += CustomLabelBox_GotFocus;
            labelBox.LostFocus += CustomLabelBox_LostFocus;
            labelBox.TextChanged += CustomLabelBox_TextChanged;
            labelContainer.Children.Add(labelBox);
            _customLabelBoxes[index] = labelBox;

            var resetBtn = new Button
            {
                Style = (Style)FindResource("InlineResetButtonStyle"),
                Tag = index,
                Margin = new Thickness(0, 0, 4, 0),
                Visibility = hasCustom ? Visibility.Visible : Visibility.Collapsed,
            };
            resetBtn.Click += ResetSingleLabel_Click;
            labelContainer.Children.Add(resetBtn);
            _resetButtons[index] = resetBtn;

            Grid.SetColumn(labelContainer, 1);
            grid.Children.Add(labelContainer);

            // Col 2: Active
            var activeCb = new CheckBox
            {
                IsChecked = field.IsActive,
                Style = (Style)FindResource("FilledCheckBoxStyle"),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Tag = index,
            };
            activeCb.Checked += ActiveCheckBox_Changed;
            activeCb.Unchecked += ActiveCheckBox_Changed;
            Grid.SetColumn(activeCb, 2);
            grid.Children.Add(activeCb);

            // Col 3: Mandatory
            var mandatoryCb = new CheckBox
            {
                IsChecked = field.IsMandatory,
                Style = (Style)FindResource("FilledCheckBoxStyle"),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Tag = index,
            };
            mandatoryCb.Checked += MandatoryCheckBox_Changed;
            mandatoryCb.Unchecked += MandatoryCheckBox_Changed;
            Grid.SetColumn(mandatoryCb, 3);
            grid.Children.Add(mandatoryCb);

            row.Child = grid;
            return row;
        }

        // Custom Label Handlers

        private void CustomLabelBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is not TextBox tb || tb.Tag is not int idx) return;
            var field = _fields[idx];
            if (string.IsNullOrEmpty(field.DeviceCustomLabel))
            {
                _isSuppressingUndo = true;
                tb.Text = "";
                tb.Foreground = FieldSetupTable.TextBrush;
                _isSuppressingUndo = false;
            }
        }

        private void CustomLabelBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is not TextBox tb || tb.Tag is not int idx) return;
            var field = _fields[idx];
            if (string.IsNullOrWhiteSpace(tb.Text))
            {
                _isSuppressingUndo = true;
                field.DeviceCustomLabel = null;
                tb.Text = field.InheritedLabel;
                tb.Foreground = FieldSetupTable.PlaceholderBrush;
                tb.BorderBrush = FieldSetupTable.DefaultBorderBrush;
                _resetButtons[idx].Visibility = Visibility.Collapsed;
                _isSuppressingUndo = false;
                UpdateCustomLabelCount();
            }
        }

        private void CustomLabelBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isSuppressingUndo) return;
            if (sender is not TextBox tb || tb.Tag is not int idx) return;
            if (!tb.IsFocused) return;

            PushUndo();
            string val = string.IsNullOrWhiteSpace(tb.Text) ? null : tb.Text;
            _fields[idx].DeviceCustomLabel = val;

            bool hasCustom = val != null;
            tb.BorderBrush = hasCustom ? FieldSetupTable.FilledBorderBrush : FieldSetupTable.DefaultBorderBrush;
            _resetButtons[idx].Visibility = hasCustom ? Visibility.Visible : Visibility.Collapsed;
            UpdateCustomLabelCount();
        }

        private void ResetSingleLabel_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not int idx) return;
            PushUndo();

            var field = _fields[idx];
            _isSuppressingUndo = true;
            field.DeviceCustomLabel = null;

            var tb = _customLabelBoxes[idx];
            tb.Text = field.FieldName;
            tb.Foreground = FieldSetupTable.PlaceholderBrush;
            tb.BorderBrush = FieldSetupTable.DefaultBorderBrush;
            _resetButtons[idx].Visibility = Visibility.Collapsed;
            _isSuppressingUndo = false;

            UpdateCustomLabelCount();
        }

        private void ResetAllLabels_Click(object sender, RoutedEventArgs e)
        {
            int count = _fields.Count(f => !string.IsNullOrEmpty(f.DeviceCustomLabel));
            if (count == 0) return;

            var result = AppDialog.Show(
                $"This will clear all {count} device-level custom label{(count == 1 ? "" : "s")} and revert to the system defaults.\n\nThis action takes effect on save.",
                Strings.DeviceFieldSetupView_ResetAllCustomLabelsTitle,
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            PushUndo();
            _isSuppressingUndo = true;

            for (int i = 0; i < _fields.Count; i++)
            {
                if (string.IsNullOrEmpty(_fields[i].DeviceCustomLabel)) continue;
                _fields[i].DeviceCustomLabel = null;

                if (_customLabelBoxes.TryGetValue(i, out var tb))
                {
                    tb.Text = _fields[i].FieldName;
                    tb.Foreground = FieldSetupTable.PlaceholderBrush;
                    tb.BorderBrush = FieldSetupTable.DefaultBorderBrush;
                }
                if (_resetButtons.TryGetValue(i, out var btn))
                    btn.Visibility = Visibility.Collapsed;
            }

            _isSuppressingUndo = false;
            UpdateCustomLabelCount();
        }

        private void UpdateCustomLabelCount()
        {
            int count = _fields.Count(f => !string.IsNullOrEmpty(f.DeviceCustomLabel));
            ResetAllLabelsButton.IsEnabled = count > 0;
        }

        // Row Checkbox Handlers

        private void ActiveCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized || _isSuppressingUndo) return;
            if (sender is CheckBox cb && cb.Tag is int idx)
            {
                PushUndo();
                _fields[idx].IsActive = cb.IsChecked == true;
                UpdateSelectAllStates();
            }
        }

        private void MandatoryCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized || _isSuppressingUndo) return;
            if (sender is CheckBox cb && cb.Tag is int idx)
            {
                PushUndo();
                _fields[idx].IsMandatory = cb.IsChecked == true;
                UpdateSelectAllStates();
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PushUndo();
        }

        // Select-All Header Checkboxes

        private void ActiveSelectAll_Click(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized || _isSuppressingUndo) return;
            PushUndo();

            bool newValue = !_fields.All(f => f.IsActive);

            _isSuppressingUndo = true;
            foreach (var field in _fields)
                field.IsActive = newValue;
            ActiveSelectAllCb.IsChecked = newValue;
            BuildFieldTable();
            UpdateSelectAllStates();
            _isSuppressingUndo = false;
        }

        private void MandatorySelectAll_Click(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized || _isSuppressingUndo) return;
            PushUndo();

            bool newValue = !_fields.All(f => f.IsMandatory);

            _isSuppressingUndo = true;
            foreach (var field in _fields)
                field.IsMandatory = newValue;
            MandatorySelectAllCb.IsChecked = newValue;
            BuildFieldTable();
            UpdateSelectAllStates();
            _isSuppressingUndo = false;
        }

        /// <summary>
        /// Refreshes the header checkboxes to reflect the current mix of row states:
        /// checked if all rows are on, unchecked if all off, indeterminate (null) if mixed.
        /// </summary>
        private void UpdateSelectAllStates()
        {
            _isSuppressingUndo = true;
            FieldSetupTable.SyncHeaderCheckBox(ActiveSelectAllCb, _fields.Select(f => f.IsActive));
            FieldSetupTable.SyncHeaderCheckBox(MandatorySelectAllCb, _fields.Select(f => f.IsMandatory));
            _isSuppressingUndo = false;
        }


        // Snapshot and Undo

        private FieldSetupSnapshot TakeSnapshot()
        {
            var states = _fields.Select(f => (f.DeviceCustomLabel, f.IsActive, f.IsMandatory)).ToList();
            return new FieldSetupSnapshot(PatientIdRuleCombo.SelectedIndex, states);
        }

        private void RestoreSnapshot(FieldSetupSnapshot snap)
        {
            _isSuppressingUndo = true;

            PatientIdRuleCombo.SelectedIndex = snap.PatientIdRuleIndex;

            for (int i = 0; i < _fields.Count && i < snap.FieldStates.Count; i++)
            {
                _fields[i].DeviceCustomLabel = snap.FieldStates[i].DeviceCustomLabel;
                _fields[i].IsActive = snap.FieldStates[i].Active;
                _fields[i].IsMandatory = snap.FieldStates[i].Mandatory;
            }

            BuildFieldTable();
            UpdateSelectAllStates();
            UpdateCustomLabelCount();
            _isSuppressingUndo = false;
        }

        private void PushUndo()
        {
            if (!_isInitialized || _isSuppressingUndo) return;
            _undoStack.Push(TakeSnapshot());
        }

        // Public API (called by toolbar)

        /// <summary>Saves the current state as the new baseline and clears the undo stack.</summary>
        // TODO: HandleSave has no actual persistence — same pattern as other config views.
        public void HandleSave()
        {
            _savedState = TakeSnapshot();
            _undoStack.Clear();
            AppDialog.Show(Strings.DeviceFieldSetupView_FieldSetupSaved, Strings.DeviceFieldSetupView_Save,
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>Reverts all changes back to the last saved state.</summary>
        public void HandleRevert()
        {
            if (_savedState == null) return;
            PushUndo();
            RestoreSnapshot(_savedState);
        }

        /// <summary>Restores the previous state from the undo stack.</summary>
        public void HandleUndo()
        {
            if (_undoStack.Count == 0) return;
            var prev = _undoStack.Pop();
            RestoreSnapshot(prev);
        }
    }
}