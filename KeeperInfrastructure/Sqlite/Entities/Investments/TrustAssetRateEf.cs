using KeeperDomain;

namespace KeeperInfrastructure;

public class TrustAssetRateEf
{
    public int Id { get; set; } // PK
    public int TrustAssetId { get; set; }
    public DateTime Date { get; set; } = DateTime.Today;
    public decimal Value { get; set; }
    public int Unit { get; set; } = 1;
    public CurrencyCode Currency { get; set; } = CurrencyCode.USD;
}
