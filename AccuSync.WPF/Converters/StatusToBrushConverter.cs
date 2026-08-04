// --------------------------------------------------------------------------------
// <copyright file="StatusToBrushConverter.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AccuSync.WPF.Converters
{
    /// <summary>
    /// Maps a protocol entry's "Active"/"Inactive" status string to a status color.
    /// Used by ABR/DPOAE protocol entries, which expose <c>Status</c> as plain data
    /// rather than a pre-built <see cref="Brush"/>.
    /// </summary>
    public class StatusToBrushConverter : IValueConverter
    {
        private static readonly SolidColorBrush ActiveBrush =
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#065F46"));
        private static readonly SolidColorBrush InactiveBrush =
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#991B1B"));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value as string == "Active" ? ActiveBrush : InactiveBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
