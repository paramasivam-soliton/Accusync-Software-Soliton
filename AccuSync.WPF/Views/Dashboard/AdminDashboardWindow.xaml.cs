// --------------------------------------------------------------------------------
// <copyright file="AdminDashboardWindow.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Windows;
using AccuSync.Application.Helpers;
using AccuSync.Presentation.ViewModels;

namespace AccuSync.WPF.Views.Dashboard
{
    public partial class AdminDashboardWindow : Window
    {
        public AdminDashboardWindow()
        {
            InitializeComponent();

            if (DevModeConfig.IsAnyDevMode)
                Title += DevModeConfig.GetDevModeLabel();
        }

        public void SetCurrentUser(string username)
        {
            Shell.SetCurrentUser(username);
        }

        /// <summary>
        /// Shows/hides sidebar nav items per the logged-in user's role. Call once,
        /// right after <see cref="SetCurrentUser"/> and before <c>Show()</c>.
        /// </summary>
        public void SetPermissions(UserPermissionsViewModel permissions)
        {
            Shell.SetPermissions(permissions);
        }

        public void NavigateToView(string screenName)
        {
            Shell.NavigateToView(screenName);
        }
    }
}