// --------------------------------------------------------------------------------
// <copyright file="PatientRiskFactorValueConfiguration.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Core.Entities.Patients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccuSync.EF.Configurations
{
    /// <summary>
    /// Maps <see cref="PatientRiskFactorValue"/> to the PatientRiskFactorValues table.
    /// RiskFactorId is a plain column, not an EF navigation — it references
    /// SettingsDatabase.RiskFactors, which SQLite cannot foreign-key across database files.
    /// </summary>
    public class PatientRiskFactorValueConfiguration : IEntityTypeConfiguration<PatientRiskFactorValue>
    {
        public void Configure(EntityTypeBuilder<PatientRiskFactorValue> builder)
        {
            builder.ToTable("PatientRiskFactorValues");

            builder.HasKey(v => v.PatientRiskFactorValueId);

            builder.Property(v => v.Value).IsRequired();

            builder.HasIndex(v => v.PatientId);
            builder.HasIndex(v => v.RiskFactorId);
            builder.HasIndex(v => new { v.PatientId, v.RiskFactorId }).IsUnique();
        }
    }
}
