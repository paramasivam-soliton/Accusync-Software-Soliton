// --------------------------------------------------------------------------------
// <copyright file="TestResultToBrushConverter.cs" company="Natus Sensory">
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
    /// Maps a test result ("Pass"/"Refer"/other) to a display color.
    /// A <c>null</c> result (ear not tested) maps to <see cref="Brushes.Transparent"/>.
    /// </summary>
    public class TestResultToBrushConverter : IValueConverter
    {
        private static readonly SolidColorBrush PassBrush = new SolidColorBrush(Color.FromRgb(0x42, 0x9C, 0x10));
        private static readonly SolidColorBrush ReferBrush = new SolidColorBrush(Color.FromRgb(0xdc, 0x35, 0x45));
        private static readonly SolidColorBrush OtherBrush = new SolidColorBrush(Color.FromRgb(0x9E, 0xA2, 0xAC));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as string) switch
            {
                null => Brushes.Transparent,
                "Pass" => PassBrush,
                "Refer" => ReferBrush,
                _ => OtherBrush
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
