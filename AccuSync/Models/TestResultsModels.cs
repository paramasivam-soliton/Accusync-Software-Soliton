// --------------------------------------------------------------------------------
// <copyright file="TestResultsModels.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using System.Windows.Media;

namespace AccuSync.Models
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
        public Brush LeftColor => Source.IsLeftEar ? GetBrush(Source.TestResult) : Brushes.Transparent;

        public string RightSymbol => Source.IsRightEar ? GetSymbol(Source.TestResult) : "";
        public Brush RightColor => Source.IsRightEar ? GetBrush(Source.TestResult) : Brushes.Transparent;

        private static string GetSymbol(string result) => result switch
        {
            "Pass" => "✓",
            "Refer" => "✗",
            _ => "?"
        };

        private static Brush GetBrush(string result) => result switch
        {
            "Pass" => new SolidColorBrush(Color.FromRgb(0x42, 0x9C, 0x10)),
            "Refer" => new SolidColorBrush(Color.FromRgb(0xdc, 0x35, 0x45)),
            _ => new SolidColorBrush(Color.FromRgb(0x9E, 0xA2, 0xAC))
        };
    }
}
