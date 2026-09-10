// --------------------------------------------------------------------------------
// <copyright file="StringToThicknessConverter.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AccuSync.WPF.Converters
{
    /// <summary>
    /// Parses a "left,top,right,bottom" (or single-value) string into a <see cref="Thickness"/>,
    /// using WPF's own <see cref="ThicknessConverter"/> under the hood. Lets Models expose
    /// margins/border-widths as plain strings instead of a WPF-typed <see cref="Thickness"/>.
    /// </summary>
    public class StringToThicknessConverter : IValueConverter
    {
        private static readonly ThicknessConverter Inner = new ThicknessConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s && !string.IsNullOrWhiteSpace(s))
            {
                return Inner.ConvertFromString(s);
            }
            return new Thickness(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
