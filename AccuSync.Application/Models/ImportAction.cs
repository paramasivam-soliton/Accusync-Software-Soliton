// --------------------------------------------------------------------------------
// <copyright file="ImportAction.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace AccuSync.Application.Models
{
    /// <summary>
    /// The action to take for a patient on import. The first value (<see cref="Import"/>)
    /// applies to new patients; the rest are the choices offered for a duplicate.
    /// </summary>
    public enum ImportAction
    {
        /// <summary>Import the patient as a new record.</summary>
        Import,

        /// <summary>Replace the existing patient's information with the imported data.</summary>
        Replace,

        /// <summary>Create a new patient record instead of matching the existing one.</summary>
        Create,

        /// <summary>Keep the existing patient's information and add the imported tests to it.</summary>
        AddTests,
        
        /// <summary>Skip this patient and do not import it.</summary>
        Skip
    }
}
