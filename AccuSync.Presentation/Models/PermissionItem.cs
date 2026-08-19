// --------------------------------------------------------------------------------
// <copyright file="PermissionItem.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.ComponentModel;

namespace AccuSync.Presentation.Models
{
    /// <summary>
    /// A single named permission and whether it has been granted.
    /// </summary>
    public class PermissionItem : INotifyPropertyChanged
    {
        private bool _isGranted;

        /// <summary>The permission's display name.</summary>
        public string Name { get; set; } = "";

        /// <summary>Whether the permission is granted.</summary>
        public bool IsGranted
        {
            get => _isGranted;
            set { _isGranted = value; OnPropertyChanged(nameof(IsGranted)); }
        }

        /// <summary>Raised when a property value changes.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>Raises <see cref="PropertyChanged"/> for the given property name.</summary>
        /// <param name="n">The name of the property that changed.</param>
        protected void OnPropertyChanged(string n) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
