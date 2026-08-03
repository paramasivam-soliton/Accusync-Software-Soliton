// --------------------------------------------------------------------------------
// <copyright file="ImportModels.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel;

namespace AccuSync.Application.Models
{
    /// <summary>
    /// Whether an incoming patient is new or already exists in the database.
    /// </summary>
    public enum ImportStatus
    {
        New,
        Duplicate
    }

    /// <summary>
    /// The action to take for a patient on import. The first value (<see cref="Import"/>)
    /// applies to new patients; the rest are the choices offered for a duplicate.
    /// </summary>
    public enum ImportAction
    {
        Import,
        Replace,
        Create,
        AddTests,
        Skip
    }

    /// <summary>
    /// Input wrapper — one per patient found in the import file.
    /// Populated by the parser before calling LoadPreviewData.
    /// </summary>
    public class ImportPatientData
    {
        public PatientData Patient { get; set; }
        public ImportStatus Status { get; set; } = ImportStatus.New;
        public List<TestPreviewItem> Tests { get; set; } = new();
        public DuplicateInfo DupInfo { get; set; }
    }

    public class TestPreviewItem
    {
        public string Date { get; set; }
        public string Time { get; set; }
        public string Ear { get; set; }
        public string Type { get; set; }
        public string Result { get; set; }                     // "Pass", "Refer", "Incomplete"
        public bool IsExisting { get; set; }
    }

    public class DuplicateInfo
    {
        public List<string> InfoChanges { get; set; } = new();
        public int NewTests { get; set; }
    }

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

    /// <summary>
    /// Sent to the ImportFunction delegate per selected patient.
    /// </summary>
    public class ImportRequest
    {
        public PatientData Patient { get; set; }
        public ImportAction Action { get; set; }
    }

    /// <summary>
    /// One row in the Step 3 results lists.
    /// </summary>
    // NOTE: The "Error:" prefix in Detail is a contract between ImportFunction producers
    // and PopulateResults — both must agree on the prefix string.
    public class ImportResultItem
    {
        public string Name { get; set; }
        public string PatientId { get; set; }
        public string Detail { get; set; }
        public string Badge { get; set; }                      // null, "TESTS ADDED", "REPLACED", "NEW RECORD"
        public PatientData Patient { get; set; }
    }

    /// <summary>
    /// Result of parsing a file — patients with optional test previews and any
    /// parse-level errors. Errors are returned rather than thrown so the caller
    /// can display them in the import UI without catching exceptions.
    /// </summary>
    public class ParseResult
    {
        public List<ImportPatientData> Patients { get; set; } = new();
        public List<string> ParseErrors { get; set; } = new();

        /// <summary>
        /// File-level metadata extracted from the import file header.
        /// Used to populate the <c>ImportBatches</c> table when persisting.
        /// </summary>
        public ImportFileMetadata Metadata { get; set; }
    }

    /// <summary>
    /// Header-level metadata from the import file, stored in the
    /// <c>ImportBatches</c> database row for audit/traceability.
    /// </summary>
    public class ImportFileMetadata
    {
        public string SourceSystem { get; set; }    // ALGO 5, AccuLink, AccuSync, or ALGO Pro
        public string SourceVersion { get; set; }
        public string ExportTimestamp { get; set; }
        public string BaseLanguage { get; set; }
    }
}
