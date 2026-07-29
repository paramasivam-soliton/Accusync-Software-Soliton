// --------------------------------------------------------------------------------
// <copyright file="PasswordToggleTooltipConverter.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Windows.Data;

namespace AccuSync.WPF.Converters
{
    /// <summary>
    /// Provides the tooltip text for a password visibility toggle button,
    /// mirroring the logic in <see cref="PasswordToggleIconConverter"/>.
    /// </summary>
    public class PasswordToggleTooltipConverter : IValueConverter
    {
        // TODO: Move these strings to a resource file if the app needs localization.
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is bool isVisible)
                {
                    return isVisible ? "Hide password" : "Show password";
                }
                return "Show password";
            }
            catch
            {
                return "Show password";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}