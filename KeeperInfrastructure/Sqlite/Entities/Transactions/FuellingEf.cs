using KeeperDomain;

namespace KeeperInfrastructure;

public class FuellingEf
{
    public int Id { get; set; } //PK
    public int TransactionId { get; set; }

    public int CarAccountId { get; set; }
    public double Volume { get; set; }
    public FuelType FuelType { get; set; }
}
