// --------------------------------------------------------------------------------
// <copyright file="ImportRecord.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel;

namespace AccuSync.Application.Models
{
    /// <summary>
    /// Row model for the Step 2 preview table. Tracks selection,
    /// active state, and per-patient duplicate action.
    /// </summary>
    public class ImportRecord : INotifyPropertyChanged
    {
        private bool _isSelected = true;
        private bool _isActive;
        private ImportAction _dupAction = ImportAction.AddTests;

        /// <summary>The patient's full name.</summary>
        public string Name { get; set; }
        /// <summary>The patient's date of birth as parsed from the import file.</summary>
        public string DateOfBirth { get; set; }
        /// <summary>The patient's gender.</summary>
        public string Gender { get; set; }
        /// <summary>The patient's identifier.</summary>
        public string PatientId { get; set; }
        /// <summary>Whether this record is new or a duplicate of an existing patient.</summary>
        public ImportStatus Status { get; set; }
        /// <summary>The full patient data parsed from the import file.</summary>
        public PatientData Patient { get; set; }
        /// <summary>The tests found for this patient in the import file.</summary>
        public List<TestPreviewItem> Tests { get; set; } = new();
        /// <summary>Details of how this record differs from the existing patient, if a duplicate.</summary>
        public DuplicateInfo DupInfo { get; set; }

        /// <summary>The number of tests found for this patient.</summary>
        public int TestCount => Tests.Count;
        /// <summary>Whether this record matches an existing patient.</summary>
        public bool IsDuplicate => Status == ImportStatus.Duplicate;

        /// <summary>Whether this record is checked for import in the preview table.</summary>
        public bool IsSelected
        {
            get => _isSelected;
            set { if (_isSelected != value) { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); } }
        }

        /// <summary>Whether this record's row is enabled for editing in the preview table.</summary>
        public bool IsActive
        {
            get => _isActive;
            set { if (_isActive != value) { _isActive = value; OnPropertyChanged(nameof(IsActive)); } }
        }

        /// <summary>The action to take for this record when it is a duplicate.</summary>
        public ImportAction DupAction
        {
            get => _dupAction;
            set { if (_dupAction != value) { _dupAction = value; OnPropertyChanged(nameof(DupAction)); } }
        }

        /// <summary>Raised when a property value changes.</summary>
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
