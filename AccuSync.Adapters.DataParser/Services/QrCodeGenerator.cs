// --------------------------------------------------------------------------------
// <copyright file="QrCodeGenerator.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Application.Abstractions.Parsing;
using QRCoder;
using System;

namespace AccuSync.Adapters.DataParser.Services
{
    /// <summary>
    /// Generates QR codes for patient data.
    /// Uses the QRCoder library and returns raw PNG bytes â€” the caller (View layer)
    /// is responsible for turning that into whatever image type it can display.
    /// </summary>
    public class QrCodeGenerator : IQrCodeGenerator
    {
        /// <summary>
        /// Generates a QR code and returns it as PNG-encoded bytes.
        /// Returns <c>null</c> if <paramref name="content"/> is empty or generation fails.
        /// </summary>
        /// <param name="content">The text to encode (typically from <see cref="FormatPatientData"/>).</param>
        /// <param name="pixelsPerModule">Size of each QR module in pixels. Higher values produce a larger image.</param>
        public byte[] GenerateQRCode(string content, int pixelsPerModule = 8)
        {
            if (string.IsNullOrWhiteSpace(content))
                return null;

            try
            {
                using (var qrGenerator = new QRCodeGenerator())
                {
                    var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.M);

                    using (var qrCode = new PngByteQRCode(qrCodeData))
                    {
                        // TODO: Pull this color from the NavyBrush theme resource (#003049)
                        //       so the QR code stays in sync with the rest of the UI.
                        return qrCode.GetGraphic(
                            pixelsPerModule,
                            new byte[] { 0, 48, 73, 255 },
                            new byte[] { 255, 255, 255, 255 });
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
        /// Builds a newline-delimited string of patient fields for QR encoding.
        /// Uses a simple <c>KEY:VALUE</c> format that's easy to parse on the receiving end.
        /// </summary>
        // TODO: Define this format in a shared spec or constant so producers and
        //       consumers (scanners, importers) stay in sync.
        public string FormatPatientData(
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
        public string FormatPatientDataAsJson(
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
