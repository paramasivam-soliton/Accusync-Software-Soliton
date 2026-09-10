// --------------------------------------------------------------------------------
// <copyright file="ImportRequest.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Core.Entities;

namespace AccuSync.Application.Models
{
    /// <summary>
    /// Sent to the ImportFunction delegate per selected patient.
    /// </summary>
    public class ImportRequest
    {
        /// <summary>The patient to import.</summary>
        public PatientData Patient { get; set; }
      
        /// <summary>The action to take for this patient.</summary>
        public ImportAction Action { get; set; }
    }
}
