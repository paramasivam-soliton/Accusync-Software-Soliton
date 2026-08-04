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
    public class LocationEntry : INotifyPropertyChanged
    {
        private string _name = "";
        private string _description = "";
        private string _code = "";

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
        public string Code
        {
            get => _code;
            set { _code = value; OnPropertyChanged(nameof(Code)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public LocationEntry Clone() => new LocationEntry
        {
            Name = Name,
            Description = Description,
            Code = Code
        };
    }
}
