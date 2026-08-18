// --------------------------------------------------------------------------------
// <copyright file="RibbonItemType.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

namespace AccuSync.WPF.Controls
{
    /// <summary>
    /// Defines the type of control rendered in the ribbon toolbar.
    /// </summary>
    public enum RibbonItemType
    {
        /// <summary>An icon+text button.</summary>
        Button,
        /// <summary>A dropdown selection control.</summary>
        Dropdown,
        /// <summary>A text search input with clear button.</summary>
        SearchField,
        /// <summary>A non-interactive visual spacer.</summary>
        Separator
    }
}
