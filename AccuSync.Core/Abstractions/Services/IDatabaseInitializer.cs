// --------------------------------------------------------------------------------
// <copyright file="IDatabaseInitializer.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Threading.Tasks;

namespace AccuSync.Core.Abstractions.Services
{
    /// <summary>
    /// Single entry point for bringing the database up to date at application startup
    /// (migrations, seeding). Not tied to any one repository — a repository's job is
    /// reading/writing its own entities, not owning the database's lifecycle.
    /// </summary>
    public interface IDatabaseInitializer
    {
        /// <summary>Applies pending migrations and seeds default data, if needed.</summary>
        Task InitializeAsync();
    }
}
