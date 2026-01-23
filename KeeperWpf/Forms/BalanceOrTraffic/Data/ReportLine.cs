using System;

namespace KeeperWpf;

public class ReportLine
{
    public DateTime Date { get; set; }
    public Balance Before { get; set; } = null!;
    public Balance Income { get; set; } = null!;
    public Balance Outcome { get; set; } = null!;
    public Balance After { get; set; } = null!;
    public string Comment { get; set; } = string.Empty;

    public DepositOperationType Type { get; set; }
}

public enum DepositOperationType
{
    Contribution,
    Revenue,
    Consumption,
    Exchange,
}