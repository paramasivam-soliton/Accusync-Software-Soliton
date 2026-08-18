// --------------------------------------------------------------------------------
// <copyright file="ProfilesContentView.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AccuSync.Application.Models;
using AccuSync.WPF.Resources;
using AccuSync.WPF.Controls;

namespace AccuSync.WPF.Views.UsersProfiles
{
    /// <summary>
    /// Immutable snapshot of a profile's editable form state and per-permission
    /// grants, used for undo/revert.
    /// </summary>
    /// <param name="Name">The profile name.</param>
    /// <param name="Description">The profile description.</param>
    /// <param name="Permissions">The granted state of each (component, permission) pair.</param>
    public record ProfileSnapshot(
        string Name, string Description,
        List<(string Component, string Permission, bool Granted)> Permissions);

    /// <summary>
    /// Profiles screen content: profile list/detail form with a per-component
    /// permissions matrix and undo/revert support.
    /// </summary>
    public partial class ProfilesContentView : UserControl
    {
        private ObservableCollection<ProfileEntry> _profiles;
        private bool _isInitialized;
        private bool _isSuppressingUndo;
        private bool _isLoadingProfile;

        private ProfileSnapshot _savedState;
        private Stack<ProfileSnapshot> _undoStack = new();

        private readonly List<(CheckBox ComponentCb, List<CheckBox> PermissionCbs, ComponentPermissions Data)> _componentGroups = new();

        /// <summary>
        /// Initializes the control and populates the default profile list once loaded.
        /// </summary>
        public ProfilesContentView()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                _isInitialized = true;
                InitializeDefaultProfiles();
            };
        }

        // Initialization

        // TODO: Replace hardcoded profiles with data from DatabaseService
        private void InitializeDefaultProfiles()
        {
            _profiles = new ObservableCollection<ProfileEntry>
            {
                BuildProfile("Administrator", "Profile with administrative permissions", BuildAdminPermissions()),
                BuildProfile("Screener", "Profile with permission for daily work to screen babies.", BuildScreenerPermissions()),
                BuildProfile("Supervisor", "Profile with permission for daily work to manage AccuLink and screen babies.", BuildSupervisorPermissions())
            };

            ProfilesListView.ItemsSource = _profiles;
            if (_profiles.Count > 0)
                ProfilesListView.SelectedIndex = 0;
        }

        private ProfileEntry BuildProfile(string name, string desc, List<ComponentPermissions> comps)
        {
            return new ProfileEntry { Name = name, Description = desc, Components = comps };
        }

        // Permission Data

        private static List<string> AccuScreenPerms => new()
        {
            "All Tests", "Allow a user to perform a quick test",
            "Basic Tests", "Delete Patients", "Edit Patients"
        };
        private static List<string> DeviceMgmtPerms => new()
        {
            "Add and edit devices", "Configure test modules",
            "Delete devices", "View Device"
        };
        private static List<string> PatientsTestsPerms => new()
        {
            "Add and edit patients", "Comment Maintenance",
            "Configure Patient Management", "Delete patients",
            "Reassign tests", "Risk Factor Maintenance", "View patients"
        };
        private static List<string> SitesFacilitiesPerms => new()
        {
            "Add and edit facilities", "Add and edit sites",
            "Configure site and facility management",
            "Delete facilities", "Delete sites",
            "View facilities", "View sites"
        };
        private static List<string> SysConfigPerms => new()
        {
            "Configure Application", "View Configuration"
        };
        private static List<string> UsersProfilesPerms => new()
        {
            "Add and edit profiles", "Add and edit users",
            "Configure user and profile management",
            "Delete profiles", "Delete users",
            "Reset users", "View profiles", "View users"
        };

        // Boolean lists map positionally to the perm name lists above
        private List<ComponentPermissions> BuildAdminPermissions()
        {
            return BuildComponents(
                AccuScreenPerms.Select(p => true).ToList(),
                DeviceMgmtPerms.Select(p => true).ToList(),
                PatientsTestsPerms.Select(p => true).ToList(),
                SitesFacilitiesPerms.Select(p => true).ToList(),
                SysConfigPerms.Select(p => true).ToList(),
                UsersProfilesPerms.Select(p => true).ToList()
            );
        }

        private List<ComponentPermissions> BuildScreenerPermissions()
        {
            return BuildComponents(
                new() { true, true, true, false, true },
                new() { false, false, false, true },
                new() { true, true, false, false, false, false, true },
                new() { false, false, false, false, false, true, true },
                new() { false, true },
                new() { false, false, false, false, false, false, true, true }
            );
        }

        private List<ComponentPermissions> BuildSupervisorPermissions()
        {
            return BuildComponents(
                new() { true, true, true, true, true },
                new() { true, true, true, true },
                new() { true, true, true, true, true, true, true },
                new() { false, false, false, false, false, true, true },
                new() { false, true },
                new() { false, true, false, false, true, true, true, true }
            );
        }

        private List<ComponentPermissions> BuildComponents(
            List<bool> accu, List<bool> device, List<bool> patients,
            List<bool> sites, List<bool> sysConfig, List<bool> users)
        {
            return new List<ComponentPermissions>
            {
                BuildComponent("AccuScreen Management", AccuScreenPerms, accu),
                BuildComponent("Device Management", DeviceMgmtPerms, device),
                BuildComponent("Patients and Tests", PatientsTestsPerms, patients),
                BuildComponent("Sites and Facilities", SitesFacilitiesPerms, sites),
                BuildComponent("System Configuration", SysConfigPerms, sysConfig),
                BuildComponent("Users and Profiles", UsersProfilesPerms, users)
            };
        }

        private ComponentPermissions BuildComponent(string name, List<string> permNames, List<bool> grants)
        {
            var comp = new ComponentPermissions { ComponentName = name };
            for (int i = 0; i < permNames.Count; i++)
                comp.Permissions.Add(new PermissionItem
                {
                    Name = permNames[i],
                    IsGranted = i < grants.Count && grants[i]
                });
            return comp;
        }

        // Card Selection

        private void ProfilesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var profile = ProfilesListView.SelectedItem as ProfileEntry;
            if (profile == null)
            {
                EmptyStatePanel.Visibility = Visibility.Visible;
                return;
            }
            EmptyStatePanel.Visibility = Visibility.Collapsed;
            LoadProfileIntoForm(profile);
        }

        private void LoadProfileIntoForm(ProfileEntry profile)
        {
            _isLoadingProfile = true;

            DetailHeaderText.Text = profile.Name.ToUpperInvariant();
            NameBox.Text = profile.Name;
            DescriptionBox.Text = profile.Description;

            BuildPermissionsTable(profile);

            _savedState = CaptureSnapshot();
            _undoStack.Clear();

            _isLoadingProfile = false;
        }

        // Build Permissions Table

        private void BuildPermissionsTable(ProfileEntry profile)
        {
            PermissionsContainer.Children.Clear();
            _componentGroups.Clear();

            for (int ci = 0; ci < profile.Components.Count; ci++)
            {
                var comp = profile.Components[ci];
                bool isLastComp = (ci == profile.Components.Count - 1);
                var permCbs = new List<CheckBox>();

                // Component header row
                var headerBorder = new Border
                {
                    Background = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#f5f7fa")),
                    Padding = new Thickness(12, 7, 12, 7),
                    BorderBrush = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#E5E7EB")),
                    BorderThickness = new Thickness(0, 0, 0, 1)
                };

                var headerGrid = new Grid();
                headerGrid.ColumnDefinitions.Add(
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                headerGrid.ColumnDefinitions.Add(
                    new ColumnDefinition { Width = new GridLength(60) });

                var headerLeft = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var compLabel = new TextBlock
                {
                    Text = string.Format(Strings.ProfilesContentView_ComponentPrefix, comp.ComponentName),
                    FontSize = 13,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#005FBE")),
                    VerticalAlignment = VerticalAlignment.Center
                };

                int checkedCount = comp.Permissions.Count(p => p.IsGranted);
                int totalCount = comp.Permissions.Count;
                var countBadge = new TextBlock
                {
                    Text = string.Format(Strings.ProfilesContentView_CountBadge, checkedCount, totalCount),
                    FontSize = 12,
                    Foreground = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#999")),
                    VerticalAlignment = VerticalAlignment.Center
                };

                headerLeft.Children.Add(compLabel);
                headerLeft.Children.Add(countBadge);
                Grid.SetColumn(headerLeft, 0);
                headerGrid.Children.Add(headerLeft);

                // Toggles all permissions in this component
                var compCb = new CheckBox
                {
                    Style = (Style)FindResource("PermissionCheckBoxStyle"),
                    IsThreeState = true,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Tag = comp
                };
                UpdateComponentCheckState(compCb, comp);
                compCb.Click += ComponentCheckBox_Click;
                Grid.SetColumn(compCb, 1);
                headerGrid.Children.Add(compCb);

                headerBorder.Child = headerGrid;
                PermissionsContainer.Children.Add(headerBorder);

                // Permission rows
                for (int pi = 0; pi < comp.Permissions.Count; pi++)
                {
                    var perm = comp.Permissions[pi];
                    bool isLastPerm = (pi == comp.Permissions.Count - 1);

                    // Thicker border between component groups for visual separation
                    var rowBorder = new Border
                    {
                        Padding = new Thickness(12, 7, 12, 7),
                        Background = Brushes.White,
                        BorderBrush = isLastPerm && !isLastComp
                            ? new SolidColorBrush(
                                (Color)ColorConverter.ConvertFromString("#E5E7EB"))
                            : new SolidColorBrush(
                                (Color)ColorConverter.ConvertFromString("#f5f5f5")),
                        BorderThickness = new Thickness(
                            0, 0, 0, isLastPerm && !isLastComp ? 2 : 1)
                    };

                    var hoverBrush = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#f9fbfe"));
                    rowBorder.MouseEnter += (s, ev) => rowBorder.Background = hoverBrush;
                    rowBorder.MouseLeave += (s, ev) => rowBorder.Background = Brushes.White;

                    var rowGrid = new Grid();
                    rowGrid.ColumnDefinitions.Add(
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    rowGrid.ColumnDefinitions.Add(
                        new ColumnDefinition { Width = new GridLength(60) });

                    var permLabel = new TextBlock
                    {
                        Text = perm.Name,
                        FontSize = 13,
                        Foreground = new SolidColorBrush(
                            (Color)ColorConverter.ConvertFromString("#333")),
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(32, 0, 0, 0)
                    };
                    Grid.SetColumn(permLabel, 0);
                    rowGrid.Children.Add(permLabel);

                    // Tag carries references needed by the click handler
                    var permCb = new CheckBox
                    {
                        Style = (Style)FindResource("PermissionCheckBoxStyle"),
                        IsChecked = perm.IsGranted,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Tag = new object[] { perm, comp, compCb, countBadge }
                    };
                    permCb.Click += PermissionCheckBox_Click;
                    Grid.SetColumn(permCb, 1);
                    rowGrid.Children.Add(permCb);

                    permCbs.Add(permCb);

                    rowBorder.Child = rowGrid;
                    PermissionsContainer.Children.Add(rowBorder);
                }

                _componentGroups.Add((compCb, permCbs, comp));
            }
        }

        // Checkbox Interactions

        private void ComponentCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (_isLoadingProfile) return;

            var cb = sender as CheckBox;
            var comp = cb?.Tag as ComponentPermissions;
            if (comp == null) return;

            PushUndo();

            bool setAll = cb.IsChecked == true;

            foreach (var perm in comp.Permissions)
                perm.IsGranted = setAll;

            var group = _componentGroups.FirstOrDefault(g => g.Data == comp);
            if (group.ComponentCb != null)
            {
                foreach (var pcb in group.PermissionCbs)
                    pcb.IsChecked = setAll;
            }

            UpdateCountBadgeForComponent(comp);

            // Force determinate — skip indeterminate on direct click
            cb.IsChecked = setAll;
        }

        private void PermissionCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (_isLoadingProfile) return;

            var cb = sender as CheckBox;
            var refs = cb?.Tag as object[];
            if (refs == null || refs.Length < 4) return;

            PushUndo();

            var perm = refs[0] as PermissionItem;
            var comp = refs[1] as ComponentPermissions;
            var compCb = refs[2] as CheckBox;
            var countBadge = refs[3] as TextBlock;

            if (perm != null)
                perm.IsGranted = cb.IsChecked == true;

            if (comp != null && compCb != null)
                UpdateComponentCheckState(compCb, comp);

            if (comp != null && countBadge != null)
            {
                int c = comp.Permissions.Count(p => p.IsGranted);
                int t = comp.Permissions.Count;
                countBadge.Text = string.Format(Strings.ProfilesContentView_CountBadge, c, t);
            }
        }

        private void UpdateComponentCheckState(CheckBox compCb, ComponentPermissions comp)
        {
            int total = comp.Permissions.Count;
            int granted = comp.Permissions.Count(p => p.IsGranted);

            if (granted == 0) compCb.IsChecked = false;
            else if (granted == total) compCb.IsChecked = true;
            else compCb.IsChecked = null;
        }

        // Walks the visual tree from the component checkbox to find its sibling count badge.
        // Fragile — depends on the exact structure built by BuildPermissionsTable.
        private void UpdateCountBadgeForComponent(ComponentPermissions comp)
        {
            var group = _componentGroups.FirstOrDefault(g => g.Data == comp);
            if (group.ComponentCb == null) return;

            var headerGrid = group.ComponentCb.Parent as Grid;
            if (headerGrid == null) return;

            foreach (UIElement child in headerGrid.Children)
            {
                if (child is StackPanel sp && sp.Children.Count >= 2
                    && sp.Children[1] is TextBlock badge)
                {
                    int c = comp.Permissions.Count(p => p.IsGranted);
                    int t = comp.Permissions.Count;
                    badge.Text = string.Format(Strings.ProfilesContentView_CountBadge, c, t);
                    return;
                }
            }
        }

        // Snapshot Helpers

        private ProfileSnapshot CaptureSnapshot()
        {
            var perms = new List<(string, string, bool)>();
            var profile = ProfilesListView.SelectedItem as ProfileEntry;
            if (profile != null)
            {
                foreach (var comp in profile.Components)
                    foreach (var perm in comp.Permissions)
                        perms.Add((comp.ComponentName, perm.Name, perm.IsGranted));
            }

            return new ProfileSnapshot(
                NameBox?.Text ?? "",
                DescriptionBox?.Text ?? "",
                perms);
        }

        private void ApplySnapshot(ProfileSnapshot s)
        {
            _isSuppressingUndo = true;
            _isLoadingProfile = true;

            NameBox.Text = s.Name;
            DescriptionBox.Text = s.Description;

            var profile = ProfilesListView.SelectedItem as ProfileEntry;
            if (profile != null)
            {
                foreach (var (comp, perm, granted) in s.Permissions)
                {
                    var c = profile.Components.FirstOrDefault(
                        x => x.ComponentName == comp);
                    var p = c?.Permissions.FirstOrDefault(x => x.Name == perm);
                    if (p != null) p.IsGranted = granted;
                }

                BuildPermissionsTable(profile);
            }

            _isLoadingProfile = false;
            _isSuppressingUndo = false;
        }

        private void PushUndo()
        {
            if (!_isInitialized || _isSuppressingUndo || _isLoadingProfile) return;
            _undoStack.Push(CaptureSnapshot());
        }

        // Save / Revert / Undo — called from toolbar

        // NOTE: HandleSave updates the in-memory model but doesn't persist to database
        /// <summary>
        /// Validates the selected profile's required fields and saves the current state
        /// as the new baseline.
        /// </summary>
        public void HandleSave()
        {
            var profile = ProfilesListView.SelectedItem as ProfileEntry;
            if (profile == null) return;

            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                AppDialog.Show(Strings.ProfilesContentView_NameRequired, Strings.ProfilesContentView_Validation,
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                NameBox.Focus();
                return;
            }

            profile.Name = NameBox.Text;
            profile.Description = DescriptionBox.Text;

            DetailHeaderText.Text = profile.Name.ToUpperInvariant();

            _savedState = CaptureSnapshot();
            _undoStack.Clear();

            AppDialog.Show(Strings.ProfilesContentView_ProfileSaved, Strings.ProfilesContentView_Save,
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Reverts all changes back to the last saved state.
        /// </summary>
        public void HandleRevert()
        {
            if (_savedState == null) return;
            PushUndo();
            ApplySnapshot(_savedState);
        }

        /// <summary>
        /// Restores the previous state from the undo stack.
        /// </summary>
        public void HandleUndo()
        {
            if (_undoStack.Count == 0) return;
            var prev = _undoStack.Pop();
            ApplySnapshot(prev);
        }

        // Add / Delete — called from toolbar

        /// <summary>
        /// Adds a new profile with all permissions ungranted and selects it.
        /// </summary>
        public void HandleAdd()
        {
            var emptyComps = BuildComponents(
                AccuScreenPerms.Select(_ => false).ToList(),
                DeviceMgmtPerms.Select(_ => false).ToList(),
                PatientsTestsPerms.Select(_ => false).ToList(),
                SitesFacilitiesPerms.Select(_ => false).ToList(),
                SysConfigPerms.Select(_ => false).ToList(),
                UsersProfilesPerms.Select(_ => false).ToList()
            );

            var newProfile = new ProfileEntry
            {
                Name = "New Profile",
                Description = "",
                Components = emptyComps
            };
            _profiles.Add(newProfile);
            ProfilesListView.SelectedItem = newProfile;
        }

        /// <summary>
        /// Deletes the selected profile after user confirmation.
        /// </summary>
        public void HandleDelete()
        {
            var profile = ProfilesListView.SelectedItem as ProfileEntry;
            if (profile == null) return;

            var result = AppDialog.Show(
                string.Format(Strings.ProfilesContentView_DeleteConfirm, profile.Name),
                Strings.ProfilesContentView_DeleteProfile, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                int idx = _profiles.IndexOf(profile);
                _profiles.Remove(profile);
                if (_profiles.Count > 0)
                    ProfilesListView.SelectedIndex = Math.Min(idx, _profiles.Count - 1);
            }
        }

        private void Field_Changed(object sender, TextChangedEventArgs e)
        {
            PushUndo();
        }
    }
}