// --------------------------------------------------------------------------------
// <copyright file="ImportResultItem.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace AccuSync.Application.Models
{
    /// <summary>
    /// One row in the Step 3 results lists.
    /// </summary>
    // NOTE: The "Error:" prefix in Detail is a contract between ImportFunction producers
    // and PopulateResults — both must agree on the prefix string.
    public class ImportResultItem
    {
        /// <summary>The patient's full name.</summary>
        public string Name { get; set; }
        /// <summary>The patient's identifier.</summary>
        public string PatientId { get; set; }
        /// <summary>Detail text describing the result, or an error prefixed with "Error:".</summary>
        public string Detail { get; set; }
        /// <summary>Outcome badge text: null, "TESTS ADDED", "REPLACED", or "NEW RECORD".</summary>
        public string Badge { get; set; }                      // null, "TESTS ADDED", "REPLACED", "NEW RECORD"
        /// <summary>The imported patient data.</summary>
        public PatientData Patient { get; set; }
    }
}
