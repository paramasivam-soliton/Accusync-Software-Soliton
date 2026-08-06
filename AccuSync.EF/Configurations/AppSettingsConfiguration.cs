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

            builder.HasKey(s => s.Id);

            builder.Property(s => s.LockoutDurationMinutes).HasDefaultValue(15);
        }
    }
}
