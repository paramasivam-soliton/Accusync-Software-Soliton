// --------------------------------------------------------------------------------
// <copyright file="SitesContentModels.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.ComponentModel;

namespace AccuSync.Application.Models
{
    public class SiteEntry : INotifyPropertyChanged
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

        public SiteEntry Clone() => new SiteEntry
        {
            Name = Name,
            Description = Description,
            Code = Code
        };
    }
}
