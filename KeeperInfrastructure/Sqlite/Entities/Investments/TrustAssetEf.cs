using KeeperDomain;

namespace KeeperInfrastructure;

public class TrustAssetEf
{
    public int Id { get; set; }

    public int TrustAccountId { get; set; }
    public string Ticker { get; set; }
    public string Title { get; set; }
    public StockMarket StockMarket { get; set; }
    public AssetType AssetType { get; set; }

    #region Bonds special properties

    public decimal? Nominal { get; set; }

    // 2 поля для хранения Duration - например 182 Days
    public int? BondCouponDurationValue { get; set; }
    public Durations? BondCouponDuration { get; set; }

    public double? CouponRate { get; set; } // if fixed and known
    public DateTime? PreviousCouponDate { get; set; }
    public DateTime? BondExpirationDate { get; set; } = DateTime.MaxValue;

    #endregion

    public string Comment { get; set; } = string.Empty;

}
