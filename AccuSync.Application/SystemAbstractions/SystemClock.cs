// --------------------------------------------------------------------------------
// <copyright file="SystemClock.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using AccuSync.Application.Abstractions.SystemAbstractions;

namespace AccuSync.Application.SystemAbstractions
{
    public class SystemClock : IClock
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}
