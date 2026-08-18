// --------------------------------------------------------------------------------
// <copyright file="FacilityEntry.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.ComponentModel;

namespace AccuSync.Presentation.Models
{
    // NOTE: FacilityEntry implements INotifyPropertyChanged (unlike most other inline models),
    // so the ListView card updates live when properties change. Good pattern.
    /// <summary>
    /// A configurable facility entry shown in the facility management list.
    /// </summary>
    public class FacilityEntry : INotifyPropertyChanged
    {
        private string _name = "";
        private string _description = "";
        private string _code = "";
        private string _site = "";
        private string _locationType = "";

        /// <summary>The facility's display name.</summary>
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        /// <summary>A short description of the facility.</summary>
        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(nameof(Description)); }
        }

        /// <summary>The facility's code.</summary>
        public string Code
        {
            get => _code;
            set { _code = value; OnPropertyChanged(nameof(Code)); }
        }

        /// <summary>The site the facility belongs to.</summary>
        public string Site
        {
            get => _site;
            set { _site = value; OnPropertyChanged(nameof(Site)); }
        }

        /// <summary>The facility's location type.</summary>
        public string LocationType
        {
            get => _locationType;
            set { _locationType = value; OnPropertyChanged(nameof(LocationType)); }
        }

        /// <summary>Raised when a property value changes.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>Raises <see cref="PropertyChanged"/> for the given property name.</summary>
        /// <param name="name">The name of the property that changed.</param>
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        /// <summary>Creates a copy of this entry with the same property values.</summary>
        /// <returns>A new <see cref="FacilityEntry"/> with the same values.</returns>
        public FacilityEntry Clone() => new FacilityEntry
        {
            Name = Name,
            Description = Description,
            Code = Code,
            Site = Site,
            LocationType = LocationType
        };
    }
}
