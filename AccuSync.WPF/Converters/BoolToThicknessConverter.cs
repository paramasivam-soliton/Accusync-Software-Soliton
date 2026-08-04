// --------------------------------------------------------------------------------
// <copyright file="BoolToThicknessConverter.cs" company="Natus Sensory">
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
    /// Maps a selected/unselected bool to a border thickness — 2 when selected, 1 otherwise.
    /// </summary>
    public class BoolToThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool isSelected && isSelected ? new Thickness(2) : new Thickness(1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
