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

        public string Name { get; set; }
        public string DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string PatientId { get; set; }
        public ImportStatus Status { get; set; }
        public PatientData Patient { get; set; }
        public List<TestPreviewItem> Tests { get; set; } = new();
        public DuplicateInfo DupInfo { get; set; }

        public int TestCount => Tests.Count;
        public bool IsDuplicate => Status == ImportStatus.Duplicate;

        public bool IsSelected
        {
            get => _isSelected;
            set { if (_isSelected != value) { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); } }
        }

        public bool IsActive
        {
            get => _isActive;
            set { if (_isActive != value) { _isActive = value; OnPropertyChanged(nameof(IsActive)); } }
        }

        public ImportAction DupAction
        {
            get => _dupAction;
            set { if (_dupAction != value) { _dupAction = value; OnPropertyChanged(nameof(DupAction)); } }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
