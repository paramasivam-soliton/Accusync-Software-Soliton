// --------------------------------------------------------------------------------
// <copyright file="IAuditableEntity.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;

namespace AccuSync.Core.Entities.Patients
{
    /// <summary>
    /// Implemented by entities whose CreatedAt/ModifiedAt are maintained automatically
    /// by TimestampInterceptor rather than set at each call site.
    /// </summary>
    public interface IAuditableEntity
    {
        DateTime CreatedAt { get; set; }
        DateTime ModifiedAt { get; set; }
    }
}
