// --------------------------------------------------------------------------------
// <copyright file="PagedResult.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Collections.Generic;

namespace AccuSync.Core.Abstractions
{
    /// <summary>
    /// One page of results plus the total count across all pages. <see cref="IUserRepository"/>
    /// has no paging today, so there's nothing to mirror here — this is new for
    /// <see cref="Repositories.IPatientRepository.GetPagedAsync"/>.
    /// </summary>
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
