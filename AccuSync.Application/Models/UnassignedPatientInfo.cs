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
    public class UnassignedPatientInfo : INotifyPropertyChanged
    {
        private bool _isSelected;

        public string MRN { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DOB { get; set; }
        public string WaitTime { get; set; }
        public bool IsHighPriority { get; set; }
        public string PriorityText { get; set; }

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

        public string CardBackground => IsSelected ? "#EFF6FF" : "White";
        public string CardBorder => IsSelected ? "#3B82F6" : "#E5E7EB";
        public Thickness CardBorderThickness => IsSelected ? new Thickness(2) : new Thickness(1);
        public string CheckboxBackground => IsSelected ? "#3B82F6" : "White";
        public string CheckboxBorder => IsSelected ? "#3B82F6" : "#D1D5DB";
        public Visibility CheckmarkVisibility => IsSelected ? Visibility.Visible : Visibility.Collapsed;

        // Priority badge — shown only for high-priority patients (matches PriorityText).
        public Visibility PriorityVisibility => IsHighPriority ? Visibility.Visible : Visibility.Collapsed;
        public string PriorityBackground => "#FEE2E2";
        public string PriorityForeground => "#DC2626";

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
