// --------------------------------------------------------------------------------
// <copyright file="IUnitOfWork.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Threading.Tasks;

namespace AccuSync.Application.Abstractions.Repositories
{
    /// <summary>
    /// Commits staged repository changes as a single transaction. Implemented by
    /// AccuSync.Persistence, wrapping the underlying DbContext's SaveChanges.
    /// </summary>
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync();
    }
}
