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

        public virtual DbSet<User> Users => Set<User>();
        public virtual DbSet<AppSettings> AppSettings => Set<AppSettings>();

        public DbSet<Profile> Profiles => Set<Profile>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ProfileConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new AppSettingsConfiguration());
        }
    }
}
