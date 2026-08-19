// --------------------------------------------------------------------------------
// <copyright file="TestPreview.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace AccuSync.Application.Models
{
    /// <summary>
    /// A single test result parsed from an import file, before any screen displays it.
    /// The UI-bound counterpart is <c>AccuSync.Presentation.Models.TestPreviewItem</c> —
    /// kept separate so this project never needs to reference AccuSync.Presentation.
    /// </summary>
    public class TestPreview
    {
        /// <summary>The test date.</summary>
        public string Date { get; set; }

        /// <summary>The test time.</summary>
        public string Time { get; set; }

        /// <summary>The ear tested.</summary>
        public string Ear { get; set; }

        /// <summary>The test type (e.g., TEOAE, ABR, DPOAE).</summary>
        public string Type { get; set; }

        /// <summary>The test result ("Pass", "Refer", or "Incomplete").</summary>
        public string Result { get; set; }

        /// <summary>Whether this test already exists in the database.</summary>
        public bool IsExisting { get; set; }
    }
}
