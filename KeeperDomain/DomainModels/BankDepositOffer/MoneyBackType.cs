namespace KeeperDomain;


[StoreEnumAsString]
public enum MoneyBackType
{
    AnyWithExemptions,
    SpecificCategories,
}

public static class MoneyBackTypeExtensions
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
}
