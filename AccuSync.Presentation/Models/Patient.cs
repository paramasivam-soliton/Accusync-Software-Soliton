// --------------------------------------------------------------------------------
// <copyright file="Patient.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AccuSync.Core.Entities;

namespace AccuSync.Presentation.Models
{
    /// <summary>
    /// A patient record with demographic information and associated screening tests.
    /// </summary>
    public class Patient : INotifyPropertyChanged
    {
        private bool _isSelected;

        /// <summary>The patient's first name.</summary>
        public string FirstName { get; set; }
        /// <summary>The patient's last name.</summary>
        public string LastName { get; set; }
        /// <summary>The patient's date of birth.</summary>
        public DateTime BirthDate { get; set; }
        /// <summary>The patient's identifier.</summary>
        public string PatientId { get; set; }
        /// <summary>The patient's gender.</summary>
        public string Gender { get; set; }
        /// <summary>The patient's left ear screening result.</summary>
        public string LeftEarResult { get; set; }
        /// <summary>The patient's right ear screening result.</summary>
        public string RightEarResult { get; set; }
        /// <summary>The date the patient was last screened, if any.</summary>
        public DateTime? DateOfScreen { get; set; }

        /// <summary>Whether the patient is checked in a selectable list.</summary>
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

        /// <summary>The patient's hospital identifier.</summary>
        public string HospitalId { get; set; }
        /// <summary>The patient's birth location.</summary>
        public string BirthLocation { get; set; }
        /// <summary>The patient's gestational age at birth.</summary>
        public string GestationalAge { get; set; }
        /// <summary>The patient's birth time.</summary>
        public string BirthTime { get; set; }
        /// <summary>Whether the patient was in the NICU (Neonatal Intensive Care Unit).</summary>
        public string NICU { get; set; }
        /// <summary>Any other names the patient is known by.</summary>
        public string AlsoKnownAs { get; set; }
        /// <summary>The patient's title.</summary>
        public string Title { get; set; }
        /// <summary>The patient's middle initial.</summary>
        public string Initial { get; set; }
        /// <summary>The patient's weight.</summary>
        public string Weight { get; set; }
        /// <summary>The patient's height.</summary>
        public string Height { get; set; }
        /// <summary>Whether screening consent has been given for the patient.</summary>
        public string ScreeningConsent { get; set; }
        /// <summary>Whether tracking consent has been given for the patient.</summary>
        public string TrackingConsent { get; set; }

        /// <summary>The patient's first and last name initials.</summary>
        public string Initials => $"{FirstName?.FirstOrDefault()}{LastName?.FirstOrDefault()}";
        /// <summary>An icon representing the patient's gender.</summary>
        public string GenderIcon => Gender?.ToLower() == "male" ? "👨" : Gender?.ToLower() == "female" ? "👩" : "👤";

        /// <summary>Raised when a property value changes.</summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>Raises <see cref="PropertyChanged"/> for the given property name.</summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>The patient's screening test results.</summary>
        public List<TestRecord> Tests { get; set; } = new();
    }
}
