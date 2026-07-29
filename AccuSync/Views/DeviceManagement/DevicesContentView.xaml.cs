// --------------------------------------------------------------------------------
// <copyright file="DevicesContentView.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AccuSync.Models;
using AccuSync.Resources;
using AccuSync.Controls;

namespace AccuSync.Views.DeviceManagement
{
    public partial class DevicesContentView : UserControl
    {
        private ObservableCollection<DeviceEntry> _devices = new();
        private bool _isInitialized;
        private bool _isSuppressingUndo;
        private bool _isLoadingItem;

        private int _powerTimeout = 5;
        private int _displayTimeout = 3;
        private int _calibrationPause = 10;

        private readonly List<CheckBox> _userCheckBoxes = new();
        private readonly List<string> _userNames = new();
        private readonly List<CheckBox> _facilityCheckBoxes = new();
        private readonly List<string> _facilityNames = new();

        // Undo Infrastructure
        private record DeviceSnapshot(
            string Name, string Serial, string Code, int SiteIndex, int LanguageIndex,
            int TestResultTermsIndex, int PowerTimeout, int DisplayTimeout, int CalibrationPause,
            int DataDeletionIndex, int ABRAutostartIndex, int TEOAEProbeFitIndex,
            bool[] UserAssignments, bool[] FacilityAssignments
        );

        private DeviceSnapshot _savedState;
        private readonly Stack<DeviceSnapshot> _undoStack = new();
        public ABRConfigurationView ABRConfigView => ABRConfigurationControl;
        public DPOAEConfigurationView DPOAEConfigView => DPOAEConfigurationControl;
        public DeviceFieldSetupView FieldSetupConfigView => FieldSetupConfigurationControl;

        // Initialization

        public DevicesContentView()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                _isInitialized = true;
                InitializeDefaults();
            };
        }

        // TODO: All device data is hardcoded. Wire to database/service.
        private void InitializeDefaults()
        {
            _isSuppressingUndo = true;

            BuildUserAssignmentRows(new[] { "Admin", "Screener" });

            _devices.Add(new DeviceEntry
            {
                Name = "2020614",
                Serial = "2020614",
                LanguageIndex = 0,
                TestResultTermsIndex = 0,
                PowerTimeout = 5,
                DisplayTimeout = 3,
                CalibrationPause = 10,
                DataDeletionIndex = 0,
                ABRAutostartIndex = 0,
                TEOAEProbeFitIndex = 0,
                LastSeen = "02/15/2026 14:32",
                LastUpdated = "02/10/2026 09:15",
                HardwareVersion = "3.2.1",
                FirmwareVersion = "5.1.0.42"
            });

            _devices.Add(new DeviceEntry
            {
                Name = "3077519",
                Serial = "3077519",
                LanguageIndex = 0,
                TestResultTermsIndex = 0,
                PowerTimeout = 5,
                DisplayTimeout = 3,
                CalibrationPause = 10,
                DataDeletionIndex = 0,
                ABRAutostartIndex = 0,
                TEOAEProbeFitIndex = 0,
                LastSeen = "02/14/2026 10:05",
                LastUpdated = "02/08/2026 16:22",
                HardwareVersion = "3.2.1",
                FirmwareVersion = "5.1.0.40"
            });

            DevicesListView.ItemsSource = _devices;
            EmptyListPanel.Visibility = _devices.Count > 0 ? Visibility.Collapsed : Visibility.Visible;

            if (_devices.Count > 0)
                DevicesListView.SelectedIndex = 0;

            _isSuppressingUndo = false;
        }

        // User Assignment Table

        // TODO: BuildUserAssignmentRows and the facility row builder in UpdateFacilityList
        //       are ~90% identical. Extract a shared BuildAssignmentRows helper.
        private void BuildUserAssignmentRows(IEnumerable<string> loginNames)
        {
            UserAssignmentRows.Children.Clear();
            _userCheckBoxes.Clear();
            _userNames.Clear();

            foreach (var name in loginNames)
            {
                _userNames.Add(name);

                var row = new Grid();
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });

                var nameText = new TextBlock
                {
                    Text = name,
                    FontSize = 13,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333")),
                    VerticalAlignment = VerticalAlignment.Center,
                    Padding = new Thickness(14, 12, 14, 12)
                };
                Grid.SetColumn(nameText, 0);
                row.Children.Add(nameText);

                var cb = new CheckBox
                {
                    Style = (Style)FindResource("AssignCheckBoxStyle"),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                cb.Checked += Assignment_Changed;
                cb.Unchecked += Assignment_Changed;
                Grid.SetColumn(cb, 1);
                row.Children.Add(cb);
                _userCheckBoxes.Add(cb);

                var divider = new Border
                {
                    Height = 1,
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f0f0f0"))
                };

                var wrapper = new StackPanel();
                wrapper.Children.Add(row);
                wrapper.Children.Add(divider);

                UserAssignmentRows.Children.Add(wrapper);
            }
        }

        /// <summary>
        /// Called externally to refresh user list (e.g., when users are added/removed).
        /// </summary>
        public void UpdateUserList(IEnumerable<string> loginNames)
        {
            _isSuppressingUndo = true;
            BuildUserAssignmentRows(loginNames);
            _isSuppressingUndo = false;
        }

        // Facility Assignment Table

        /// <summary>
        /// Called externally to refresh facility list.
        /// </summary>
        public void UpdateFacilityList(IEnumerable<string> facilityNames)
        {
            _isSuppressingUndo = true;
            FacilityAssignmentRows.Children.Clear();
            _facilityCheckBoxes.Clear();
            _facilityNames.Clear();

            var names = facilityNames.ToList();

            if (names.Count == 0)
            {
                FacilitiesTableHeader.Visibility = Visibility.Collapsed;
                FacilitiesTableDivider.Visibility = Visibility.Collapsed;
                FacilitiesEmptyState.Visibility = Visibility.Visible;
            }
            else
            {
                FacilitiesTableHeader.Visibility = Visibility.Visible;
                FacilitiesTableDivider.Visibility = Visibility.Visible;
                FacilitiesEmptyState.Visibility = Visibility.Collapsed;

                foreach (var name in names)
                {
                    _facilityNames.Add(name);

                    var row = new Grid();
                    row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });

                    var nameText = new TextBlock
                    {
                        Text = name,
                        FontSize = 13,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333")),
                        VerticalAlignment = VerticalAlignment.Center,
                        Padding = new Thickness(14, 12, 14, 12)
                    };
                    Grid.SetColumn(nameText, 0);
                    row.Children.Add(nameText);

                    var cb = new CheckBox
                    {
                        Style = (Style)FindResource("AssignCheckBoxStyle"),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    cb.Checked += Assignment_Changed;
                    cb.Unchecked += Assignment_Changed;
                    Grid.SetColumn(cb, 1);
                    row.Children.Add(cb);
                    _facilityCheckBoxes.Add(cb);

                    var divider = new Border
                    {
                        Height = 1,
                        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f0f0f0"))
                    };

                    var wrapper = new StackPanel();
                    wrapper.Children.Add(row);
                    wrapper.Children.Add(divider);

                    FacilityAssignmentRows.Children.Add(wrapper);
                }
            }

            _isSuppressingUndo = false;
        }

        /// <summary>
        /// Called externally to refresh site dropdown.
        /// </summary>
        public void UpdateSiteList(IEnumerable<string> siteNames)
        {
            _isSuppressingUndo = true;
            var selected = SiteCombo.SelectedIndex;
            SiteCombo.Items.Clear();
            SiteCombo.Items.Add(new ComboBoxItem { Content = Strings.DevicesContentView_SelectPlaceholder });
            foreach (var name in siteNames)
                SiteCombo.Items.Add(new ComboBoxItem { Content = name });
            SiteCombo.SelectedIndex = selected >= 0 && selected < SiteCombo.Items.Count ? selected : 0;
            _isSuppressingUndo = false;
        }

        // Tab Switching

        private void ConfigTab_Click(object sender, RoutedEventArgs e) => SwitchTab("config");
        private void UsersTab_Click(object sender, RoutedEventArgs e) => SwitchTab("users");
        private void FacilitiesTab_Click(object sender, RoutedEventArgs e) => SwitchTab("facilities");

        private void SwitchTab(string tab)
        {
            ConfigTab.Style = (Style)FindResource("TabButtonStyle");
            UsersTab.Style = (Style)FindResource("TabButtonStyle");
            FacilitiesTab.Style = (Style)FindResource("TabButtonStyle");
            ConfigPanel.Visibility = Visibility.Collapsed;
            UsersPanel.Visibility = Visibility.Collapsed;
            FacilitiesPanel.Visibility = Visibility.Collapsed;

            switch (tab)
            {
                case "config":
                    ConfigTab.Style = (Style)FindResource("ActiveTabStyle");
                    ConfigPanel.Visibility = Visibility.Visible;
                    break;
                case "users":
                    UsersTab.Style = (Style)FindResource("ActiveTabStyle");
                    UsersPanel.Visibility = Visibility.Visible;
                    break;
                case "facilities":
                    FacilitiesTab.Style = (Style)FindResource("ActiveTabStyle");
                    FacilitiesPanel.Visibility = Visibility.Visible;
                    break;
            }
        }

        // List Selection

        private void DevicesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DevicesListView.SelectedItem is DeviceEntry device)
            {
                _isLoadingItem = true;
                _isSuppressingUndo = true;

                EmptyDetailPanel.Visibility = Visibility.Collapsed;
                DetailContent.Visibility = Visibility.Visible;
                DetailHeaderText.Text = device.Name;

                NameBox.Text = device.Name;
                SerialBox.Text = device.Serial;
                CodeBox.Text = device.Code;
                SiteCombo.SelectedIndex = 0;
                LanguageCombo.SelectedIndex = device.LanguageIndex;
                TestResultTermsCombo.SelectedIndex = device.TestResultTermsIndex;

                _powerTimeout = device.PowerTimeout;
                _displayTimeout = device.DisplayTimeout;
                _calibrationPause = device.CalibrationPause;
                UpdateSpinnerDisplays();

                DataDeletionCombo.SelectedIndex = device.DataDeletionIndex;
                ABRAutostartCombo.SelectedIndex = device.ABRAutostartIndex;
                TEOAEProbeFitCombo.SelectedIndex = device.TEOAEProbeFitIndex;

                LastSeenBox.Text = device.LastSeen;
                LastUpdatedBox.Text = device.LastUpdated;
                HardwareVersionBox.Text = device.HardwareVersion;
                FirmwareVersionBox.Text = device.FirmwareVersion;

                SwitchTab("config");

                _undoStack.Clear();
                _savedState = CaptureSnapshot();

                _isLoadingItem = false;
                _isSuppressingUndo = false;
            }
            else
            {
                EmptyDetailPanel.Visibility = Visibility.Visible;
                DetailContent.Visibility = Visibility.Collapsed;
                DetailHeaderText.Text = Strings.DevicesContentView_DeviceDetails;
            }
        }

        // Spinner Handlers

        private void PowerTimeout_Up(object sender, RoutedEventArgs e) => AdjustSpinner(ref _powerTimeout, 1, 1, 30, PowerTimeoutText);
        private void PowerTimeout_Down(object sender, RoutedEventArgs e) => AdjustSpinner(ref _powerTimeout, -1, 1, 30, PowerTimeoutText);
        private void DisplayTimeout_Up(object sender, RoutedEventArgs e) => AdjustSpinner(ref _displayTimeout, 1, 1, 5, DisplayTimeoutText);
        private void DisplayTimeout_Down(object sender, RoutedEventArgs e) => AdjustSpinner(ref _displayTimeout, -1, 1, 5, DisplayTimeoutText);
        private void CalibrationPause_Up(object sender, RoutedEventArgs e) => AdjustSpinner(ref _calibrationPause, 1, 1, 30, CalibrationPauseText);
        private void CalibrationPause_Down(object sender, RoutedEventArgs e) => AdjustSpinner(ref _calibrationPause, -1, 1, 30, CalibrationPauseText);

        private void AdjustSpinner(ref int value, int delta, int min, int max, TextBlock display)
        {
            int newVal = Math.Max(min, Math.Min(max, value + delta));
            if (newVal == value) return;

            PushUndo();
            value = newVal;
            display.Text = string.Format(Strings.DevicesContentView_MinutesFormat, value);
            SyncToSelectedDevice();
        }

        private void UpdateSpinnerDisplays()
        {
            PowerTimeoutText.Text = string.Format(Strings.DevicesContentView_MinutesFormat, _powerTimeout);
            DisplayTimeoutText.Text = string.Format(Strings.DevicesContentView_MinutesFormat, _displayTimeout);
            CalibrationPauseText.Text = string.Format(Strings.DevicesContentView_MinutesFormat, _calibrationPause);
        }

        // Field Change Tracking

        private void Field_Changed(object sender, RoutedEventArgs e)
        {
            PushUndo();
            SyncToSelectedDevice();
        }

        private void Field_Changed(object sender, TextChangedEventArgs e)
        {
            PushUndo();
            SyncToSelectedDevice();
        }

        private void Assignment_Changed(object sender, RoutedEventArgs e)
        {
            PushUndo();
        }

        private void SyncToSelectedDevice()
        {
            if (_isLoadingItem || !_isInitialized) return;
            if (DevicesListView.SelectedItem is not DeviceEntry device) return;

            device.Name = NameBox.Text;
            device.Serial = SerialBox.Text;
            device.Code = CodeBox.Text;
            device.LanguageIndex = LanguageCombo.SelectedIndex;
            device.TestResultTermsIndex = TestResultTermsCombo.SelectedIndex;
            device.PowerTimeout = _powerTimeout;
            device.DisplayTimeout = _displayTimeout;
            device.CalibrationPause = _calibrationPause;
            device.DataDeletionIndex = DataDeletionCombo.SelectedIndex;
            device.ABRAutostartIndex = ABRAutostartCombo.SelectedIndex;
            device.TEOAEProbeFitIndex = TEOAEProbeFitCombo.SelectedIndex;

            DetailHeaderText.Text = device.Name;
            DevicesListView.Items.Refresh();
        }

        // Snapshot and Undo

        private DeviceSnapshot CaptureSnapshot()
        {
            return new DeviceSnapshot(
                NameBox.Text,
                SerialBox.Text,
                CodeBox.Text,
                SiteCombo.SelectedIndex,
                LanguageCombo.SelectedIndex,
                TestResultTermsCombo.SelectedIndex,
                _powerTimeout,
                _displayTimeout,
                _calibrationPause,
                DataDeletionCombo.SelectedIndex,
                ABRAutostartCombo.SelectedIndex,
                TEOAEProbeFitCombo.SelectedIndex,
                _userCheckBoxes.Select(cb => cb.IsChecked == true).ToArray(),
                _facilityCheckBoxes.Select(cb => cb.IsChecked == true).ToArray()
            );
        }

        private void RestoreSnapshot(DeviceSnapshot snap)
        {
            _isLoadingItem = true;
            _isSuppressingUndo = true;

            NameBox.Text = snap.Name;
            SerialBox.Text = snap.Serial;
            CodeBox.Text = snap.Code;
            SiteCombo.SelectedIndex = snap.SiteIndex;
            LanguageCombo.SelectedIndex = snap.LanguageIndex;
            TestResultTermsCombo.SelectedIndex = snap.TestResultTermsIndex;

            _powerTimeout = snap.PowerTimeout;
            _displayTimeout = snap.DisplayTimeout;
            _calibrationPause = snap.CalibrationPause;
            UpdateSpinnerDisplays();

            DataDeletionCombo.SelectedIndex = snap.DataDeletionIndex;
            ABRAutostartCombo.SelectedIndex = snap.ABRAutostartIndex;
            TEOAEProbeFitCombo.SelectedIndex = snap.TEOAEProbeFitIndex;

            for (int i = 0; i < _userCheckBoxes.Count && i < snap.UserAssignments.Length; i++)
                _userCheckBoxes[i].IsChecked = snap.UserAssignments[i];

            for (int i = 0; i < _facilityCheckBoxes.Count && i < snap.FacilityAssignments.Length; i++)
                _facilityCheckBoxes[i].IsChecked = snap.FacilityAssignments[i];

            SyncToSelectedDevice();

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
            var newDevice = new DeviceEntry
            {
                Name = "New Device",
                Serial = "",
                LanguageIndex = 0,
                TestResultTermsIndex = 0,
                PowerTimeout = 5,
                DisplayTimeout = 3,
                CalibrationPause = 10,
                DataDeletionIndex = 0,
                ABRAutostartIndex = 0,
                TEOAEProbeFitIndex = 0
            };

            _devices.Add(newDevice);
            EmptyListPanel.Visibility = Visibility.Collapsed;
            DevicesListView.SelectedItem = newDevice;
        }

        public void HandleDelete()
        {
            if (DevicesListView.SelectedItem is not DeviceEntry device) return;

            var result = AppDialog.Show(
                string.Format(Strings.DevicesContentView_ConfirmDeleteDevice, device.Name),
                Strings.DevicesContentView_ConfirmDelete, MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            int idx = DevicesListView.SelectedIndex;
            _devices.Remove(device);
            DevicesListView.Items.Refresh();

            if (_devices.Count == 0)
            {
                EmptyListPanel.Visibility = Visibility.Visible;
                EmptyDetailPanel.Visibility = Visibility.Visible;
                DetailContent.Visibility = Visibility.Collapsed;
            }
            else
            {
                DevicesListView.SelectedIndex = Math.Min(idx, _devices.Count - 1);
            }
        }

        // TODO: HandleSave has no actual persistence — same pattern as all other config views.
        public void HandleSave()
        {
            _savedState = CaptureSnapshot();
            _undoStack.Clear();
            AppDialog.Show(Strings.DevicesContentView_DeviceSaved, Strings.DevicesContentView_Save, MessageBoxButton.OK, MessageBoxImage.Information);
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

        // Overlay View Toggling
        // TODO: Show/Hide methods for ABR, DPOAE, and FieldSetup are identical except for the
        //       control reference. Consider a single ToggleOverlay(UIElement) method.

        public void ShowABR()
        {
            NormalDevicesGrid.Visibility = Visibility.Collapsed;
            ABRConfigurationControl.Visibility = Visibility.Visible;
        }

        public void HideABR()
        {
            ABRConfigurationControl.Visibility = Visibility.Collapsed;
            NormalDevicesGrid.Visibility = Visibility.Visible;
        }

        public void ShowDPOAE()
        {
            NormalDevicesGrid.Visibility = Visibility.Collapsed;
            DPOAEConfigurationControl.Visibility = Visibility.Visible;
        }

        public void HideDPOAE()
        {
            DPOAEConfigurationControl.Visibility = Visibility.Collapsed;
            NormalDevicesGrid.Visibility = Visibility.Visible;
        }

        public void ShowFieldSetup()
        {
            NormalDevicesGrid.Visibility = Visibility.Collapsed;
            FieldSetupConfigurationControl.Visibility = Visibility.Visible;
        }

        public void HideFieldSetup()
        {
            FieldSetupConfigurationControl.Visibility = Visibility.Collapsed;
            NormalDevicesGrid.Visibility = Visibility.Visible;
        }

        public DeviceInfo GetSelectedDeviceInfo()
        {
            if (DevicesListView.SelectedItem is DeviceEntry device)
            {
                return new DeviceInfo
                {
                    Name = device.Name,
                    FirmwareVersion = device.FirmwareVersion,
                    HardwareVersion = device.HardwareVersion
                };
            }

            return new DeviceInfo
            {
                Name = "—",
                FirmwareVersion = "—",
                HardwareVersion = "—"
            };
        }
    }
}