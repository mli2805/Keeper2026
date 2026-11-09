namespace KeeperDomain;


[StoreEnumAsString]
public enum MoneyBackType
{
    AnyWithExemptions,
    SpecificCategories,
    PartnerCounterparties,
}

public static class MoneyBackTypeExtensions
{
    public static string ToDisplayString(this MoneyBackType moneyBackType)
    {
        return moneyBackType switch
        {
            MoneyBackType.AnyWithExemptions => "На любые покупки кроме",
            MoneyBackType.SpecificCategories => "Только по категориям",
            MoneyBackType.PartnerCounterparties => "Только по партнерам",
            _ => throw new ArgumentOutOfRangeException(nameof(moneyBackType), moneyBackType, null)
        };
    }
}
