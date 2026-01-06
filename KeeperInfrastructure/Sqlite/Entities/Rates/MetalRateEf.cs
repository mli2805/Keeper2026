using KeeperDomain;

namespace KeeperInfrastructure;

public class MetalRateEf
{
    public int Id { get; set; } //PK
    public DateTime Date { get; set; }
    public Metal Metal { get; set; }
    public int Proba { get; set; }
    public double Price { get; set; }
}
