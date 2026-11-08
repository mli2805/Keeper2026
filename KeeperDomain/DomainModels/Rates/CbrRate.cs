namespace KeeperDomain;

public class CbrRate
{
    public int Id { get; set; } //PK
    public OneRate Usd { get; set; } = new OneRate();
}
