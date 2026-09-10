// --------------------------------------------------------------------------------
// <copyright file="ProfileConfiguration.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccuSync.EF.Configurations
{
    /// <summary>
    /// Maps <see cref="Profile"/> to the Profiles table and seeds the two roles the
    /// application currently supports, with ids matching <see cref="UserRole"/> exactly.
    /// </summary>
    public class ProfileConfiguration : IEntityTypeConfiguration<Profile>
    {
        /// <summary>Applies the EF Core mapping for <see cref="Profile"/> to the given builder.</summary>
        public void Configure(EntityTypeBuilder<Profile> builder)
        {
            builder.ToTable("Profiles");

            // Screener's id is 0 — a value EF's default int-key convention treats as "not yet
            // assigned" and refuses to seed. ValueGeneratedNever is correct anyway: profile ids
            // are fixed, application-defined values tied to UserRole, never database-generated.
            builder.HasKey(p => p.ProfileId);
            builder.Property(p => p.ProfileId).ValueGeneratedNever();

            builder.Property(p => p.Name).IsRequired();

            builder.HasData(
                new Profile { ProfileId = (int)UserRole.Screener, Name = nameof(UserRole.Screener) },
                new Profile { ProfileId = (int)UserRole.Admin, Name = nameof(UserRole.Admin) });
        }
    }
}
