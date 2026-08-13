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
    /// Maps <see cref="Patient"/> to the Patients table, matching Databases/PatientDatabase.sql.
    /// </summary>
    public class PatientConfiguration : IEntityTypeConfiguration<Patient>
    {
        public void Configure(EntityTypeBuilder<Patient> builder)
        {
            builder.ToTable("Patients");

            builder.HasKey(p => p.PatientId);

            builder.HasIndex(p => p.SourceId).HasDatabaseName("IX_Patients_SourceId");
            builder.HasIndex(p => p.SiteId).HasDatabaseName("IX_Patients_SiteId");
            builder.HasIndex(p => p.AssignedUserId).HasDatabaseName("IX_Patients_AssignedUser");
            builder.HasIndex(p => p.IsExported).HasDatabaseName("IX_Patients_IsExported");
            builder.HasIndex(p => p.IsDeleted).HasDatabaseName("IX_Patients_IsDeleted");

            builder.Property(p => p.IsDeleted).HasDefaultValue(false);
            builder.Property(p => p.IsExported).HasDefaultValue(false);

            builder.HasMany(p => p.Contacts)
                .WithOne(c => c.Patient)
                .HasForeignKey(c => c.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.TestSessions)
                .WithOne(s => s.Patient)
                .HasForeignKey(s => s.PatientId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
