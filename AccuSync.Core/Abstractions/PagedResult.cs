// --------------------------------------------------------------------------------
// <copyright file="PagedResult.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Collections.Generic;

namespace AccuSync.Core.Abstractions
{
    /// <summary>
    /// A page of results plus the total count across all pages, so callers can render
    /// pagination controls without a second query.
    /// </summary>
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
    }
}
