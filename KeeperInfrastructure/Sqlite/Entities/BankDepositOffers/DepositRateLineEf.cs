namespace KeeperInfrastructure;

public class DepositRateLineEf
{
    public int Id { get; set; }
    public int DepositOfferConditionsId { get; set; }
    public DateTime DateFrom { get; set; }
    public decimal AmountFrom { get; set; }
    public decimal AmountTo { get; set; }
    public decimal Rate { get; set; }
}