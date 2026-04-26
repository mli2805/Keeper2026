using KeeperDomain;
using KeeperModels;
using System;

namespace KeeperWpf;

public class YearExpenseRow
{
    public DateTime Timestamp { get; set; }
    public AccountItemModel? Category { get; set; }
    public decimal Amount { get; set; }
    public CurrencyCode Currency { get; set; }
    public decimal AmountInUsd { get; set; }
    public string Comment { get; set; } = string.Empty;
}
