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
        /// <summary>The underlying test record this row wraps.</summary>
        public TestRecord Source { get; }

        /// <summary>Creates a row wrapping the given test record.</summary>
        /// <param name="test">The test record to wrap.</param>
        public TestResultRow(TestRecord test)
        {
            Source = test;
        }

        /// <summary>The test type, from <see cref="Source"/>.</summary>
        public string TestType => Source.TestType;
        /// <summary>The formatted test date, from <see cref="Source"/>.</summary>
        public string TestDateFormatted => Source.TestDateFormatted;
        /// <summary>The formatted test duration, from <see cref="Source"/>.</summary>
        public string DurationFormatted => Source.DurationFormatted;
        /// <summary>The test configuration; currently the same as <see cref="TestType"/>.</summary>
        public string Configuration => Source.TestType;  // Same as type for now
        /// <summary>The examiner who performed the test, from <see cref="Source"/>.</summary>
        public string Examiner => Source.Examiner;

        /// <summary>The result symbol for the left ear column, blank if the test was not on the left ear.</summary>
        public string LeftSymbol => Source.IsLeftEar ? GetSymbol(Source.TestResult) : "";
        /// <summary>The left ear's raw result string, null if the test was not on the left ear (mapped to a transparent color by the View).</summary>
        public string LeftResult => Source.IsLeftEar ? Source.TestResult : null;

        /// <summary>The result symbol for the right ear column, blank if the test was not on the right ear.</summary>
        public string RightSymbol => Source.IsRightEar ? GetSymbol(Source.TestResult) : "";
        /// <summary>The right ear's raw result string, null if the test was not on the right ear (mapped to a transparent color by the View).</summary>
        public string RightResult => Source.IsRightEar ? Source.TestResult : null;

        private static string GetSymbol(string result) => result switch
        {
            "Pass" => "✓",
            "Refer" => "✗",
            _ => "?"
        };
    }
}
