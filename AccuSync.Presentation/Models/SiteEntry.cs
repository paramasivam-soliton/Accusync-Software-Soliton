// --------------------------------------------------------------------------------
// <copyright file="SiteEntry.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.ComponentModel;

namespace AccuSync.Presentation.Models
{
    /// <summary>
    /// A configurable site entry shown in the site management list.
    /// </summary>
    public class SiteEntry : INotifyPropertyChanged
    {
        private string _name = "";
        private string _description = "";
        private string _code = "";

        /// <summary>The site's display name.</summary>
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        /// <summary>A short description of the site.</summary>
        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(nameof(Description)); }
        }

        /// <summary>The site's code.</summary>
        public string Code
        {
            get => _code;
            set { _code = value; OnPropertyChanged(nameof(Code)); }
        }

        /// <summary>Raised when a property value changes.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>Raises <see cref="PropertyChanged"/> for the given property name.</summary>
        /// <param name="name">The name of the property that changed.</param>
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        /// <summary>Creates a copy of this entry with the same property values.</summary>
        /// <returns>A new <see cref="SiteEntry"/> with the same values.</returns>
        public SiteEntry Clone() => new SiteEntry
        {
            Name = Name,
            Description = Description,
            Code = Code
        };
    }
}
