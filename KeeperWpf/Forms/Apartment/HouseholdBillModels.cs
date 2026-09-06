using System;
using System.Windows.Media;

namespace KeeperWpf;

public enum HouseholdBillsTableMode
{
    Separate,
    Monthly,
    Annual
}

public enum HouseholdBillsChartInterval
{
    Monthly,
    Annual
}

public sealed class HouseholdBillRow
{
    private static readonly Brush AlternateMonthBackground = CreateAlternateMonthBackground();

    public DateTime Date { get; init; }
    public string AccountName { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public decimal AmountInUsd { get; init; }
    public string Comment { get; init; } = string.Empty;

    public Brush MonthBackground => (Date.Year * 12 + Date.Month) % 2 == 0
        ? Brushes.White
        : AlternateMonthBackground;

    private static Brush CreateAlternateMonthBackground()
    {
        var brush = new SolidColorBrush(Color.FromRgb(242, 242, 242));
        brush.Freeze();
        return brush;
    }
}
