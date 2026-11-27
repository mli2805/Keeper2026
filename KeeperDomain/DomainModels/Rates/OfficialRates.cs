namespace KeeperDomain;

public class OfficialRates
{
    public int Id { get; set; } //PK
    public DateTime Date { get; set; }
    public NbRbRates NbRates { get; set; } = new NbRbRates();
    public CbrRate CbrRate { get; set; } = new CbrRate();
}
