// --------------------------------------------------------------------------------
// <copyright file="AdminDashboardWindow.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Windows;
using AccuSync.Application.Helpers;

namespace AccuSync.WPF.Views.Dashboard
{
    /// <summary>
    /// Main admin shell window. Hosts the <see cref="SidebarNavigation"/> control
    /// and forwards user/navigation calls to it.
    /// </summary>
    public partial class AdminDashboardWindow : Window
    {
        /// <summary>
        /// Initializes the window and appends the dev mode label to the title when applicable.
        /// </summary>
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
        /// Forwards a navigation request to the sidebar shell.
        /// </summary>
        /// <param name="screenName">The name of the screen to navigate to.</param>
        public void NavigateToView(string screenName)
        {
            Shell.NavigateToView(screenName);
        }
    }
}