// --------------------------------------------------------------------------------
// <copyright file="RibbonItemClickEventArgs.cs" company="AccuSync">
//     Copyright (c) 2026 AccuSync. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Application.Models;
using System;

namespace AccuSync.WPF.Controls
{
    /// <summary>
    /// Event args for toolbar item clicks. Carries the item name for routing
    /// and the full <see cref="RibbonItem"/> for additional context.
    /// </summary>
    public class RibbonItemClickEventArgs : EventArgs
    {
        public string ItemName { get; }
        public RibbonItem Item { get; }

        public RibbonItemClickEventArgs(string name, RibbonItem item)
        {
            ItemName = name;
            Item = item;
        }
    }
}