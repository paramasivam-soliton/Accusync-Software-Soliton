// --------------------------------------------------------------------------------
// <copyright file="AppSettingsConfiguration.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccuSync.EF.Configurations
{
    /// <summary>
    /// Maps <see cref="AppSettings"/> to the AppSettings table — a single-row table
    /// of app-wide configuration values.
    /// </summary>
    public class AppSettingsConfiguration : IEntityTypeConfiguration<AppSettings>
    {
        public void Configure(EntityTypeBuilder<AppSettings> builder)
        {
            builder.ToTable("AppSettings");

            // A genuine singleton row has nothing meaningful to identify it by, so this
            // key exists purely to satisfy EF/SQLite's requirement that every table have
            // one — it's a shadow property (no corresponding property on AppSettings
            // itself), auto-assigned on insert, and AppSettingsRepository never queries by it.
            builder.Property<int>("SettingsId").ValueGeneratedOnAdd();
            builder.HasKey("SettingsId");

            builder.Property(s => s.LockoutDurationMinutes).HasDefaultValue(15);
        }
    }
}
