// --------------------------------------------------------------------------------
// <copyright file="FieldSetupTable.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;

namespace AccuSync.WPF.Helpers
{
    /// <summary>
    /// Shared visuals and helpers for the field-setup table views
    /// (DeviceFieldSetupView and FieldSetupConfigView), which build their row
    /// tables imperatively with the same palette and header-checkbox logic.
    /// </summary>
    public static class FieldSetupTable
    {
        /// <summary>
        /// Background brush for alternating (zebra-striped) table rows.
        /// </summary>
        public static readonly SolidColorBrush AltRowBg =
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fafbfc"));

        /// <summary>
        /// Brush used for table and cell borders.
        /// </summary>
        public static readonly SolidColorBrush BorderBrush =
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e5e7eb"));

        /// <summary>
        /// Brush used for standard cell text.
        /// </summary>
        public static readonly SolidColorBrush TextBrush =
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5C5149"));

        /// <summary>
        /// Brush used for placeholder/watermark text in empty cells.
        /// </summary>
        public static readonly SolidColorBrush PlaceholderBrush =
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BBBBBB"));

        /// <summary>
        /// Default border brush for an input cell that has no value yet.
        /// </summary>
        public static readonly SolidColorBrush DefaultBorderBrush =
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D1D5DB"));

        /// <summary>
        /// Border brush applied to an input cell once it has been filled in.
        /// </summary>
        public static readonly SolidColorBrush FilledBorderBrush =
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#28a745"));

        /// <summary>
        /// Sets a tri-state header checkbox from its rows: checked if all on,
        /// unchecked if all off, indeterminate otherwise.
        /// </summary>
        public static void SyncHeaderCheckBox(CheckBox header, IEnumerable<bool> rowStates)
        {
            var values = rowStates.ToList();
            bool allOn = values.All(v => v);
            bool allOff = values.All(v => !v);
            header.IsChecked = allOn ? true : allOff ? false : null;
        }
    }
}
