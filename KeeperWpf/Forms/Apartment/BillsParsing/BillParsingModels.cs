using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace KeeperWpf;

public sealed record ParsedBillRow(
    DateTime Period,
    string ServiceName,
    string Unit,
    decimal? Volume,
    decimal? Tariff,
    decimal? Charged,
    string SourceFile)
{
    private static readonly Brush AlternateMonthBackground = CreateAlternateMonthBackground();

    public Brush MonthBackground => (Period.Year * 12 + Period.Month) % 2 == 0
        ? Brushes.White
        : AlternateMonthBackground;

    private static Brush CreateAlternateMonthBackground()
    {
        var brush = new SolidColorBrush(Color.FromRgb(242, 242, 242));
        brush.Freeze();
        return brush;
    }
}

public sealed record BillParsingDiagnostic(string SourceFile, string Message);

public sealed record BillParsingResult(
    IReadOnlyList<ParsedBillRow> Rows,
    IReadOnlyList<BillParsingDiagnostic> Diagnostics);
