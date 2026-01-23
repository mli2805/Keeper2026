using System;
using KeeperDomain;

namespace KeeperWpf;

public class TranLine
{
    public DateTime Timestamp { get; set; }
    public string Category { get; set; } = null!;
    public string CounterpartyName { get; set; } = null!;
    public decimal Amount { get; set; }
    public string AmountStr => Amount.ToString("#,0.##") + " " + Currency.ToString().ToLower();
    public CurrencyCode Currency { get; set; }
    public string Comment { get; set; } = string.Empty;

    public string ColorStr { get; set; } = null!;
}