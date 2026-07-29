// --------------------------------------------------------------------------------
// <copyright file="BooleanToCheckIconConverter.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AccuSync.Converters
{
    /// <summary>
    /// Converts a boolean validation result into a checkmark (valid) or X (invalid)
    /// SVG path geometry, typically used next to password-rule indicators.
    /// </summary>
    public class BooleanToCheckIconConverter : IValueConverter
    {
        // Material Design check / close icons.
        private const string CheckIcon = "M9 16.17L4.83 12l-1.42 1.41L9 19 21 7l-1.41-1.41z";
        private const string XIcon = "M19 6.41L17.59 5 12 10.59 6.41 5 5 6.41 10.59 12 5 17.59 6.41 19 12 13.41 17.59 19 19 17.59 13.41 12z";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is bool isValid)
                {
                    return Geometry.Parse(isValid ? CheckIcon : XIcon);
                }
                return Geometry.Parse(XIcon);
            }
            catch
            {
                return Geometry.Parse(XIcon);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}