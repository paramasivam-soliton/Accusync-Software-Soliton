// --------------------------------------------------------------------------------
// <copyright file="ImportOutcome.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Core.Entities;

namespace AccuSync.Application.Models
{
    /// <summary>
    /// The outcome of importing one patient — returned by <see cref="Abstractions.Parsing.IImportService.GetImportFunction"/>.
    /// The UI-bound counterpart is <c>AccuSync.Presentation.Models.ImportResultItem</c> —
    /// kept separate so this project never needs to reference AccuSync.Presentation.
    /// </summary>
    // NOTE: The "Error:" prefix in Detail is a contract between ImportFunction producers
    // and their callers — both must agree on the prefix string.
    public class ImportOutcome
    {
        /// <summary>The patient's full name.</summary>
        public string Name { get; set; }


        /// <summary>The patient's identifier.</summary>
        public string PatientId { get; set; }


        /// <summary>Detail text describing the result, or an error prefixed with "Error:".</summary>
        public string Detail { get; set; }
        

        /// <summary>Outcome badge text: null, "TESTS ADDED", "REPLACED", or "NEW RECORD".</summary>
        public string Badge { get; set; }


        /// <summary>The imported patient data.</summary>
        public PatientData Patient { get; set; }
    }
}
