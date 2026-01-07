namespace KeeperInfrastructure;

public class CardBalanceMemoEf
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public decimal BalanceThreshold { get; set; }
}
