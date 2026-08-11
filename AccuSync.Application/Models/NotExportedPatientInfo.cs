// --------------------------------------------------------------------------------
// <copyright file="NotExportedPatientInfo.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace AccuSync.Application.Models
{
    /// <summary>
    /// A patient with completed screenings that have not yet been exported.
    /// </summary>
    public class NotExportedPatientInfo
    {
        /// <summary>The patient's medical record number.</summary>
        public string MRN { get; set; }
        /// <summary>The patient's first name.</summary>
        public string FirstName { get; set; }
        /// <summary>The patient's last name.</summary>
        public string LastName { get; set; }
        /// <summary>The screener who owns the patient's unexported screening.</summary>
        public string OwnerScreener { get; set; }
    }
}
