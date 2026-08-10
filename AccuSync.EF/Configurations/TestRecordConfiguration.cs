// --------------------------------------------------------------------------------
// <copyright file="TestRecordConfiguration.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Core.Entities.Patients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccuSync.EF.Configurations
{
    /// <summary>
    /// Maps <see cref="TestRecord"/> to the TestRecords table. MatchedDeviceId/MatchedProtocolId
    /// are plain columns, not EF navigations — they reference SettingsDatabase, which SQLite
    /// cannot foreign-key across database files.
    /// </summary>
    public class TestRecordConfiguration : IEntityTypeConfiguration<TestRecord>
    {
        public void Configure(EntityTypeBuilder<TestRecord> builder)
        {
            builder.ToTable("TestRecords");

            builder.HasKey(r => r.TestRecordId);

            builder.Property(r => r.TestType).IsRequired();

            builder.HasIndex(r => r.SessionId);
            builder.HasIndex(r => r.PatientId);
            builder.HasIndex(r => r.SourceId).IsUnique();
            builder.HasIndex(r => r.TestType);
            builder.HasIndex(r => r.TestDate);
            builder.HasIndex(r => r.ImportBatchId);
        }
    }
}
