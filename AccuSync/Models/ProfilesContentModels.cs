// --------------------------------------------------------------------------------
// <copyright file="ProfilesContentModels.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel;

namespace AccuSync.Models
{
    public class PermissionItem : INotifyPropertyChanged
    {
        private bool _isGranted;
        public string Name { get; set; } = "";
        public bool IsGranted
        {
            get => _isGranted;
            set { _isGranted = value; OnPropertyChanged(nameof(IsGranted)); }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string n) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    public class ComponentPermissions
    {
        public string ComponentName { get; set; } = "";
        public List<PermissionItem> Permissions { get; set; } = new();
    }

    public class ProfileEntry : INotifyPropertyChanged
    {
        private string _name = "";
        private string _description = "";

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }
        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(nameof(Description)); }
        }
        public List<ComponentPermissions> Components { get; set; } = new();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string n) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
