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
    /// Sets timestamps on tracked entities before every save, replacing inline
    /// DateTimeOffset.UtcNow/DateTime.UtcNow calls at each repository call site.
    /// <see cref="User"/> uses its own long Unix-seconds fields (CreationDate/ModificationDate),
    /// handled separately below since it predates <see cref="IAuditableEntity"/> and uses a
    /// different timestamp representation than the Patient-side entities.
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
