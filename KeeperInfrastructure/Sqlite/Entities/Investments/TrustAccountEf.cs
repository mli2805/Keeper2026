using KeeperDomain;

namespace KeeperInfrastructure;

public class TrustAccountEf
{
    public int Id { get; set; }
    public string Title { get; set; }
    public StockMarket StockMarket { get; set; }
    public string Number { get; set; }
    public CurrencyCode Currency { get; set; }
    public int AccountId { get; set; }
    public string Comment { get; set; }
}
