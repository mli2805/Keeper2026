namespace KeeperDomain;


[StoreEnumAsString]
public enum MoneyBackType
{
    AnyWithExemptions,
    SpecificCategories,
}


[StoreEnumAsString]
public enum MoneyBackWhen
{
    NextMonth,
    NextWorkingDay,
}

public static class MoneyBackExtensions
{
    public static string ToDisplayString(this MoneyBackType moneyBackType)
    {
        return moneyBackType switch
        {
            MoneyBackType.AnyWithExemptions => "На любые покупки кроме",
            MoneyBackType.SpecificCategories => "По избранным категориям",
            _ => throw new ArgumentOutOfRangeException(nameof(moneyBackType), moneyBackType, null)
        };
    }

    public static string ToDisplayString(this MoneyBackWhen moneyBackWhen)
    {
        return moneyBackWhen switch
        {
            MoneyBackWhen.NextMonth => "В следующем месяце",
            MoneyBackWhen.NextWorkingDay => "На следующий рабочий день",
            _ => throw new ArgumentOutOfRangeException(nameof(moneyBackWhen), moneyBackWhen, null)
        };
    }
}

