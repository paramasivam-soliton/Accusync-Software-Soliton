// --------------------------------------------------------------------------------
// <copyright file="TestResultRow.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Core.Entities;

namespace AccuSync.Application.Models
{
    /// <summary>
    /// Wraps <see cref="TestRecord"/> for DataGrid display with computed columns
    /// for left/right ear result symbols and colors.
    /// </summary>
    public class TestResultRow
    {
        public TestRecord Source { get; }

        public TestResultRow(TestRecord test)
        {
            Source = test;
        }

        public string TestType => Source.TestType;
        public string TestDateFormatted => Source.TestDateFormatted;
        public string DurationFormatted => Source.DurationFormatted;
        public string Configuration => Source.TestType;  // Same as type for now
        public string Examiner => Source.Examiner;

        public string LeftSymbol => Source.IsLeftEar ? GetSymbol(Source.TestResult) : "";
        // Null when the ear wasn't tested — the View maps that to a transparent color.
        public string LeftResult => Source.IsLeftEar ? Source.TestResult : null;

        public string RightSymbol => Source.IsRightEar ? GetSymbol(Source.TestResult) : "";
        public string RightResult => Source.IsRightEar ? Source.TestResult : null;

        private static string GetSymbol(string result) => result switch
        {
            "Pass" => "✓",
            "Refer" => "✗",
            _ => "?"
        };
    }
}
