// --------------------------------------------------------------------------------
// <copyright file="IQrCodeGenerator.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System;

namespace AccuSync.Application.Abstractions.Parsing
{
    /// <summary>
    /// Generates QR codes for patient data. Implemented by <c>AccuSync.Adapters.DataParser</c>.
    /// </summary>
    public interface IQrCodeGenerator
    {
        /// <summary>
        /// Generates a QR code and returns it as PNG-encoded bytes — the caller (View layer)
        /// is responsible for turning that into whatever image type it can display.
        /// Returns <c>null</c> if <paramref name="content"/> is empty or generation fails.
        /// </summary>
        /// <param name="content">The text to encode (typically from <see cref="FormatPatientData"/>).</param>
        /// <param name="pixelsPerModule">Size of each QR module in pixels. Higher values produce a larger image.</param>
        byte[] GenerateQRCode(string content, int pixelsPerModule = 8);

        /// <summary>
        /// Builds a newline-delimited string of patient fields for QR encoding.
        /// Uses a simple <c>KEY:VALUE</c> format that's easy to parse on the receiving end.
        /// </summary>
        string FormatPatientData(
            string firstName,
            string lastName,
            string patientId,
            DateTime? dateOfBirth,
            string gender,
            string hospitalId);

        /// <summary>
        /// JSON alternative to <see cref="FormatPatientData"/>.
        /// Produces a more portable format at the cost of a larger QR code.
        /// </summary>
        string FormatPatientDataAsJson(
            string firstName,
            string lastName,
            string patientId,
            DateTime? dateOfBirth,
            string gender,
            string hospitalId);
    }
}
