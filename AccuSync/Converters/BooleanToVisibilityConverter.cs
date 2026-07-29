// --------------------------------------------------------------------------------
// <copyright file="BooleanToVisibilityConverter.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AccuSync.Converters
{
    /// <summary>
    /// Converts a <see cref="bool"/> to a <see cref="Visibility"/> value.
    /// <c>true</c> maps to <see cref="Visibility.Visible"/>;
    /// anything else (including non-bool values) collapses the element.
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        // TODO: Implement ConvertBack if two-way binding is ever needed.
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}