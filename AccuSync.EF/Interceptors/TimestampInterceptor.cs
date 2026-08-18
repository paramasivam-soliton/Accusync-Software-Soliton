// --------------------------------------------------------------------------------
// <copyright file="TimestampInterceptor.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using AccuSync.Core.Entities;
using AccuSync.Core.Entities.Patients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AccuSync.EF.Interceptors
{
    /// <summary>
    /// Sets CreationDate/ModificationDate on tracked <see cref="User"/> entities, and
    /// CreatedAt/ModifiedAt on tracked <see cref="IAuditableEntity"/> entities (Patient,
    /// PatientContact, TestSession, TestRecord), before every save. The two passes are
    /// independent — <see cref="User"/> uses Unix-seconds <c>long</c> and is left exactly as
    /// it was; <see cref="IAuditableEntity"/> uses <see cref="DateTime"/>, matching the
    /// Patient schema's ISO-text timestamp columns.
    /// </summary>
    public class TimestampInterceptor : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            ApplyTimestamps(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            ApplyTimestamps(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void ApplyTimestamps(DbContext context)
        {
            if (context == null) return;

            long nowUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            foreach (var entry in context.ChangeTracker.Entries<User>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreationDate = nowUnix;
                    entry.Entity.ModificationDate = nowUnix;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.ModificationDate = nowUnix;
                }
            }

            var nowUtc = DateTime.UtcNow;

            foreach (var entry in context.ChangeTracker.Entries<IAuditableEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = nowUtc;
                    entry.Entity.ModifiedAt = nowUtc;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.ModifiedAt = nowUtc;
                }
            }
        }
    }
}
