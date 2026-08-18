// --------------------------------------------------------------------------------
// <copyright file="ByteArrayToBitmapImageConverter.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace AccuSync.WPF.Converters
{
    /// <summary>
    /// Bridges PNG-encoded bytes (e.g. <c>PatientViewModel.QRCodeImage</c>) to a WPF-bindable
    /// <see cref="BitmapImage"/> for an <c>Image.Source</c> binding. Kept in AccuSync.WPF —
    /// not AccuSync.Presentation — so the ViewModel itself carries no WPF reference.
    /// </summary>
    public class ByteArrayToBitmapImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not byte[] pngBytes || pngBytes.Length == 0) return null;

            using var memory = new MemoryStream(pngBytes);
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            // OnLoad reads everything into memory before the stream is disposed.
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            // Freeze makes the image immutable so it can be used from any thread.
            bitmapImage.Freeze();

            return bitmapImage;
        }

        // TODO: Implement ConvertBack if two-way binding is ever needed.
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
