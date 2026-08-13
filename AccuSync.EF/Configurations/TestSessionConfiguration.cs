// --------------------------------------------------------------------------------
// <copyright file="TestSessionConfiguration.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Core.Entities.Patients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccuSync.EF.Configurations
{
    /// <summary>
    /// Maps <see cref="TestSession"/> to the TestSessions table, matching
    /// Databases/PatientDatabase.sql.
    /// </summary>
    public class TestSessionConfiguration : IEntityTypeConfiguration<TestSession>
    {
        public void Configure(EntityTypeBuilder<TestSession> builder)
        {
            builder.ToTable("TestSessions");

            builder.HasKey(s => s.SessionId);

            builder.Property(s => s.IsExported).HasDefaultValue(false);

            builder.HasIndex(s => s.PatientId).HasDatabaseName("IX_Sessions_PatientId");
            builder.HasIndex(s => s.SessionDate).HasDatabaseName("IX_Sessions_Date");

            builder.HasMany(s => s.TestRecords)
                .WithOne(r => r.TestSession)
                .HasForeignKey(r => r.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
