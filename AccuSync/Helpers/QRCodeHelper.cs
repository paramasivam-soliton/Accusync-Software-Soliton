// --------------------------------------------------------------------------------
// <copyright file="QRCodeHelper.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using QRCoder;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace AccuSync.Helpers
{
    /// <summary>
    /// Generates QR codes for patient data.
    /// Uses the QRCoder library and converts the output to WPF-compatible images.
    /// </summary>
    public static class QRCodeHelper
    {
        /// <summary>
        /// Generates a QR code as a WPF <see cref="BitmapImage"/>.
        /// Returns <c>null</c> if <paramref name="content"/> is empty or generation fails.
        /// </summary>
        /// <param name="content">The text to encode (typically from <see cref="FormatPatientData"/>).</param>
        /// <param name="pixelsPerModule">Size of each QR module in pixels. Higher values produce a larger image.</param>
        public static BitmapImage GenerateQRCode(string content, int pixelsPerModule = 8)
        {
            if (string.IsNullOrWhiteSpace(content))
                return null;

            try
            {
                using (var qrGenerator = new QRCodeGenerator())
                {
                    var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.M);

                    using (var qrCode = new QRCoder.QRCode(qrCodeData))
                    {
                        // TODO: Pull this color from the NavyBrush theme resource (#003049)
                        //       so the QR code stays in sync with the rest of the UI.
                        using (var bitmap = qrCode.GetGraphic(
                            pixelsPerModule,
                            Color.FromArgb(0, 48, 73),
                            Color.White,
                            true))
                        {
                            return BitmapToBitmapImage(bitmap);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"QR Code generation failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Bridges System.Drawing (<see cref="Bitmap"/>) to WPF (<see cref="BitmapImage"/>)
        /// by round-tripping through a PNG in memory.
        /// </summary>
        private static BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

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
        }

        /// <summary>
        /// Builds a newline-delimited string of patient fields for QR encoding.
        /// Uses a simple <c>KEY:VALUE</c> format that's easy to parse on the receiving end.
        /// </summary>
        // TODO: Define this format in a shared spec or constant so producers and
        //       consumers (scanners, importers) stay in sync.
        public static string FormatPatientData(
            string firstName,
            string lastName,
            string patientId,
            DateTime? dateOfBirth,
            string gender,
            string hospitalId)
        {
            var lines = new[]
            {
                $"FNAME:{firstName}",
                $"LNAME:{lastName}",
                $"PID:{patientId}",
                $"DOB:{dateOfBirth?.ToString("yyyy-MM-dd") ?? "N/A"}",
                $"GENDER:{gender ?? "N/A"}",
                $"HID:{hospitalId}"
            };

            return string.Join(",", lines);
        }

        /// <summary>
        /// JSON alternative to <see cref="FormatPatientData"/>.
        /// Produces a more portable format at the cost of a larger QR code.
        /// </summary>
        // TODO: Decide on one encoding format (KEY:VALUE vs JSON) and remove the other,
        //       or make the choice configurable. Having both risks inconsistency.
        public static string FormatPatientDataAsJson(
            string firstName,
            string lastName,
            string patientId,
            DateTime? dateOfBirth,
            string gender,
            string hospitalId)
        {
            return System.Text.Json.JsonSerializer.Serialize(new
            {
                name = $"{firstName} {lastName}",
                patientId = patientId,
                dob = dateOfBirth?.ToString("yyyy-MM-dd"),
                gender = gender,
                hospitalId = hospitalId
            });
        }
    }
}