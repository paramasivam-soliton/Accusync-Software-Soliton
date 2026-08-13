// --------------------------------------------------------------------------------
// <copyright file="PatientContactConfiguration.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Core.Entities.Patients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccuSync.EF.Configurations
{
    /// <summary>
    /// Maps <see cref="PatientContact"/> to the PatientContacts table, matching
    /// Databases/PatientDatabase.sql.
    /// </summary>
    public class PatientContactConfiguration : IEntityTypeConfiguration<PatientContact>
    {
        public void Configure(EntityTypeBuilder<PatientContact> builder)
        {
            builder.ToTable("PatientContacts");

            builder.HasKey(c => c.ContactId);

            builder.Property(c => c.ContactType).IsRequired();

            builder.HasIndex(c => c.PatientId).HasDatabaseName("IX_Contacts_PatientId");
            builder.HasIndex(c => c.ContactType).HasDatabaseName("IX_Contacts_Type");
        }
    }
}
