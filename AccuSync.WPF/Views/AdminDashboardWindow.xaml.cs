// --------------------------------------------------------------------------------
// <copyright file="AdminDashboardWindow.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Windows;
using AccuSync.Application.Helpers;

namespace AccuSync.WPF.Views
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

        public void NavigateToView(string screenName)
        {
            Shell.NavigateToView(screenName);
        }
    }
}