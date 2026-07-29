// --------------------------------------------------------------------------------
// <copyright file="ClinicalOptions.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

namespace AccuSync.Helpers
{
    /// <summary>
    /// Single source of truth for clinical option lists shared across views, so
    /// the same field can't drift between screens (e.g. the gestational age range
    /// previously differed between PatientDetailsTab and FacesheetReview).
    /// Exposed as static properties so XAML can bind via <c>{x:Static}</c>.
    /// </summary>
    public static class ClinicalOptions
    {
        /// <summary>
        /// Gestational age in completed weeks (20–45 inclusive).
        /// </summary>
        public static IReadOnlyList<string> GestationalAgeWeeks { get; } =
            Enumerable.Range(20, 26).Select(w => w.ToString()).ToList();
    }
}
