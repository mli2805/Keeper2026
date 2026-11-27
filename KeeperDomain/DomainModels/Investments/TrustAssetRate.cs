namespace KeeperDomain;

public class TrustAssetRate
{
    public int Id { get; set; }
    public int TrustAssetId { get; set; }
    public DateTime Date { get; set; } = DateTime.Today;
    public decimal Value { get; set; }
    public int Unit { get; set; } = 1;
    public CurrencyCode Currency { get; set; } = CurrencyCode.USD;
}
