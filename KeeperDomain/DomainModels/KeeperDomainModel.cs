namespace KeeperDomain;

public interface IDumpable
{
    string Dump();
}

public interface IParsable<T>
{
    T FromString(string s);
}

[Serializable]
public class KeeperDomainModel
{
    public List<OfficialRates> OfficialRates { get; set; } = null!;
    public List<ExchangeRates> ExchangeRates { get; set; } = null!;
    public List<MetalRate> MetalRates { get; set; } = null!;
    public List<RefinancingRate> RefinancingRates { get; set; } = null!;

    public List<TrustAccount> TrustAccounts { get; set; } = null!;
    public List<TrustAsset> TrustAssets { get; set; } = null!;
    public List<TrustAssetRate> TrustAssetRates { get; set; } = null!;
    public List<TrustTransaction> TrustTransactions { get; set; } = null!;

    public List<Account> AccountPlaneList { get; set; } = null!;
    public List<BankAccount> BankAccounts { get; set; } = null!;
    public List<Deposit> Deposits { get; set; } = null!;
    public List<PayCard> PayCards { get; set; } = null!;
    public List<ButtonCollection> ButtonCollections { get; set; } = null!;

    public List<Transaction> Transactions { get; set; } = null!;

    public List<Car> Cars { get; set; } = null!;
    public List<CarYearMileage> CarYearMileages { get; set; } = null!;
    public List<Fuelling> Fuellings { get; set; } = null!;

    public List<DepositOffer> DepositOffers { get; set; } = null!;
    public List<DepositRateLine> DepositRateLines { get; set; } = null!;
    public List<DepositConditions> DepositConditions { get; set; } = null!;

    public List<CardBalanceMemo> CardBalanceMemos { get; set; } = null!;
    public List<SalaryChange> SalaryChanges { get; set; } = null!;
    public List<LargeExpenseThreshold> LargeExpenseThresholds { get; set; } = null!;

}