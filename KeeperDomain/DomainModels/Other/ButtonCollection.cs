namespace KeeperDomain;

public class ButtonCollection
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    public List<int> AccountIds { get; set; } = new List<int>();
}
