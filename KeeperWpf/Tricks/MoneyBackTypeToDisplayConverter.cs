using System;
using System.Globalization;
using System.Windows.Data;

namespace KeeperWpf;

public class MoneyBackTypeToDisplayConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is MoneyBackType moneyBackType ? moneyBackType.ToDisplayString() : "";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Binding.DoNothing;
}
