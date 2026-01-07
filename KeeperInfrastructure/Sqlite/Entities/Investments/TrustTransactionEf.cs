using KeeperDomain;

namespace KeeperInfrastructure;

public class TrustTransactionEf
{
    public int Id { get; set; }
    public InvestOperationType InvestOperationType { get; set; }
    public DateTime Timestamp { get; set; }

    public int AccountId { get; set; }
    public int TrustAccountId { get; set; }

    public decimal CurrencyAmount { get; set; }
    public decimal CouponAmount { get; set; }
    public CurrencyCode Currency { get; set; }

    public int AssetAmount { get; set; }
    public int AssetId { get; set; }

    public decimal PurchaseFee { get; set; }
    public CurrencyCode PurchaseFeeCurrency { get; set; } = CurrencyCode.BYN;
    public int FeePaymentOperationId { get; set; }

    public string Comment { get; set; }
}
