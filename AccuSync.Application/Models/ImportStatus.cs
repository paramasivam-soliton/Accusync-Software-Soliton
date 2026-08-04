// --------------------------------------------------------------------------------
// <copyright file="ImportStatus.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace AccuSync.Application.Models
{
    /// <summary>
    /// Whether an incoming patient is new or already exists in the database.
    /// </summary>
    public enum ImportStatus
    {
        New,
        Duplicate
    }
}
