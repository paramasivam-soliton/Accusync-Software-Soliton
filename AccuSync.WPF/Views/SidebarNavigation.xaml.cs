// --------------------------------------------------------------------------------
// <copyright file="SidebarNavigation.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AccuSync.WPF.Controls;
using AccuSync.Helpers;
using AccuSync.Presentation.ViewModels;
using AccuSync.WPF.Views.UsersProfiles;
using AccuSync.WPF.Views.About;
using AccuSync.WPF.Views.Settings;
using AccuSync.WPF.Views.DeviceManagement;
using AccuSync.WPF.Views.DeviceManagement.Dialogs;
using AccuSync.WPF.Views.SitesFacilities;
using AccuSync.WPF.Views.PatientsTests;
using AccuSync.WPF.Views.PatientsTests.Dialogs;
using AccuSync.WPF.Views.SystemConfiguration;
using AccuSync.WPF.Resources;
using AccuSync.WPF.Resources.Constants;
using AccuSync.Models;

namespace AccuSync.WPF.Views
{
    public partial class SidebarNavigation : UserControl
    {
        private string _currentView = "Dashboard";
        private string _currentUser = "Admin";
        private UserPermissionsViewModel? _currentPermissions;

        private PatientsView _patientsView;
        private UsersContentView _usersContent;
        private SitesContentView _sitesContent;
        private DevicesContentView _devicesContent;
        private SettingsContentView _settingsContent;
        private AboutContentView _aboutContent;
        private SystemConfigContentView _systemConfigContent;

        // Content takeover mode flags — only one should be true at a time
        private bool _isProfilesMode;
        private bool _isFacilitiesMode;
        private bool _isLocationsMode;
        private bool _isABRMode;
        private bool _isDPOAEMode;
        private bool _isRiskFactorsMode;
        private bool _isCommentsMode;
        private bool _isUserProfileMode;
        private bool _isSiteFacilityMode;
        private bool _isFieldSetupMode;
        private bool _isImportConfigMode;
        private bool _isExportConfigMode;
        private bool _isDeviceFieldSetupMode;

        private Border _activeNav;
        private readonly SolidColorBrush _navActiveBg = new SolidColorBrush(Color.FromArgb(0x40, 0xFF, 0xFF, 0xFF));
        private readonly SolidColorBrush _navInactiveBg = new SolidColorBrush(Color.FromArgb(0x1A, 0xFF, 0xFF, 0xFF));
        private readonly SolidColorBrush _navHoverBg = new SolidColorBrush(Color.FromArgb(0x26, 0xFF, 0xFF, 0xFF));

        public SidebarNavigation()
        {
            InitializeComponent();
            InitializeBaseScreens();
            Loaded += (s, e) => SwitchView(GetDefaultView());
        }

        /// <summary>
        /// Returns the first visible nav item as the default landing view.
        /// Dashboard takes priority when visible, otherwise falls through to Patients.
        /// </summary>
        private string GetDefaultView()
        {
            if (NavDashboard.Visibility == Visibility.Visible) return "Dashboard";
            if (NavPatients.Visibility == Visibility.Visible) return "Patients";
            if (NavUsers.Visibility == Visibility.Visible) return "Users";
            if (NavSites.Visibility == Visibility.Visible) return "Sites";
            return "Patients";
        }

        // Public API — called from AdminDashboardWindow

        public void SetCurrentUser(string username)
        {
            _currentUser = username;
            DashboardView.SetUsername(username);
        }

        /// <summary>
        /// Navigate to a specific view — maps dev screen names to internal names.
        /// Called by App.xaml.cs for dev mode skip_dashboard targeting.
        /// </summary>
        public void NavigateToView(string viewName)
        {
            switch (viewName)
            {
                case "PatientInformation":
                case "Patients":
                    SwitchView("Patients");
                    break;
                case "UserManagement":
                case "Users":
                    SwitchView("Users");
                    break;
                case "SiteManagement":
                case "Sites":
                    SwitchView("Sites");
                    break;
                case "DeviceManagement":
                case "Devices":
                    SwitchView("Devices");
                    break;
                case "SystemConfiguration":
                case "SystemConfig":
                    SwitchView("SystemConfig");
                    break;
                case "Settings":
                    SwitchView("Settings");
                    break;
                case "About":
                    SwitchView("About");
                    break;
                default:
                    SwitchView("Dashboard");
                    break;
            }
        }

        /// <summary>
        /// Shows/hides sidebar nav items based on the logged-in user's permissions.
        /// Call immediately after window construction, before Show().
        /// </summary>
        public void SetPermissions(UserPermissionsViewModel p)
        {
            NavUsers.Visibility = p.CanAccessUsers ? Visibility.Visible : Visibility.Collapsed;
            NavSites.Visibility = p.CanAccessSites ? Visibility.Visible : Visibility.Collapsed;
            NavDevices.Visibility = p.CanAccessDevices ? Visibility.Visible : Visibility.Collapsed;
            NavSystemConfig.Visibility = p.CanAccessSysConfig ? Visibility.Visible : Visibility.Collapsed;
            NavSettings.Visibility = p.CanAccessSettings ? Visibility.Visible : Visibility.Collapsed;

            _currentPermissions = p;
        }

        /// <summary>
        /// Returns the active permissions so child views can gate features.
        /// Falls back to Screener permissions if none have been set.
        /// </summary>
        public UserPermissionsViewModel GetPermissions() => _currentPermissions ?? UserPermissionsViewModel.Screener();

        // Initialization

        private void InitializeBaseScreens()
        {
            PatientsBaseView.SetRibbonDefinition(RibbonDefinitions.Patients());
            _patientsView = new PatientsView();
            PatientsBaseView.SetContent(_patientsView);
            PatientsBaseView.RibbonItemClicked += OnRibbonItemClicked;

            UsersView.SetRibbonDefinition(RibbonDefinitions.Users());
            _usersContent = new UsersContentView();
            UsersView.SetContent(_usersContent);
            UsersView.RibbonItemClicked += OnRibbonItemClicked;

            SitesView.SetRibbonDefinition(RibbonDefinitions.Sites());
            _sitesContent = new SitesContentView();
            SitesView.SetContent(_sitesContent);
            SitesView.RibbonItemClicked += OnRibbonItemClicked;

            DevicesView.SetRibbonDefinition(RibbonDefinitions.Devices());
            _devicesContent = new DevicesContentView();
            DevicesView.SetContent(_devicesContent);
            DevicesView.RibbonItemClicked += OnRibbonItemClicked;

            SystemConfigView.SetRibbonDefinition(RibbonDefinitions.SystemConfig());
            _systemConfigContent = new SystemConfigContentView();
            SystemConfigView.SetContent(_systemConfigContent);
            SystemConfigView.RibbonItemClicked += OnRibbonItemClicked;

            SettingsView.SetRibbonDefinition(RibbonDefinitions.Settings());
            _settingsContent = new SettingsContentView();
            SettingsView.SetContent(_settingsContent);
            SettingsView.RibbonItemClicked += OnRibbonItemClicked;

            AboutView.SetRibbonDefinition(RibbonDefinitions.About());
            _aboutContent = new AboutContentView();
            AboutView.SetContent(_aboutContent);
            AboutView.RibbonItemClicked += OnRibbonItemClicked;

            DashboardView.NavigateRequested += target => SwitchView(target);
        }

        /// <summary>
        /// Creates a styled placeholder panel for screens not yet implemented.
        /// </summary>
        private UIElement CreatePlaceholderContent(string title, string description)
        {
            var card = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(40),
                Margin = new Thickness(0),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black,
                    Direction = 270,
                    ShadowDepth = 2,
                    BlurRadius = 10,
                    Opacity = 0.08
                }
            };

            var stack = new StackPanel();

            var iconBorder = new Border
            {
                Width = 64,
                Height = 64,
                CornerRadius = new CornerRadius(16),
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 20)
            };
            iconBorder.Background = new LinearGradientBrush(
                (Color)ColorConverter.ConvertFromString("#473089"),
                (Color)ColorConverter.ConvertFromString("#6B4BA8"), 0);
            var iconPath = new System.Windows.Shapes.Path
            {
                Width = 28,
                Height = 28,
                Stretch = Stretch.Uniform,
                Fill = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Data = Geometry.Parse(RibbonIcons.FieldSetup)
            };
            iconBorder.Child = iconPath;
            stack.Children.Add(iconBorder);

            stack.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#111827")),
                Margin = new Thickness(0, 0, 0, 8)
            });

            stack.Children.Add(new TextBlock
            {
                Text = description,
                FontSize = 14,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7280")),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 22,
                Margin = new Thickness(0, 0, 0, 20)
            });

            var badge = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F3F0FF")),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(12, 6, 12, 6),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            badge.Child = new TextBlock
            {
                Text = "⚙️  " + Strings.SidebarNavigation_ContentImplementationPending,
                FontSize = 12,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#473089")),
                FontWeight = FontWeights.Medium
            };
            stack.Children.Add(badge);

            card.Child = stack;
            return card;
        }

        // Ribbon Button Routing
        // NOTE: The action handlers below duplicate the same view+takeover dispatch pattern
        // for Add, Delete, Save, Revert, and Undo. Consider extracting into a dispatch table
        // keyed by (_currentView, active takeover mode) to reduce the repetition.

        private void OnRibbonItemClicked(object sender, RibbonItemClickEventArgs e)
        {
            Debug.WriteLine($"[Ribbon] {_currentView} → {e.ItemName}");

            switch (e.ItemName)
            {
                case "Add": HandleAdd(); break;
                case "Edit": HandleEdit(); break;
                case "Delete": HandleDelete(); break;
                case "Save": HandleSave(); break;
                case "Revert": HandleRevert(); break;
                case "Undo": HandleUndo(); break;
                case "Help": HandleHelp(); break;

                case "Unlock":
                    if (_currentView == "Users") _usersContent.HandleUnlock();
                    break;

                case "Profiles": EnterProfilesMode(); break;

                case "Back":
                    if (_isProfilesMode) ExitProfilesMode();
                    else if (_isFacilitiesMode) ExitFacilitiesMode();
                    else if (_isLocationsMode) ExitLocationsMode();
                    else if (_isABRMode) ExitABRMode();
                    else if (_isDPOAEMode) ExitDPOAEMode();
                    else if (_isRiskFactorsMode) ExitRiskFactorsMode();
                    else if (_isCommentsMode) ExitCommentsMode();
                    else if (_isUserProfileMode) ExitUserProfileMode();
                    else if (_isSiteFacilityMode) ExitSiteFacilityMode();
                    else if (_isFieldSetupMode) ExitFieldSetupMode();
                    else if (_isImportConfigMode) ExitImportConfigMode();
                    else if (_isExportConfigMode) ExitExportConfigMode();
                    else if (_isDeviceFieldSetupMode) ExitDeviceFieldSetupMode();
                    break;

                case "Facilities": EnterFacilitiesMode(); break;
                case "Location": EnterLocationsMode(); break;

                case "ABR": EnterABRMode(); break;
                case "DPOAE": EnterDPOAEMode(); break;
                case "Firmware": HandleFirmware(); break;
                case "DeviceFieldSetup": EnterDeviceFieldSetupMode(); break;

                case "RiskFactors": EnterRiskFactorsMode(); break;
                case "Comments": EnterCommentsMode(); break;
                case "FieldSetup": EnterFieldSetupMode(); break;
                case "UserProfile": EnterUserProfileMode(); break;
                case "SiteFacility": EnterSiteFacilityMode(); break;
                case "ImportConfig": EnterImportConfigMode(); break;
                case "ExportConfig": EnterExportConfigMode(); break;

                case "Import": HandleImport(); break;
                case "Export": HandleExport(); break;
                case "Print": HandlePrint(); break;

                case "CopyInfo":
                    Clipboard.SetText("AccuSync v1.0.0 — Build 2025.10.17");
                    break;
                case "Send":
                    AppDialog.Show(
                        Strings.SidebarNavigation_SendMessage,
                        Strings.SidebarNavigation_SendToDeviceCaption, MessageBoxButton.OK, MessageBoxImage.Information);
                    break;

                case "Receive":
                    AppDialog.Show(
                        Strings.SidebarNavigation_ReceiveMessage,
                        Strings.SidebarNavigation_ReceiveFromDeviceCaption, MessageBoxButton.OK, MessageBoxImage.Information);
                    break;

                case "UpdateDevice":
                    string screen = _currentView ?? "Unknown";
                    AppDialog.Show(
                        string.Format(Strings.SidebarNavigation_UpdateDeviceMessage, screen),
                        Strings.SidebarNavigation_UpdateDeviceCaption, MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                default:
                    Debug.WriteLine($"[Ribbon] Unhandled: {e.ItemName}");
                    break;
            }
        }

        // Action Handlers

        private void HandleAdd()
        {
            if (_currentView == "Patients")
            {
                _patientsView.StartAddPatient();
                return;
            }
            if (_currentView == "Users")
            {
                if (_isProfilesMode) { _usersContent.ProfilesView.HandleAdd(); return; }
                _usersContent.HandleAdd();
                return;
            }
            if (_currentView == "Sites")
            {
                if (_isFacilitiesMode) { _sitesContent.FacilitiesView.HandleAdd(); return; }
                if (_isLocationsMode) { _sitesContent.LocationsView.HandleAdd(); return; }
                _sitesContent.HandleAdd();
                return;
            }
            if (_currentView == "Devices")
            {
                if (_isABRMode) { _devicesContent.ABRConfigView.HandleAdd(); return; }
                if (_isDPOAEMode) { _devicesContent.DPOAEConfigView.HandleAdd(); return; }
                _devicesContent.HandleAdd();
                return;
            }

            if (_currentView == "SystemConfig")
            {
                if (_isRiskFactorsMode) { _systemConfigContent.RiskFactorsView.HandleAdd(); return; }
                if (_isCommentsMode) { _systemConfigContent.CommentsView.HandleAdd(); return; }
                if (_isImportConfigMode) { _systemConfigContent.ImportConfigView.HandleAdd(); return; }
                if (_isExportConfigMode) { _systemConfigContent.ExportConfigView.HandleAdd(); return; }
            }

            AppDialog.Show(string.Format(Strings.SidebarNavigation_AddNewMessage, GetEntityName(_currentView)),
                Strings.SidebarNavigation_AddCaption, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void HandleEdit()
        {
            AppDialog.Show(string.Format(Strings.SidebarNavigation_EditSelectedMessage, GetEntityName(_currentView)),
                Strings.SidebarNavigation_EditCaption, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void HandleDelete()
        {
            if (_currentView == "Users")
            {
                if (_isProfilesMode) { _usersContent.ProfilesView.HandleDelete(); return; }
                _usersContent.HandleDelete();
                return;
            }
            if (_currentView == "Sites")
            {
                if (_isFacilitiesMode) { _sitesContent.FacilitiesView.HandleDelete(); return; }
                if (_isLocationsMode) { _sitesContent.LocationsView.HandleDelete(); return; }
                _sitesContent.HandleDelete();
                return;
            }
            if (_currentView == "Devices")
            {
                if (_isABRMode) { _devicesContent.ABRConfigView.HandleDelete(); return; }
                if (_isDPOAEMode) { _devicesContent.DPOAEConfigView.HandleDelete(); return; }
                _devicesContent.HandleDelete();
                return;
            }

            if (_currentView == "SystemConfig")
            {
                if (_isRiskFactorsMode) { _systemConfigContent.RiskFactorsView.HandleDelete(); return; }
                if (_isCommentsMode) { _systemConfigContent.CommentsView.HandleDelete(); return; }
                if (_isImportConfigMode) { _systemConfigContent.ImportConfigView.HandleDelete(); return; }
                if (_isExportConfigMode) { _systemConfigContent.ExportConfigView.HandleDelete(); return; }
            }

            var result = AppDialog.Show(
                string.Format(Strings.SidebarNavigation_ConfirmDeleteMessage, GetEntityName(_currentView)),
                Strings.SidebarNavigation_ConfirmDeleteCaption, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                AppDialog.Show(Strings.SidebarNavigation_DeleteMessage, Strings.SidebarNavigation_DeleteCaption, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void HandleSave()
        {
            if (_currentView == "Settings")
            {
                _settingsContent.SaveState();
                return;
            }
            if (_currentView == "Users")
            {
                if (_isProfilesMode) { _usersContent.ProfilesView.HandleSave(); return; }
                _usersContent.HandleSave();
                return;
            }
            if (_currentView == "Sites")
            {
                if (_isFacilitiesMode) { _sitesContent.FacilitiesView.HandleSave(); return; }
                if (_isLocationsMode) { _sitesContent.LocationsView.HandleSave(); return; }
                _sitesContent.HandleSave();
                return;
            }
            if (_currentView == "Devices")
            {
                if (_isABRMode) { _devicesContent.ABRConfigView.HandleSave(); return; }
                if (_isDPOAEMode) { _devicesContent.DPOAEConfigView.HandleSave(); return; }
                if (_isDeviceFieldSetupMode) { _devicesContent.FieldSetupConfigView.HandleSave(); return; }
                _devicesContent.HandleSave();
                return;
            }

            if (_currentView == "SystemConfig")
            {
                if (_isRiskFactorsMode) { _systemConfigContent.RiskFactorsView.HandleSave(); return; }
                if (_isCommentsMode) { _systemConfigContent.CommentsView.HandleSave(); return; }
                if (_isFieldSetupMode) { _systemConfigContent.FieldSetupView.HandleSave(); return; }
                if (_isUserProfileMode) { _systemConfigContent.UserProfileConfigView.HandleSave(); return; }
                if (_isSiteFacilityMode) { _systemConfigContent.SiteFacilityConfigView.HandleSave(); return; }
                if (_isImportConfigMode) { _systemConfigContent.ImportConfigView.HandleSave(); return; }
                if (_isExportConfigMode) { _systemConfigContent.ExportConfigView.HandleSave(); return; }
                _systemConfigContent.SaveState();
                return;
            }

            AppDialog.Show(string.Format(Strings.SidebarNavigation_SaveChangesMessage, _currentView),
                Strings.SidebarNavigation_SaveCaption, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void HandleRevert()
        {
            if (_currentView == "Settings")
            {
                _settingsContent.Revert(); return;
            }
            if (_currentView == "Users")
            {
                if (_isProfilesMode) { _usersContent.ProfilesView.HandleRevert(); return; }
                _usersContent.HandleRevert();
                return;
            }
            if (_currentView == "Sites")
            {
                if (_isFacilitiesMode) { _sitesContent.FacilitiesView.HandleRevert(); return; }
                if (_isLocationsMode) { _sitesContent.LocationsView.HandleRevert(); return; }
                _sitesContent.HandleRevert();
                return;
            }
            if (_currentView == "Devices")
            {
                if (_isABRMode) { _devicesContent.ABRConfigView.HandleRevert(); return; }
                if (_isDPOAEMode) { _devicesContent.DPOAEConfigView.HandleRevert(); return; }
                if (_isDeviceFieldSetupMode) { _devicesContent.FieldSetupConfigView.HandleRevert(); return; }
                _devicesContent.HandleRevert();
                return;
            }
            if (_currentView == "SystemConfig")
            {
                if (_isRiskFactorsMode) { _systemConfigContent.RiskFactorsView.HandleRevert(); return; }
                if (_isCommentsMode) { _systemConfigContent.CommentsView.HandleRevert(); return; }
                if (_isFieldSetupMode) { _systemConfigContent.FieldSetupView.HandleRevert(); return; }
                if (_isUserProfileMode) { _systemConfigContent.UserProfileConfigView.HandleRevert(); return; }
                if (_isSiteFacilityMode) { _systemConfigContent.SiteFacilityConfigView.HandleRevert(); return; }
                if (_isImportConfigMode) { _systemConfigContent.ImportConfigView.HandleRevert(); return; }
                if (_isExportConfigMode) { _systemConfigContent.ExportConfigView.HandleRevert(); return; }

                _systemConfigContent.Revert();
                return;
            }

            AppDialog.Show(string.Format(Strings.SidebarNavigation_RevertChangesMessage, _currentView),
                Strings.SidebarNavigation_RevertCaption, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void HandleUndo()
        {
            if (_currentView == "Settings")
            {
                _settingsContent.Undo(); return;
            }
            if (_currentView == "Users")
            {
                if (_isProfilesMode) { _usersContent.ProfilesView.HandleUndo(); return; }
                _usersContent.HandleUndo();
                return;
            }
            if (_currentView == "Sites")
            {
                if (_isFacilitiesMode) { _sitesContent.FacilitiesView.HandleUndo(); return; }
                if (_isLocationsMode) { _sitesContent.LocationsView.HandleUndo(); return; }
                _sitesContent.HandleUndo();
                return;
            }
            if (_currentView == "Devices")
            {
                if (_isABRMode) { _devicesContent.ABRConfigView.HandleUndo(); return; }
                if (_isDPOAEMode) { _devicesContent.DPOAEConfigView.HandleUndo(); return; }
                if (_isDeviceFieldSetupMode) { _devicesContent.FieldSetupConfigView.HandleUndo(); return; }
                _devicesContent.HandleUndo(); return;
            }

            if (_currentView == "SystemConfig")
            {
                if (_isRiskFactorsMode) { _systemConfigContent.RiskFactorsView.HandleUndo(); return; }
                if (_isCommentsMode) { _systemConfigContent.CommentsView.HandleUndo(); return; }
                if (_isFieldSetupMode) { _systemConfigContent.FieldSetupView.HandleUndo(); return; }
                if (_isUserProfileMode) { _systemConfigContent.UserProfileConfigView.HandleUndo(); return; }
                if (_isSiteFacilityMode) { _systemConfigContent.SiteFacilityConfigView.HandleUndo(); return; }
                if (_isImportConfigMode) { _systemConfigContent.ImportConfigView.HandleUndo(); return; }
                if (_isExportConfigMode) { _systemConfigContent.ExportConfigView.HandleUndo(); return; }
                _systemConfigContent.Undo();
                return;
            }

            AppDialog.Show(string.Format(Strings.SidebarNavigation_UndoActionMessage, _currentView),
                Strings.SidebarNavigation_UndoCaption, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void HandleHelp()
        {
            AppDialog.Show(string.Format(Strings.SidebarNavigation_HelpMessage, _currentView),
                Strings.SidebarNavigation_HelpCaption, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void HandleImport()
        {
            if (_currentView == "Patients")
                _patientsView.StartImport();
        }

        private void HandleExport()
        {
            if (_currentView == "Patients")
            {
                var dialog = new ExportDialog();
                dialog.Owner = Window.GetWindow(this);

                if (dialog.ShowDialog() == true)
                {
                    string format = dialog.SelectedFormat;
                    ExportScope scope = dialog.SelectedScope;
                    DateTime? from = dialog.DateFrom;
                    DateTime? to = dialog.DateTo;
                    bool deidentify = dialog.IsDeidentified;

                    AppDialog.Show(
                        $"Export requested:\n" +
                        $"Format: {format}\n" +
                        $"Scope: {scope}\n" +
                        (scope == ExportScope.DateRange ? $"Range: {from:d} – {to:d}\n" : "") +
                        $"Deidentify: {deidentify}",
                        Strings.SidebarNavigation_ExportCaption, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void HandlePrint()
        {
            if (_currentView == "Patients")
            {
                var dialog = new PatientsTests.Dialogs.PrintDialog();
                dialog.Owner = Window.GetWindow(this);

                if (dialog.ShowDialog() == true)
                {
                    PrintReportCategory category = dialog.SelectedCategory;
                    PrintReportType report = dialog.SelectedReport;

                    AppDialog.Show(
                        string.Format(Strings.SidebarNavigation_PrintRequestedMessage, category, report),
                        Strings.SidebarNavigation_PrintCaption, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void HandleFirmware()
        {
            var deviceInfo = _devicesContent.GetSelectedDeviceInfo();

            var dialog = new FirmwareUpdateDialog
            {
                DeviceName = deviceInfo.Name ?? "—",
                CurrentFirmware = deviceInfo.FirmwareVersion ?? "—",
                HardwareVersion = deviceInfo.HardwareVersion ?? "—"
            };
            dialog.Owner = Window.GetWindow(this);

            if (dialog.ShowDialog() == true)
            {
                string folder = dialog.SelectedFolderPath;
                string file = dialog.FirmwareFileName;

                AppDialog.Show(
                    $"Firmware update requested:\n" +
                    $"Device: {deviceInfo.Name}\n" +
                    $"Folder: {folder}\n" +
                    (file != null ? $"File: {file}\n" : "No firmware file detected in folder.\n") +
                    $"Current FW: {deviceInfo.FirmwareVersion}",
                    Strings.SidebarNavigation_FirmwareUpdateCaption, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private string GetEntityName(string screen) => screen switch
        {
            "Users" => "user",
            "Sites" => "site",
            "Devices" => "device",
            "SystemConfig" => "configuration",
            "Settings" => "setting",
            "Patients" => "patient",
            _ => "item"
        };

        // Content Takeover Modes
        // Each Enter/Exit pair swaps the ribbon definition and toggles the child view.

        private void EnterProfilesMode()
        {
            _isProfilesMode = true;
            UsersView.SetRibbonDefinition(RibbonDefinitions.Profiles());
            _usersContent.ShowProfiles();
        }

        private void ExitProfilesMode()
        {
            _isProfilesMode = false;
            UsersView.SetRibbonDefinition(RibbonDefinitions.Users());
            _usersContent.HideProfiles();
        }

        private void EnterFacilitiesMode()
        {
            _isFacilitiesMode = true;
            SitesView.SetRibbonDefinition(RibbonDefinitions.Facilities());
            _sitesContent.ShowFacilities();
        }

        private void ExitFacilitiesMode()
        {
            _isFacilitiesMode = false;
            SitesView.SetRibbonDefinition(RibbonDefinitions.Sites());
            _sitesContent.HideFacilities();
        }

        private void EnterLocationsMode()
        {
            _isLocationsMode = true;
            SitesView.SetRibbonDefinition(RibbonDefinitions.Locations());
            _sitesContent.ShowLocations();
        }

        private void ExitLocationsMode()
        {
            _isLocationsMode = false;
            SitesView.SetRibbonDefinition(RibbonDefinitions.Sites());
            _sitesContent.HideLocations();
        }

        private void EnterABRMode()
        {
            _isABRMode = true;
            DevicesView.SetRibbonDefinition(RibbonDefinitions.ABR());
            _devicesContent.ShowABR();
        }

        private void ExitABRMode()
        {
            _isABRMode = false;
            DevicesView.SetRibbonDefinition(RibbonDefinitions.Devices());
            _devicesContent.HideABR();
        }

        private void EnterDPOAEMode()
        {
            _isDPOAEMode = true;
            DevicesView.SetRibbonDefinition(RibbonDefinitions.DPOAE());
            _devicesContent.ShowDPOAE();
        }

        private void ExitDPOAEMode()
        {
            _isDPOAEMode = false;
            DevicesView.SetRibbonDefinition(RibbonDefinitions.Devices());
            _devicesContent.HideDPOAE();
        }

        private void EnterDeviceFieldSetupMode()
        {
            _isDeviceFieldSetupMode = true;
            DevicesView.SetRibbonDefinition(RibbonDefinitions.DeviceFieldSetup());
            _devicesContent.ShowFieldSetup();
        }

        private void ExitDeviceFieldSetupMode()
        {
            _isDeviceFieldSetupMode = false;
            DevicesView.SetRibbonDefinition(RibbonDefinitions.Devices());
            _devicesContent.HideFieldSetup();
        }

        private void EnterRiskFactorsMode()
        {
            _isRiskFactorsMode = true;
            SystemConfigView.SetRibbonDefinition(RibbonDefinitions.RiskFactors());
            _systemConfigContent.ShowRiskFactors();
        }

        private void ExitRiskFactorsMode()
        {
            _isRiskFactorsMode = false;
            SystemConfigView.SetRibbonDefinition(RibbonDefinitions.SystemConfig());
            _systemConfigContent.HideRiskFactors();
        }

        private void EnterCommentsMode()
        {
            _isCommentsMode = true;
            SystemConfigView.SetRibbonDefinition(RibbonDefinitions.Comments());
            _systemConfigContent.ShowComments();
        }

        private void ExitCommentsMode()
        {
            _isCommentsMode = false;
            SystemConfigView.SetRibbonDefinition(RibbonDefinitions.SystemConfig());
            _systemConfigContent.HideComments();
        }

        private void EnterFieldSetupMode()
        {
            _isFieldSetupMode = true;
            SystemConfigView.SetRibbonDefinition(RibbonDefinitions.FieldSetupConfig());
            _systemConfigContent.ShowFieldSetup();
        }

        private void ExitFieldSetupMode()
        {
            _isFieldSetupMode = false;
            SystemConfigView.SetRibbonDefinition(RibbonDefinitions.SystemConfig());
            _systemConfigContent.HideFieldSetup();
        }

        private void EnterUserProfileMode()
        {
            _isUserProfileMode = true;
            SystemConfigView.SetRibbonDefinition(RibbonDefinitions.UserProfileConfig());
            _systemConfigContent.ShowUserProfile();
        }

        private void ExitUserProfileMode()
        {
            _isUserProfileMode = false;
            SystemConfigView.SetRibbonDefinition(RibbonDefinitions.SystemConfig());
            _systemConfigContent.HideUserProfile();
        }

        private void EnterSiteFacilityMode()
        {
            _isSiteFacilityMode = true;
            SystemConfigView.SetRibbonDefinition(RibbonDefinitions.SiteFacilityConfig());
            _systemConfigContent.ShowSiteFacility();
        }

        private void ExitSiteFacilityMode()
        {
            _isSiteFacilityMode = false;
            SystemConfigView.SetRibbonDefinition(RibbonDefinitions.SystemConfig());
            _systemConfigContent.HideSiteFacility();
        }

        private void EnterImportConfigMode()
        {
            _isImportConfigMode = true;
            SystemConfigView.SetRibbonDefinition(RibbonDefinitions.ImportConfig());
            _systemConfigContent.ShowImportConfig();
        }

        private void ExitImportConfigMode()
        {
            _isImportConfigMode = false;
            SystemConfigView.SetRibbonDefinition(RibbonDefinitions.SystemConfig());
            _systemConfigContent.HideImportConfig();
        }

        private void EnterExportConfigMode()
        {
            _isExportConfigMode = true;
            SystemConfigView.SetRibbonDefinition(RibbonDefinitions.ExportConfig());
            _systemConfigContent.ShowExportConfig();
        }

        private void ExitExportConfigMode()
        {
            _isExportConfigMode = false;
            SystemConfigView.SetRibbonDefinition(RibbonDefinitions.SystemConfig());
            _systemConfigContent.HideExportConfig();
        }

        // View Switching

        private void SwitchView(string viewName)
        {
            // Exit any active takeover mode before switching views
            if (_isProfilesMode) ExitProfilesMode();
            if (_isFacilitiesMode) ExitFacilitiesMode();
            if (_isLocationsMode) ExitLocationsMode();
            if (_isABRMode) ExitABRMode();
            if (_isDPOAEMode) ExitDPOAEMode();
            if (_isRiskFactorsMode) ExitRiskFactorsMode();
            if (_isCommentsMode) ExitCommentsMode();
            if (_isUserProfileMode) ExitUserProfileMode();
            if (_isSiteFacilityMode) ExitSiteFacilityMode();
            if (_isFieldSetupMode) ExitFieldSetupMode();
            if (_isImportConfigMode) ExitImportConfigMode();
            if (_isExportConfigMode) ExitExportConfigMode();
            if (_isDeviceFieldSetupMode) ExitDeviceFieldSetupMode();
            _currentView = viewName;

            DashboardView.Visibility = Visibility.Collapsed;
            PatientsBaseView.Visibility = Visibility.Collapsed;
            UsersView.Visibility = Visibility.Collapsed;
            SitesView.Visibility = Visibility.Collapsed;
            DevicesView.Visibility = Visibility.Collapsed;
            SystemConfigView.Visibility = Visibility.Collapsed;
            SettingsView.Visibility = Visibility.Collapsed;
            AboutView.Visibility = Visibility.Collapsed;

            ResetNavHighlights();

            switch (viewName)
            {
                case "Dashboard":
                    DashboardView.Visibility = Visibility.Visible;
                    SetNavActive(NavDashboard);
                    // TODO: Swap dashboard content based on role when ScreenerDashboardWindow is retired
                    break;
                case "Patients":
                    PatientsBaseView.Visibility = Visibility.Visible;
                    SetNavActive(NavPatients);
                    break;
                case "Users":
                    UsersView.Visibility = Visibility.Visible;
                    SetNavActive(NavUsers);
                    break;
                case "Sites":
                    SitesView.Visibility = Visibility.Visible;
                    SetNavActive(NavSites);
                    break;
                case "Devices":
                    DevicesView.Visibility = Visibility.Visible;
                    SetNavActive(NavDevices);
                    break;
                case "SystemConfig":
                    SystemConfigView.Visibility = Visibility.Visible;
                    SetNavActive(NavSystemConfig);
                    break;
                case "Settings":
                    SettingsView.Visibility = Visibility.Visible;
                    SetNavActive(NavSettings);
                    break;
                case "About":
                    AboutView.Visibility = Visibility.Visible;
                    SetNavActive(NavAbout);
                    break;
            }

            Debug.WriteLine($"[Nav] Switched to: {viewName}");
        }

        private void NavDashboard_Click(object s, MouseButtonEventArgs e) => SwitchView("Dashboard");
        private void NavPatients_Click(object s, MouseButtonEventArgs e) => SwitchView("Patients");
        private void NavUsers_Click(object s, MouseButtonEventArgs e) => SwitchView("Users");
        private void NavSites_Click(object s, MouseButtonEventArgs e) => SwitchView("Sites");
        private void NavDevices_Click(object s, MouseButtonEventArgs e) => SwitchView("Devices");
        private void NavSystemConfig_Click(object s, MouseButtonEventArgs e) => SwitchView("SystemConfig");
        private void NavSettings_Click(object s, MouseButtonEventArgs e) => SwitchView("Settings");
        private void NavAbout_Click(object s, MouseButtonEventArgs e) => SwitchView("About");

        private void Logout_Click(object sender, MouseButtonEventArgs e)
        {
            if (DevModeConfig.SkipLogin)
            {
                Application.Current.Shutdown();
                return;
            }

            var result = AppDialog.Show(Strings.SidebarNavigation_ConfirmLogoutMessage,
                Strings.SidebarNavigation_ConfirmLogoutCaption, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                App.NavigateAfterLogin(Window.GetWindow(this), null, null);
            }
        }

        // Sidebar Visual State

        private void ResetNavHighlights()
        {
            NavDashboard.Background = _navInactiveBg;
            NavPatients.Background = _navInactiveBg;
            NavUsers.Background = _navInactiveBg;
            NavSites.Background = _navInactiveBg;
            NavDevices.Background = _navInactiveBg;
            NavSystemConfig.Background = _navInactiveBg;
            NavSettings.Background = _navInactiveBg;
            NavAbout.Background = _navInactiveBg;
        }

        private void SetNavActive(Border navItem)
        {
            _activeNav = navItem;
            navItem.Background = _navActiveBg;
        }

        private void NavItem_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border b && b != _activeNav)
                b.Background = _navHoverBg;
        }

        private void NavItem_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border b && b != _activeNav)
                b.Background = _navInactiveBg;
        }
    }
}