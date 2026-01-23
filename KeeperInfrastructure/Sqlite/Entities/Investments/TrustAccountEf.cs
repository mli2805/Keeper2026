using KeeperDomain;
using System.ComponentModel.DataAnnotations;

namespace KeeperInfrastructure;

public class TrustAccountEf
{
    public int Id { get; set; }
    [MaxLength(100)] public string Title { get; set; } = null!;
    public StockMarket StockMarket { get; set; }
    [MaxLength(30)] public string Number { get; set; } = null!;
    public CurrencyCode Currency { get; set; }
    public int AccountId { get; set; }
    [MaxLength(250)] public string Comment { get; set; } = string.Empty;
}
