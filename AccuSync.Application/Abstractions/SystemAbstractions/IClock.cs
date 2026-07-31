// --------------------------------------------------------------------------------
// <copyright file="IClock.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;

namespace AccuSync.Application.Abstractions.SystemAbstractions
{
    public interface IClock
    {
        DateTimeOffset UtcNow { get; }
    }
}
