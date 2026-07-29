// --------------------------------------------------------------------------------
// <copyright file="InverseBooleanConverter.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Windows.Data;

namespace AccuSync.WPF.Converters
{
    /// <summary>
    /// Negates a <see cref="bool"/> value. Supports round-tripping, so it works
    /// with two-way bindings (e.g., toggling an inverse IsEnabled state).
    /// </summary>
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            // Default to true so bound controls stay enabled when the source isn't a bool.
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return false;
        }
    }
}