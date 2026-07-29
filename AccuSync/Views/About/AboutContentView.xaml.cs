// --------------------------------------------------------------------------------
// <copyright file="AboutContentView.xaml.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.IO;
using System.Reflection;
using System.Windows.Controls;

namespace AccuSync.Views.About
{
    public partial class AboutContentView : UserControl
    {
        public AboutContentView()
        {
            InitializeComponent();
            PopulateBuildInfo();
        }

        // Drives the Version/Build labels from assembly metadata (csproj <Version>)
        // and the assembly's build timestamp, rather than hardcoded strings.
        private void PopulateBuildInfo()
        {
            var assembly = Assembly.GetExecutingAssembly();

            var version = assembly.GetName().Version;
            if (version != null)
                VersionText.Text = $"Version: {version.Major}.{version.Minor}.{version.Build}";

            try
            {
                var location = assembly.Location;
                if (!string.IsNullOrEmpty(location) && File.Exists(location))
                    BuildText.Text = $"Build: {File.GetLastWriteTime(location):yyyy.MM.dd}";
            }
            catch (Exception)
            {
                // Leave the placeholder if the build date can't be determined
                // (e.g. single-file publish where Location is empty).
            }
        }
    }
}