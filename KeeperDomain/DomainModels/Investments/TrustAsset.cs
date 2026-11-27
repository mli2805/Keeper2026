namespace KeeperDomain;

public class TrustAsset
{
    public int Id { get; set; }

    public int TrustAccountId { get; set; }
    public string Ticker { get; set; } = null!;
    public string Title { get; set; } = null!;
    public StockMarket StockMarket { get; set; }
    public AssetType AssetType { get; set; }

    #region Bonds special properties

    public decimal Nominal { get; set; }
    public Duration BondCouponPeriod { get; set; } = null!;
    public double CouponRate { get; set; } // if fixed and known
    public DateTime PreviousCouponDate { get; set; }
    public DateTime BondExpirationDate { get; set; } = DateTime.MaxValue;

    #endregion

    public string Comment { get; set; } = string.Empty;
}
