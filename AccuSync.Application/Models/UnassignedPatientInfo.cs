// --------------------------------------------------------------------------------
// <copyright file="UnassignedPatientInfo.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Windows;

namespace AccuSync.Application.Models
{
    /// <summary>
    /// A patient waiting to be assigned to a screener, shown on the unassigned-patients worklist.
    /// </summary>
    public class UnassignedPatientInfo : INotifyPropertyChanged
    {
        private bool _isSelected;

        /// <summary>The patient's medical record number.</summary>
        public string MRN { get; set; }
        /// <summary>The patient's first name.</summary>
        public string FirstName { get; set; }
        /// <summary>The patient's last name.</summary>
        public string LastName { get; set; }
        /// <summary>The patient's date of birth.</summary>
        public DateTime DOB { get; set; }
        /// <summary>Display text for how long the patient has been waiting.</summary>
        public string WaitTime { get; set; }
        /// <summary>Whether the patient is flagged as high priority.</summary>
        public bool IsHighPriority { get; set; }
        /// <summary>Display text for the priority badge.</summary>
        public string PriorityText { get; set; }

        /// <summary>Whether the patient is checked for a batch assignment action.</summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                    OnPropertyChanged(nameof(CardBackground));
                    OnPropertyChanged(nameof(CardBorder));
                    OnPropertyChanged(nameof(CardBorderThickness));
                    OnPropertyChanged(nameof(CheckboxBackground));
                    OnPropertyChanged(nameof(CheckboxBorder));
                    OnPropertyChanged(nameof(CheckmarkVisibility));
                }
            }
        }

        /// <summary>The card's background color, reflecting <see cref="IsSelected"/>.</summary>
        public string CardBackground => IsSelected ? "#EFF6FF" : "White";
        /// <summary>The card's border color, reflecting <see cref="IsSelected"/>.</summary>
        public string CardBorder => IsSelected ? "#3B82F6" : "#E5E7EB";
        /// <summary>The card's border thickness, reflecting <see cref="IsSelected"/>.</summary>
        public Thickness CardBorderThickness => IsSelected ? new Thickness(2) : new Thickness(1);
        /// <summary>The selection checkbox's background color, reflecting <see cref="IsSelected"/>.</summary>
        public string CheckboxBackground => IsSelected ? "#3B82F6" : "White";
        /// <summary>The selection checkbox's border color, reflecting <see cref="IsSelected"/>.</summary>
        public string CheckboxBorder => IsSelected ? "#3B82F6" : "#D1D5DB";
        /// <summary>Whether the selection checkmark is visible, reflecting <see cref="IsSelected"/>.</summary>
        public Visibility CheckmarkVisibility => IsSelected ? Visibility.Visible : Visibility.Collapsed;

        // Priority badge — shown only for high-priority patients (matches PriorityText).
        /// <summary>Whether the priority badge is visible, shown only for high-priority patients.</summary>
        public Visibility PriorityVisibility => IsHighPriority ? Visibility.Visible : Visibility.Collapsed;
        /// <summary>Background color for the priority badge.</summary>
        public string PriorityBackground => "#FEE2E2";
        /// <summary>Foreground color for the priority badge.</summary>
        public string PriorityForeground => "#DC2626";

        /// <summary>Raised when a property value changes.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>Raises <see cref="PropertyChanged"/> for the given property name.</summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
