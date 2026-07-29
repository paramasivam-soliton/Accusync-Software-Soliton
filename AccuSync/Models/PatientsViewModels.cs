// --------------------------------------------------------------------------------
// <copyright file="PatientsViewModels.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace AccuSync.Models
{
    public class Patient : INotifyPropertyChanged
    {
        private bool _isSelected;

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public string PatientId { get; set; }
        public string Gender { get; set; }
        public string LeftEarResult { get; set; }
        public string RightEarResult { get; set; }
        public DateTime? DateOfScreen { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public string HospitalId { get; set; }
        public string BirthLocation { get; set; }
        public string GestationalAge { get; set; }
        public string BirthTime { get; set; }
        public string NICU { get; set; }
        public string AlsoKnownAs { get; set; }
        public string Title { get; set; }
        public string Initial { get; set; }
        public string Weight { get; set; }
        public string Height { get; set; }
        public string ScreeningConsent { get; set; }
        public string TrackingConsent { get; set; }

        public string Initials => $"{FirstName?.FirstOrDefault()}{LastName?.FirstOrDefault()}";
        public string GenderIcon => Gender?.ToLower() == "male" ? "👨" : Gender?.ToLower() == "female" ? "👩" : "👤";

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public List<TestRecord> Tests { get; set; } = new();
    }
}
