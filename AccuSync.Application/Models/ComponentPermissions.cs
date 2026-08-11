// --------------------------------------------------------------------------------
// <copyright file="ComponentPermissions.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Collections.Generic;

namespace AccuSync.Application.Models
{
    /// <summary>
    /// The set of permissions granted for a single application component under a profile.
    /// </summary>
    public class ComponentPermissions
    {
        /// <summary>The component's display name.</summary>
        public string ComponentName { get; set; } = "";
        /// <summary>The permissions defined for this component.</summary>
        public List<PermissionItem> Permissions { get; set; } = new();
    }
}
