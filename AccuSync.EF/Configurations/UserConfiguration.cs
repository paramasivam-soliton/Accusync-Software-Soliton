// --------------------------------------------------------------------------------
// <copyright file="UserConfiguration.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccuSync.EF.Configurations
{
    /// <summary>
    /// Maps <see cref="User"/> to the Users table. Matches the shape of the
    /// hand-written schema this replaces (see SettingsDatabase.sql at the repo root).
    /// </summary>
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Guid);

            builder.Property(u => u.AccountName).IsRequired();
            builder.HasIndex(u => u.AccountName).IsUnique();

            builder.Property(u => u.ProfilePassword).IsRequired();

            builder.Property(u => u.Status).HasDefaultValue(0);
            builder.Property(u => u.FirstLogin).HasDefaultValue(1);
            builder.Property(u => u.FailedLoginAttemptCount).HasDefaultValue(0);
            builder.Property(u => u.FailedResetAttemptCount).HasDefaultValue(0);
            builder.Property(u => u.FirstFailedLoginTime).HasDefaultValue(0L);
            builder.Property(u => u.FirstResetLoginTime).HasDefaultValue(0L);
            builder.Property(u => u.CreationDate).HasDefaultValue(0L);
            builder.Property(u => u.ModificationDate).HasDefaultValue(0L);
            builder.Property(u => u.PasswordModificationDate).HasDefaultValue(0L);
            builder.Property(u => u.LastThreePasswords).HasDefaultValue(string.Empty);
        }
    }
}
