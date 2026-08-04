// --------------------------------------------------------------------------------
// <copyright file="ImportResultItem.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Core.Entities;

namespace AccuSync.Application.Models
{
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
}
