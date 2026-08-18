// --------------------------------------------------------------------------------
// <copyright file="RibbonItemClickEventArgs.cs" company="AccuSync">
//     Copyright (c) 2026 AccuSync. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;

namespace AccuSync.WPF.Controls
{
    /// <summary>
    /// Event args for toolbar item clicks. Carries the item name for routing
    /// and the full <see cref="RibbonItem"/> for additional context.
    /// </summary>
    public class RibbonItemClickEventArgs : EventArgs
    {
        /// <summary>Name of the clicked item.</summary>
        public string ItemName { get; }
        /// <summary>The clicked item's full definition.</summary>
        public RibbonItem Item { get; }

        /// <summary>Creates event args for a toolbar item click.</summary>
        /// <param name="name">Name of the clicked item.</param>
        /// <param name="item">The clicked item's full definition.</param>
        public RibbonItemClickEventArgs(string name, RibbonItem item)
        {
            ItemName = name;
            Item = item;
        }
    }
}