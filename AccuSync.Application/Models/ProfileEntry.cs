// --------------------------------------------------------------------------------
// <copyright file="ProfileEntry.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel;

namespace AccuSync.Application.Models
{
    /// <summary>
    /// A user profile defining component permissions, shown in the profile management list.
    /// </summary>
    public class ProfileEntry : INotifyPropertyChanged
    {
        private string _name = "";
        private string _description = "";

        /// <summary>The profile's display name.</summary>
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }
        /// <summary>A short description of the profile.</summary>
        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(nameof(Description)); }
        }
        /// <summary>The per-component permissions granted by this profile.</summary>
        public List<ComponentPermissions> Components { get; set; } = new();

        /// <summary>Raised when a property value changes.</summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>Raises <see cref="PropertyChanged"/> for the given property name.</summary>
        /// <param name="n">The name of the property that changed.</param>
        protected void OnPropertyChanged(string n) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
