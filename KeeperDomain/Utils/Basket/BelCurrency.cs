namespace KeeperDomain;

public class BelCurrency
{
    public string Iso { get; set; } = null!;
    public int Denomination { get; set; }

    public DateTime From { get; set; }
    public DateTime To { get; set; }
}
