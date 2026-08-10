// --------------------------------------------------------------------------------
// <copyright file="PatientConfiguration.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Core.Entities.Patients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccuSync.EF.Configurations
{
    /// <summary>
    /// Maps <see cref="Patient"/> to the Patients table. Matches the shape of the
    /// hand-written schema this replaces (see Databases/PatientDatabase.sql at the repo root).
    /// SiteId/AssignedUserId are plain columns, not EF navigations — SQLite cannot enforce
    /// foreign keys across the separate SettingsDatabase.db file.
    /// </summary>
    public class PatientConfiguration : IEntityTypeConfiguration<Patient>
    {
        public void Configure(EntityTypeBuilder<Patient> builder)
        {
            builder.ToTable("Patients");

            builder.HasKey(p => p.PatientId);

            builder.HasIndex(p => p.SourceId).IsUnique();
            builder.HasIndex(p => p.SiteId);
            builder.HasIndex(p => p.AssignedUserId);
            builder.HasIndex(p => p.IsExported);
            builder.HasIndex(p => p.IsDeleted);

            builder.Property(p => p.IsDeleted).HasDefaultValue(false);
            builder.Property(p => p.IsExported).HasDefaultValue(false);

            builder.HasMany(p => p.Contacts)
                .WithOne()
                .HasForeignKey(c => c.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.TestSessions)
                .WithOne()
                .HasForeignKey(s => s.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.RiskFactorValues)
                .WithOne()
                .HasForeignKey(r => r.PatientId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
