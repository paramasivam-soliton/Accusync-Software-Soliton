// --------------------------------------------------------------------------------
// <copyright file="IAuditableEntity.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;

namespace AccuSync.Core.Entities.Patients
{
    /// <summary>
    /// Marker for entities whose CreatedAt/ModifiedAt are maintained automatically by
    /// <see cref="AccuSync.EF.Interceptors.TimestampInterceptor"/>, rather than set by
    /// each repository call site. Separate from <see cref="User"/>'s own timestamp
    /// fields, which use Unix-seconds <c>long</c> rather than <see cref="DateTime"/>.
    /// </summary>
    public interface IAuditableEntity
    {
        DateTime CreatedAt { get; set; }
        DateTime ModifiedAt { get; set; }
    }
}
