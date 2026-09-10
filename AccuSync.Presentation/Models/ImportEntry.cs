// --------------------------------------------------------------------------------
// <copyright file="ImportEntry.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace AccuSync.Presentation.Models
{
    /// <summary>
    /// A configurable import profile entry.
    /// </summary>
    public class ImportEntry
    {
        /// <summary>The import profile's display name.</summary>
        public string Name { get; set; } = "";

        /// <summary>A short description of the import profile.</summary>
        public string Description { get; set; } = "";

        /// <summary>Selected index into the import-format ComboBox.</summary>
        public int ImportFormatIndex { get; set; } = 0;

        /// <summary>Selected index into the user-profile ComboBox.</summary>
        public int UserProfileIndex { get; set; } = 0;

        /// <summary>The password required to run this import profile.</summary>
        public string Password { get; set; } = "1234";

        /// <summary>Confirmation entry for <see cref="Password"/>.</summary>
        public string PasswordVerify { get; set; } = "1234";

        /// <summary>The source folder for imported files.</summary>
        public string ImportFolder { get; set; } = "";
    }
}
