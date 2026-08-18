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
    /// <summary>
    /// Main admin shell window. Hosts the <see cref="SidebarNavigation"/> control
    /// and forwards user/navigation calls to it.
    /// </summary>
    public partial class AdminDashboardWindow : Window
    {
        /// <summary>Creates the window and applies the dev-mode title suffix if active.</summary>
        public AdminDashboardWindow()
        {
            InitializeComponent();

            if (DevModeConfig.IsAnyDevMode)
                Title += DevModeConfig.GetDevModeLabel();
        }

        /// <summary>
        /// Forwards the current username to the sidebar shell.
        /// </summary>
        /// <param name="username">The logged-in user's display name.</param>
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

        /// <summary>
        /// Forwards a navigation request to the sidebar shell.
        /// </summary>
        /// <param name="screenName">The name of the screen to navigate to.</param>
        public void NavigateToView(string screenName)
        {
            Shell.NavigateToView(screenName);
        }
    }
}