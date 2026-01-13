using System;
using System.Globalization;
using System.Windows.Data;
using KeeperModels;

namespace KeeperWpf;

public class AccountIconConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is AccountItemModel accountItem)
        {
            return accountItem.GetIconPath();
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}