// --------------------------------------------------------------------------------
// <copyright file="AuthenticationResult.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using AccuSync.Application.Models;

namespace AccuSync.Application.Services
{
    /// <summary>
    /// Result of an authentication attempt. On failure, includes the reason
    /// and lockout details so the UI can display an appropriate message.
    /// </summary>
    public class AuthenticationResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public User User { get; set; }
        public bool IsLocked { get; set; }
        public TimeSpan RemainingLockTime { get; set; }
    }
}
