// --------------------------------------------------------------------------------
// <copyright file="PermissionItem.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.ComponentModel;

namespace AccuSync.Application.Models
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
}
