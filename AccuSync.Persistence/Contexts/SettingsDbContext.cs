// --------------------------------------------------------------------------------
// <copyright file="SettingsDbContext.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Application.Models;
using AccuSync.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace AccuSync.Persistence.Contexts
{
    /// <summary>
    /// Configuration/organisational-data database (SettingsDatabase.db). Provider-agnostic —
    /// the connection/provider is configured by whichever adapter project (e.g.
    /// AccuSync.SQLite) constructs the DbContextOptions.
    /// </summary>
    public class SettingsDbContext : DbContext
    {
        public SettingsDbContext(DbContextOptions<SettingsDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserConfiguration());
        }
    }
}
