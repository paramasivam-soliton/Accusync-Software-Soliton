// --------------------------------------------------------------------------------
// <copyright file="ComponentPermissions.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Collections.Generic;

namespace AccuSync.Application.Models
{
    public class ComponentPermissions
    {
        public string ComponentName { get; set; } = "";
        public List<PermissionItem> Permissions { get; set; } = new();
    }
}
