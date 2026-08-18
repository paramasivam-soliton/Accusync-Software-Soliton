// --------------------------------------------------------------------------------
// <copyright file="FieldSetupConfigView.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AccuSync.Application.Models;
using AccuSync.WPF.Helpers;
using AccuSync.WPF.Resources;
using AccuSync.WPF.Controls;

namespace AccuSync.WPF.Views.SystemConfiguration
{
    // NOTE: This view and DeviceFieldSetupView share ~90% of their code. The only difference
    // is the QR column. Extract a shared FieldSetupBase with a configurable column set.
    /// <summary>
    /// Toolbar takeover view for system-wide configuration of which patient fields are
    /// active, mandatory, and included in the QR code.
    /// </summary>
    public partial class FieldSetupConfigView : UserControl
    {
        private bool _isInitialized = false;
        private bool _isSuppressingUndo = false;

        private List<FieldEntry> _fields;


        private record FieldSetupSnapshot(
            int PatientIdRuleIndex,
            List<(string CustomLabel, bool Active, bool Mandatory, bool QR)> FieldStates
        );

        private FieldSetupSnapshot _savedState;
        private readonly Stack<FieldSetupSnapshot> _undoStack = new();

        // Per-row UI references for custom label management
        private readonly Dictionary<int, TextBox> _customLabelBoxes = new();
        private readonly Dictionary<int, Button> _resetButtons = new();

        /// <summary>
        /// Initializes the control and builds the field table once loaded.
        /// </summary>
        public FieldSetupConfigView()
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

        // TODO: Field list is hardcoded — same list as DeviceFieldSetupView. Load from DB and share.
        private void InitializeFields()
        {
            _fields = new List<FieldEntry>
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
            {
                var field = _fields[i];
                var row = CreateFieldRow(field, i);
                FieldTableBody.Children.Add(row);
            }
        }

        private Border CreateFieldRow(FieldEntry field, int index)
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
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
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
            bool hasCustom = !string.IsNullOrEmpty(field.CustomLabel);
            var labelContainer = new Grid { Margin = new Thickness(0, 0, 8, 0) };

            var labelBox = new TextBox
            {
                Text = hasCustom ? field.CustomLabel : field.FieldName,
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

            // Col 4: Include QR
            var qrCb = new CheckBox
            {
                IsChecked = field.IncludeInQR,
                Style = (Style)FindResource("FilledCheckBoxStyle"),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Tag = index,
            };
            qrCb.Checked += QRCheckBox_Changed;
            qrCb.Unchecked += QRCheckBox_Changed;
            Grid.SetColumn(qrCb, 4);
            grid.Children.Add(qrCb);

            row.Child = grid;
            return row;
        }

        // Custom Label Handlers

        private void CustomLabelBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is not TextBox tb || tb.Tag is not int idx) return;
            var field = _fields[idx];
            if (string.IsNullOrEmpty(field.CustomLabel))
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
                field.CustomLabel = null;
                tb.Text = field.FieldName;
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
            _fields[idx].CustomLabel = val;

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
            field.CustomLabel = null;

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
            int count = _fields.Count(f => !string.IsNullOrEmpty(f.CustomLabel));
            if (count == 0) return;

            var result = AppDialog.Show(
                $"This will clear all {count} custom label{(count == 1 ? "" : "s")} and revert every field back to its original default name.\n\nThis action takes effect on save.",
                Strings.FieldSetupConfigView_ResetAllLabelsConfirmTitle,
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            PushUndo();
            _isSuppressingUndo = true;

            for (int i = 0; i < _fields.Count; i++)
            {
                if (string.IsNullOrEmpty(_fields[i].CustomLabel)) continue;
                _fields[i].CustomLabel = null;

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
            int count = _fields.Count(f => !string.IsNullOrEmpty(f.CustomLabel));
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

        private void QRCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized || _isSuppressingUndo) return;
            if (sender is CheckBox cb && cb.Tag is int idx)
            {
                PushUndo();
                _fields[idx].IncludeInQR = cb.IsChecked == true;
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

        private void QRSelectAll_Click(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized || _isSuppressingUndo) return;
            PushUndo();

            bool newValue = !_fields.All(f => f.IncludeInQR);

            _isSuppressingUndo = true;
            foreach (var field in _fields)
                field.IncludeInQR = newValue;
            QRSelectAllCb.IsChecked = newValue;
            BuildFieldTable();
            UpdateSelectAllStates();
            _isSuppressingUndo = false;
        }

        /// <summary>
        /// Refreshes all three header checkboxes to reflect the current mix of row states:
        /// checked if all rows are on, unchecked if all off, indeterminate (null) if mixed.
        /// </summary>
        private void UpdateSelectAllStates()
        {
            _isSuppressingUndo = true;
            FieldSetupTable.SyncHeaderCheckBox(ActiveSelectAllCb, _fields.Select(f => f.IsActive));
            FieldSetupTable.SyncHeaderCheckBox(MandatorySelectAllCb, _fields.Select(f => f.IsMandatory));
            FieldSetupTable.SyncHeaderCheckBox(QRSelectAllCb, _fields.Select(f => f.IncludeInQR));
            _isSuppressingUndo = false;
        }


        // Snapshot and Undo

        private FieldSetupSnapshot TakeSnapshot()
        {
            var states = _fields.Select(f => (f.CustomLabel, f.IsActive, f.IsMandatory, f.IncludeInQR)).ToList();
            return new FieldSetupSnapshot(
                PatientIdRuleCombo.SelectedIndex,
                states
            );
        }

        private void RestoreSnapshot(FieldSetupSnapshot snap)
        {
            _isSuppressingUndo = true;

            PatientIdRuleCombo.SelectedIndex = snap.PatientIdRuleIndex;

            for (int i = 0; i < _fields.Count && i < snap.FieldStates.Count; i++)
            {
                _fields[i].CustomLabel = snap.FieldStates[i].CustomLabel;
                _fields[i].IsActive = snap.FieldStates[i].Active;
                _fields[i].IsMandatory = snap.FieldStates[i].Mandatory;
                _fields[i].IncludeInQR = snap.FieldStates[i].QR;
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

        // Public API

        // TODO: HandleSave has no actual persistence.
        /// <summary>
        /// Saves the current state as the new baseline and clears the undo stack.
        /// </summary>
        public void HandleSave()
        {
            _savedState = TakeSnapshot();
            _undoStack.Clear();
            AppDialog.Show(Strings.FieldSetupConfigView_ConfigurationSaved, Strings.FieldSetupConfigView_Save,
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