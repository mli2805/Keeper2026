using System;
using System.Windows.Media;
using Caliburn.Micro;

namespace KeeperWpf;

public enum HouseholdPaymentsTableMode
{
    Separate,
    Monthly,
    Annual
}

public enum HouseholdPaymentsChartInterval
{
    Monthly,
    Annual
}

public sealed class HouseholdPaymentRow
{
    private static readonly Brush AlternateMonthBackground = CreateAlternateMonthBackground();

    public DateTime Date { get; init; }
    public string AccountName { get; init; } = string.Empty;
    public int? ChartAccountId { get; init; }
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

public sealed class HouseholdPaymentChartAccountOption : PropertyChangedBase
{
    private readonly Action<HouseholdPaymentChartAccountOption> _onSelected;
    private bool _isSelected;

    public int? AccountId { get; }
    public string Name { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value)
                return;

            _isSelected = value;
            NotifyOfPropertyChange();

            if (value)
                _onSelected(this);
        }
    }

    public HouseholdPaymentChartAccountOption(
        int? accountId,
        string name,
        Action<HouseholdPaymentChartAccountOption> onSelected)
    {
        AccountId = accountId;
        Name = name;
        _onSelected = onSelected;
    }
}
