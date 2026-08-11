// --------------------------------------------------------------------------------
// <copyright file="SettingsDbContext.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Core.Entities;
using AccuSync.EF.Configurations;
using Microsoft.EntityFrameworkCore;

namespace AccuSync.EF.Contexts
{
    /// <summary>
    /// Configuration/organisational-data database (SettingsDatabase.db). The context itself
    /// doesn't know which provider is active — that's configured by whichever DbContextOptions
    /// are passed in (see <see cref="DependencyInjection.SqliteServiceCollectionExtensions"/>).
    /// </summary>
    public class SettingsDbContext : DbContext
    {
        public SettingsDbContext(DbContextOptions<SettingsDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<AppSettings> AppSettings => Set<AppSettings>();

        public DbSet<Profile> Profiles => Set<Profile>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Profiles must be configured (and its seed rows created) before
            // UserConfiguration, since Users.ProfileId is a foreign key into Profiles.
            modelBuilder.ApplyConfiguration(new ProfileConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new AppSettingsConfiguration());
        }
    }
}
