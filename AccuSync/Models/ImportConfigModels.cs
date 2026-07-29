// --------------------------------------------------------------------------------
// <copyright file="ImportConfigModels.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace AccuSync.Models
{
    public class ImportEntry
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int ImportFormatIndex { get; set; } = 0;
        public int UserProfileIndex { get; set; } = 0;
        public string Password { get; set; } = "1234";
        public string PasswordVerify { get; set; } = "1234";
        public string ImportFolder { get; set; } = "";
    }
}
