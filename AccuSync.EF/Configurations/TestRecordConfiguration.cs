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
    /// Maps <see cref="TestRecord"/> to the TestRecords table, matching
    /// Databases/PatientDatabase.sql. <see cref="TestRecord.PatientId"/> is indexed for query
    /// convenience but has no FK — the cascade-delete path runs through
    /// <see cref="TestRecord.SessionId"/> only (see <see cref="TestSessionConfiguration"/>),
    /// avoiding a second, redundant cascade path to the same Patients row.
    /// </summary>
    public class TestRecordConfiguration : IEntityTypeConfiguration<TestRecord>
    {
        public void Configure(EntityTypeBuilder<TestRecord> builder)
        {
            builder.ToTable("TestRecords");

            builder.HasKey(r => r.TestRecordId);

            builder.Property(r => r.TestType).IsRequired();

            builder.HasIndex(r => r.SessionId).HasDatabaseName("IX_TestRec_SessionId");
            builder.HasIndex(r => r.PatientId).HasDatabaseName("IX_TestRec_PatientId");
            builder.HasIndex(r => r.SourceId).HasDatabaseName("IX_TestRec_SourceId");
            builder.HasIndex(r => r.TestType).HasDatabaseName("IX_TestRec_TestType");
            builder.HasIndex(r => r.TestDate).HasDatabaseName("IX_TestRec_TestDate");
            builder.HasIndex(r => r.ImportBatchId).HasDatabaseName("IX_TestRec_ImportBatch");
        }
    }
}
