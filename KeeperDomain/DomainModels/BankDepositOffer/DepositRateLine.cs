namespace KeeperDomain;

public class DepositRateLine
{
    public int Id { get; set; } //PK

    public int DepositOfferConditionsId { get; set; }

    public DateTime DateFrom { get; set; }
    public decimal AmountFrom { get; set; }
    public decimal AmountTo { get; set; }
    public decimal Rate { get; set; }
}
