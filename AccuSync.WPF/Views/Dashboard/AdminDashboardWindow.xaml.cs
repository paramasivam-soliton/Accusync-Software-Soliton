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
    /// <summary>The admin dashboard window; hosts the sidebar-navigated shell content.</summary>
    public partial class AdminDashboardWindow : Window
    {
        /// <summary>Creates the window and applies the dev-mode title suffix if active.</summary>
        public AdminDashboardWindow()
        {
            InitializeComponent();

            if (DevModeConfig.IsAnyDevMode)
                Title += DevModeConfig.GetDevModeLabel();
        }

        /// <summary>Displays the signed-in user's name in the shell.</summary>
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

        /// <summary>Navigates the shell's content area to the named screen.</summary>
        public void NavigateToView(string screenName)
        {
            Shell.NavigateToView(screenName);
        }
    }
}