// --------------------------------------------------------------------------------
// <copyright file="TimestampInterceptor.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using AccuSync.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AccuSync.EF.Interceptors
{
    /// <summary>
    /// Sets CreationDate/ModificationDate on tracked <see cref="User"/> entities before
    /// every save, replacing the inline DateTimeOffset.UtcNow calls the old
    /// hand-written UserRepository used to make at each call site.
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

            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            foreach (var entry in context.ChangeTracker.Entries<User>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreationDate = now;
                    entry.Entity.ModificationDate = now;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.ModificationDate = now;
                }
            }
        }
    }
}
