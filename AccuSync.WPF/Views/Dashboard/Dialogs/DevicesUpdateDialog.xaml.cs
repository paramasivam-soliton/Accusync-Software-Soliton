// --------------------------------------------------------------------------------
// <copyright file="DevicesUpdateDialog.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AccuSync.Application.Models;
using AccuSync.Presentation.Models;
using AccuSync.WPF.Resources;
using AccuSync.WPF.Controls;

namespace AccuSync.WPF.Views.Dashboard.Dialogs
{
    /// <summary>
    /// Dialog listing devices that need a firmware update.
    /// </summary>
    public partial class DevicesUpdateDialog : Window
    {
        private List<DeviceUpdateInfo> _allDevices;
        private List<DeviceUpdateInfo> _filteredDevices;

        private static readonly SolidColorBrush _cardHoverBorder = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#428FEC"));
        private static readonly SolidColorBrush _cardHoverBg = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f0f7ff"));
        private static readonly SolidColorBrush _cardDefaultBorder = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E5E7EB"));

        /// <summary>
        /// Initializes the dialog and loads devices needing a firmware update.
        /// </summary>
        public DevicesUpdateDialog()
        {
            InitializeComponent();
            LoadDeviceData();
        }

        // Window Chrome

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        // Data

        private void LoadDeviceData()
        {
            _allDevices = GetDevicesNeedingUpdate();
            _filteredDevices = new List<DeviceUpdateInfo>(_allDevices);
            UpdateDeviceList();
        }

        private void UpdateDeviceList()
        {
            DeviceListItems.ItemsSource = null;
            DeviceListItems.ItemsSource = _filteredDevices;
            CountText.Text = $"{_filteredDevices.Count} device{(_filteredDevices.Count != 1 ? "s" : "")}";

            if (_filteredDevices.Count == 0)
            {
                EmptyView.Visibility = Visibility.Visible;
                DeviceListView.Visibility = Visibility.Collapsed;
            }
            else
            {
                EmptyView.Visibility = Visibility.Collapsed;
                DeviceListView.Visibility = Visibility.Visible;
            }
        }

        // Search

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == Strings.DevicesUpdateDialog_SearchPlaceholder) SearchBox.Text = "";
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text)) SearchBox.Text = Strings.DevicesUpdateDialog_SearchPlaceholder;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchBox.Text == Strings.DevicesUpdateDialog_SearchPlaceholder || _allDevices == null) return;
            var q = SearchBox.Text.ToLower();

            _filteredDevices = string.IsNullOrWhiteSpace(q)
                ? new List<DeviceUpdateInfo>(_allDevices)
                : _allDevices.Where(d =>
                    d.DeviceName.ToLower().Contains(q) ||
                    d.CurrentVersion.ToLower().Contains(q) ||
                    d.RequiredVersion.ToLower().Contains(q)).ToList();

            UpdateDeviceList();
        }

        // Card Hover

        private void DeviceCard_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border card)
            {
                card.BorderBrush = _cardHoverBorder;
                card.Background = _cardHoverBg;
            }
        }

        private void DeviceCard_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border card)
            {
                card.BorderBrush = _cardDefaultBorder;
                card.Background = Brushes.White;
            }
        }

        // Update Action

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is DeviceUpdateInfo device)
            {
                var result = AppDialog.Show(
                    string.Format(Strings.DevicesUpdateDialog_ConfirmUpdateMessage, device.DeviceName, device.CurrentVersion, device.RequiredVersion),
                    Strings.DevicesUpdateDialog_ConfirmUpdateCaption, MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // TODO: Wire to actual firmware update service.
                    AppDialog.Show(
                        string.Format(Strings.DevicesUpdateDialog_UpdateCompleteMessage, device.DeviceName, device.RequiredVersion),
                        Strings.DevicesUpdateDialog_UpdateCompleteCaption, MessageBoxButton.OK, MessageBoxImage.Information);

                    _allDevices.Remove(device);
                    _filteredDevices.Remove(device);
                    UpdateDeviceList();
                }
            }
        }

        // Sample Data

        // TODO: All hardcoded test data. Replace with actual firmware check service.
        private List<DeviceUpdateInfo> GetDevicesNeedingUpdate()
        {
            return new List<DeviceUpdateInfo>
            {
                new DeviceUpdateInfo { DeviceName = "2020614", CurrentVersion = "5.1.0.40", RequiredVersion = "5.1.0.42" },
                new DeviceUpdateInfo { DeviceName = "3077519", CurrentVersion = "5.1.0.38", RequiredVersion = "5.1.0.42" }
            };
        }
    }
}
