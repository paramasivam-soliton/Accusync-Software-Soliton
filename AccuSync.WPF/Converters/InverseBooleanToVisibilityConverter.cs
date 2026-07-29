// --------------------------------------------------------------------------------
// <copyright file="InverseBooleanToVisibilityConverter.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AccuSync.WPF.Converters
{
    /// <summary>
    /// The inverse of <see cref="BooleanToVisibilityConverter"/>:
    /// <c>true</c> collapses the element, <c>false</c> makes it visible.
    /// Handy for "show this when the flag is off" scenarios without negating the binding source.
    /// </summary>
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Visible;
        }

        // TODO: Implement ConvertBack if two-way binding is ever needed.
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}