using System;
using System.Globalization;
using System.Windows.Data;

namespace KeeperWpf;

public class MoneyBackWhenToDisplayConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is MoneyBackWhen moneyBackWhen ? moneyBackWhen.ToDisplayString() : "";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Binding.DoNothing;
}
