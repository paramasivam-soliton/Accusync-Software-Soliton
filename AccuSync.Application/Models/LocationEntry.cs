// --------------------------------------------------------------------------------
// <copyright file="LocationEntry.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.ComponentModel;

namespace AccuSync.Application.Models
{
    // NOTE: Near-identical to FacilitiesContentView — same model pattern, same save/revert/undo,
    // same add/delete. Extract a shared NameCodeDescriptionConfigBase.
    /// <summary>
    /// A configurable location entry shown in the location management list.
    /// </summary>
    public class LocationEntry : INotifyPropertyChanged
    {
        private string _name = "";
        private string _description = "";
        private string _code = "";

        /// <summary>The location's display name.</summary>
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }
        /// <summary>A short description of the location.</summary>
        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(nameof(Description)); }
        }
        /// <summary>The location's code.</summary>
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
        /// <returns>A new <see cref="LocationEntry"/> with the same values.</returns>
        public LocationEntry Clone() => new LocationEntry
        {
            Name = Name,
            Description = Description,
            Code = Code
        };
    }
}
