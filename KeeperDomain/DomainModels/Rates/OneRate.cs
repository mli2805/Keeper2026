namespace KeeperDomain;


public class OneRate
{
    public int Id { get; set; }
    public double Value { get; set; }
    public int Unit { get; set; } = 1;

    public OneRate Clone()
    {
        return (OneRate)MemberwiseClone();
    }
}
