// --------------------------------------------------------------------------------
// <copyright file="BooleanToColorConverter.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace AccuSync.WPF.Converters
{
    /// <summary>
    /// Maps a boolean validation state to a hex color string:
    /// green (<c>#27AE60</c>) for valid, red (<c>#E74C3C</c>) for invalid.
    /// Designed to pair with <see cref="BooleanToCheckIconConverter"/> for
    /// consistent validation feedback.
    /// </summary>
    /// <remarks>
    /// Returns raw hex strings rather than <see cref="Brush"/> objects, so
    /// the XAML binding target must accept a string (or use an additional converter).
    /// </remarks>
    public class BooleanToColorConverter : IValueConverter
    {
        private static readonly Brush ValidBrush = (Brush)System.Windows.Application.Current.Resources["ValidColorBrush"];
        private static readonly Brush InvalidBrush = (Brush)System.Windows.Application.Current.Resources["InvalidColorBrush"];

        /// <summary>Converts a boolean validation state to a brush.</summary>
        /// <param name="value">The value to convert; expected to be a <see cref="bool"/>.</param>
        /// <param name="targetType">The binding target type. Unused.</param>
        /// <param name="parameter">The converter parameter. Unused.</param>
        /// <param name="culture">The culture to use. Unused.</param>
        /// <returns><see cref="ValidBrush"/> when <paramref name="value"/> is <see langword="true"/>; otherwise <see cref="InvalidBrush"/>.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is bool isValid)
                {
                    return isValid ? ValidBrush : InvalidBrush;
                }
                return InvalidBrush;
            }
            catch
            {
                return InvalidBrush;
            }
        }

        /// <summary>Not supported for this one-way converter.</summary>
        /// <param name="value">Unused.</param>
        /// <param name="targetType">Unused.</param>
        /// <param name="parameter">Unused.</param>
        /// <param name="culture">Unused.</param>
        /// <returns>Never returns; always throws.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}