// --------------------------------------------------------------------------------
// <copyright file="FacilitiesContentModels.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.ComponentModel;

namespace AccuSync.Application.Models
{
    // NOTE: FacilityEntry implements INotifyPropertyChanged (unlike most other inline models),
    // so the ListView card updates live when properties change. Good pattern.
    public class FacilityEntry : INotifyPropertyChanged
    {
        private string _name = "";
        private string _description = "";
        private string _code = "";
        private string _site = "";
        private string _locationType = "";

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
        public string Site
        {
            get => _site;
            set { _site = value; OnPropertyChanged(nameof(Site)); }
        }
        public string LocationType
        {
            get => _locationType;
            set { _locationType = value; OnPropertyChanged(nameof(LocationType)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

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
