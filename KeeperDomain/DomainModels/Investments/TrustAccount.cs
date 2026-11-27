namespace KeeperDomain;

public class TrustAccount
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public StockMarket StockMarket { get; set; }
    public string Number { get; set; } = string.Empty;
    public CurrencyCode Currency { get; set; }
    public int AccountId { get; set; }
    public string Comment { get; set; } = string.Empty;

    public string ToCombo => Title + " - " + Number;
}
